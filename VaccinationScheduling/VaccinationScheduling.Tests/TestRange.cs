using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VaccinationScheduling.Shared;
using Xunit;
using Xunit.Abstractions;
using VaccinationScheduling.Shared.Machine;

namespace VaccinationScheduling.Tests
{
    public class TestRange
    {
        [Fact]
        public void TestCompareRangeOfLength1WithInt()
        {
            Shared.Machine.Range length1 = new(1, 1);
            Assert.True(length1.CompareTo(2) <= -1);
            Assert.True(length1.CompareTo(1) == 0);
            Assert.True(length1.CompareTo(0) >= 1);
        }

        [Fact]
        public void TestCompareRangeLength5WithInt()
        {
            Shared.Machine.Range range = new(0, 5);
            Assert.True(range.CompareTo(-1) >= 1);
            Assert.True(range.CompareTo(0)  == 0);
            Assert.True(range.CompareTo(2)  == 0);
            Assert.True(range.CompareTo(5)  == 0);
            Assert.True(range.CompareTo(6)  <= -1);
        }

        [Fact]
        public void TestCompareInfiniteRangeWithInt()
        {
            Shared.Machine.Range rangeInf = new(0, -1);
            Assert.True(rangeInf.CompareTo(-1) >= 1);
            Assert.True(rangeInf.CompareTo(0) == 0);
            Assert.True(rangeInf.CompareTo(1) == 0);
        }

        [Fact]
        public void TestCompareEqualRanges()
        {
            Shared.Machine.Range range1 = new(1, 5);
            Shared.Machine.Range range2 = new(1, 5);

            Assert.True(range1.CompareTo(range2) == 0);
            Assert.Equal(range1.CompareTo(range2), range2.CompareTo(range1));
        }

        [Fact]
        public void TestCompareNonOverlappingRanges()
        {
            Shared.Machine.Range lowerRange = new(0, 5);
            Shared.Machine.Range higherRange = new(6, 10);

            Assert.True(lowerRange.CompareTo(higherRange) <= -1);
            Assert.True(higherRange.CompareTo(lowerRange) >= 1);
        }

        [Fact]
        public void TestCompareOverlappingRanges()
        {
            Shared.Machine.Range range1 = new(1, 5);
            Shared.Machine.Range range2 = new(2, 5);

            Assert.True(range1.CompareTo(range2) == 0);
            Assert.True(range2.CompareTo(range1) == 0);

            Shared.Machine.Range range3 = new(1, 6);
            Assert.True(range2.CompareTo(range3) == 0);
            Assert.True(range3.CompareTo(range2) == 0);
        }

        [Fact]
        public void TestCompareWithInfiniteRange()
        {
            Shared.Machine.Range infiniteRange = new(0, -1);
            Shared.Machine.Range otherRange = new(1, 2);

            Assert.True(infiniteRange.CompareTo(otherRange) == 0);
            Assert.True(otherRange.CompareTo(infiniteRange) == 0);

            Assert.True(infiniteRange.CompareTo(infiniteRange) == 0);
        }
    }
}
