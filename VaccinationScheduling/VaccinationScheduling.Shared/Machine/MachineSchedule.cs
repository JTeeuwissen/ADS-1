using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace VaccinationScheduling.Shared.Machine
{
    public class MachineSchedule
    {
        /// <summary>
        /// RedBlackTree containing the ranges at which there are free spots to START a first job.
        /// </summary>
        public RedBlackTree freeRangesFirstJob;

        /// <summary>
        /// RedBlackTree containing the ranges at which there are free spots to START a second job.
        /// </summary>
        public RedBlackTree freeRangesSecondJob;

        /// <summary>
        /// Create a machine schedule
        /// </summary>
        /// <param name="global">Global parameters of the program</param>
        public MachineSchedule(Global global)
        {
            freeRangesFirstJob = new RedBlackTree(global.TFirstDose);
            freeRangesSecondJob = new RedBlackTree(global.TSecondDose);
        }

        // Finds the first available spot inside the range.
        public (int, int) FindGreedySpot(Job job)
        {
            IEnumerable<Range> firstJobEnumerable = freeRangesFirstJob
                .EnumerateRange(job.MinFirstIntervalStart, job.MaxFirstIntervalStart));
            IEnumerator<Range> secondJobEnumerate = freeRangesSecondJob.EnumerateRange(
                job.MinFirstIntervalStart + job.MinGapIntervalStarts,
                job.MaxFirstIntervalStart + job.MaxGapIntervalStarts
            ).GetEnumerator();

            List<Range> secondRanges = new();
            int slidingWindowIndex = 0;

            // Whilst there are still possible places left
            foreach (Range firstJob in firstJobEnumerable)
            {
                int minTSecondJob = Math.Max(firstJob.Start, job.MinFirstIntervalStart) +
                                    job.MinGapIntervalStarts;
                int maxTSecondJob = Math.Min(
                    firstJob.End,
                    job.MaxFirstIntervalStart + job.MaxGapIntervalStarts
                );

                // Go through current
                for (int i = slidingWindowIndex; i < secondRanges.Count; i++)
                {
                    // This value cannot be inside the range anymore.
                    if (minTSecondJob < secondRanges[i].Start)
                    {
                        slidingWindowIndex++;
                        continue;
                    }

                    (int, int) overlap = secondRanges[i].GetOverlap(minTSecondJob, maxTSecondJob);
                    // There is no overlap
                    if (overlap == (-1, -1))
                    {
                        continue;
                    }

                    // The job can never start before the first job
                    int tFirstJob = Math.Max(
                        Math.Max(firstJob.Start, job.MinFirstIntervalStart),
                        overlap.Item1 - job.MaxGapIntervalStarts
                    );
                    int tSecondJob = Math.Max(tFirstJob + job.MinGapIntervalStarts, overlap.Item1);

                    Console.WriteLine("---------------------------------");
                    Console.WriteLine($"First Job: {tFirstJob}, Second Job: {tSecondJob}");
                    Console.WriteLine(freeRangesFirstJob);
                    Console.WriteLine(freeRangesSecondJob);

                    // Make sure the job is within the constraints
                    Debug.Assert(job.MinFirstIntervalStart <= tFirstJob && tFirstJob <= job.MaxFirstIntervalStart);
                    Debug.Assert(tFirstJob + job.MinGapIntervalStarts <= tSecondJob);
                    Debug.Assert(tFirstJob + job.MaxGapIntervalStarts >= tSecondJob);

                    return (tFirstJob, tSecondJob);
                }
                
                // Add new items to the list since we can expand the second
                while (secondJobEnumerate.MoveNext())
                {
                    Range secondJob = secondJobEnumerate.Current;

                    secondRanges.Add(secondJob);
                    if (secondJob.Start > maxTSecondJob)
                        break;

                    (int, int) overlap = secondJob.GetOverlap(minTSecondJob, maxTSecondJob);

                    // There is no overlap
                    if (overlap == (-1, -1))
                        continue;

                    // The job can never start before the first job
                    int tFirstJob = Math.Max(
                        Math.Max(firstJob.Start, job.MinFirstIntervalStart),
                        overlap.Item1 - job.MaxGapIntervalStarts
                    );
                    int tSecondJob = Math.Max(tFirstJob + job.MinGapIntervalStarts, overlap.Item1);

                    Console.WriteLine("---------------------------------");
                    Console.WriteLine($"First Job: {tFirstJob}, Second Job: {tSecondJob}");
                    Console.WriteLine(freeRangesFirstJob);
                    Console.WriteLine(freeRangesSecondJob);

                    // Make sure the job is within the constraints
                    Debug.Assert(job.MinFirstIntervalStart <= tFirstJob && tFirstJob <= job.MaxFirstIntervalStart);
                    Debug.Assert(tFirstJob + job.MinGapIntervalStarts <= tSecondJob);
                    Debug.Assert(tFirstJob + job.MaxGapIntervalStarts >= tSecondJob);

                    return (tFirstJob, tSecondJob);
                }
            }

            return (-1, -1);
        }

        /// <summary>
        /// Schedule two jobs containing given timestamps
        /// </summary>
        /// <param name="tFirstJob">Time at which to schedule the first job</param>
        /// <param name="tSecondJob">Time at which to schedule the second job</param>
        public void ScheduleJobs(int tFirstJob, int tSecondJob)
        {
            // The two jobs cannot overlap
            Debug.Assert(tFirstJob + freeRangesFirstJob.JobLength <= tSecondJob);

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
            string tree1 = freeRangesFirstJob.ToString();
            string tree2 = freeRangesSecondJob.ToString();

            // Schedule the two jobs
            ScheduleJob(freeRangesFirstJob, firstJob, tFirstJob, freeRangesFirstJob.JobLength);
            ScheduleJob(freeRangesSecondJob, secondJob, tSecondJob, freeRangesSecondJob.JobLength);

            // Both should not be found anymore in the tree
            freeRangesFirstJob.Find(tFirstJob, out Range temp1);
            Debug.Assert(temp1 == null);
            freeRangesSecondJob.Find(tSecondJob, out Range temp2);
            Debug.Assert(temp2 == null);

            Debug.Assert(tree1 != freeRangesFirstJob.ToString());
            Debug.Assert(tree2 != freeRangesSecondJob.ToString());

            // Update the opposite tree to keep the newly scheduled jobs into account
            ScheduleJob(freeRangesFirstJob, tSecondJob, freeRangesSecondJob.JobLength);
            ScheduleJob(freeRangesSecondJob, tFirstJob, freeRangesFirstJob.JobLength);

            Console.WriteLine("Added to both trees!");
            Console.WriteLine(freeRangesFirstJob);
            Console.WriteLine(freeRangesSecondJob);
        }

        /// <summary>
        /// First find the range to insert on before calling the below method
        /// </summary>
        /// <param name="tree">Tree to insert the job on</param>
        /// <param name="tJob">The time at which the job takes place</param>
        /// <param name="jobLength">Length of the job</param>
        public static void ScheduleJob(RedBlackTree tree, int tJob, int jobLength)
        {
            tree.FindOrPrevious(tJob, out Range job);
            ScheduleJob(tree, job, tJob, jobLength);
        }

        /// <summary>
        /// Schedule a job in the given tree in the range
        /// </summary>
        /// <param name="tree">Tree to insert the job on</param>
        /// <param name="foundRange">Range that needs to get adapted when scheduling this job</param>
        /// <param name="tJob">The time at which the job takes place</param>
        /// <param name="jobLength">Length of the job</param>
        public static void ScheduleJob(RedBlackTree tree, Range foundRange, int tJob, int jobLength)
        {
            // When the second value is -1. It represents 'infinite'
            bool infiniteRange = foundRange.End == -1;

            // Job adjusts the start of the range
            if (foundRange.Start + tree.JobLength > tJob)
            {
                // The new range would be negative, so delete it instead
                if (foundRange.End < tJob + jobLength && !infiniteRange)
                {
                    bool res = tree.Delete(foundRange, false, out foundRange);
                    Console.WriteLine("Deleted item");
                    Debug.Assert(res);
                }
                else
                {
                    // Define the new start of the range
                    foundRange.Start = Math.Max(tJob + jobLength, foundRange.Start);
                }
            }
            // Only need to adapt the end of the range
            else if (foundRange.End - jobLength < tJob && !infiniteRange)
            {
                // The new range would be negative, so delete it instead
                if (tJob - tree.JobLength < foundRange.Start)
                {
                    bool res = tree.Delete(foundRange, false, out foundRange);
                    Debug.Assert(res);
                    Console.WriteLine("Deleted item");
                }
                else
                {
                    // Define new end of the range
                    foundRange.End = Math.Min(tJob - tree.JobLength, foundRange.End);
                }
            }
            else
            {
                // It hovers somewhere in the middle, so we need to insert another item
                int newRangeEnd = foundRange.End;
                foundRange.End = tJob - tree.JobLength;
                tree.Insert(new Range(tJob + jobLength, newRangeEnd));
            }

            // We do not have to adapt subsequent items
            if (jobLength <= tree.JobLength)
                return;

            throw new Exception("Not yet implemented to remove trailing range");
        }
    }
}