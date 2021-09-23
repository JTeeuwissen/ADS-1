using System;
using System.IO;
using System.Numerics;

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
        private static BigInteger _maxExtraGap = 3;

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
        static void Main()
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
                BigInteger firstIntervalStart = (random.NextBigInteger(
                    _minFirstIntervalStart,
                    _maxFirstIntervalStart
                ));
                BigInteger firstIntervalEnd = firstIntervalStart + (random.NextBigInteger(
                    _minFirstIntervalLength,
                    _maxFirstIntervalLength
                ));
                BigInteger patientGap = (random.NextBigInteger(0, _maxExtraGap));
                BigInteger secondIntervalLength = (random.NextBigInteger(
                    _minSecondIntervalLength,
                    _maxSecondIntervalLength
                ));

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

            if (RequestBigInteger($"{nameof(_maxExtraGap)} ({_maxExtraGap})") is { } maxExtraGapBytes)
            {
                // Check fixed values
                if (maxExtraGapBytes < 0)
                    throw new ArgumentException("Cannot have negative gaps");

                _maxExtraGap = maxExtraGapBytes;
            }

            if (RequestBigInteger($"{nameof(_minFirstIntervalStart)} ({_minFirstIntervalStart})") is
            {
            } minFirstIntervalStartBytes)
            {
                // Check fixed values
                if (minFirstIntervalStartBytes < 0)
                    throw new ArgumentException("First interval start is wrong");

                _minFirstIntervalStart = minFirstIntervalStartBytes;
            }

            if (RequestBigInteger($"{nameof(_maxFirstIntervalStart)} ({_maxFirstIntervalStart})") is
            {
            } maxFirstIntervalStartBytes)
            {
                // Check fixed values
                if (_minFirstIntervalStart > maxFirstIntervalStartBytes)
                    throw new ArgumentException("First interval start is wrong");

                _maxFirstIntervalStart = maxFirstIntervalStartBytes;
            }

            if (RequestBigInteger($"{nameof(_minFirstIntervalLength)} ({_minFirstIntervalLength})") is
            {
            } minFirstIntervalLengthBytes)
            {
                // Check fixed values
                if (minFirstIntervalLengthBytes < 0)
                    throw new ArgumentException("First interval length variables are wrong");

                _minFirstIntervalLength = minFirstIntervalLengthBytes;
            }

            if (RequestBigInteger($"{nameof(_maxFirstIntervalLength)} ({_maxFirstIntervalLength})") is
            {
            } maxFirstIntervalLengthBytes)
            {
                // Check fixed values
                if (_minFirstIntervalLength > _maxFirstIntervalLength)
                    throw new ArgumentException("First interval length variables are wrong");

                _maxFirstIntervalLength = maxFirstIntervalLengthBytes;
            }

            if (RequestBigInteger($"{nameof(_minSecondIntervalLength)} ({_minSecondIntervalLength})") is
            {
            } minSecondIntervalLengthBytes)
            {
                // Check fixed values
                if (_minSecondIntervalLength < 0)
                    throw new ArgumentException("First interval length variables are wrong");

                _minSecondIntervalLength = minSecondIntervalLengthBytes;
            }


            if (RequestBigInteger($"{nameof(_maxSecondIntervalLength)} ({_maxSecondIntervalLength})") is
            {
            } maxSecondIntervalLengthBytes)
            {
                // Check fixed values
                if (_minSecondIntervalLength > _maxSecondIntervalLength)
                    throw new ArgumentException("Second interval length variables are wrong");

                _maxSecondIntervalLength = maxSecondIntervalLengthBytes;
            }

            // Compare dose length with patient range
            if (_firstDoseLength > _minFirstIntervalLength || _secondDoseLength > _minSecondIntervalLength)
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

        private static BigInteger? RequestBigInteger(string request)
        {
            string? responseMaybe = RequestString(request);
            return responseMaybe is { } response ? BigInteger.Parse(response) : null;
        }
    }
}