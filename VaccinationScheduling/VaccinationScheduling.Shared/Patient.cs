﻿using System;

namespace VaccinationScheduling.Shared
{
    // Using class so it is nullable
    public class Patient : IComparable<Patient>
    {
        // Instead of using the parsed values, we pre-calculate when we can schedule
        // What is the minimum and maximum start T of the first dose interval
        public int MinFirstIntervalStart;
        public int MaxFirstIntervalStart;

        // What is the minimum and maximum Gap between the first and second dose
        public int MinGapIntervalStarts;
        public int MaxGapIntervalStarts;

        // Do NOT use these for the main program
        // Only for verification since these are inefficient to use whilst scheduling
        private int firstIntervalStart;
        private int firstIntervalEnd;
        private int secondIntervalLength;
        private int extraDelay;

        /// <summary>
        /// Construct a patients scheduling needs
        /// </summary>
        /// <param name="parameters">Parameters of the program</param>
        /// <param name="firstIntervalStart">First slot patient is available in for the first jab</param>
        /// <param name="firstIntervalEnd">Last slot patient is available in for the first jab</param>
        /// <param name="secondIntervalLength">Second slot length patient is available at</param>
        /// <param name="extraDelay">The extra delay that the patient wants between the jabs</param>
        public Patient(Parameters parameters, int firstIntervalStart, int firstIntervalEnd, int secondIntervalLength, int extraDelay)
        {
            MinFirstIntervalStart = firstIntervalStart;
            MaxFirstIntervalStart = firstIntervalEnd - parameters.TFirstDose + 1;
            MinGapIntervalStarts = parameters.TGap + extraDelay + parameters.TFirstDose;
            MaxGapIntervalStarts = MinGapIntervalStarts + secondIntervalLength - parameters.TSecondDose - 1;

            // DO NOT USE THESE VARIABLES
            // Only for later verification of the answer
            this.firstIntervalStart = firstIntervalStart;
            this.firstIntervalEnd = firstIntervalEnd;
            this.secondIntervalLength = secondIntervalLength;
            this.extraDelay = extraDelay;
        }

        /// <summary>
        /// Implements IComparable interface, makes a list of patient sortable.
        /// </summary>
        /// <param name="other">other patient object to compare to</param>
        /// <returns>
        /// ret==0 Are equal
        /// ret<0 This one predecends other
        /// ret>0 This one follows other
        /// </returns>
        public int CompareTo(Patient? other)
        {
            // Always is before a null patient
            if (other == null)
            {
                return -1;
            }

            // Compare on properties in this order:
            // First slot, gap length, second slot
            int deltaMinFirstIntervalStart = MinFirstIntervalStart - other.MinFirstIntervalStart;
            if (deltaMinFirstIntervalStart != 0)
            {
                return deltaMinFirstIntervalStart;
            }

            int deltaMaxFirstIntervalStart = MaxFirstIntervalStart - other.MaxFirstIntervalStart;
            if (deltaMaxFirstIntervalStart != 0)
            {
                return deltaMaxFirstIntervalStart;
            }

            int deltaMinGapIntervalStarts = MinGapIntervalStarts - other.MinGapIntervalStarts;
            if (deltaMinGapIntervalStarts != 0)
            {
                return deltaMinGapIntervalStarts;
            }

            int deltaMaxGapIntervalStarts = MaxGapIntervalStarts - other.MaxGapIntervalStarts;
            return deltaMaxGapIntervalStarts;
        }
    }
}
