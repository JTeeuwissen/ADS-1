using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VaccinationScheduling.Shared;
using Xunit;
using Xunit.Abstractions;
using VaccinationScheduling.Shared.Machine;

namespace VaccinationScheduling.Tests.Shared.Machine
{
    public class TestRange
    {
        [Fact]
        public void TestCompareRangeOfLength1WithInt()
        {
            VaccinationScheduling.Shared.Machine.Range length1 = new(1, 1);
            Assert.True(length1.CompareTo(2) <= -1);
            Assert.True(length1.CompareTo(1) == 0);
            Assert.True(length1.CompareTo(0) >= 1);
        }

        [Fact]
        public void TestCompareRangeLength5WithInt()
        {
            VaccinationScheduling.Shared.Machine.Range range = new(0, 5);
            Assert.True(range.CompareTo(-1) >= 1);
            Assert.True(range.CompareTo(0)  == 0);
            Assert.True(range.CompareTo(2)  == 0);
            Assert.True(range.CompareTo(5)  == 0);
            Assert.True(range.CompareTo(6)  <= -1);
        }

        [Fact]
        public void TestCompareInfiniteRangeWithInt()
        {
            VaccinationScheduling.Shared.Machine.Range rangeInf = new(0, -1);
            Assert.True(rangeInf.CompareTo(-1) >= 1);
            Assert.True(rangeInf.CompareTo(0) == 0);
            Assert.True(rangeInf.CompareTo(1) == 0);
        }

        [Fact]
        public void TestCompareEqualRanges()
        {
            VaccinationScheduling.Shared.Machine.Range range1 = new(1, 5);
            VaccinationScheduling.Shared.Machine.Range range2 = new(1, 5);

            Assert.True(range1.CompareTo(range2) == 0);
            Assert.Equal(range1.CompareTo(range2), range2.CompareTo(range1));
        }

        [Fact]
        public void TestCompareNonOverlappingRanges()
        {
            VaccinationScheduling.Shared.Machine.Range lowerRange = new(0, 5);
            VaccinationScheduling.Shared.Machine.Range higherRange = new(6, 10);

            Assert.True(lowerRange.CompareTo(higherRange) <= -1);
            Assert.True(higherRange.CompareTo(lowerRange) >= 1);
        }

        [Fact]
        public void TestCompareOverlappingRanges()
        {
            VaccinationScheduling.Shared.Machine.Range range1 = new(1, 5);
            VaccinationScheduling.Shared.Machine.Range range2 = new(2, 5);
            VaccinationScheduling.Shared.Machine.Range range3 = new(1, 6);

            Assert.True(range1.CompareTo(range2) == 0);
            Assert.True(range2.CompareTo(range1) == 0);

            Assert.True(range2.CompareTo(range3) == 0);
            Assert.True(range3.CompareTo(range2) == 0);
        }

        [Fact]
        public void TestCompareWithInfiniteRange()
        {
            VaccinationScheduling.Shared.Machine.Range infiniteRange = new(0, -1);
            VaccinationScheduling.Shared.Machine.Range otherRange = new(1, 2);

            Assert.True(infiniteRange.CompareTo(otherRange) == 0);
            Assert.True(otherRange.CompareTo(infiniteRange) == 0);

            Assert.True(infiniteRange.CompareTo(infiniteRange) == 0);
        }

        [Fact]
        public void testFindOverlapNonOverlapping()
        {
            VaccinationScheduling.Shared.Machine.Range range1 = new(1, -1);
            VaccinationScheduling.Shared.Machine.Range range2 = new(0, 0);
            VaccinationScheduling.Shared.Machine.Range range3 = new(4, 10);

            // No overlap
            (int, int) noOverlap = (-1, -1);

            Assert.Equal(noOverlap, range1.GetOverlap(0, 0));

            Assert.Equal(noOverlap, range2.GetOverlap(1, -1));
            Assert.Equal(noOverlap, range2.GetOverlap(1, 100));

            Assert.Equal(noOverlap, range3.GetOverlap(0, 3));
            Assert.Equal(noOverlap, range3.GetOverlap(0, 0));
            Assert.Equal(noOverlap, range3.GetOverlap(11, -1));
            Assert.Equal(noOverlap, range3.GetOverlap(11, 19));
            Assert.Equal(noOverlap, range3.GetOverlap(20, -1));
        }

        [Fact]
        public void testFindOverlapOneOverlap()
        {
            VaccinationScheduling.Shared.Machine.Range range1 = new(1, -1);
            VaccinationScheduling.Shared.Machine.Range range2 = new(0, 0);
            VaccinationScheduling.Shared.Machine.Range range3 = new(4, 10);

            Assert.Equal((1, 1), range1.GetOverlap(1, 1));
            Assert.Equal((1, 1), range1.GetOverlap(0, 1));
            Assert.Equal((2, 2), range1.GetOverlap(2, 2));

            Assert.Equal((0, 0), range2.GetOverlap(0, 0));
            Assert.Equal((0, 0), range2.GetOverlap(0, 10));
            Assert.Equal((0, 0), range2.GetOverlap(0, -1));

            Assert.Equal((4, 4), range3.GetOverlap(1, 4));
            Assert.Equal((4, 4), range3.GetOverlap(4, 4));
            Assert.Equal((4, 4), range3.GetOverlap(0, 4));
            Assert.Equal((5, 5), range3.GetOverlap(5, 5));
            Assert.Equal((10, 10), range3.GetOverlap(10, 10));
            Assert.Equal((10, 10), range3.GetOverlap(10, 14));
            Assert.Equal((10, 10), range3.GetOverlap(10, -1));
        }

        [Fact]
        public void testFindOverlap()
        {
            VaccinationScheduling.Shared.Machine.Range range1 = new(0, -1);
            VaccinationScheduling.Shared.Machine.Range range2 = new(10, -1);
            VaccinationScheduling.Shared.Machine.Range range3 = new(0, 10);
            VaccinationScheduling.Shared.Machine.Range range4 = new(10, 20);

            Assert.Equal((0, -1), range1.GetOverlap(0, -1));
            Assert.Equal((0, 10), range1.GetOverlap(0, 10));
            Assert.Equal((1, 10), range1.GetOverlap(1, 10));

            Assert.Equal((10, 11), range2.GetOverlap(0, 11));
            Assert.Equal((10, -1), range2.GetOverlap(10, -1));
            Assert.Equal((10, -1), range2.GetOverlap(0, -1));
            Assert.Equal((20, -1), range2.GetOverlap(20, -1));

            Assert.Equal((0, 10), range3.GetOverlap(0, -1));
            Assert.Equal((0, 10), range3.GetOverlap(0, 11));
            Assert.Equal((0, 5), range3.GetOverlap(0, 5));
            Assert.Equal((4, 5), range3.GetOverlap(4, 5));

            Assert.Equal((10, 20), range4.GetOverlap(0, -1));
            Assert.Equal((10, 20), range4.GetOverlap(10, 20));
            Assert.Equal((10, 20), range4.GetOverlap(10, -1));
            Assert.Equal((10, 20), range4.GetOverlap(5, 25));
            Assert.Equal((15, 16), range4.GetOverlap(15, 16));
        }
    }
}
