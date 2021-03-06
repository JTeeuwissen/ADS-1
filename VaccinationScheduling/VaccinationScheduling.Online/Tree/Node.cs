//******************************
// Written by Peter Golde
// Copyright (c) 2004-2007, Wintellect
//
// Use and restribution of this code is subject to the license agreement
// contained in the file "License.txt" accompanying this file.
//******************************

// License link: https://www.nuget.org/packages/SoftUni.Wintellect.PowerCollections/2.0.0

using System.Diagnostics;

namespace VaccinationScheduling.Online.Tree
{
    /// <summary>
    /// The class that is each node in the red-black tree.
    /// </summary>
    public class Node
    {
        public Node? left, right;
        public Range item;

        private const uint REDMASK = 0x80000000;
        private uint count;

        /// <summary>
        /// Is this a red node?
        /// </summary>
        public bool IsRed
        {
            get => (count & REDMASK) != 0;
            set
            {
                if (value)
                    count |= REDMASK;
                else
                    count &= ~REDMASK;
            }
        }

        public Node(Range range)
        {
            item = range;
        }

        /// <summary>
        /// Get or set the Count field -- a 31-bit field
        /// that holds the number of nodes at or below this
        /// level.
        /// </summary>
        public int Count
        {
            get => (int)(count & ~REDMASK);
            set => count = (count & REDMASK) | (uint)value;
        }

        /// <summary>
        /// Add one to the Count.
        /// </summary>
        public void IncrementCount()
        {
            ++count;
        }

        /// <summary>
        /// Subtract one from the Count. The current
        /// Count must be non-zero.
        /// </summary>
        public void DecrementCount()
        {
            Debug.Assert(Count != 0);
            --count;
        }

        /// <summary>
        /// Clones a node and all its descendants.
        /// </summary>
        /// <returns>The cloned node.</returns>
        public Node Clone()
        {
            Node newNode = new(item) { count = count };

            if (left != null)
                newNode.left = left.Clone();

            if (right != null)
                newNode.right = right.Clone();

            return newNode;
        }
    }
}
