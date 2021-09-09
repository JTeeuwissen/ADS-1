using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VaccinationScheduling.Shared;

namespace VaccinationScheduling.Online
{
    internal class Program
    {
        private int tFirstDose;
        private int tSecondDose;
        private int tGap;
        private int currentPatientNr;

        private List<Patient> patients = new List<Patient>();

        public Program()
        {
            ReadUtils.SetInput(true);
            (tFirstDose, tSecondDose, tGap, currentPatientNr) = ReadUtils.ParseGeneralInformation(true);

            while (true)
            {
                Patient patient = ReadUtils.ParsePatient();

                // There are no more patients
                if (patient == null)
                {
                    break;
                }

                patients.Add(patient);

                // TODO Schedule

                currentPatientNr++;
            }

            // TODO Output
        }


        private static void Main()
        {
            // Easy way to get out of static
            new Program();
        }
    }
}
