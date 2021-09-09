namespace VaccinationScheduling.Shared
{
    public class Patient
    {
        public int FirstIntervalStart;
        public int FirstIntervalEnd;
        public int SecondIntervalLength;
        public int ExtraDelay;

        public Patient(int firstIntervalStart, int firstIntervalEnd, int secondIntervalLength, int extraDelay)
        {
            FirstIntervalStart = firstIntervalStart;
            FirstIntervalEnd = firstIntervalEnd;
            SecondIntervalLength = secondIntervalLength;
            ExtraDelay = extraDelay;
        }
    }
}
