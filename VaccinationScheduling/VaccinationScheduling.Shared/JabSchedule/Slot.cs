using System;
using System.Numerics;

namespace VaccinationScheduling.Shared.JabSchedule
{
    internal class Slot
    {
        private readonly BigInteger _slot;
        private readonly (BigInteger _start, BigInteger _end) _range;

        public Slot(BigInteger slot, (BigInteger _start, BigInteger _end) range)
        {
            _slot = slot;
            _range = range;
        }


        /// <summary>
        /// Get the hashcode from this range using the bucket.
        /// </summary>
        /// <returns>The hashcode of this range.</returns>
        public override int GetHashCode() => _slot.GetHashCode();

        public override bool Equals(object obj)
        {
            return Equals(obj as Slot ?? throw new InvalidOperationException());
        }

        /// <summary>
        /// Ranges are equal if they overlap
        /// </summary>
        /// <param name="other">The other range to equality check with.</param>
        /// <returns>Whether the two ranges are equal and thus overlap.</returns>
        public bool Equals(Slot other) => _range._start <= other._range._end && other._range._start <= _range._end;

    }
}