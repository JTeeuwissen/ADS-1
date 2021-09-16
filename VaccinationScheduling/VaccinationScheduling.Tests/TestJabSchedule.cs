using System.Collections.Generic;
using System.Linq;
using VaccinationScheduling.Shared;
using VaccinationScheduling.Shared.JabSchedule;
using Xunit;

namespace VaccinationScheduling.Tests
{
    public class TestJabSchedule
    {
        [Fact]
        public void Test1()
        {
            Global global = new(2, 1, default);

            JabSchedule jabSchedule = new(global);

            jabSchedule.Add(JabEnum.JabOne, (10, 10), new Job(global,1, 2, 3, 4));
            jabSchedule.Add(JabEnum.JabOne, (20, 20), new Job(global, 2, 3, 4, 5));
            jabSchedule.Add(JabEnum.JabOne, (40, 40), new Job(global, 3, 4, 5, 6));
            jabSchedule.Add(JabEnum.JabOne, (50, 50), new Job(global, 4, 5, 6, 7));

            // Do not overlap
            Assert.False(jabSchedule.Contains((19, 19)));
            Assert.False(jabSchedule.Contains((21, 21)));
            Assert.False(jabSchedule.Contains((21, 39)));

            // Do overlap
            Assert.True(jabSchedule.Contains((39, 40)));
            Assert.True(jabSchedule.Contains((40, 40)));
            Assert.True(jabSchedule.Contains((40, 41)));

            // Lengths
            Assert.Empty(jabSchedule.Get((11, 19)));

            // First jab
            IEnumerable<Job> single = jabSchedule.Get((10, 39)).ToArray();
            Assert.Single(single);
            Assert.Equal(1, single.Single().MaxFirstIntervalStart);

            // Third jab
            IEnumerable<Job> single2 = jabSchedule.Get((40, 49)).ToArray();
            Assert.Single(single2);
            Assert.Equal(3, single2.Single().MaxFirstIntervalStart);

            // 20-20 is not in this collection due to the request being bigger than the jab size
            Assert.Equal(2, jabSchedule.Get((10, 40)).Count()); 
        }
    }
}