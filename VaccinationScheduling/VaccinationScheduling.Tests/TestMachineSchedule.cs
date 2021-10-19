using VaccinationScheduling.Online;
using Xunit;
using VaccinationScheduling.Shared;

namespace VaccinationScheduling.Tests
{
    public class TestMachineSchedule
    {
        // 927, 932, 0, 8
        [Fact]
        public void TestPreviousBug()
        {
            Global global = new(2, 5, 0);
            Machine machine = new(global);
            ScheduleToBoth(machine, 929, 25);
            ScheduleToBoth(machine, 957, 15);
            ScheduleToBoth(machine, 976, 15);
            ScheduleToBoth(machine, 994, 7);

            Assert.Equal(
                "(0,927)->(954,955)->(972,974)->(991,992)->(1001,INFINITY)",
                machine.freeRangesFirstJob.ToString()
            );
            Assert.Equal("(0,931)->(1001,INFINITY)", machine.freeRangesSecondJob.ToString());
        }

        [Fact]
        public void PreviousBug()
        {
            Global global = new(2, 5, 0);
            Machine machine = new(global);
            ScheduleToBoth(machine, 929, 25);
            ScheduleToBoth(machine, 957, 15);
            ScheduleToBoth(machine, 976, 15);
            ScheduleToBoth(machine, 994, 7);

            //Assert.Equal("(0,927)->(954,INFINITY)", machine.freeRangesFirstJob.ToString());
            Assert.Equal("(0,0)", machine.freeRangesSecondJob.ToString());
        }

        private void ScheduleToBoth(Machine ms, int tJobStart, int jobLength)
        {
            ms.ScheduleJob(ms.freeRangesFirstJob, tJobStart, jobLength);
            ms.ScheduleJob(ms.freeRangesSecondJob, tJobStart, jobLength);
        }

        // (0,927)->(954,955)->(972,974)->(991,992)->(1001,INFINITY)
        // (0,931)->(1001,INFINITY)
        // Added to both trees!
        // (0,927)->(954,955)->(972,974)->(991,992)->(1001,INFINITY)
        // (0,922)->(1001,INFINITY)
    }
}
