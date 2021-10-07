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
    public class TestMachineSchedule
    {
        [Fact]
        public void TestScheduleJobInEmptyTreeAtStart()
        {
            MachineSchedule ms = new(new Global(1, 2, 0));
            RedBlackTree firstTree = ms.freeRangesFirstJob;
            RedBlackTree secondTree = ms.freeRangesSecondJob;

            // First tree
            Assert.Equal("(0,INFINITY)", firstTree.ToString());
            ms.ScheduleJob(firstTree, 0, 1);
            Assert.Equal("(1,INFINITY)", firstTree.ToString());

            // Second tree
            Assert.Equal("(0,INFINITY)", secondTree.ToString());
            ms.ScheduleJob(secondTree, 0, 2);
            Assert.Equal("(2,INFINITY)", secondTree.ToString());
        }

        [Fact]
        public void TestScheduleJobInEmptyTreeAfterStart()
        {
            MachineSchedule ms = new(new Global(2, 3, 0));
            RedBlackTree firstTree = ms.freeRangesFirstJob;
            RedBlackTree secondTree = ms.freeRangesSecondJob;

            // First tree
            Assert.Equal("(0,INFINITY)", firstTree.ToString());
            ms.ScheduleJob(firstTree, 1, 2);
            Assert.Equal("(3,INFINITY)", firstTree.ToString());

            // Second tree
            Assert.Equal("(0,INFINITY)", secondTree.ToString());
            ms.ScheduleJob(secondTree, 2, 3);
            Assert.Equal("(5,INFINITY)", secondTree.ToString());
        }

        [Fact]
        public void TestScheduleJobInEmptyTreeInMiddle()
        {
            MachineSchedule ms = new(new Global(2, 3, 0));
            RedBlackTree firstTree = ms.freeRangesFirstJob;
            RedBlackTree secondTree = ms.freeRangesSecondJob;

            // First tree
            Assert.Equal("(0,INFINITY)", firstTree.ToString());
            ms.ScheduleJob(firstTree, 10, 2);
            Assert.Equal("(0,8)->(12,INFINITY)", firstTree.ToString());

            // Second tree
            Assert.Equal("(0,INFINITY)", secondTree.ToString());
            ms.ScheduleJob(secondTree, 3, 3);
            Assert.Equal("(0,0)->(6,INFINITY)", secondTree.ToString());
        }

        [Fact]
        public void TestScheduleJobRemovesNodeAtStart()
        {
            MachineSchedule ms = new(new Global(2, 3, 0));
            RedBlackTree firstTree = ms.freeRangesFirstJob;
            RedBlackTree secondTree = ms.freeRangesSecondJob;

            ms.ScheduleJob(firstTree, 2, 2);
            ms.ScheduleJob(firstTree, 0, 2);
            Assert.Equal("(4,INFINITY)", firstTree.ToString());

            ms.ScheduleJob(secondTree, 6, 3);
            ms.ScheduleJob(secondTree, 2, 3);
            Assert.Equal("(9,INFINITY)", secondTree.ToString());
        }

        [Fact]
        public void TestScheduleJobRemovesNodeInMiddle()
        {
            MachineSchedule ms = new(new Global(2, 3, 0));
            RedBlackTree firstTree = ms.freeRangesFirstJob;
            RedBlackTree secondTree = ms.freeRangesSecondJob;

            // Create hovering node
            ms.ScheduleJob(firstTree, 2, 2);
            ms.ScheduleJob(firstTree, 6, 2);

            // Insert job that sits flush inbetween
            ms.ScheduleJob(firstTree, 4, 2);
            Assert.Equal("(0,0)->(8,INFINITY)", firstTree.ToString());

            // Create hovering node
            ms.ScheduleJob(secondTree, 6, 3);
            ms.ScheduleJob(secondTree, 13, 3);

            // Insert job that does not sit flush
            ms.ScheduleJob(secondTree, 10, 3);
            Assert.Equal("(0,3)->(16,INFINITY)", secondTree.ToString());
        }

        [Fact]
        public void TestScheduleJobAdjustsNodeInMiddle()
        {
            MachineSchedule ms = new(new Global(2, 3, 0));
            RedBlackTree firstTree = ms.freeRangesFirstJob;
            RedBlackTree secondTree = ms.freeRangesSecondJob;

            // Create hovering node
            ms.ScheduleJob(firstTree, 2, 2);
            ms.ScheduleJob(firstTree, 8, 2);

            // Insert job that sits flush inbetween
            ms.ScheduleJob(firstTree, 6, 2);
            Assert.Equal("(0,0)->(4,4)->(10,INFINITY)", firstTree.ToString());

            // Create hovering node
            ms.ScheduleJob(secondTree, 6, 3);
            ms.ScheduleJob(secondTree, 15, 3);

            // Insert job that does not sit flush
            ms.ScheduleJob(secondTree, 9, 3);
            Assert.Equal("(0,3)->(12,12)->(18,INFINITY)", secondTree.ToString());
        }

        [Fact]
        public void TestScheduleMultipleJobs()
        {
            MachineSchedule ms = new(new Global(2, 5, 0));
            RedBlackTree firstTree = ms.freeRangesFirstJob;
            RedBlackTree secondTree = ms.freeRangesSecondJob;

            // Insert into first tree
            ms.ScheduleJob(firstTree, 2, 2);
            Assert.Equal("(0,0)->(4,INFINITY)", firstTree.ToString());
            ms.ScheduleJob(firstTree, 8, 10);
            Assert.Equal("(0,0)->(4,6)->(18,INFINITY)", firstTree.ToString());
            ms.ScheduleJob(firstTree, 5, 1);
            Assert.Equal("(0,0)->(6,6)->(18,INFINITY)", firstTree.ToString());
            ms.ScheduleJob(firstTree, 0, 2);
            Assert.Equal("(6,6)->(18,INFINITY)", firstTree.ToString());

            // Insert into second tree
            ms.ScheduleJob(secondTree, 6, 3);
            Assert.Equal("(0,1)->(9,INFINITY)", secondTree.ToString());
            ms.ScheduleJob(secondTree, 15, 3);
            Assert.Equal("(0,1)->(9,10)->(18,INFINITY)", secondTree.ToString());
            ms.ScheduleJob(secondTree, 9, 3);
            Assert.Equal("(0,1)->(18,INFINITY)", secondTree.ToString());
        }

        [Fact]
        public void TestMultipleSchedules()
        {
            VaccinationScheduling.Shared.Machine.Range range = new(0, 0);
            System.Random random = new System.Random();

            Prop.ForAll(Arb.From(Gen.Choose(0, 7).Four()), valueTuple =>
            {
                MachineSchedule ms = new MachineSchedule(new Global(3, 5, 0));

                int tFirstJob = valueTuple.Item1;
                int tSecondJob = tFirstJob + 3 + valueTuple.Item2;
                int tThirdJob = tSecondJob + 3 + valueTuple.Item3;
                int tFourthJob = tThirdJob + 3 + valueTuple.Item4;

                int count = 1;

                Assert.Equal(count, ms.freeRangesFirstJob.ElementCount);

                ms.ScheduleJob(ms.freeRangesFirstJob, tFirstJob, 3);
                if (valueTuple.Item1 >= 3)
                {
                    count++;
                }
                Assert.Equal(count, ms.freeRangesFirstJob.ElementCount);

                ms.ScheduleJob(ms.freeRangesFirstJob, tSecondJob, 3);
                if (valueTuple.Item2 >= 3)
                {
                    count++;
                }
                Assert.Equal(count, ms.freeRangesFirstJob.ElementCount);

                ms.ScheduleJob(ms.freeRangesFirstJob, tThirdJob, 3);
                if (valueTuple.Item3 >= 3)
                {
                    count++;
                }
                Assert.Equal(count, ms.freeRangesFirstJob.ElementCount);

                ms.ScheduleJob(ms.freeRangesFirstJob, tFourthJob, 3);
                if (valueTuple.Item4 > 3)
                {
                    count++;
                }
                Assert.Equal(count, ms.freeRangesFirstJob.ElementCount);

                return true;

            }).QuickCheckThrowOnFailure();
        }
    }
}
