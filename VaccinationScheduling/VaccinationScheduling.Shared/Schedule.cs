using System.Numerics;

namespace VaccinationScheduling.Shared
{
    public class Schedule
    {
        /// <summary>
        /// Timeslot jab 1
        /// </summary>
        public int T1 { get; set; }

        /// <summary>
        /// Hospital jab 1
        /// </summary>
        public int M1 { get; set; }

        /// <summary>
        /// Timeslot jab 2
        /// </summary>
        public BigInteger T2 { get; set; }

        /// <summary>
        /// Hospital jab 2
        /// </summary>
        public BigInteger M2 { get; set; }

        public Schedule(int t1, int m1, BigInteger t2, BigInteger m2)
        {
            T1 = t1;
            M1 = m1;
            T2 = t2;
            M2 = m2;
        }

        /// <summary>
        /// Format the schedule to the desired output.
        /// </summary>
        /// <returns>A string version of the schedule.</returns>
        public override string ToString() => $"{T1}, {M1}, {T2}, {M2}";
    }
}
