namespace VaccinationScheduling.Shared
{
    /// <summary>
    /// Global parameters, which are the same for all patients
    /// </summary>
    public struct Global
    {
        /// <summary>
        /// Time it takes to get the first dose
        /// </summary>
        public int TFirstDose;
        /// <summary>
        /// Time it takes to get the second dose
        /// </summary>
        public int TSecondDose;

        /// <summary>
        /// The set time between the first and second dose
        /// </summary>
        public int TGap;

        /// <summary>
        /// Create an object that holds the global data
        /// </summary>
        /// <param name="tFirstDose">Time the first dose takes</param>
        /// <param name="tSecondDose">Time the second dose takes</param>
        /// <param name="tGap">Minimum time between the first and second dose</param>
        public Global(int tFirstDose, int tSecondDose, int tGap)
        {
            TFirstDose = tFirstDose;
            TSecondDose = tSecondDose;
            TGap = tGap;
        }
    }
}
