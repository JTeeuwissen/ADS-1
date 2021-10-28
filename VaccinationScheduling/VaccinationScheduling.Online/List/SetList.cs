using System;
using System.Collections.Generic;
using System.Text;

namespace VaccinationScheduling.Online.List
{
    public class SetList : IEquatable<SetList>
    {
        // https://stackoverflow.com/questions/10453256/fast-way-to-find-a-intersection-between-two-sets-of-numbers-one-defined-by-a-bi
        // Method of finding the first: https://stackoverflow.com/questions/21279844/how-to-find-the-first-bit-that-is-different-in-c

        public List<uint> Set = new List<uint>();
        public int Count = 0;

        // Masks to extract single bits
        static readonly uint[] masks = new uint[32]
        {
            1 << 0, 1 << 1, 1 << 2, 1 << 3, 1 << 4, 1 << 5, 1 << 6, 1 << 7, 1 << 8, 1 << 9,
            1 << 10, 1 << 11, 1 << 12, 1 << 13, 1 << 14, 1 << 15, 1 << 16, 1 << 17, 1 << 18, 1 << 19,
            1 << 20, 1 << 21, 1 << 22, 1 << 23, 1 << 24, 1 << 25, 1 << 26, 1 << 27, 1 << 28, 1 << 29,
            1 << 30, (uint)1 << 31,
        };

        public SetList() { }

        public SetList(int i)
        {
            Add(i);
        }

        public SetList(List<uint> set, int count)
        {
            Set = set;
            Count = count;
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

        public (bool, int?) FindFirstDifference(SetList setList)
        {
            int maxCommonIndex = Math.Min(Count, setList.Count) - 1;
            for (int i = 0; i <= maxCommonIndex; i++)
            {
                // The two are different
                if (Set[i] != setList.Set[i])
                {
                    uint c = Set[i] ^ setList.Set[i];
                    int result = getIndex(c);
                    bool isCurrentList = (Set[i] & masks[result]) != 0;
                    return (isCurrentList, i * 32 + result);
                }
            }
            // No difference found
            if (Count == setList.Count)
            {
                return (false, null);
            }
            // The difference is in the length of the lists
            if (Count > maxCommonIndex)
            {
                uint c = Set[maxCommonIndex + 1];
                int result = getIndex(c);
                return (true, (maxCommonIndex + 1) * 32 + result);
            }
            else
            {
                uint c = setList.Set[maxCommonIndex + 1];
                int result = getIndex(c);
                return (false, (maxCommonIndex + 1) * 32 + result);
            }
        }

        public int? FindItemInBothNeighbours(SetList leftSet, SetList rightSet)
        {
            int maxSurroundingIndex = Math.Min(leftSet.Set.Count, rightSet.Set.Count);
            int maxCommonIndex = Math.Min(maxSurroundingIndex, Set.Count);
            for (int i = 0; i < maxCommonIndex; i++)
            {
                if (leftSet.Set[i] == Set[i] || rightSet.Set[i] == Set[i])
                {
                    continue;
                }
                uint union = leftSet.Set[i] & rightSet.Set[i];
                uint c = union & (~Set[i]);
                // There is no value that is in both neighbours but not in the current
                if (c == 0)
                {
                    continue;
                }
                return i * 32 + getIndex(c);
            }
            for (int i = maxCommonIndex; i < maxSurroundingIndex; i++)
            {
                uint union = leftSet.Set[i] & rightSet.Set[i];
                // There is no value that is in both neighbours but not in the current
                if (union == 0)
                {
                    continue;
                }
                return i * 32 + getIndex(union);
            }

            return null;
        }

        public int? FindFirstUniqueInOtherSet(SetList other)
        {
            int maxCommonIndex = Math.Min(Set.Count, other.Set.Count);
            for (int i = 0; i < maxCommonIndex; i++)
            {
                uint c = other.Set[i] & (~Set[i]);
                // There is no value that is in both neighbours but not in the current
                if (c == 0)
                {
                    continue;
                }
                return i * 32 + getIndex(c);
            }
            // There is no index not in the other list
            if (maxCommonIndex == other.Set.Count)
            {
                return null;
            }
            return maxCommonIndex * 32 + getIndex(other.Set[maxCommonIndex]);
        }

        public int FindFirstNotContained()
        {
            // Loop through the entire 'set'
            for (int i = 0; i < Set.Count; i++)
            {
                // An item is not contained in the current 32
                if (Set[i] != uint.MaxValue)
                {
                    uint c = Set[i] ^ uint.MaxValue;
                    int result = getIndex(c);
                    //Extensions.WriteDebugLine("Found non used machine:" + result);
                    return i * 32 + result;
                }
            }
            // The first machine that is not contained
            return Count;
        }

        private int getIndex(uint c)
        {
            return MultiplyDeBruijnBitPosition[((UInt32)((c & -c) * 0x077CB531U)) >> 27];
        }

        // Clone the set
        public SetList Clone()
        {
            return new SetList(new List<uint>(Set), Count);
        }

        // Convert the set to a string containing all the numbers that are not contained
        public override string ToString()
        {
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
            if (sb.Length == 0)
            {
                return "";
            }

            return " Not(" + sb.ToString() + ")";
        }

        public bool Equals(SetList? other)
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
