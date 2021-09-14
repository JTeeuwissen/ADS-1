using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VaccinationScheduling.Shared
{
    public static class ReadUtils
    {
        public static void SetInput(bool online)
        {
            // Automatically parse input file depending on what key is pressed
            // For example, pressing 0 will instantly read Input/0.in
            string file = Console.ReadKey().KeyChar.ToString();
            string fileName;

            if (online)
            {
                fileName = $"..\\..\\..\\..\\..\\Input\\Online\\{file}.in";
            }
            else
            {
                fileName = $"..\\..\\..\\..\\..\\Input\\Offline\\{file}.in";
            }

            Console.SetIn(new StreamReader(fileName));
            Console.WriteLine();
        }

        public static (Parameters, int) ParseGeneralInformation(bool online)
        {
            // Read and return first 4 lines of input
            int tFirstDose = int.Parse(Console.ReadLine());
            int tSecondDose = int.Parse(Console.ReadLine());
            int tGap = int.Parse(Console.ReadLine());
            Parameters parameters = new Parameters(tFirstDose, tSecondDose, tGap);

            if (online)
            {
                return (parameters, 0);
            }

            int nrPatients = int.Parse(Console.ReadLine());
            return (parameters, nrPatients);
        }

        public static Patient? ParsePatient(Parameters parameters)
        {
            // Parse patient line in input
            string[] values = Console.ReadLine().Split(',');

            // No more patient to read, final line is x
            if (values.Length == 1)
            {
                return null;
            }

            // Return created patient
            return new Patient(
                parameters,
                int.Parse(values[0]),
                int.Parse(values[1]),
                int.Parse(values[2]),
                int.Parse(values[3])
            );
        }
    }
}
