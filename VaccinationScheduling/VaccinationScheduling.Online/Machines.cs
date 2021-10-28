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
        public RedBlackTree freeRangesFirstJob;

        /// <summary>
        /// RedBlackTree containing the ranges at which there are free spots to START a second job.
        /// </summary>
        public RedBlackTree freeRangesSecondJob;

        public int NrMachines = 0;

        /// <summary>
        /// Create a machine schedule
        /// </summary>
        /// <param name="global">Global parameters of the program</param>
        public Machines(Global global)
        {
            freeRangesFirstJob = new RedBlackTree(global.TimeFirstDose);
            freeRangesSecondJob = new RedBlackTree(global.TimeSecondDose);
        }

        // Finds the first available spot inside the range.
        public (int, int, int, int) FindGreedySpot(Job job)
        {
            IEnumerator<Range> firstJobEnumerate = freeRangesFirstJob.FastEnumerateRange(job.MinFirstIntervalStart, job.MaxFirstIntervalStart).GetEnumerator();
            IEnumerator<Range> secondJobEnumerate = freeRangesSecondJob.FastEnumerateRange(job.MinFirstIntervalStart + job.MinGapIntervalStarts, job.MaxFirstIntervalStart + job.MaxGapIntervalStarts).GetEnumerator();

            List<(Range, int)> secondRanges = new();
            int slidingWindowIndex = 0;

            int bestScore = 0;
            int minFirstJobScore = 0;
            (int, int, int) bestFirstJobScore = (0, NrMachines, job.MinFirstIntervalStart);
            (int, int, int) bestSecondJobScore = (0, NrMachines, job.MinFirstIntervalStart + job.MinGapIntervalStarts);

            // Whilst there are still possible places left
            while (firstJobEnumerate.MoveNext())
            {
                // Found the best answer possible
                if (bestScore == 2)
                {
                    break;
                }

                bool expandCache = true;
                Range firstJob = firstJobEnumerate.Current;

                // Score is 0 if it does not fit, otherwise 1
                int firstJobScore = firstJob.NotList.Count == NrMachines ? 0 : 1;
                if (firstJobScore < minFirstJobScore)
                {
                    continue;
                }
                // The second job score is
                int minSecondJobScore = bestScore - firstJobScore;
                // Cannot have a second score higher than 1;
                if (minSecondJobScore >= 1)
                {
                    continue;
                }

                // Minimum T at which the first job can be scheduled is the minum of TMinJobStart and TRangeStart
                int minTFirstJob = Math.Max(firstJobEnumerate.Current.Start, job.MinFirstIntervalStart);
                // Maximum T at which the first job can be schedule is the maximum of TMaxJobStart and TRangeStart
                int maxTFirstJob = firstJobEnumerate.Current.EndMaybe == null ? job.MaxFirstIntervalStart : Math.Min(job.MaxFirstIntervalStart, (int)firstJobEnumerate.Current.EndMaybe);

                // The second job needs to be within these values given the first bounds.
                int minTSecondJob = minTFirstJob + job.MinGapIntervalStarts;
                int maxTSecondJob = maxTFirstJob + job.MaxGapIntervalStarts;

                // Go through current 'cache'
                for (int i = slidingWindowIndex; i < secondRanges.Count; i++)
                {
                    // The first value is too high for the current last item
                    // We slide the window further so we don't go past it in future anymore.
                    if (minTSecondJob < secondRanges[i].Item1.Start)
                    {
                        slidingWindowIndex++;
                        continue;
                    }
                    // Score is lower than the minimum score
                    if (secondRanges[i].Item2 < minSecondJobScore)
                    {
                        continue;
                    }
                    // The current item is out of range
                    if (secondJobEnumerate.Current.Start > maxTSecondJob)
                    {
                        expandCache = false;
                        break;
                    }

                    (int, int) overlap = getOverlapWithRange(secondJobEnumerate.Current, minTSecondJob, maxTSecondJob);
                    (int, int) scheduledTimes = getScheduleTimes(job, ref minTFirstJob, ref maxTFirstJob, ref overlap);

                    int firstJobMachine = firstJobEnumerate.Current.NotList.FindFirstNotContained();
                    int secondJobMachine = secondJobEnumerate.Current.NotList.FindFirstNotContained();
                    bestFirstJobScore = (firstJobScore, firstJobMachine, scheduledTimes.Item1);
                    bestSecondJobScore = (secondRanges[i].Item2, secondJobMachine, scheduledTimes.Item2);
                }

                // We do not want to expand the list since the last item is outside the range
                if (!expandCache)
                {
                    continue;
                }

                // Add new items to the list since we can expand the second
                while (secondJobEnumerate.MoveNext())
                {
                    int secondJobScore = secondJobEnumerate.Current.NotList.Count == NrMachines ? 0 : 1;
                    // Score is lower than the minimum score
                    if (secondJobScore < minSecondJobScore)
                    {
                        continue;
                    }

                    secondRanges.Add((secondJobEnumerate.Current, secondJobScore));

                    // The current item is too large
                    if (secondJobEnumerate.Current.Start > maxTSecondJob)
                        break;

                    (int, int) overlap = getOverlapWithRange(secondJobEnumerate.Current, minTSecondJob, maxTSecondJob);
                    (int, int) scheduledTimes = getScheduleTimes(job, ref minTFirstJob, ref maxTFirstJob, ref overlap);

                    int firstJobMachine = firstJobEnumerate.Current.NotList.FindFirstNotContained();
                    int secondJobMachine = secondJobEnumerate.Current.NotList.FindFirstNotContained();
                    bestFirstJobScore = (firstJobScore, firstJobMachine, scheduledTimes.Item1);
                    bestSecondJobScore = (secondJobScore, secondJobMachine, scheduledTimes.Item2);
                    bestScore = firstJobScore + secondJobScore;
                }
            }

            return (bestFirstJobScore.Item2, bestSecondJobScore.Item2, bestFirstJobScore.Item3, bestSecondJobScore.Item3);
        }

        private List<Range> EnumerateToList(int tMin, int tMax, bool firstJab)
        {
            List<Range> firstJobs = new List<Range>();
            RedBlackTree tree = firstJab ? freeRangesFirstJob : freeRangesSecondJob;

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

            Extensions.WriteDebugLine($"Best score {(int)bestFirstJobScore.Item1 + (int)bestSecondJobScore.Item1}");
            Extensions.WriteDebugLine($"#1 Machine: {bestFirstJobScore.Item2} T:{bestFirstJobScore.Item3}  #2 Machine: {bestSecondJobScore.Item2} T:{bestSecondJobScore.Item3}");
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
            int secondJobStart = tSecondInterval.Item1;
            // The first job timing depends on the second job timing
            int firstJobStart = Math.Max(tFirstIntervalStart, secondJobStart - job.MaxGapIntervalStarts);
            return (firstJobStart, secondJobStart);
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
        /// <param name="tFirstJob">The time at which to schedule the first job</param>
        /// <param name="firstJob">The range that the first job gets scheduled on</param>
        /// <param name="tSecondJob">The time at which to schedule the second job</param>
        /// <param name="secondJob">The range that the first job gets scheduled on</param>
        public void ScheduleJobs(int firstMachineNr, int secondMachineNr, int tFirstJob, int tSecondJob)
        {
            if (firstMachineNr == NrMachines || secondMachineNr == NrMachines)
            {
                NrMachines++;
            }

            // Remove ranges of each of the jobs
            freeRangesFirstJob.RemoveRange(tFirstJob - freeRangesFirstJob.JobLength + 1, tFirstJob + freeRangesFirstJob.JobLength - 1, firstMachineNr);
            freeRangesSecondJob.RemoveRange(tSecondJob - freeRangesSecondJob.JobLength + 1, tSecondJob + freeRangesSecondJob.JobLength - 1, secondMachineNr);

            // Remove ranges of the second job
            freeRangesFirstJob.RemoveRange(tSecondJob - freeRangesFirstJob.JobLength + 1, tSecondJob + freeRangesSecondJob.JobLength - 1, secondMachineNr);
            freeRangesSecondJob.RemoveRange(tFirstJob - freeRangesSecondJob.JobLength + 1, tFirstJob + freeRangesFirstJob.JobLength - 1, firstMachineNr);

            //Extensions.WriteDebugLine("Added to both trees!");
            //Extensions.WriteDebugLine(freeRangesFirstJob);
            //Extensions.WriteDebugLine(freeRangesSecondJob);
        }
    }
}
