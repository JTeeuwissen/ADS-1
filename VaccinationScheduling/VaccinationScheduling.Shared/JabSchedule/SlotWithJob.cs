using System.Numerics;

namespace VaccinationScheduling.Shared.JabSchedule
{
    internal class SlotWithJob : Slot
    {
        public readonly Job Job;

        public SlotWithJob(BigInteger slot, ( BigInteger start, BigInteger end) range, Job job)
            : base(slot, range)
        {
            Job = job;
        }
    }
}