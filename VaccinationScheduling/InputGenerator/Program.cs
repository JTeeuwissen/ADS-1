using System;
using System.IO;

namespace InputGenerator
{
    class Program
    {
        // Output type
        static bool online = true;

        // Vaccine
        static int firstDoseLength = 2;
        static int secondDoseLength = 3;
        static int defaultGap = 6;

        // Patient
        static int nrPatients = 20;
        static int maxExtraGap = 3;

        // Intervals
        // First interval range
        static int minFirstIntervalStart = 0;
        static int maxFirstIntervalStart = 60;

        // First interval length
        static int minFirstIntervalLength = 3;
        static int maxFirstIntervalLength = 7;

        // Second interval length
        static int minSecondIntervalLength = 4;
        static int maxSecondIntervalLength = 6;

        /// <summary>
        /// Entrypoint of the program, generates an input file given the settings in this class
        /// </summary>
        static void Main()
        {
            // Check whether settings are valid
            CheckSettings();

            // Generate the filename and determine the folder to put it in
            string fileName = DateTime.Now.ToString("MM-dd-yy_H-mm-ss");
            string dirName = online ? "Online" : "Offline";

            // Write the file with the given settings
            using (StreamWriter outputFile = new StreamWriter($"../../../../../Input/{dirName}/{fileName}", true))
            {
                PrintSettings(outputFile);
                PrintPatients(outputFile);
            }
        }

        /// <summary>
        /// Print the first 3/4 lines containing the parameters
        /// </summary>
        static void PrintSettings(StreamWriter outputFile)
        {
            outputFile.WriteLine(firstDoseLength);
            outputFile.WriteLine(secondDoseLength);
            outputFile.WriteLine(defaultGap);

            // Offline does not contain number of patients
            if (!online)
            {
                outputFile.WriteLine(nrPatients);
            }
        }

        /// <summary>
        /// Print the patients to the file, in csv format
        /// </summary>
        /// <param name="outputFile">File to write the patient information to</param>
        static void PrintPatients(StreamWriter outputFile)
        {
            Random random = new Random();

            // Write line with random numbers to the file
            for (int i = 0; i < nrPatients; i++)
            {
                int firstIntervalStart = random.Next(minFirstIntervalStart, maxFirstIntervalStart);
                int firstIntervalEnd = firstIntervalStart + random.Next(minFirstIntervalLength, maxFirstIntervalLength);
                int patientGap = random.Next(maxExtraGap);
                int secondIntervalLength = random.Next(minSecondIntervalLength, maxSecondIntervalLength);

                outputFile.WriteLine($"{firstIntervalStart}, {firstIntervalEnd}, {patientGap}, {secondIntervalLength}");
            }

            // Indicate final line in online file
            if (online)
            {
                outputFile.WriteLine("x");
            }
        }

        /// <summary>
        /// Check whether the settings are valid for a solution
        /// </summary>
        /// <exception cref="ArgumentException">Whenever the given configuration is invalid</exception>
        static void CheckSettings()
        {
            // Check fixed values
            if (nrPatients <= 0)
            {
                throw new ArgumentException("No use in generating for <= 0 patients");
            }

            if (firstDoseLength <= 0 || secondDoseLength <= 0)
            {
                throw new ArgumentException("No dose lengths have to be greater than 0");
            }

            if (defaultGap < 0 || maxExtraGap < 0)
            {
                throw new ArgumentException("Cannot have negative gaps");
            }

            // Check range and time variables
            if (minFirstIntervalStart < 0 || minFirstIntervalStart > maxFirstIntervalStart)
            {
                throw new ArgumentException("First interval start is wrong");
            }

            if (minFirstIntervalLength < 0 || minFirstIntervalLength > maxFirstIntervalLength)
            {
                throw new ArgumentException("First interval length variables are wrong");
            }

            if (minSecondIntervalLength < 0 || minSecondIntervalLength > maxSecondIntervalLength)
            {
                throw new ArgumentException("Second interval length variables are wrong");
            }

            // Compare dose length with patient range
            if (firstDoseLength > minFirstIntervalLength || secondDoseLength > minSecondIntervalLength)
            {
                throw new ArgumentException("Dose must have enough time sechduled to be valid");
            }
        }
    }
}
