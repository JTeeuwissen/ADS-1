namespace VaccinationScheduling.Shared
{
    /// <summary>
    /// Global parameters, which are the same for all patients
    /// </summary>
    public class Global
    {
        /// <summary>
        /// The processing time of the first dose.
        /// </summary>
        public int P1 { get; }

        /// <summary>
        /// The processing time of the second dose.
        /// </summary>
        public int P2 { get; }

        /// <summary>
        /// The time gap between the first and the second dose.
        /// </summary>
        public int G { get; }

        public Global(int p1, int p2, int g)
        {
            P1 = p1;
            P2 = p2;
            G = g;
        }
    }
}
