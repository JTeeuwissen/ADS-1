using System;
using System.Collections.Generic;
using System.Text;

namespace VaccinationScheduling.Online.List
{
    /// <summary>
    /// Implements a set using list of uintegers to speed up comparing sets.
    /// The assumption that the number of items will go up from 0 to x and does not have a lot of gaps makes it so we can contain all items in an array.
    /// For quick checks we use uints instead of arrays. Making use of bitwise operations to check equality etc, which makes it really fast.
    /// </summary>
    public class CustomSet : IEquatable<CustomSet>
    {
        // https://stackoverflow.com/questions/10453256/fast-way-to-find-a-intersection-between-two-sets-of-numbers-one-defined-by-a-bi
        // Method of finding the first: https://stackoverflow.com/questions/21279844/how-to-find-the-first-bit-that-is-different-in-c

        public List<uint> Set = new List<uint>();
        public int Count = 0;
        public int FirstItemNotContained = 0;

        // Masks to extract single bits from an integer
        static readonly uint[] masks = new uint[32]
        {
            1 << 0, 1 << 1, 1 << 2, 1 << 3, 1 << 4, 1 << 5, 1 << 6, 1 << 7, 1 << 8, 1 << 9,
            1 << 10, 1 << 11, 1 << 12, 1 << 13, 1 << 14, 1 << 15, 1 << 16, 1 << 17, 1 << 18, 1 << 19,
            1 << 20, 1 << 21, 1 << 22, 1 << 23, 1 << 24, 1 << 25, 1 << 26, 1 << 27, 1 << 28, 1 << 29,
            1 << 30, (uint)1 << 31,
        };

        public CustomSet() { }

        public CustomSet(int i)
        {
            Add(i);
        }

        public CustomSet(List<uint> set, int count, int firstItemNotContained)
        {
            Set = set;
            Count = count;
            FirstItemNotContained = firstItemNotContained;
        }

        // Sources listed above, used to find which bit index is different
        static readonly int[] MultiplyDeBruijnBitPosition = new int[] {
            0, 1, 28, 2, 29, 14, 24, 3, 30, 22, 20, 15, 25, 17, 4, 8,
            31, 27, 13, 23, 21, 19, 16, 7, 26, 12, 18, 6, 11, 5, 10, 9
        };

        /// <summary>
        /// Add a machine number to the set.
        /// </summary>
        /// <param name="machineNr">Machine to add</param>
        public void Add(int machineNr)
        {
            int listIndex = machineNr / 32;
            int bitIndex = machineNr % 32;

            // Extend set if it is not big enough yet
            while (listIndex >= Set.Count)
            {
                Set.Add(0);
            }

            // If number is not yet added add it to the set
            if ((Set[listIndex] & masks[bitIndex]) == 0)
            {
                Set[listIndex] += (uint)1 << bitIndex;
                Count++;
                if (FirstItemNotContained == machineNr)
                {
                    UpdateFirstItemNotContained();
                }
            }
        }

        /// <summary>
        /// Checks whether a machine number is contained in the set.
        /// </summary>
        /// <param name="machineNr">Number to find in the set</param>
        /// <returns>Whether or not the number is contained in the </returns>
        public bool Contains(int machineNr)
        {
            if (machineNr >= Set.Count * 32)
            {
                return false;
            }

            int listIndex = machineNr / 32;
            int bitIndex = machineNr % 32;
            return (Set[listIndex] & masks[bitIndex]) != 0;
        }

        /// <summary>
        /// Searches for an item which is present in both neighbours but not in the current set.
        /// </summary>
        /// <param name="leftSet">The first set that neighbours the current one</param>
        /// <param name="rightSet">The second set that neighbours the current one.</param>
        /// <returns>The first item that is in both neighbours but not in the current item. Or null</returns>
        public int? FindUniqueItemInBothNeighbours(CustomSet leftSet, CustomSet rightSet)
        {
            // Maximum index that both neighbouring sets have.
            int maxSurroundingIndex = Math.Min(leftSet.Set.Count, rightSet.Set.Count);
            // Maximum index of all 3 sets.
            int maxCommonIndex = Math.Min(maxSurroundingIndex, Set.Count);
            // Go through each set
            for (int i = 0; i < maxCommonIndex; i++)
            {
                // Current set overlaps with one neighbour, means there is no unique index in both neighbours.
                if (leftSet.Set[i] == Set[i] || rightSet.Set[i] == Set[i])
                {
                    continue;
                }
                // Takes union of both sets and checks whether there is an item in both left and right but NOT in current.
                uint union = leftSet.Set[i] & rightSet.Set[i];
                uint c = union & (~Set[i]);
                // There is no value that is in both neighbours but not in the current.
                if (c == 0)
                {
                    continue;
                }
                // return the item.
                return i * 32 + getIndexOfRightMost1(c);
            }
            // The neighbours have more items than the current. So check if there is an overlapping item.
            for (int i = maxCommonIndex; i < maxSurroundingIndex; i++)
            {
                uint union = leftSet.Set[i] & rightSet.Set[i];
                // There is no value that is in both neighbours but not in the current
                if (union == 0)
                {
                    continue;
                }
                return i * 32 + getIndexOfRightMost1(union);
            }

            // There is no difference
            return null;
        }

        /// <summary>
        /// Finds the first unique item that is in the othe set but not in the current
        /// </summary>
        /// <param name="other">Set to compare with</param>
        /// <returns>Int if there is a unique value in the other set. Otherwise null</returns>
        public int? FindFirstUniqueInOtherSet(CustomSet other)
        {
            // Max index that both have.
            int maxCommonIndex = Math.Min(Set.Count, other.Set.Count);
            for (int i = 0; i < maxCommonIndex; i++)
            {
                uint c = other.Set[i] & (~Set[i]);
                // There is no value that is in both neighbours but not in the current.
                if (c == 0)
                {
                    continue;
                }
                return i * 32 + getIndexOfRightMost1(c);
            }
            // There is no index not in the other list
            if (maxCommonIndex == other.Set.Count)
            {
                return null;
            }
            // There is a difference so return the value.
            return maxCommonIndex * 32 + getIndexOfRightMost1(other.Set[maxCommonIndex]);
        }

        /// <summary>
        /// Finds first item that is not cointained in the current set.
        /// </summary>
        /// <returns></returns>
        private void UpdateFirstItemNotContained()
        {
            // Loop through the entire 'set'
            for (int i = 0; i < Set.Count; i++)
            {
                // Not all items are occupied (1's), find the first 0
                if (Set[i] != uint.MaxValue)
                {
                    uint c = Set[i] ^ uint.MaxValue;
                    int result = getIndexOfRightMost1(c);
                    FirstItemNotContained = i * 32 + result;
                }
            }
            // The first machine that is not contained is the maximum value.
            FirstItemNotContained = Count;
        }

        /// <summary>
        /// Gets the index of the first 1 in an uint
        /// </summary>
        /// <param name="c"></param>
        /// <returns>The index of the least significant 1 in the int</returns>
        private int getIndexOfRightMost1(uint c)
        {
            return MultiplyDeBruijnBitPosition[((UInt32)((c & -c) * 0x077CB531U)) >> 27];
        }

        /// <summary>
        /// Clone the current set
        /// </summary>
        /// <returns>The cloned set</returns>
        public CustomSet Clone()
        {
            return new CustomSet(new List<uint>(Set), Count, FirstItemNotContained);
        }

        /// <summary>
        /// Convert the set to a string containing all the numbers that are not contained
        /// </summary>
        /// <returns>A string representing the current object.</returns>
        public override string ToString()
        {
            // Use stringbuilder so concatinating strings is cheap.
            StringBuilder sb = new StringBuilder();
            for (int itemIndex = 0; itemIndex < Set.Count; itemIndex++)
            {
                uint item = Set[itemIndex];
                for (int i = 0; i < 32; i++)
                {
                    if ((item & masks[i]) != 0)
                    {
                        sb.Append(itemIndex * 32 + i + ",");
                    }
                }
            }
            // There is no item contained in the set.
            if (sb.Length == 0)
            {
                return "";
            }

            return " Not(" + sb.ToString() + ")";
        }

        /// <summary>
        /// Implements IEquatable interface. Making it possible to check whether current Setlist is the same as the other item.
        /// </summary>
        /// <param name="other">other set to compare with</param>
        /// <returns>true when both setlists are equal, otherwise false.</returns>
        public bool Equals(CustomSet? other)
        {
            if (other == null)
            {
                return false;
            }
            if (Set.Count != other.Set.Count)
            {
                return false;
            }
            for (int i = 0; i < Set.Count; i++)
            {
                if (Set[i] != other.Set[i])
                {
                    return false;
                }
            }
            return true;
        }
    }
}
