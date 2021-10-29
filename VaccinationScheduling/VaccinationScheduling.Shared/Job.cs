using System;
using System.Numerics;

namespace VaccinationScheduling.Shared
{
    // Using class so it is nullable
    /// <summary>
    /// Represents one patient
    /// </summary>
    public class Job
    {
        // Instead of using the parsed values, we pre-calculate when we can schedule
        /// <summary>
        /// Minimum time at which the first interval starts
        /// </summary>
        public int MinFirstIntervalStart;

        /// <summary>
        /// Maximum time at which the first interval starts
        /// </summary>
        public int MaxFirstIntervalStart;

        /// <summary>
        /// The minimum gap between the first and the second interval
        /// </summary>
        public int MinGapIntervalStarts;

        /// <summary>
        /// The maximum gap between the first and the second interval
        /// </summary>
        public int MaxGapIntervalStarts;

        // Only for offline solver.
        // And for verification since these are inefficient to use whilst scheduling
        public int FirstIntervalStart;
        public int FirstIntervalEnd;
        public int SecondIntervalLength;
        public int ExtraDelay;

        /// <summary>
        /// Construct a patients scheduling needs
        /// </summary>
        /// <param name="global">Global parameters of the program</param>
        /// <param name="firstIntervalStart">First slot patient is available in for the first jab</param>
        /// <param name="firstIntervalEnd">Last slot patient is available in for the first jab</param>
        /// <param name="secondIntervalLength">Second slot length patient is available at</param>
        /// <param name="extraDelay">The extra delay that the patient wants between the jabs</param>
        public Job(
            Global global,
            int firstIntervalStart,
            int firstIntervalEnd,
            int extraDelay,
            int secondIntervalLength
        )
        {
            MinFirstIntervalStart = firstIntervalStart;
            MaxFirstIntervalStart = firstIntervalEnd - global.TimeFirstDose + 1;
            MinGapIntervalStarts = global.TimeGap + extraDelay + global.TimeFirstDose;
            MaxGapIntervalStarts = MinGapIntervalStarts + secondIntervalLength - global.TimeSecondDose;

            // DO NOT USE THESE VARIABLES
            // Only for later verification of the answer
            FirstIntervalStart = firstIntervalStart;
            FirstIntervalEnd = firstIntervalEnd;
            SecondIntervalLength = secondIntervalLength;
            ExtraDelay = extraDelay;
        }
    }


    // Using class so it is nullable
    /// <summary>
    /// Represents one patient
    /// </summary>
    public class BigJob
    {
        // Instead of using the parsed values, we pre-calculate when we can schedule
        /// <summary>
        /// Minimum time at which the first interval starts
        /// </summary>
        public BigInteger MinFirstIntervalStart;

        /// <summary>
        /// Maximum time at which the first interval starts
        /// </summary>
        public BigInteger MaxFirstIntervalStart;

        /// <summary>
        /// The minimum gap between the first and the second interval
        /// </summary>
        public BigInteger MinGapIntervalStarts;

        /// <summary>
        /// The maximum gap between the first and the second interval
        /// </summary>
        public BigInteger MaxGapIntervalStarts;

        // Only for offline solver.
        // And for verification since these are inefficient to use whilst scheduling
        public BigInteger FirstIntervalStart;
        public BigInteger FirstIntervalEnd;
        public BigInteger SecondIntervalLength;
        public BigInteger ExtraDelay;

        /// <summary>
        /// Construct a patients scheduling needs
        /// </summary>
        /// <param name="global">Global parameters of the program</param>
        /// <param name="firstIntervalStart">First slot patient is available in for the first jab</param>
        /// <param name="firstIntervalEnd">Last slot patient is available in for the first jab</param>
        /// <param name="secondIntervalLength">Second slot length patient is available at</param>
        /// <param name="extraDelay">The extra delay that the patient wants between the jabs</param>
        public BigJob(
            BigGlobal global,
            BigInteger firstIntervalStart,
            BigInteger firstIntervalEnd,
            BigInteger extraDelay,
            BigInteger secondIntervalLength
        )
        {
            MinFirstIntervalStart = firstIntervalStart;
            MaxFirstIntervalStart = firstIntervalEnd - global.TimeFirstDose + 1;
            MinGapIntervalStarts = global.TimeGap + extraDelay + global.TimeFirstDose;
            MaxGapIntervalStarts = MinGapIntervalStarts + secondIntervalLength - global.TimeSecondDose;

            // DO NOT USE THESE VARIABLES
            // Only for later verification of the answer
            FirstIntervalStart = firstIntervalStart;
            FirstIntervalEnd = firstIntervalEnd;
            SecondIntervalLength = secondIntervalLength;
            ExtraDelay = extraDelay;
        }
    }
}
