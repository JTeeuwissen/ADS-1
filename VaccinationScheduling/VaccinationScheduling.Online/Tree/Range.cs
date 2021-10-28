using System;
using System.Collections;
using System.Collections.Generic;
using VaccinationScheduling.Online.List;

namespace VaccinationScheduling.Online.Tree
{
    /// <summary>
    /// Range consisting out of a start annd enttime. When endttime is null it has no maximum and so goes to 'infinity'
    /// </summary>
    public class Range : IComparable<int>, IComparable<Range>
    {
        /// <summary>
        /// Start time, makes sure the starttime cannot be higher than the endtime when set.
        /// </summary>
        public int Start
        {
            get => start;
            set
            {
                if (endMaybe == null)
                {
                    start = value;
                }
                else if (start > endMaybe)
                {
                    throw new ArgumentOutOfRangeException("StartTime cannot be higher than the endTime");
                }
                else
                {
                    start = value;
                }
            }
        }

        /// <summary>
        /// End time, make sure the end time cannot be lower than the starttime.
        /// </summary>
        public int? EndMaybe
        {
            get => endMaybe;
            set
            {
                if (value == null)
                {
                    endMaybe = null;
                }
                else if (value < start)
                {
                    throw new ArgumentOutOfRangeException("EndTime cannot be lower than the startTime");
                }
                else
                {
                    endMaybe = value;
                }
            }
        }

        // Start of the range
        private int start;
        private int? endMaybe;

        // For sticky greedy the neighbours are precomputed.
        public int? MachineNrInBothNeighbours = null;
        public int? InLeftItem = null;
        public int? InRightItem = null;

        // Set containing what machines are unavailable at the current range.
        public CustomSet OccupiedMachineNrs = new CustomSet();

        /// <summary>
        /// Create a range object. The range is where there is no job.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        public Range(int start, int? end)
        {
            this.start = start;
            this.endMaybe = end;
        }

        /// <summary>
        /// Create a range object initialized with a set.
        /// </summary>
        /// <param name="start">Starttime</param>
        /// <param name="end">EndTime of range</param>
        /// <param name="occupiedMachineNrs">Machines that are occupied at the moment</param>
        public Range(int start, int? end, CustomSet occupiedMachineNrs)
        {
            this.start = start;
            this.endMaybe = end;
            OccupiedMachineNrs = occupiedMachineNrs;
        }

        /// <summary>
        /// Gets the overlap between the current range and the range given to the
        /// </summary>
        /// <param name="tStart"></param>
        /// <param name="tEnd"></param>
        /// <returns>Returns the overlap between both items.</returns>
        public (int, int?)? GetOverlap(int tStart, int? tEnd)
        {
            // Check for overlap
            bool overlap = (endMaybe == null || tStart < endMaybe) && Start < tEnd;
            if (!overlap) return null;

            // There is overlapping range
            return (Math.Max(Start, tStart), EndMaybe is {} end ? Math.Min(end, (int)tEnd) : tEnd);
        }

        /// <summary>
        /// Get the overlap between the current range and the start and end.
        /// </summary>
        /// <param name="tStart">Start of the range to get overlap of</param>
        /// <param name="tEnd">End of the range to get overlap of</param>
        /// <returns>The overlap between the two ranges.</returns>
        public (int, int) GetOverlap(int tStart, int tEnd)
        {
            return (Math.Max(Start, tStart), EndMaybe is {} end ? Math.Min(end, tEnd) : tEnd);
        }

        /// <summary>
        /// Implements IComparable interface, makes time slots comparable.
        /// </summary>
        /// <param name="other">other timeslot object to compare to</param>
        /// <returns>
        /// ret==0 Are equal
        /// ret<0 This one predecends other
        /// ret>0 This one follows other
        /// </returns>
        public int CompareTo(Range other)
        {
            // Compare on properties in this order:
            // (10, 20) >= (0, 1)
            if (other.EndMaybe != null && Start > other.EndMaybe)
            {
                return 1;
            }

            // (0, 1) <= (10, 20)
            if (EndMaybe != null && other.Start > EndMaybe)
            {
                return -1;
            }

            // Are equal
            return 0;
        }

        /// <summary>
        /// See whether an integer is within the range (inclusive)
        /// </summary>
        /// <param name="other"></param>
        /// <returns>
        /// ret==0 Are equal
        /// ret<0 This one predecends other
        /// ret>0 This one follows other
        /// </returns>
        public int CompareTo(int time)
        {
            if (Start > time)
            {
                return 1;
            }

            if (EndMaybe == null)
            {
                return 0;
            }

            if (EndMaybe < time)
            {
                return -1;
            }

            return 0;
        }

        /// <summary>
        /// Custom way of printing the range object.
        /// </summary>
        /// <returns>Object in string format</returns>
        public override string ToString()
        {
            string range = EndMaybe == null ? $"{Start}-INFINITY" : $"{Start}-{EndMaybe}";
            string set = OccupiedMachineNrs.ToString();
            string left = InLeftItem == null ? "" : " L:" + InLeftItem.ToString();
            string middle = MachineNrInBothNeighbours == null ? "" : " M:" + MachineNrInBothNeighbours;
            string right = InRightItem == null ? "" : " R:" + InRightItem.ToString();
            return range + set + left + middle + right;
        }
    }
}
