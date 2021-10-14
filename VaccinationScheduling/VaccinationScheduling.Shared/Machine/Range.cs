using System;

namespace VaccinationScheduling.Shared.Machine
{
    public class Range : IComparable<int>, IComparable<Range>
    {
        /// <summary>
        /// Start time
        /// </summary>
        public int Start;

        /// <summary>
        /// End time
        /// </summary>
        public int End;

        /// <summary>
        /// Create a range object. The range is where there is no job.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        public Range(int start, int end)
        {
            Start = start;
            End = end;
        }

        /// <summary>
        /// Implements IComparable interface, makes timpeslots comparable.
        /// </summary>
        /// <param name="other">other timeslot object to compare to</param>
        /// <returns>
        /// ret==0 Are equal
        /// ret<0 This one predecends other
        /// ret>0 This one follows other
        /// </returns>
        public (int, int)? GetOverlap(int tStart, int tEnd)
        {
            // There is no overlap between the two
            if (Start > tEnd && tEnd != -1 || End < tStart && End != -1)
                return null;

            // There is overlapping range
            int minValue = Math.Min(End, tEnd);
            return (Math.Max(Start, tStart), minValue != -1 ? minValue : Math.Max(End, tEnd));
        }

        /// <summary>
        /// Implements IComparable interface, makes timpeslots comparable.
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
            if (Start > other.End && other.End != -1)
            {
                return 1;
            }

            // (0, 1) <= (10, 20)
            if (End < other.Start && End != -1)
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

            // 0 At the end means there is no end to the range, aka infinity.
            if (End == -1)
            {
                return 0;
            }

            if (End < time)
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
            if (End == -1)
            {
                return $"({Start},INFINITY)";
            }
            return $"({Start},{End})";
        }
    }
}
