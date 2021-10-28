using System;
using System.Collections.Generic;
using VaccinationScheduling.Online.Tree;
using VaccinationScheduling.Shared;
using Range = VaccinationScheduling.Online.Tree.Range;

namespace VaccinationScheduling.Online
{
    public class Machines
    {
        /// <summary>
        /// RedBlackTree containing the ranges at which there are free spots to START a first job.
        /// </summary>
        public RedBlackTree freeRangesFirstJab;

        /// <summary>
        /// RedBlackTree containing the ranges at which there are free spots to START a second job.
        /// </summary>
        public RedBlackTree freeRangesSecondJab;

        public int NrMachines = 0;

        /// <summary>
        /// Create a machine schedule
        /// </summary>
        /// <param name="global">Global parameters of the program</param>
        public Machines(Global global)
        {
            freeRangesFirstJab = new RedBlackTree(global.TimeFirstDose);
            freeRangesSecondJab = new RedBlackTree(global.TimeSecondDose);
        }

        // Finds the first available spot inside the range.
        public (int, int, int, int) FindGreedySpot(Job job)
        {
            IEnumerator<Range> firstJabEnumerate = freeRangesFirstJab.FastEnumerateRange(job.MinFirstIntervalStart, job.MaxFirstIntervalStart).GetEnumerator();
            IEnumerator<Range> secondJabEnumerate = freeRangesSecondJab.FastEnumerateRange(job.MinFirstIntervalStart + job.MinGapIntervalStarts, job.MaxFirstIntervalStart + job.MaxGapIntervalStarts).GetEnumerator();

            List<(Range, int)> secondRanges = new();
            int slidingWindowIndex = 0;

            int bestScore = 0;
            int minFirstJabScore = 0;
            (int, int, int) bestFirstJabScore = (0, NrMachines, job.MinFirstIntervalStart);
            (int, int, int) bestSecondJabScore = (0, NrMachines, job.MinFirstIntervalStart + job.MinGapIntervalStarts);

            // Whilst there are still possible places left
            while (firstJabEnumerate.MoveNext())
            {
                // Found the best answer possible
                if (bestScore == 2)
                    break;

                bool expandCache = true;
                Range firstJab = firstJabEnumerate.Current;

                // Score is 0 if it does not fit, otherwise 1
                int firstJabScore = firstJab.NotList.Count == NrMachines ? 0 : 1;
                if (firstJabScore < minFirstJabScore)
                {
                    continue;
                }
                // The second job score is
                int minSecondJabScore = bestScore - firstJabScore;
                // Cannot have a second score higher than 1;
                if (minSecondJabScore >= 1)
                    continue;

                // Minimum T at which the first job can be scheduled is the minum of TMinJobStart and TRangeStart
                int minTFirstJab = Math.Max(firstJabEnumerate.Current.Start, job.MinFirstIntervalStart);
                // Maximum T at which the first job can be schedule is the maximum of TMaxJobStart and TRangeStart
                int maxTFirstJab = firstJabEnumerate.Current.EndMaybe == null ? job.MaxFirstIntervalStart : Math.Min(job.MaxFirstIntervalStart, (int)firstJabEnumerate.Current.EndMaybe);

                // The second job needs to be within these values given the first bounds.
                int minTSecondJab = minTFirstJab + job.MinGapIntervalStarts;
                int maxTSecondJab = maxTFirstJab + job.MaxGapIntervalStarts;

                // Go through current 'cache'
                for (int i = slidingWindowIndex; i < secondRanges.Count; i++)
                {
                    // The first value is too high for the current last item
                    // We slide the window further so we don't go past it in future anymore.
                    if (minTSecondJab < secondRanges[i].Item1.Start)
                    {
                        slidingWindowIndex++;
                        continue;
                    }
                    // Score is lower than the minimum score
                    if (secondRanges[i].Item2 < minSecondJabScore)
                        continue;
                    // The current item is out of range
                    if (secondJabEnumerate.Current.Start > maxTSecondJab)
                    {
                        expandCache = false;
                        break;
                    }

                    (int, int) overlap = getOverlapWithRange(secondJabEnumerate.Current, minTSecondJab, maxTSecondJab);
                    (int, int) scheduledTimes = getScheduleTimes(job, ref minTFirstJab, ref maxTFirstJab, ref overlap);

                    int firstJabMachine = firstJabEnumerate.Current.NotList.FindFirstNotContained();
                    int secondJabMachine = secondJabEnumerate.Current.NotList.FindFirstNotContained();
                    bestFirstJabScore = (firstJabScore, firstJabMachine, scheduledTimes.Item1);
                    bestSecondJabScore = (secondRanges[i].Item2, secondJabMachine, scheduledTimes.Item2);
                }

                // We do not want to expand the list since the last item is outside the range
                if (!expandCache)
                    continue;

                // Add new items to the list since we can expand the second
                while (secondJabEnumerate.MoveNext())
                {
                    int secondJabScore = secondJabEnumerate.Current.NotList.Count == NrMachines ? 0 : 1;
                    // Score is lower than the minimum score
                    if (secondJabScore < minSecondJabScore)
                    {
                        continue;
                    }

                    secondRanges.Add((secondJabEnumerate.Current, secondJabScore));

                    // The current item is too large
                    if (secondJabEnumerate.Current.Start > maxTSecondJab)
                        break;

                    (int, int) overlap = getOverlapWithRange(secondJabEnumerate.Current, minTSecondJab, maxTSecondJab);
                    (int, int) scheduledTimes = getScheduleTimes(job, ref minTFirstJab, ref maxTFirstJab, ref overlap);

                    int firstJabMachine = firstJabEnumerate.Current.NotList.FindFirstNotContained();
                    int secondJabMachine = secondJabEnumerate.Current.NotList.FindFirstNotContained();
                    bestFirstJabScore = (firstJabScore, firstJabMachine, scheduledTimes.Item1);
                    bestSecondJabScore = (secondJabScore, secondJabMachine, scheduledTimes.Item2);
                    bestScore = firstJabScore + secondJabScore;
                }
            }

            return (bestFirstJabScore.Item2, bestSecondJabScore.Item2, bestFirstJabScore.Item3, bestSecondJabScore.Item3);
        }

        private List<Range> EnumerateToList(int tMin, int tMax, bool firstJab)
        {
            List<Range> firstJobs = new List<Range>();
            RedBlackTree tree = firstJab ? freeRangesFirstJab : freeRangesSecondJab;

            foreach (Range range in tree.FastEnumerateRange(tMin, tMax))
            {
                firstJobs.Add(range);
            }

            return firstJobs;
        }

        // Finds the first available spot inside the range.
        public (int, int, int, int) FindSmartGreedySpot(Job job)
        {
            List<Range> firstJobRanges = EnumerateToList(job.MinFirstIntervalStart, job.MaxFirstIntervalStart, true);
            List<Range> secondJobRanges = EnumerateToList(job.MinFirstIntervalStart + job.MinGapIntervalStarts, job.MaxFirstIntervalStart + job.MaxGapIntervalStarts, false);

            int slidingWindowIndex = 0;

            int bestScore = (int)Score.NEWMACHINE + (int)Score.NEWMACHINE;
            (Score, int, int) bestFirstJobScore = (Score.NEWMACHINE, NrMachines, job.MinFirstIntervalStart);
            (Score, int, int) bestSecondJobScore = (Score.NEWMACHINE, NrMachines, job.MinFirstIntervalStart + job.MinGapIntervalStarts);

            // Whilst there are still possible places left
            for (int i = 0; i < firstJobRanges.Count; i++)
            {
                // Found the best answer possible
                if (bestScore == (int)Score.FLUSH + (int)Score.FLUSH)
                {
                    break;
                }

                Range firstJob = firstJobRanges[i];
                Overlap firstRangeOverlap = new(firstJob, job.MinFirstIntervalStart, job.MaxFirstIntervalStart);
                // Score is 0 if it does not fit, otherwise 1
                (Score, Score) firstJabScore = getScores(firstRangeOverlap, firstJob.NotList.Count, firstJob.MachineNrInBothNeighbours);
                // Cannot have a second score higher than 1;
                int minSecondJobScore = bestScore - (int)firstJabScore.Item1;
                if (minSecondJobScore > (int)Score.FLUSH)
                {
                    continue;
                }

                // The second job needs to be within these values given the first bounds.
                int minTSecondJob = firstRangeOverlap.OverlapStart + job.MinGapIntervalStarts;
                int maxTSecondJob = firstRangeOverlap.OverlapEnd + job.MaxGapIntervalStarts;

                for (int j = slidingWindowIndex; j < secondJobRanges.Count; j++)
                {
                    Range secondJab = secondJobRanges[j];
                    // The first value is too high for the current last item
                    // We slide the window further so we don't go past it in future anymore.
                    if (minTSecondJob < secondJab.Start)
                    {
                        slidingWindowIndex++;
                        continue;
                    }
                    // Out of range for the second item
                    if (secondJab.Start > maxTSecondJob)
                    {
                        break;
                    }

                    Overlap secondRangeOverlap = new(secondJab, minTSecondJob, maxTSecondJob);
                    (Score, Score) secondJabScores = getScores(secondRangeOverlap, secondJab.NotList.Count, secondJab.MachineNrInBothNeighbours);
                    // Score cannot be higher than the minimum score
                    if ((int)secondJabScores.Item2 <= minSecondJobScore)
                    {
                        continue;
                    }
                    (Score, Score, int, int) scheduledTimes = getSmartScheduleTimes(job, firstJabScore, secondJabScores, firstRangeOverlap, secondRangeOverlap);
                    // Score of the solution is not better than the best solution.
                    if ((int)scheduledTimes.Item1 + (int)scheduledTimes.Item2 <= bestScore)
                    {
                        continue;
                    }
                    int firstMachineNr = getMachineNr(scheduledTimes.Item1, scheduledTimes.Item3, firstRangeOverlap, firstJob);
                    int secondMachineNr = getMachineNr(scheduledTimes.Item2, scheduledTimes.Item4, secondRangeOverlap, secondJab);

                    bestFirstJobScore = (scheduledTimes.Item1, firstMachineNr, scheduledTimes.Item3);
                    bestSecondJobScore = (scheduledTimes.Item2, secondMachineNr, scheduledTimes.Item4);
                    bestScore = (int)scheduledTimes.Item1 + (int)scheduledTimes.Item2;
                }
            }

            //Extensions.WriteDebugLine($"Best score {(int)bestFirstJobScore.Item1 + (int)bestSecondJobScore.Item1}");
            //Extensions.WriteDebugLine($"#1 Machine: {bestFirstJobScore.Item2} T:{bestFirstJobScore.Item3}  #2 Machine: {bestSecondJobScore.Item2} T:{bestSecondJobScore.Item3}");
            return (bestFirstJobScore.Item2, bestSecondJobScore.Item2, bestFirstJobScore.Item3, bestSecondJobScore.Item3);
        }

        private (Score, Score) getScores(Overlap overlap, int machinesInRange, int? hasNeighboursFlush)
        {
            if (hasNeighboursFlush != null)
            {
                return (Score.FLUSH, Score.FLUSH);
            }
            if (machinesInRange == NrMachines)
            {
                // Scheduled on new machine
                return (Score.NEWMACHINE, Score.NEWMACHINE);
            }
            if (overlap.OverlapEnd - overlap.OverlapStart < 2 && overlap.StartOverlaps && overlap.EndOverlaps)
            {
                return (Score.NEIGHBOURSONE, Score.NEIGHBOURSONE);
            }
            if (overlap.StartOverlaps || overlap.EndOverlaps)
            {
                return (Score.EXISTINGMACHINE, Score.NEIGHBOURSONE);
            }
            return (Score.EXISTINGMACHINE, Score.EXISTINGMACHINE);
        }

        private (int, int) getOverlapWithRange(Range range, int tStart, int tEnd)
        {
            return (Math.Max(range.Start, tStart), range.EndMaybe is {} end ? Math.Min(end, tEnd) : tEnd);
        }

        private (int, int) getScheduleTimes(Job job, ref int tFirstIntervalStart, ref int tFirstIntervalEnd, ref (int, int) tSecondInterval)
        {
            // We start the second job as soon as possible
            int secondJabStart = tSecondInterval.Item1;
            // The first job timing depends on the second job timing
            int firstJabStart = Math.Max(tFirstIntervalStart, secondJabStart - job.MaxGapIntervalStarts);
            return (firstJabStart, secondJabStart);
        }

        private (Score, Score, int, int) getSmartScheduleTimes(Job job, (Score, Score) firstJabScores, (Score, Score) secondJabScores, Overlap firstJobOverlap, Overlap secondJobOverlap)
        {
            // Schedule it neighbouring another node
            switch ((firstJobOverlap.Length1, secondJobOverlap.Length1))
            {
                case (true, true):
                    return (firstJabScores.Item2, secondJabScores.Item2, firstJobOverlap.OverlapStart, secondJobOverlap.OverlapStart);
                case (true, false):
                    if (secondJobOverlap.EndOverlaps)
                    {
                        return (firstJabScores.Item2, secondJabScores.Item2, firstJobOverlap.OverlapStart, secondJobOverlap.OverlapEnd);
                    }
                    return (firstJabScores.Item2, secondJabScores.Item2, firstJobOverlap.OverlapStart, secondJobOverlap.OverlapStart);
                case (false, true):
                    if (firstJobOverlap.EndOverlaps)
                    {
                        return (firstJabScores.Item2, secondJabScores.Item2, firstJobOverlap.OverlapEnd, secondJobOverlap.OverlapStart);
                    }
                    return (firstJabScores.Item2, secondJabScores.Item2, firstJobOverlap.OverlapStart, secondJobOverlap.OverlapStart);
            }
            // Try schedule it where two of them neighbouring
            if (firstJobOverlap.StartOverlaps)
            {
                if (secondJobOverlap.StartOverlaps && firstJobOverlap.OverlapStart + job.MinGapIntervalStarts <= secondJobOverlap.OverlapStart)
                {
                    return (firstJabScores.Item2, secondJabScores.Item2, firstJobOverlap.OverlapStart, secondJobOverlap.OverlapStart);
                }
                else if (secondJobOverlap.EndOverlaps && firstJobOverlap.OverlapStart + job.MaxGapIntervalStarts <= secondJobOverlap.OverlapEnd)
                {
                    return (firstJabScores.Item2, secondJabScores.Item2, firstJobOverlap.OverlapStart, secondJobOverlap.OverlapEnd);
                }
            }
            else if (firstJobOverlap.EndOverlaps)
            {
                if (secondJobOverlap.EndOverlaps && firstJobOverlap.OverlapEnd + job.MinGapIntervalStarts <= secondJobOverlap.OverlapEnd)
                {
                    return (firstJabScores.Item2, secondJabScores.Item2, firstJobOverlap.OverlapEnd, secondJobOverlap.OverlapEnd);
                }
                else if (secondJobOverlap.StartOverlaps && firstJobOverlap.OverlapEnd + job.MinGapIntervalStarts <= secondJobOverlap.OverlapStart)
                {
                    return (firstJabScores.Item2, secondJabScores.Item2, firstJobOverlap.OverlapEnd, secondJobOverlap.OverlapStart);
                }
            }

            // Try to schedule it so it at least neighbours one other machine
            if (firstJobOverlap.StartOverlaps)
            {
                return (firstJabScores.Item2, secondJabScores.Item1, firstJobOverlap.OverlapStart, Math.Max(secondJobOverlap.OverlapStart, firstJobOverlap.OverlapStart + job.MinGapIntervalStarts));
            }
            else if (firstJobOverlap.EndOverlaps && firstJobOverlap.OverlapEnd + job.MinGapIntervalStarts <= secondJobOverlap.OverlapEnd)
            {
                return (firstJabScores.Item2, secondJabScores.Item1, firstJobOverlap.OverlapEnd, Math.Max(secondJobOverlap.OverlapStart, firstJobOverlap.OverlapEnd + job.MinGapIntervalStarts));
            }
            else if (secondJobOverlap.StartOverlaps && secondJobOverlap.OverlapStart - job.MinGapIntervalStarts >= firstJobOverlap.OverlapStart)
            {
                return (firstJabScores.Item1, secondJabScores.Item2, Math.Min(firstJobOverlap.OverlapEnd, secondJobOverlap.OverlapStart - job.MinGapIntervalStarts), secondJobOverlap.OverlapStart);
            }
            else if (secondJobOverlap.EndOverlaps)
            {
                return (firstJabScores.Item1, secondJabScores.Item2, Math.Min(firstJobOverlap.OverlapEnd, secondJobOverlap.OverlapEnd - job.MinGapIntervalStarts), secondJobOverlap.OverlapEnd);
            }
            else
            {
                return (firstJabScores.Item1, secondJabScores.Item1, firstJobOverlap.OverlapStart, Math.Max(secondJobOverlap.OverlapStart, firstJobOverlap.OverlapStart + job.MinGapIntervalStarts));
            }
        }

        private int getMachineNr(Score jabScore, int tScheduled, Overlap overlap, Range range)
        {
            // Sits flush
            if (jabScore == Score.FLUSH)
            {
                return (int)range.MachineNrInBothNeighbours;
            }

            else if (jabScore == Score.NEIGHBOURSONE && tScheduled == range.Start && overlap.StartOverlaps)
            {
                if (range.Start == 0)
                {
                    return range.NotList.FindFirstNotContained();
                }
                else
                {
                    return (int)range.InLeftItem;
                }
            }
            else if (jabScore == Score.NEIGHBOURSONE/* && tScheduled == (int)range.EndMaybe && overlap.EndOverlaps*/)
            {
                return (int)range.InRightItem;
            }
            else
            {
                return range.NotList.FindFirstNotContained();
            }
        }

        /// <summary>
        /// Schedule two jobs given both ranges to insert the job on
        /// </summary>
        /// <param name="tFirstJab">The time at which to schedule the first job</param>
        /// <param name="firstJab">The range that the first job gets scheduled on</param>
        /// <param name="tSecondJab">The time at which to schedule the second job</param>
        /// <param name="secondJab">The range that the first job gets scheduled on</param>
        public void ScheduleJobs(int firstMachineNr, int secondMachineNr, int tFirstJab, int tSecondJab)
        {
            if (firstMachineNr == NrMachines || secondMachineNr == NrMachines) NrMachines++;

            // Remove ranges of each of the jobs
            freeRangesFirstJab.RemoveRange(tFirstJab - freeRangesFirstJab.JabLength + 1, tFirstJab + freeRangesFirstJab.JabLength - 1, firstMachineNr);
            freeRangesSecondJab.RemoveRange(tSecondJab - freeRangesSecondJab.JabLength + 1, tSecondJab + freeRangesSecondJab.JabLength - 1, secondMachineNr);

            // Remove ranges of the second job
            freeRangesFirstJab.RemoveRange(tSecondJab - freeRangesFirstJab.JabLength + 1, tSecondJab + freeRangesSecondJab.JabLength - 1, secondMachineNr);
            freeRangesSecondJab.RemoveRange(tFirstJab - freeRangesSecondJab.JabLength + 1, tFirstJab + freeRangesFirstJab.JabLength - 1, firstMachineNr);

            //Extensions.WriteDebugLine("Added to both trees!");
            //Extensions.WriteDebugLine(freeRangesFirstJob);
            //Extensions.WriteDebugLine(freeRangesSecondJob);
        }
    }
}
