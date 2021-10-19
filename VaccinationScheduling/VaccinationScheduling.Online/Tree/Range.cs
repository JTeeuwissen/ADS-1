using System;

namespace VaccinationScheduling.Online.Tree
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
        public int? EndMaybe;

        /// <summary>
        /// Create a range object. The range is where there is no job.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        public Range(int start, int end)
        {
            Start = start;
            EndMaybe = end;
        }


        public (int, int)? GetOverlap(int tStart, int tEnd)
        {
            // Check for overlap
            bool overlap = (EndMaybe == null || tStart < EndMaybe) && Start < tEnd;
            if (!overlap) return null;

            // There is overlapping range
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
            return EndMaybe == null ? $"({Start},INFINITY)" : $"({Start},{EndMaybe})";
        }
    }
}
