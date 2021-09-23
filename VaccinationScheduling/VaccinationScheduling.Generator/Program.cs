using System;
using System.IO;
using System.Numerics;
using System.Security.Cryptography;

namespace VaccinationScheduling.Generator
{
    class Program
    {
        // Output type
        private static bool _online = true;

        // Vaccine
        private static BigInteger _firstDoseLength = 2;
        private static BigInteger _secondDoseLength = 3;
        private static BigInteger _defaultGap = 6;

        // Patient
        private static BigInteger _nrPatients = 20;
        private static int _maxExtraGapBytes = 3;

        // Intervals
        // First BigInteger range
        private static int _minFirstIntervalStartBytes = 0;
        private static int _maxFirstIntervalStartBytes = 60;

        // First interval length
        private static int _minFirstIntervalLengthBytes = 3;
        private static int _maxFirstIntervalLengthBytes = 7;

        // Second interval length
        private static int _minSecondIntervalLengthBytes = 4;
        private static int _maxSecondIntervalLengthBytes = 6;

        /// <summary>
        /// Entrypoint of the program, generates an input file given the settings in this class
        /// </summary>
        static void Main()
        {
            LoadSettings();


            string? filePath = RequestString("Path (Console)");

            // Write the file with the given settings
            using TextWriter outputFile = filePath is { } ? new StreamWriter(filePath.Trim('"'), false) : Console.Out;
            PrintSettings(outputFile);
            PrintPatients(outputFile);
        }

        /// <summary>
        /// Print the first 3/4 lines containing the parameters
        /// </summary>
        static void PrintSettings(TextWriter writer)
        {
            writer.WriteLine(_firstDoseLength);
            writer.WriteLine(_secondDoseLength);
            writer.WriteLine(_defaultGap);

            // Offline does not contain number of patients
            if (!_online) writer.WriteLine(_nrPatients);
        }

        /// <summary>
        /// Print the patients to the file, in csv format
        /// </summary>
        /// <param name="writer">File to write the patient information to</param>
        static void PrintPatients(TextWriter writer)
        {
            Random random = new();

            // Write line with random numbers to the file
            for (int i = 0; i < _nrPatients; i++)
            {
                BigInteger firstIntervalStart =
                    RandomBigInteger(random.Next(_minFirstIntervalStartBytes, _maxFirstIntervalStartBytes));
                BigInteger firstIntervalEnd = firstIntervalStart + RandomBigInteger(
                    random.Next(_minFirstIntervalLengthBytes, _maxFirstIntervalLengthBytes)
                );
                BigInteger patientGap = RandomBigInteger(random.Next(_maxExtraGapBytes));
                BigInteger secondIntervalLength = RandomBigInteger(
                    random.Next(_minSecondIntervalLengthBytes, _maxSecondIntervalLengthBytes)
                );

                writer.WriteLine($"{firstIntervalStart}, {firstIntervalEnd}, {patientGap}, {secondIntervalLength}");
            }

            // Indicate final line in online file
            if (_online) writer.WriteLine("x");
        }


        static void LoadSettings()
        {
            if (RequestBool($"{nameof(_online)} ({_online})") is { } online) _online = online;

            if (RequestBigInteger($"{nameof(_nrPatients)} ({_nrPatients})") is { } nrPatients)
            {
                // Check fixed values
                if (nrPatients <= 0)
                    throw new ArgumentException("No use in generating for <= 0 patients");

                _nrPatients = nrPatients;
            }

            if (RequestBigInteger($"{nameof(_firstDoseLength)} ({_firstDoseLength})") is { } firstDoseLength)
            {
                // Check fixed values
                if (firstDoseLength <= 0)
                    throw new ArgumentException("No use in generating for <= 0 patients");

                _firstDoseLength = firstDoseLength;
            }

            if (RequestBigInteger($"{nameof(_secondDoseLength)} ({_secondDoseLength})") is { } secondDoseLength)
            {
                // Check fixed values
                if (secondDoseLength <= 0)
                    throw new ArgumentException("No use in generating for <= 0 patients");

                _secondDoseLength = secondDoseLength;
            }

            if (RequestBigInteger($"{nameof(_defaultGap)} ({_defaultGap})") is { } defaultGap)
            {
                // Check fixed values
                if (defaultGap < 0)
                    throw new ArgumentException("Cannot have negative gaps");

                _defaultGap = defaultGap;
            }

            if (RequestInt($"{nameof(_maxExtraGapBytes)} ({_maxExtraGapBytes})") is { } maxExtraGapBytes)
            {
                // Check fixed values
                if (maxExtraGapBytes < 0)
                    throw new ArgumentException("Cannot have negative gaps");

                _maxExtraGapBytes = maxExtraGapBytes;
            }

            if (RequestInt($"{nameof(_minFirstIntervalStartBytes)} ({_minFirstIntervalStartBytes})") is
            {
            } minFirstIntervalStartBytes)
            {
                // Check fixed values
                if (minFirstIntervalStartBytes < 0)
                    throw new ArgumentException("First interval start is wrong");

                _minFirstIntervalStartBytes = minFirstIntervalStartBytes;
            }

            if (RequestInt($"{nameof(_maxFirstIntervalStartBytes)} ({_maxFirstIntervalStartBytes})") is
            {
            } maxFirstIntervalStartBytes)
            {
                // Check fixed values
                if (_minFirstIntervalStartBytes > maxFirstIntervalStartBytes)
                    throw new ArgumentException("First interval start is wrong");

                _maxFirstIntervalStartBytes = maxFirstIntervalStartBytes;
            }

            if (RequestInt($"{nameof(_minFirstIntervalLengthBytes)} ({_minFirstIntervalLengthBytes})") is
            {
            } minFirstIntervalLengthBytes)
            {
                // Check fixed values
                if (minFirstIntervalLengthBytes < 0)
                    throw new ArgumentException("First interval length variables are wrong");

                _minFirstIntervalLengthBytes = minFirstIntervalLengthBytes;
            }

            if (RequestInt($"{nameof(_maxFirstIntervalLengthBytes)} ({_maxFirstIntervalLengthBytes})") is
            {
            } maxFirstIntervalLengthBytes)
            {
                // Check fixed values
                if (_minFirstIntervalLengthBytes > _maxFirstIntervalLengthBytes)
                    throw new ArgumentException("First interval length variables are wrong");

                _maxFirstIntervalLengthBytes = maxFirstIntervalLengthBytes;
            }

            if (RequestInt($"{nameof(_minSecondIntervalLengthBytes)} ({_minSecondIntervalLengthBytes})") is
            {
            } minSecondIntervalLengthBytes)
            {
                // Check fixed values
                if (_minSecondIntervalLengthBytes < 0)
                    throw new ArgumentException("First interval length variables are wrong");

                _minSecondIntervalLengthBytes = minSecondIntervalLengthBytes;
            }


            if (RequestInt($"{nameof(_maxSecondIntervalLengthBytes)} ({_maxSecondIntervalLengthBytes})") is
            {
            } maxSecondIntervalLengthBytes)
            {
                // Check fixed values
                if (_minSecondIntervalLengthBytes > _maxSecondIntervalLengthBytes)
                    throw new ArgumentException("Second interval length variables are wrong");

                _maxSecondIntervalLengthBytes = maxSecondIntervalLengthBytes;
            }

            // Compare dose length with patient range
            if (_firstDoseLength > _minFirstIntervalLengthBytes || _secondDoseLength > _minSecondIntervalLengthBytes)
            {
                throw new ArgumentException("Dose must have enough time sechduled to be valid");
            }
        }

        private static string? RequestString(string request)
        {
            Console.WriteLine(request);
            string? responseMaybe = Console.ReadLine();
            return responseMaybe is { } response ? string.IsNullOrEmpty(response) ? null : response : null;
        }

        private static bool? RequestBool(string request)
        {
            string? responseMaybe = RequestString(request);
            return responseMaybe is { } response ? bool.Parse(response) : null;
        }

        private static int? RequestInt(string request)
        {
            string? responseMaybe = RequestString(request);
            return responseMaybe is { } response ? int.Parse(response) : null;
        }

        private static BigInteger? RequestBigInteger(string request)
        {
            string? responseMaybe = RequestString(request);
            return responseMaybe is { } response ? BigInteger.Parse(response) : null;
        }


        private static readonly RNGCryptoServiceProvider Rng = new();

        private static BigInteger RandomBigInteger(int n = 10)
        {
            byte[] bytes = new byte[n];
            Rng.GetBytes(bytes);
            return BigInteger.Abs(new BigInteger(bytes));
        }
    }
}