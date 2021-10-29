using System;
using System.Numerics;
using VaccinationScheduling.Online.Set;

namespace VaccinationScheduling.Online.Tree
{
    /// <summary>
    /// Range consisting out of a start annd enttime. When endttime is null it has no maximum and so goes to 'infinity'
    /// </summary>
    public class Range : IComparable<BigInteger>, IComparable<Range>
    {
        /// <summary>
        /// Start time, makes sure the starttime cannot be higher than the endtime when set.
        /// </summary>
        public BigInteger Start
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
        public BigInteger? EndMaybe
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
        private BigInteger start;
        private BigInteger? endMaybe;

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
        public Range(BigInteger start, BigInteger? end)
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
        public Range(BigInteger start, BigInteger? end, CustomSet occupiedMachineNrs)
        {
            this.start = start;
            this.endMaybe = end;
            OccupiedMachineNrs = occupiedMachineNrs;
        }

        /// <summary>
        /// Get the overlap between the current range and the start and end.
        /// </summary>
        /// <param name="tStart">Start of the range to get overlap of</param>
        /// <param name="tEnd">End of the range to get overlap of</param>
        /// <returns>The overlap between the two ranges.</returns>
        public (BigInteger, BigInteger) GetOverlap(BigInteger tStart, BigInteger tEnd)
        {
            return (BigInteger.Max(Start, tStart), EndMaybe is { } end ? BigInteger.Min(end, tEnd) : tEnd);
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
        public int CompareTo(Range? other)
        {
            if (other == null)
            {
                return -1;
            }
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
        public int CompareTo(BigInteger time)
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
