using System;
using System.Linq;
using FsCheck;
using VaccinationScheduling.Online.Tree;
using Xunit;
using VaccinationScheduling.Shared;
using Range = VaccinationScheduling.Online.Tree.Range;

namespace VaccinationScheduling.Tests.Shared.Machine
{
    public class TestRedBlackTreeExtensions
    {
        [Fact]
        public void FindInInfiniteRange()
        {
            RedBlackTree tree = new(1);
            Prop.ForAll<int>(
                x =>
                {
                    x = Math.Abs(x);
                    tree.Find(x, out Range range);
                    return range.Start != -1;
                }
            ).QuickCheckThrowOnFailure();
        }

        [Fact]
        public void FindFiniteRange()
        {
            Prop.ForAll(
                Arb.From(Gen.Choose(0, 100).Two()),
                valueTuple =>
                {
                    Online.Machine ms = new(new Global(10, 5, 0));

                    int tJob = valueTuple.Item1;
                    int tFind = valueTuple.Item2;

                    ms.ScheduleJob(ms.freeRangesFirstJob, tJob, 10);
                    ms.freeRangesFirstJob.Find(tFind, out Range range);

                    // Searching below the range
                    if (tFind <= tJob - 10)
                    {
                        return range.Start == 0 && range.End == tJob - 10;
                    }

                    if (tFind >= tJob + 10)
                    {
                        return range.Start == tJob + 10 && range.End == -1;
                    }

                    return range.Start == -1 && range.End == -1;
                }
            ).QuickCheckThrowOnFailure();
        }

        [Fact]
        public void FindOrPrevious()
        {
            Prop.ForAll(
                Arb.From(Gen.Choose(0, 100).Two()),
                valueTuple =>
                {
                    Online.Machine ms = new(new Global(10, 5, 0));

                    int tJob = valueTuple.Item1;
                    int tFind = valueTuple.Item2;

                    ms.ScheduleJob(ms.freeRangesFirstJob, tJob, 10);
                    ms.freeRangesFirstJob.FindOrPrevious(tFind, out Range range);

                    // Searching below the range
                    return tFind < tJob + 10
                        ? range.Start == 0 && range.End == tJob - 10
                        : range.Start == tJob + 10 && range.End == -1;
                }
            ).QuickCheckThrowOnFailure();
        }

        [Fact]
        public void TestEnumerateRange()
        {
            System.Random random = new();

            Prop.ForAll(
                Arb.From(Gen.Choose(3, 10).Four()),
                valueTuple =>
                {
                    Online.Machine ms = new(new Global(3, 5, 0));

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
                    return ms.freeRangesFirstJob.EnumerateRange(minValue, maxValue).All(
                        r => (r.Start <= maxValue || maxValue == -1) && (r.End >= minValue || r.End == -1)
                    );
                }
            ).QuickCheckThrowOnFailure();
        }
    }
}