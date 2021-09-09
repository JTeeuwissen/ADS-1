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
        private int tFirstDose;
        private int tSecondDose;
        private int tGap;
        private int nrPatients;

        private Patient[] patients;

        public Program()
        {
            ReadUtils.SetInput(false);
            (tFirstDose, tSecondDose, tGap, nrPatients) = ReadUtils.ParseGeneralInformation(false);

            patients = new Patient[nrPatients];
            for (int i = 0; i < nrPatients; i++)
            {
                patients[i] = ReadUtils.ParsePatient();
            }

            // TODO Schedule
            // TODO Output

            Console.Write(nrPatients);
            Console.Write(nrPatients);
        }

        private static void Main()
        {
            // Easy way to get out of static
            new Program();
        }
    }
}
