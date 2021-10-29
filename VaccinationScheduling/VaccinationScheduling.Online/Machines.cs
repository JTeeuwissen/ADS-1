using System;
using System.Collections.Generic;
using System.Numerics;
using VaccinationScheduling.Online.Tree;
using VaccinationScheduling.Shared.BigNumbers;
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

        // Current number of machines used for the solution
        public int NrMachines = 0;

        /// <summary>
        /// Create a machine schedule
        /// </summary>
        /// <param name="global">Global parameters of the program</param>
        public Machines(BigGlobal global)
        {
            freeRangesFirstJab = new RedBlackTree(global.TimeFirstDose);
            freeRangesSecondJab = new RedBlackTree(global.TimeSecondDose);
        }

        /// <summary>
        /// Finds the first available spot inside the range for which the most amount of jabs possible are scheduled on an existing machine.
        /// It starts with the worst solution, scheduling both jobs as soon as possible on new machines, tries to then improve that solution.
        /// </summary>
        /// <param name="job">Job to find a spot for</param>
        /// <returns>(machinNr1, machineNr2, tFirstJab, tSecondJab) of the found spot</returns>
        public (int, int, BigInteger, BigInteger) FindGreedySpots(BigJob job)
        {
            // We use the both enumerators and go through them like the sliding window algorithm. Keeping track of what we have gone through for the second enumerator.
            IEnumerator<Range> firstJabEnumerate = freeRangesFirstJab.FastEnumerateRangeInOrder(job.MinFirstIntervalStart, job.MaxFirstIntervalStart).GetEnumerator();
            IEnumerator<Range> secondJabEnumerate = freeRangesSecondJab.FastEnumerateRangeInOrder(job.MinFirstIntervalStart + job.MinGapIntervalStarts, job.MaxFirstIntervalStart + job.MaxGapIntervalStarts).GetEnumerator();

            // Save score and ranges enumerated for the second jabs, so we can go through them again without creating a new enumerator.
            List<(Range, int)> secondRanges = new();
            int slidingWindowIndex = 0;

            int bestScore = 0;
            // The initial best solution is to schedule on new machines as early as possible.
            (int, int, BigInteger) bestFirstJabScore = (0, NrMachines, job.MinFirstIntervalStart);
            (int, int, BigInteger) bestSecondJabScore = (0, NrMachines, job.MinFirstIntervalStart + job.MinGapIntervalStarts);

            // Whilst there are still possible places left
            while (firstJabEnumerate.MoveNext())
            {
                // Found the best answer possible
                if (bestScore == 2)
                    break;

                bool expandCache = true;
                Range firstJab = firstJabEnumerate.Current;

                // Score is 0 if it does not fit, otherwise 1
                int firstJabScore = firstJab.OccupiedMachineNrs.Count == NrMachines ? 0 : 1;
                // The second job score is
                int minSecondJabScore = bestScore - firstJabScore;
                // Cannot have a second score higher than 1;
                if (minSecondJabScore >= 1)
                    continue;

                // Minimum T at which the first job can be scheduled is the minum of TMinJobStart and TRangeStart
                BigInteger minTFirstJab = BigInteger.Max(firstJabEnumerate.Current.Start, job.MinFirstIntervalStart);
                // Maximum T at which the first job can be schedule is the maximum of TMaxJobStart and TRangeStart
                BigInteger maxTFirstJab = firstJabEnumerate.Current.EndMaybe == null ? job.MaxFirstIntervalStart : BigInteger.Min(job.MaxFirstIntervalStart, (BigInteger)firstJabEnumerate.Current.EndMaybe);

                // The second job needs to be within these values given the first bounds.
                BigInteger minTSecondJab = minTFirstJab + job.MinGapIntervalStarts;
                BigInteger maxTSecondJab = maxTFirstJab + job.MaxGapIntervalStarts;

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

                    // Better timeslot is found. Improve the solution
                    (BigInteger, BigInteger) overlap = secondJabEnumerate.Current.GetOverlap(minTSecondJab, maxTSecondJab);
                    BigInteger tSecondJab = overlap.Item1;
                    BigInteger tFirstJab = BigInteger.Max(minTFirstJab, tSecondJab - job.MaxGapIntervalStarts);

                    int firstJabMachine = firstJabEnumerate.Current.OccupiedMachineNrs.FirstItemNotContained;
                    int secondJabMachine = secondJabEnumerate.Current.OccupiedMachineNrs.FirstItemNotContained;
                    bestFirstJabScore = (firstJabScore, firstJabMachine, tFirstJab);
                    bestSecondJabScore = (secondRanges[i].Item2, secondJabMachine, tSecondJab);
                    bestScore = firstJabScore + secondRanges[i].Item2;
                }

                // We do not want to expand the list since the last item is outside the range
                if (!expandCache)
                    continue;

                // Add new items to the list since we can expand the second
                while (secondJabEnumerate.MoveNext())
                {
                    int secondJabScore = secondJabEnumerate.Current.OccupiedMachineNrs.Count == NrMachines ? 0 : 1;
                    secondRanges.Add((secondJabEnumerate.Current, secondJabScore));
                    // Score is lower than the minimum score
                    if (secondJabScore < minSecondJabScore)
                    {
                        continue;
                    }

                    // The current item is too late for the current range of the first jab.
                    if (secondJabEnumerate.Current.Start > maxTSecondJab)
                        break;

                    // A better solution was found. Get overlap and schedule both jabs.
                    (BigInteger, BigInteger) overlap = secondJabEnumerate.Current.GetOverlap(minTSecondJab, maxTSecondJab);
                    BigInteger tSecondJab = overlap.Item1;
                    BigInteger tFirstJab = BigInteger.Max(minTFirstJab, tSecondJab - job.MaxGapIntervalStarts);

                    // Find the first machine numbers that are free at the given time.
                    int firstJabMachine = firstJabEnumerate.Current.OccupiedMachineNrs.FirstItemNotContained;
                    int secondJabMachine = secondJabEnumerate.Current.OccupiedMachineNrs.FirstItemNotContained;

                    // Update optimal solution with the new best.
                    bestFirstJabScore = (firstJabScore, firstJabMachine, tFirstJab);
                    bestSecondJabScore = (secondJabScore, secondJabMachine, tSecondJab);
                    bestScore = firstJabScore + secondJabScore;
                }
            }

            // Return the optimal solution found.
            return (bestFirstJabScore.Item2, bestSecondJabScore.Item2, bestFirstJabScore.Item3, bestSecondJabScore.Item3);
        }

        /// <summary>
        /// Enumerate the tree within the given range and put it into a list
        /// </summary>
        /// <param name="leftBound">Leftbound of the range</param>
        /// <param name="RightBound">Rightbound of the range</param>
        /// <param name="isFirstJab">Whether it is the first or second jab schedule that needs to get enumerated</param>
        /// <returns>A list containing the range items within the given bounds.</returns>
        private List<Range> EnumerateToList(BigInteger leftBound, BigInteger RightBound, bool isFirstJab)
        {
            List<Range> firstJobs = new List<Range>();
            // Choose the correct tree.
            RedBlackTree tree = isFirstJab ? freeRangesFirstJab : freeRangesSecondJab;

            // FIll the list
            foreach (Range range in tree.FastEnumerateRangeInOrder(leftBound, RightBound))
            {
                firstJobs.Add(range);
            }

            return firstJobs;
        }

        /// <summary>
        /// Choose time slots for the jabs preferring scheduling next to another jab.
        /// </summary>
        /// <param name="job">Object containing the job parameters</param>
        /// <returns>(machinNr1, machineNr2, tFirstJab, tSecondJab) of the best found spot</returns>
        public (int, int, BigInteger, BigInteger) FindStickyGreedySpot(BigJob job)
        {
            List<Range> firstJobRanges = EnumerateToList(job.MinFirstIntervalStart, job.MaxFirstIntervalStart, true);
            List<Range> secondJobRanges = EnumerateToList(job.MinFirstIntervalStart + job.MinGapIntervalStarts, job.MaxFirstIntervalStart + job.MaxGapIntervalStarts, false);

            int slidingWindowIndex = 0;

            int bestScore = (int)Score.NEWMACHINE + (int)Score.NEWMACHINE;
            (Score, int, BigInteger) bestFirstJabScore = (Score.NEWMACHINE, NrMachines, job.MinFirstIntervalStart);
            (Score, int, BigInteger) bestSecondJabScore = (Score.NEWMACHINE, NrMachines, job.MinFirstIntervalStart + job.MinGapIntervalStarts);

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
                (Score, Score) firstJabScore = getScores(firstRangeOverlap, firstJob.OccupiedMachineNrs.Count, firstJob.MachineNrInBothNeighbours);
                // Cannot have a second score higher than 1;
                int minSecondJobScore = bestScore - (int)firstJabScore.Item1;
                if (minSecondJobScore > (int)Score.FLUSH)
                {
                    continue;
                }

                // The second job needs to be within these values given the first bounds.
                BigInteger minTSecondJob = firstRangeOverlap.OverlapStart + job.MinGapIntervalStarts;
                BigInteger maxTSecondJob = firstRangeOverlap.OverlapEnd + job.MaxGapIntervalStarts;

                for (int j = slidingWindowIndex; j < secondJobRanges.Count; j++)
                {
                    Range secondJab = secondJobRanges[j];
                    // The first value is too high for the current last item
                    // We slide the window further so we don't go past it in future anymore.
                    if (secondJab.EndMaybe != null)
                    {
                        if (minTSecondJob > secondJab.EndMaybe)
                        {
                            slidingWindowIndex++;
                            continue;
                        }
                    }
                    // Out of range for the second item
                    if (secondJab.Start > maxTSecondJob)
                    {
                        break;
                    }

                    Overlap secondRangeOverlap = new(secondJab, minTSecondJob, maxTSecondJob);
                    (Score, Score) secondJabScores = getScores(secondRangeOverlap, secondJab.OccupiedMachineNrs.Count, secondJab.MachineNrInBothNeighbours);
                    // Score cannot be higher than the minimum score
                    if ((int)secondJabScores.Item2 <= minSecondJobScore)
                    {
                        continue;
                    }
                    (Score, Score, BigInteger, BigInteger) scheduledTimes = getSmartScheduleTimes(job, firstJabScore, secondJabScores, firstRangeOverlap, secondRangeOverlap);
                    // Score of the solution is not better than the best solution.
                    if ((int)scheduledTimes.Item1 + (int)scheduledTimes.Item2 <= bestScore)
                    {
                        continue;
                    }
                    int firstMachineNr = getMachineNr(scheduledTimes.Item3, firstJob);
                    int secondMachineNr = getMachineNr(scheduledTimes.Item4, secondJab);

                    bestFirstJabScore = (scheduledTimes.Item1, firstMachineNr, scheduledTimes.Item3);
                    bestSecondJabScore = (scheduledTimes.Item2, secondMachineNr, scheduledTimes.Item4);
                    bestScore = (int)scheduledTimes.Item1 + (int)scheduledTimes.Item2;
                }
            }

            return (bestFirstJabScore.Item2, bestSecondJabScore.Item2, bestFirstJabScore.Item3, bestSecondJabScore.Item3);
        }

        /// <summary>
        /// Gets the possible score minimum and maximum given a overlap and the number of machines that are occupied during the range
        /// </summary>
        /// <param name="overlap">Overlap between the possible jab times and the range</param>
        /// <param name="nrMachinesOccupied">The amount of machines that are occupied at that moment</param>
        /// <param name="hasNeighboursFlush">Int representing the machinenr that is flush</param>
        /// <returns>The score range at which the the job can be scheduled</returns>
        private (Score, Score) getScores(Overlap overlap, int nrMachinesOccupied, int? hasNeighboursFlush)
        {
            if (hasNeighboursFlush != null)
            {
                return (Score.FLUSH, Score.FLUSH);
            }
            if (nrMachinesOccupied == NrMachines)
            {
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

        private (Score, Score, BigInteger, BigInteger) getSmartScheduleTimes(BigJob job, (Score, Score) firstJabScores, (Score, Score) secondJabScores, Overlap firstJobOverlap, Overlap secondJobOverlap)
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
                    if (firstJobOverlap.EndOverlaps && firstJobOverlap.OverlapEnd + job.MinGapIntervalStarts <= secondJobOverlap.OverlapStart)
                    {
                        return (firstJabScores.Item2, secondJabScores.Item2, firstJobOverlap.OverlapEnd, secondJobOverlap.OverlapStart);
                    }
                    else if (firstJobOverlap.StartOverlaps && firstJobOverlap.OverlapStart + job.MaxGapIntervalStarts >= secondJobOverlap.OverlapStart)
                    {
                        return (firstJabScores.Item2, secondJabScores.Item2, firstJobOverlap.OverlapStart, secondJobOverlap.OverlapStart);
                    }
                    return (firstJabScores.Item1, secondJabScores.Item2, BigInteger.Max(firstJobOverlap.OverlapStart, secondJobOverlap.OverlapStart - job.MaxGapIntervalStarts), secondJobOverlap.OverlapStart);
            }
            // Try schedule it where two of them neighbouring
            if (firstJobOverlap.StartOverlaps)
            {
                if (secondJobOverlap.StartOverlaps && firstJobOverlap.OverlapStart + job.MinGapIntervalStarts <= secondJobOverlap.OverlapStart && firstJobOverlap.OverlapStart + job.MaxGapIntervalStarts >= secondJobOverlap.OverlapStart)
                {
                    return (firstJabScores.Item2, secondJabScores.Item2, firstJobOverlap.OverlapStart, secondJobOverlap.OverlapStart);
                }
                else if (secondJobOverlap.EndOverlaps && firstJobOverlap.OverlapStart + job.MaxGapIntervalStarts >= secondJobOverlap.OverlapEnd)
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
            if (firstJobOverlap.StartOverlaps && firstJobOverlap.OverlapStart + job.MaxGapIntervalStarts >= secondJobOverlap.OverlapStart)
            {
                return (firstJabScores.Item2, secondJabScores.Item1, firstJobOverlap.OverlapStart, BigInteger.Max(secondJobOverlap.OverlapStart, firstJobOverlap.OverlapStart + job.MinGapIntervalStarts));
            }
            else if (firstJobOverlap.EndOverlaps && firstJobOverlap.OverlapEnd + job.MinGapIntervalStarts <= secondJobOverlap.OverlapEnd)
            {
                return (firstJabScores.Item2, secondJabScores.Item1, firstJobOverlap.OverlapEnd, BigInteger.Max(secondJobOverlap.OverlapStart, firstJobOverlap.OverlapEnd + job.MinGapIntervalStarts));
            }
            else if (secondJobOverlap.StartOverlaps && secondJobOverlap.OverlapStart - job.MinGapIntervalStarts >= firstJobOverlap.OverlapStart)
            {
                return (firstJabScores.Item1, secondJabScores.Item2, BigInteger.Min(firstJobOverlap.OverlapEnd, secondJobOverlap.OverlapStart - job.MinGapIntervalStarts), secondJobOverlap.OverlapStart);
            }
            else if (secondJobOverlap.EndOverlaps)
            {
                return (firstJabScores.Item1, secondJabScores.Item2, BigInteger.Min(firstJobOverlap.OverlapEnd, secondJobOverlap.OverlapEnd - job.MinGapIntervalStarts), secondJobOverlap.OverlapEnd);
            }
            else
            {
                return (firstJabScores.Item1, secondJabScores.Item1, firstJobOverlap.OverlapStart, BigInteger.Max(secondJobOverlap.OverlapStart, firstJobOverlap.OverlapStart + job.MinGapIntervalStarts));
            }
        }

        /// <summary>
        /// Gets the machine number given the scheduled time for the jab.
        /// </summary>
        /// <param name="tScheduled">Time at which the jab is scheduled</param>
        /// <param name="range">Range the jab will be scheduled at</param>
        /// <returns>The machineNr the jab will be scheduled on.</returns>
        private int getMachineNr(BigInteger tScheduled, Range range)
        {
            // Sits flush inbetween two jabs on the current machine
            if (range.MachineNrInBothNeighbours != null)
            {
                return (int)range.MachineNrInBothNeighbours;
            }
            // Sits against a jab left to it
            else if (tScheduled == range.Start && range.InLeftItem != null)
            {
                return (int)range.InLeftItem;
            }
            // Sits against a jab to the right
            else if (tScheduled == range.EndMaybe && range.InRightItem != null)
            {
                return (int)range.InRightItem;
            }
            // Does not border another jab
            else
            {
                return range.OccupiedMachineNrs.FirstItemNotContained;
            }
        }

        /// <summary>
        /// Schedule two jobs given both ranges to insert the job on
        /// </summary>
        /// <param name="firstMachineNr">On which machine the first jab is scheduled</param>
        /// <param name="secondMachineNr">On which machine the second jab is scheduled</param>
        /// <param name="tFirstJab">The time at which to schedule the first job</param>
        /// <param name="tSecondJab">The time at which to schedule the second job</param>
        /// <param name="runningStickyAlgorithm">The </param>
        public void ScheduleJobs(int firstMachineNr, int secondMachineNr, BigInteger tFirstJab, BigInteger tSecondJab, bool runningStickyAlgorithm)
        {
            if (firstMachineNr == NrMachines || secondMachineNr == NrMachines) NrMachines++;

            // Remove ranges of each main schedule
            freeRangesFirstJab.MarkRangeOccupied(tFirstJab - freeRangesFirstJab.JabLength + 1, tFirstJab + freeRangesFirstJab.JabLength - 1, firstMachineNr, runningStickyAlgorithm);
            freeRangesSecondJab.MarkRangeOccupied(tSecondJab - freeRangesSecondJab.JabLength + 1, tSecondJab + freeRangesSecondJab.JabLength - 1, secondMachineNr, runningStickyAlgorithm);

            // Also adapt the schedule of the opposite jab
            freeRangesFirstJab.MarkRangeOccupied(tSecondJab - freeRangesFirstJab.JabLength + 1, tSecondJab + freeRangesSecondJab.JabLength - 1, secondMachineNr, runningStickyAlgorithm);
            freeRangesSecondJab.MarkRangeOccupied(tFirstJab - freeRangesSecondJab.JabLength + 1, tFirstJab + freeRangesFirstJab.JabLength - 1, firstMachineNr, runningStickyAlgorithm);
        }
    }
}
