using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FsCheck;
using FsCheck.Xunit;
using Xunit;
using VaccinationScheduling.Shared;
using VaccinationScheduling.Shared.Machine;

namespace VaccinationScheduling.Tests.Shared.Machine
{
    public class TestRedBlackTreeExtensions
    {
        [Fact]
        public void FindInInfiniteRange()
        {
            RedBlackTree tree = new RedBlackTree(1);
            VaccinationScheduling.Shared.Machine.Range range = new(0, 0);
            Prop.ForAll<int>(x =>
            {
                x = Math.Abs(x);
                tree.Find(x, out range);
                return range.Start != -1;
            }).QuickCheckThrowOnFailure();
        }

        [Fact]
        public void FindFiniteRange()
        {
            VaccinationScheduling.Shared.Machine.Range range = new(0, 0);

            Prop.ForAll(Arb.From(Gen.Choose(0, 100).Two()), valueTuple =>
            {
                MachineSchedule ms = new MachineSchedule(new Global(10, 5, 0));

                int tJob = valueTuple.Item1;
                int tFind = valueTuple.Item2;

                ms.ScheduleJob(ms.freeRangesFirstJob, tJob, 10);
                ms.freeRangesFirstJob.Find(tFind, out range);

                // Searching below the range
                if (tFind <= tJob - 10)
                {
                    return range.Start == 0 && range.End == tJob - 10;
                }
                else if (tFind >= tJob + 10)
                {
                    return range.Start == tJob + 10 && range.End == -1;
                }
                else
                {
                    return range.Start == -1 && range.End == -1;
                }
            }).QuickCheckThrowOnFailure();
        }

        [Fact]
        public void FindOrPrevious()
        {
            VaccinationScheduling.Shared.Machine.Range range = new(0, 0);

            Prop.ForAll(Arb.From(Gen.Choose(0, 100).Two()), valueTuple =>
            {
                MachineSchedule ms = new MachineSchedule(new Global(10, 5, 0));

                int tJob = valueTuple.Item1;
                int tFind = valueTuple.Item2;

                ms.ScheduleJob(ms.freeRangesFirstJob, tJob, 10);
                ms.freeRangesFirstJob.FindOrPrevious(tFind, out range);

                // Searching below the range
                if (tFind < tJob + 10)
                {
                    return range.Start == 0 && range.End == tJob - 10;
                }
                else
                {
                    return range.Start == tJob + 10 && range.End == -1;
                }
            }).QuickCheckThrowOnFailure();
        }

        [Fact]
        public void TestEnumerateRange()
        {
            VaccinationScheduling.Shared.Machine.Range range = new(0, 0);
            System.Random random = new System.Random();

            Prop.ForAll(Arb.From(Gen.Choose(3, 10).Four()), valueTuple =>
            {
                MachineSchedule ms = new MachineSchedule(new Global(3, 5, 0));

                int tFirstJob = valueTuple.Item1;
                int tSecondJob = tFirstJob + 3 + valueTuple.Item2;
                int tThirdJob = tSecondJob + 3 + valueTuple.Item3;
                int tFourthJob = tThirdJob + 3 + valueTuple.Item4;

                ms.ScheduleJob(ms.freeRangesFirstJob, tFirstJob, 3);
                ms.ScheduleJob(ms.freeRangesFirstJob, tSecondJob, 3);
                ms.ScheduleJob(ms.freeRangesFirstJob, tThirdJob, 3);
                ms.ScheduleJob(ms.freeRangesFirstJob, tFourthJob, 3);

                int firstVal = random.Next(0, tFourthJob + 10);
                int secondVal = random.Next(-1, tFourthJob + 10);

                int minValue = secondVal == -1 ? firstVal : Math.Min(firstVal, secondVal);
                int maxValue = secondVal == -1 ? secondVal : Math.Max(firstVal, secondVal);

                // Searching below the range
                foreach (VaccinationScheduling.Shared.Machine.Range r in ms.freeRangesFirstJob.EnumerateRange(minValue, maxValue))
                {
                    if (r.Start > maxValue && maxValue != -1)
                    {
                        return false;
                    }
                    else if (r.End < minValue && r.End != -1)
                    {
                        return false;
                    }
                }

                return true;

            }).QuickCheckThrowOnFailure();
        }
    }
}
