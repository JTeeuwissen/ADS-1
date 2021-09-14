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
        public int T2 { get; set; }

        /// <summary>
        /// Hospital jab 2
        /// </summary>
        public int M2 { get; set; }

        /// <summary>
        /// Format the schedule to the desired output.
        /// </summary>
        /// <returns>A string version of the schedule.</returns>
        public override string ToString()
        {
            return $"{T1}, {M1}, {T2}, {M2}";
        }
    }
}
