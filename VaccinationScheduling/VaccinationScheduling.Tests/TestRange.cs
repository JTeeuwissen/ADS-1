using VaccinationScheduling.Online.Machine;
using Xunit;

namespace VaccinationScheduling.Tests
{
    public class TestRange
    {
        [Fact]
        public void TestCompareRangeOfLength1WithInt()
        {
            Range length1 = new(1, 1);
            Assert.True(length1.CompareTo(2) <= -1);
            Assert.True(length1.CompareTo(1) == 0);
            Assert.True(length1.CompareTo(0) >= 1);
        }

        [Fact]
        public void TestCompareRangeLength5WithInt()
        {
            Range range = new(0, 5);
            Assert.True(range.CompareTo(-1) >= 1);
            Assert.True(range.CompareTo(0)  == 0);
            Assert.True(range.CompareTo(2)  == 0);
            Assert.True(range.CompareTo(5)  == 0);
            Assert.True(range.CompareTo(6)  <= -1);
        }

        [Fact]
        public void TestCompareInfiniteRangeWithInt()
        {
            Range rangeInf = new(0, -1);
            Assert.True(rangeInf.CompareTo(-1) >= 1);
            Assert.True(rangeInf.CompareTo(0) == 0);
            Assert.True(rangeInf.CompareTo(1) == 0);
        }

        [Fact]
        public void TestCompareEqualRanges()
        {
            Range range1 = new(1, 5);
            Range range2 = new(1, 5);

            Assert.True(range1.CompareTo(range2) == 0);
            Assert.Equal(range1.CompareTo(range2), range2.CompareTo(range1));
        }

        [Fact]
        public void TestCompareNonOverlappingRanges()
        {
            Range lowerRange = new(0, 5);
            Range higherRange = new(6, 10);

            Assert.True(lowerRange.CompareTo(higherRange) <= -1);
            Assert.True(higherRange.CompareTo(lowerRange) >= 1);
        }

        [Fact]
        public void TestCompareOverlappingRanges()
        {
            Range range1 = new(1, 5);
            Range range2 = new(2, 5);

            Assert.True(range1.CompareTo(range2) == 0);
            Assert.True(range2.CompareTo(range1) == 0);

            Range range3 = new(1, 6);
            Assert.True(range2.CompareTo(range3) == 0);
            Assert.True(range3.CompareTo(range2) == 0);
        }

        [Fact]
        public void TestCompareWithInfiniteRange()
        {
            Range infiniteRange = new(0, -1);
            Range otherRange = new(1, 2);

            Assert.True(infiniteRange.CompareTo(otherRange) == 0);
            Assert.True(otherRange.CompareTo(infiniteRange) == 0);

            Assert.True(infiniteRange.CompareTo(infiniteRange) == 0);
        }
    }
}
