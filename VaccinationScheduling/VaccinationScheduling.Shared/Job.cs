namespace VaccinationScheduling.Shared
{
    /// <summary>
    /// Represents 1 patient
    /// </summary>
    public class Job
    {
        /// <summary>   
        /// The first feasible interval for the first dose.
        /// </summary>
        public (int start, int end) I1 { get; }

        /// <summary>
        /// The patient-dependent delay.
        /// How long you have to wait after the gap before you can give the second shot.
        /// </summary>
        public int X { get;  }

        /// <summary>
        /// The patient-dependent (second) feasible interval length.
        /// How long you have to vaccinate the patient after his delay.
        /// </summary>
        public int L { get;  }

        public Job((int start, int end) i1, int x, int l)
        {
            I1 = i1;
            X = x;
            L = l;
        }
    }
}