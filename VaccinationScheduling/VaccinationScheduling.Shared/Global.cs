using System.Numerics;

namespace VaccinationScheduling.Shared
{
    /// <summary>
    /// Global parameters, which are the same for all patients
    /// </summary>
    public struct BigGlobal
    {
        /// <summary>
        /// Time it takes to get the first dose
        /// </summary>
        public BigInteger TimeFirstDose;
        /// <summary>
        /// Time it takes to get the second dose
        /// </summary>
        public BigInteger TimeSecondDose;

        /// <summary>
        /// The set time between the first and second dose
        /// </summary>
        public BigInteger TimeGap;

        /// <summary>
        /// Create an object that holds the global data
        /// </summary>
        /// <param name="timeFirstDose">Time the first dose takes</param>
        /// <param name="timeSecondDose">Time the second dose takes</param>
        /// <param name="timeGap">Minimum time between the first and second dose</param>
        public BigGlobal(BigInteger timeFirstDose, BigInteger timeSecondDose, BigInteger timeGap)
        {
            TimeFirstDose = timeFirstDose;
            TimeSecondDose = timeSecondDose;
            TimeGap = timeGap;
        }
    }

    /// <summary>
    /// Global parameters, which are the same for all patients
    /// </summary>
    public struct Global
    {
        /// <summary>
        /// Time it takes to get the first dose
        /// </summary>
        public int TimeFirstDose;
        /// <summary>
        /// Time it takes to get the second dose
        /// </summary>
        public int TimeSecondDose;

        /// <summary>
        /// The set time between the first and second dose
        /// </summary>
        public int TimeGap;

        /// <summary>
        /// Create an object that holds the global data
        /// </summary>
        /// <param name="timeFirstDose">Time the first dose takes</param>
        /// <param name="timeSecondDose">Time the second dose takes</param>
        /// <param name="timeGap">Minimum time between the first and second dose</param>
        public Global(int timeFirstDose, int timeSecondDose, int timeGap)
        {
            TimeFirstDose = timeFirstDose;
            TimeSecondDose = timeSecondDose;
            TimeGap = timeGap;
        }
    }
}
