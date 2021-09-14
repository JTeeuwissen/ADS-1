using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VaccinationScheduling.Shared
{
    public struct Timeslot : IComparable<Timeslot>
    {
        public int PatientNr;
        public int StartTime;
        public int EndTime;
        public bool IsSecondInterval;

        public Timeslot(int patientNr, int startTime, int endTime, bool isSecondInterval)
        {
            PatientNr = patientNr;
            StartTime = startTime;
            EndTime = endTime;
            IsSecondInterval = isSecondInterval;
        }

        /// <summary>
        /// Implements IComparable interface, makes timpeslots comparable.
        /// </summary>
        /// <param name="other">other timeslot object to compare to</param>
        /// <returns>
        /// ret==0 Are equal
        /// ret<0 This one predecends other
        /// ret>0 This one follows other
        /// </returns>
        public int CompareTo(Timeslot other)
        {
            // Compare on properties in this order:
            // StartTime, EndTime
            int deltaStartTime = StartTime - other.StartTime;
            if (deltaStartTime != 0)
            {
                return deltaStartTime;
            }

            int deltaEndTime = EndTime - other.EndTime;
            return deltaEndTime;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(int other)
        {
            return StartTime - other;
        }
    }
}
