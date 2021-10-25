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
            IEnumerator<Range> firstJabEnumerate = freeRangesFirstJab.EnumerateRange(job.MinFirstIntervalStart, job.MaxFirstIntervalStart).GetEnumerator();
            IEnumerator<Range> secondJabEnumerate = freeRangesSecondJab.EnumerateRange(job.MinFirstIntervalStart + job.MinGapIntervalStarts, job.MaxFirstIntervalStart + job.MaxGapIntervalStarts).GetEnumerator();

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
                // Tree 0-3 4-7(Not 1) 8-INF
                // Job 3-5 mingap 4 maxgap 6
                // 3 en 8
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

                // Tree 0-3 4-7(Not 1) 8-INF
                // Job 3-5 mingap 4 maxgap 6
                // 3 en 8
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

            // Extensions.WriteDebugLine($"Best score {bestFirstJobScore.Item1 + bestSecondJobScore.Item1}");
            // Extensions.WriteDebugLine($"#1 Machine: {bestFirstJobScore.Item2} T:{bestFirstJobScore.Item3}  #2 Machine: {bestSecondJobScore.Item2} T:{bestSecondJobScore.Item3}");
            return (bestFirstJabScore.Item2, bestSecondJabScore.Item2, bestFirstJabScore.Item3, bestSecondJabScore.Item3);
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
