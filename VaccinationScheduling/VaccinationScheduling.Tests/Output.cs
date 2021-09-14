using System.Collections.Generic;
using VaccinationScheduling.Shared;
using Xunit;
using Xunit.Abstractions;

namespace VaccinationScheduling.Tests
{
    public class Output
    {
        private readonly ITestOutputHelper _output;

        public Output(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void Test1()
        {
            Global global = new(1, 2, 3);
            List<Schedule> schedules = new()
            {
                new Schedule(1, 2, 3, 4),
                new Schedule(2, 3, 4, 5),
                new Schedule(3, 4, 5, 6),
                new Schedule(4, 5, 6, 7),
                new Schedule(2, 2, 5, 4),
                new Schedule(3, 3, 6, 5),
                new Schedule(4, 4, 7, 6),
                new Schedule(5, 5, 8, 7),
            };
            
            string prettySchedule = SchedulePrettier.ToPrettyString(global, schedules);
            _output.WriteLine(prettySchedule);
        }
    }
}