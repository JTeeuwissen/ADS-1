using System;
using System.Numerics;

namespace VaccinationScheduling.Generator
{
    public static class Extensions
    {
        /// <summary>
        /// Returns a random BigInteger that is within a specified range.
        /// The lower bound is inclusive, and the upper bound is exclusive.
        /// <see href="https://stackoverflow.com/questions/17357760/how-can-i-generate-a-random-biginteger-within-a-certain-range"/>
        /// </summary>
        public static BigInteger NextBigInteger(this Random random,
            BigInteger minValue, BigInteger maxValue)
        {
            if (minValue > maxValue) throw new ArgumentException();
            if (minValue == maxValue) return minValue;
            BigInteger zeroBasedUpperBound = maxValue - 1 - minValue; // Inclusive
            byte[] bytes = zeroBasedUpperBound.ToByteArray();

            // Search for the most significant non-zero bit
            byte lastByteMask = 0b11111111;
            for (byte mask = 0b10000000; mask > 0; mask >>= 1, lastByteMask >>= 1)
            {
                if ((bytes[^1] & mask) == mask) break; // We found it
            }

            while (true)
            {
                random.NextBytes(bytes);
                bytes[^1] &= lastByteMask;
                BigInteger result = new(bytes);
                if (result <= zeroBasedUpperBound) return result + minValue;
            }
        }
    }
}
