using System;
using System.IO;
using System.Numerics;

namespace VaccinationScheduling.Generator
{
    public class Program
    {
        // Output type
        private static bool _online = true;

        // Vaccine
        private static BigInteger _firstDoseLength = 2;
        private static BigInteger _secondDoseLength = 3;
        private static BigInteger _defaultGap = 6;

        // Patient
        private static BigInteger _nrPatients = 20;
        private static BigInteger _maxPatientGap = 3;

        // Intervals
        // First BigInteger range
        private static BigInteger _minFirstIntervalStart = 0;
        private static BigInteger _maxFirstIntervalStart = 60;

        // First interval length
        private static BigInteger _minFirstIntervalLength = 3;
        private static BigInteger _maxFirstIntervalLength = 7;

        // Second interval length
        private static BigInteger _minSecondIntervalLength = 4;
        private static BigInteger _maxSecondIntervalLength = 6;

        /// <summary>
        /// Entrypoint of the program, generates an input file given the settings in this class
        /// </summary>
        public static void Main()
        {
            LoadSettings();

            string? filePath = RequestString("Path (Console)")?.Trim('"');

            // Write the file with the given settings
            using TextWriter outputFile = filePath is { } ? new StreamWriter(filePath, false) : Console.Out;
            PrintSettings(outputFile);
            PrintPatients(outputFile);
        }

        /// <summary>
        /// Print the first 3/4 lines containing the parameters
        /// </summary>
        private static void PrintSettings(TextWriter writer)
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
        private static void PrintPatients(TextWriter writer)
        {
            Random random = new();

            // Write line with random numbers to the file
            for (int i = 0; i < _nrPatients; i++)
            {
                BigInteger firstIntervalStart = random.NextBigInteger(_minFirstIntervalStart, _maxFirstIntervalStart);
                BigInteger firstIntervalEnd = firstIntervalStart + random.NextBigInteger(
                    _minFirstIntervalLength,
                    _maxFirstIntervalLength
                );
                BigInteger patientGap = random.NextBigInteger(0, _maxPatientGap);
                BigInteger secondIntervalLength = random.NextBigInteger(
                    _minSecondIntervalLength,
                    _maxSecondIntervalLength
                );

                writer.WriteLine($"{firstIntervalStart}, {firstIntervalEnd}, {patientGap}, {secondIntervalLength}");
            }

            // Indicate final line in online file
            if (_online) writer.WriteLine("x");
        }


        private static void LoadSettings()
        {
            if (RequestBool($"{nameof(_online)} ({_online})") is { } online) _online = online;

            if (RequestBigInteger($"{nameof(_nrPatients)} ({_nrPatients})") is { } nrPatients)
            {
                // Check fixed values
                if (nrPatients <= 0)
                    throw new ArgumentException($"{nameof(_nrPatients)} should be a positive integer.");

                _nrPatients = nrPatients;
            }

            if (RequestBigInteger($"{nameof(_firstDoseLength)} ({_firstDoseLength})") is { } firstDoseLength)
            {
                // Check fixed values
                if (firstDoseLength <= 0)
                    throw new ArgumentException($"{nameof(_firstDoseLength)} should be a positive integer.");

                _firstDoseLength = firstDoseLength;
            }

            if (RequestBigInteger($"{nameof(_secondDoseLength)} ({_secondDoseLength})") is { } secondDoseLength)
            {
                // Check fixed values
                if (secondDoseLength <= 0)
                    throw new ArgumentException($"{nameof(_secondDoseLength)} should be a positive integer.");

                _secondDoseLength = secondDoseLength;
            }

            if (RequestBigInteger($"{nameof(_defaultGap)} ({_defaultGap})") is { } defaultGap)
            {
                // Check fixed values
                if (defaultGap < 0)
                    throw new ArgumentException($"{nameof(_defaultGap)} should be a non-negative integer.");

                _defaultGap = defaultGap;
            }

            if (RequestBigInteger($"{nameof(_maxPatientGap)} ({_maxPatientGap})") is { } maxExtraGap)
            {
                // Check fixed values
                if (maxExtraGap < 0)
                    throw new ArgumentException($"{nameof(maxExtraGap)} should be a non-negative integer.");

                _maxPatientGap = maxExtraGap;
            }

            if (RequestBigInteger($"{nameof(_minFirstIntervalStart)} ({_minFirstIntervalStart})") is
            {
            } minFirstIntervalStart)
            {
                // Check fixed values
                if (minFirstIntervalStart < 0)
                    throw new ArgumentException($"{nameof(_minFirstIntervalStart)} should be a non-negative integer.");

                _minFirstIntervalStart = minFirstIntervalStart;
            }

            if (RequestBigInteger($"{nameof(_maxFirstIntervalStart)} ({_maxFirstIntervalStart})") is
            {
            } maxFirstIntervalStart)
            {
                // Check fixed values
                if (maxFirstIntervalStart < _minFirstIntervalStart)
                    throw new ArgumentException(
                        $"{nameof(_maxFirstIntervalStart)} should be larger than or equal to {nameof(_minFirstIntervalStart)}"
                    );

                _maxFirstIntervalStart = maxFirstIntervalStart;
            }

            if (RequestBigInteger($"{nameof(_minFirstIntervalLength)} ({_minFirstIntervalLength})") is
            {
            } minFirstIntervalLength)
            {
                // Check fixed values
                if (minFirstIntervalLength < 0)
                    throw new ArgumentException($"{nameof(_minFirstIntervalLength)} should be a non-negative integer.");

                // Compare dose length with patient range
                if (minFirstIntervalLength < _firstDoseLength)
                    throw new ArgumentException(
                        $"{nameof(_minFirstIntervalLength)} should be larger than or equal to {nameof(_firstDoseLength)}"
                    );

                _minFirstIntervalLength = minFirstIntervalLength;
            }

            if (RequestBigInteger($"{nameof(_maxFirstIntervalLength)} ({_maxFirstIntervalLength})") is
            {
            } maxFirstIntervalLength)
            {
                // Check fixed values
                if (maxFirstIntervalLength < _minFirstIntervalLength)
                    throw new ArgumentException(
                        $"{nameof(_maxFirstIntervalLength)} should be larger than or equal to {nameof(_minFirstIntervalLength)}"
                    );


                _maxFirstIntervalLength = maxFirstIntervalLength;
            }

            if (RequestBigInteger($"{nameof(_minSecondIntervalLength)} ({_minSecondIntervalLength})") is
            {
            } minSecondIntervalLength)
            {
                // Check fixed values
                if (_minSecondIntervalLength < 0)
                    throw new ArgumentException(
                        $"{nameof(_minSecondIntervalLength)} should be a non-negative integer."
                    );

                // Compare dose length with patient range
                if (_minSecondIntervalLength < _secondDoseLength)
                    throw new ArgumentException(
                        $"{nameof(_minSecondIntervalLength)} should be larger than or equal to {nameof(_secondDoseLength)}"
                    );

                _minSecondIntervalLength = minSecondIntervalLength;
            }


            // ReSharper disable once InvertIf
            if (RequestBigInteger($"{nameof(_maxSecondIntervalLength)} ({_maxSecondIntervalLength})") is
            {
            } maxSecondIntervalLength)
            {
                // Check fixed values
                if (maxSecondIntervalLength < _minSecondIntervalLength)
                    throw new ArgumentException(
                        $"{nameof(_maxSecondIntervalLength)} should be larger than or equal to {nameof(_minSecondIntervalLength)}"
                    );

                _maxSecondIntervalLength = maxSecondIntervalLength;
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

        private static BigInteger? RequestBigInteger(string request)
        {
            string? responseMaybe = RequestString(request);
            return responseMaybe is { } response ? BigInteger.Parse(response) : null;
        }
    }
}