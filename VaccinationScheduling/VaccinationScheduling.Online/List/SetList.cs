using System;
using System.Collections.Generic;
using System.Text;
using VaccinationScheduling.Shared;

namespace VaccinationScheduling.Online.List
{
    public class SetList
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

        public void Add(int machineNr)
        {
            int listIndex = machineNr / 32;
            int bitIndex = machineNr % 32;

            while (listIndex >= Set.Count)
            {
                Set.Add(0);
            }

            if ((Set[listIndex] & masks[bitIndex]) == 0)
            {
                Set[listIndex] += (uint)1 << bitIndex;
                Count++;
                // Extensions.WriteDebugLine($"Added {machineNr}:" + ToString());
            }
        }

        public bool Contains(int i)
        {
            if (i >= Set.Count * 32)
            {
                return false;
            }

            int listIndex = i / 32;
            int bitIndex = i % 32;
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

        public bool AreEqual(SetList setList)
        {
            if (Set.Count != setList.Set.Count)
            {
                return false;
            }
            for (int i = 0; i < Set.Count; i++)
            {
                if (Set[i] != setList.Set[i])
                {
                    return false;
                }
            }
            return true;
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
    }
}
