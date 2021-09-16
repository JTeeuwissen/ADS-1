using System.Collections.Generic;
using VaccinationScheduling.Shared.RedBlackTree;

namespace VaccinationScheduling.Shared
{
    public class Hospital
    {
        private Global global;

        public int HospitalIndex = 0;
        public RedBlackTree<Timeslot> Schedule = new RedBlackTree<Timeslot>(Comparer<Timeslot>.Default);

        public Hospital(int hospitalIndex, Global global)
        {
            HospitalIndex = hospitalIndex;
            this.global = global;
        }

        /// <summary>
        /// Add patient timeslots to the hospital.
        /// Sofar it does it in a greedy way. If it fits then add the patient.
        /// </summary>
        /// <returns>Whether patient has been succesfully added</returns>
        public bool GreedyAddPatient(Job patient)
        {
            // TODO: Probably need to implement custom method in RedBlackTree to make this coming double loop efficient.
            Timeslot start = new Timeslot(0, patient.MinFirstIntervalStart, patient.MinFirstIntervalStart + 1, false);
            Timeslot end = new Timeslot(0, patient.MaxFirstIntervalStart + patient.MaxGapIntervalStarts, patient.MaxFirstIntervalStart + patient.MaxGapIntervalStarts, false);
            RedBlackTree<Timeslot>.RangeTester rangeTester = Schedule.DoubleBoundedRangeTester(start, true, end, true);
            foreach (Timeslot timeslot in Schedule.EnumerateRange(rangeTester))
            {

            }

            return default;
        }

        public (int, int) FindFirstTimeslot(int tMin, int tMax, int duration)
        {
            return (0, 0);
        }

        public (int, int) FindSecondTimeslot(int tMin, int tMax, int duration)
        {
            return (0, 0);
        }
    }
}
