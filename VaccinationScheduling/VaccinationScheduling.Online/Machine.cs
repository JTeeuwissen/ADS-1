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
        public (int, int) FindGreedySpot(Job job)
        {
            IEnumerator<Range> firstJobEnumerate = freeRangesFirstJob.EnumerateRange(job.MinFirstIntervalStart, job.MaxFirstIntervalStart).GetEnumerator();
            IEnumerator<Range> secondJobEnumerate = freeRangesSecondJob.EnumerateRange(job.MinFirstIntervalStart + job.MinGapIntervalStarts, job.MaxFirstIntervalStart + job.MaxGapIntervalStarts).GetEnumerator();

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
                // Tree 0-3 4-7(Not 1) 8-INF
                // Job 3-5 mingap 4 maxgap 6
                // 3 en 8
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

                // Tree 0-3 4-7(Not 1) 8-INF
                // Job 3-5 mingap 4 maxgap 6
                // 3 en 8
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

            Extensions.WriteDebugLine($"Best score {bestFirstJobScore.Item1 + bestSecondJobScore.Item1}");
            Extensions.WriteDebugLine($"#1 Machine: {bestFirstJobScore.Item2} T:{bestFirstJobScore.Item3}");
            Extensions.WriteDebugLine($"#2 Machine: {bestSecondJobScore.Item2} T:{bestSecondJobScore.Item3}");
            return (bestFirstJobScore.Item2, bestSecondJobScore.Item2);
        }

        private (int, int) getOverlapWithRange(Range range, int tStart, int tEnd)
        {
            return (Math.Max(range.Start, tStart), range.EndMaybe is {} end ? Math.Min(end, tEnd) : tEnd);
        }

        public (int, int) getScheduleTimes(Job job, ref int tFirstIntervalStart, ref int tFirstIntervalEnd, ref (int, int) tSecondInterval)
        {
            // We start the second job as soon as possible
            int secondJobStart = tSecondInterval.Item1;
            // The first job timing depends on the second job timing
            int firstJobStart = Math.Max(tFirstIntervalStart, secondJobStart - job.MaxGapIntervalStarts);
            return (firstJobStart, secondJobStart);
        }

        /// <summary>
        /// Schedule two jobs containing given timestamps
        /// </summary>
        /// <param name="tFirstJob">Time at which to schedule the first job</param>
        /// <param name="tSecondJob">Time at which to schedule the second job</param>
        public void ScheduleJobs(int tFirstJob, int tSecondJob)
        {
            // The two jobs cannot overlap
            //Debug.Assert(tFirstJob + freeRangesFirstJob.JobLength <= tSecondJob);

            // Find the ranges at which the jobs are scheduled
            freeRangesFirstJob.Find(tFirstJob, out Range firstJob);
            freeRangesSecondJob.Find(tSecondJob, out Range secondJob);

            ScheduleJobs(tFirstJob, firstJob, tSecondJob, secondJob);
        }

        /// <summary>
        /// Schedule two jobs given both ranges to insert the job on
        /// </summary>
        /// <param name="tFirstJob">The time at which to schedule the first job</param>
        /// <param name="firstJob">The range that the first job gets scheduled on</param>
        /// <param name="tSecondJob">The time at which to schedule the second job</param>
        /// <param name="secondJob">The range that the first job gets scheduled on</param>
        private void ScheduleJobs(int tFirstJob, Range firstJob, int tSecondJob, Range secondJob)
        {
            // Schedule the two jobs
            ScheduleJob(freeRangesFirstJob, firstJob, tFirstJob, freeRangesFirstJob.JobLength);
            ScheduleJob(freeRangesSecondJob, secondJob, tSecondJob, freeRangesSecondJob.JobLength);

            // Update the opposite tree to keep the newly scheduled jobs into account
            ScheduleJob(freeRangesFirstJob, tSecondJob, freeRangesSecondJob.JobLength);
            ScheduleJob(freeRangesSecondJob, tFirstJob, freeRangesFirstJob.JobLength);

            Extensions.WriteDebugLine("Added to both trees!");
            Extensions.WriteDebugLine(freeRangesFirstJob);
            Extensions.WriteDebugLine(freeRangesSecondJob);
        }

        /// <summary>
        /// First find the range to insert on before calling the below method
        /// </summary>
        /// <param name="tree">Tree to insert the job on</param>
        /// <param name="tJob">The time at which the job takes place</param>
        /// <param name="jobLength">Length of the job</param>
        public void ScheduleJob(RedBlackTree tree, int tJob, int jobLength)
        {
            Range job;
            tree.FindOrPrevious(tJob, out job);
            ScheduleJob(tree, job, tJob, jobLength);
        }

        /// <summary>
        /// Schedule a job in the given tree in the range
        /// Can only affect the range on which the job
        /// </summary>
        /// <param name="tree">Tree to insert the job on</param>
        /// <param name="foundRange">Range that needs to get adapted when scheduling this job</param>
        /// <param name="tJob">The time at which the job takes place</param>
        /// <param name="jobLength">Length of the job</param>
        public static void ScheduleJob(RedBlackTree tree, Range foundRange, int tJob, int jobLength)
        {
            // When the second value is null. It represents 'infinite'
            bool infiniteRange = foundRange.EndMaybe == null;

            // Job adjusts the start of the range
            if (foundRange.Start + tree.JobLength > tJob)
            {
                // The new range would be negative, so delete it instead
                if (foundRange.EndMaybe < tJob + jobLength && !infiniteRange)
                {
                    bool res = tree.Delete(foundRange, false, out foundRange);
                    Extensions.WriteDebugLine("Deleted item");
                    //Debug.Assert(res);
                }
                else
                {
                    // Define the new start of the range
                    foundRange.Start = Math.Max(tJob + jobLength, foundRange.Start);
                }
            }
            // Only need to adapt the end of the range
            else if (foundRange.EndMaybe - jobLength < tJob && !infiniteRange)
            {
                // The new range would be negative, so delete it instead
                if (tJob - tree.JobLength < foundRange.Start)
                {
                    bool res = tree.Delete(foundRange, false, out foundRange);
                    //Debug.Assert(res);
                    Extensions.WriteDebugLine("Deleted item");
                }
                else
                {
                    // Define new end of the range
                    foundRange.EndMaybe = foundRange.EndMaybe == null ? null : Math.Min(tJob - tree.JobLength, (int)foundRange.EndMaybe);
                }
            }
            // It hovers somewhere in the middle, so we need to insert another item
            else
            {
                int? newRangeEnd = foundRange.EndMaybe;
                foundRange.EndMaybe = tJob - tree.JobLength;
                tree.Insert(new Range(tJob + jobLength, newRangeEnd));
            }
        }
    }
}
