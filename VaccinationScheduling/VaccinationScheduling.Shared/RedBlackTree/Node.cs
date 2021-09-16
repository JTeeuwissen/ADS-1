//******************************
// Written by Peter Golde
// Copyright (c) 2004-2007, Wintellect
//
// Use and restribution of this code is subject to the license agreement
// contained in the file "License.txt" accompanying this file.
//******************************

// License link: https://www.nuget.org/packages/SoftUni.Wintellect.PowerCollections/2.0.0

using System.Diagnostics;

namespace VaccinationScheduling.Shared.RedBlackTree
{
    /// <summary>
    /// The class that is each node in the red-black tree.
    /// </summary>
    public class Node<T>
    {
        public Node<T> left, right;
        public T item;

        private const uint REDMASK = 0x80000000;
        private uint count;

        /// <summary>
        /// Is this a red node?
        /// </summary>
        public bool IsRed
        {
            get { return (count & REDMASK) != 0; }
            set
            {
                if (value)
                    count |= REDMASK;
                else
                    count &= ~REDMASK;
            }
        }

        /// <summary>
        /// Get or set the Count field -- a 31-bit field
        /// that holds the number of nodes at or below this
        /// level.
        /// </summary>
        public int Count
        {
            get { return (int)(count & ~REDMASK); }
            set { count = (count & REDMASK) | (uint)value; }
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
        public Node<T> Clone()
        {
            Node<T> newNode = new Node<T>();
            newNode.item = item;

            newNode.count = count;

            if (left != null)
                newNode.left = left.Clone();

            if (right != null)
                newNode.right = right.Clone();

            return newNode;
        }
    }
}
