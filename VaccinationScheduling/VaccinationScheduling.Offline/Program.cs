using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VaccinationScheduling.Shared;

namespace VaccinationScheduling.Offline
{
    internal class Program
    {
        private Parameters parameters;
        private int nrPatients;

        private Patient[] patients;

        public Program()
        {
            ReadUtils.SetInput(false);
            (parameters, nrPatients) = ReadUtils.ParseGeneralInformation(false);

            List<Patient> patientsList = new List<Patient>(nrPatients);
            for (int i = 0; i < nrPatients; i++)
            {
                patients[i] = ReadUtils.ParsePatient(parameters);
            }

            // Sort the input
            patientsList.Sort();
            patients = patientsList.ToArray();

            for (int i = 0; i < nrPatients; i++)
            {

            }
            // TODO Schedule
            // TODO Output
        }

        private static void Main()
        {
            // Easy way to get out of static
            new Program();
        }
    }
}
