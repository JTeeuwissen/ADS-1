using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace VaccinationScheduling.Shared.Machine
{
    public partial class RedBlackTree : IEnumerable<Range>
    {
        public int JobLength = 0;

        /// <summary>
        /// Finds the key in the tree. If multiple items in the tree have
        /// compare equal to the key, finds the first or last one. Optionally replaces the item
        /// with the one searched for.
        /// </summary>
        /// <param name="key">Key to search for.</param>
        /// <param name="item">Returns the found item, before replacing (if function returns true).</param>
        /// <returns>True if the key was found.</returns>
        public bool Find(int key, out Range item)
        {
            Node current = root;      // current search location in the tree
            Node found = null;      // last node found with the key, or null if none.

            while (current != null)
            {
                int compare = current.item.CompareTo(key);

                if (compare > 0)
                {
                    current = current.left;
                }
                else if (compare < 0)
                {
                    current = current.right;
                }
                else
                {
                    found = current;
                    break;
                }
            }

            if (found != null)
            {
                item = found.item;
                return true;
            }
            else
            {
                item = default(Range);
                return false;
            }
        }

        /// <summary>
        /// Finds the key in the tree. If the key was not found it returns the previous items
        /// </summary>
        /// <param name="key">Key to search for.</param>
        /// <param name="item">Returns the found item, which is the previous item in case if the key was not found</param>
        /// <returns>True if the key was found. False if it returns the item previous to the searched key</returns>
        public bool FindOrPrevious(int key, out Range item)
        {
            Node current = root;      // current search location in the tree
            Node found = null;      // last node found with the key, or null if none.
            Node previous = root;

            while (current != null)
            {
                int compare = current.item.CompareTo(key);

                if (compare > 0)
                {
                    current = current.left;
                }
                else if (compare < 0)
                {
                    previous = current;
                    current = current.right;
                }
                else
                {
                    found = current;
                    break;
                }
            }

            if (found != null)
            {
                item = found.item;
                return true;
            }
            else
            {
                item = previous.item;
                return false;
            }
        }

        /// <summary>
        /// Gets a range tester that defines a range by first and last items.
        /// </summary>
        /// <param name="first">The lower bound.</param>
        /// <param name="last">The upper bound.</param>
        /// <returns>A RangeTester delegate that tests for an item in the given range.</returns>
        public RangeTester DoubleBoundedRangeTester(int first, int last)
        {
            return delegate (Range item)
            {
                if (item.CompareTo(first) < 0)
                    return -1;     // item is before or equal to first.

                if (last == -1)
                    return 0;

                if (item.CompareTo(last) > 0)
                    return 1;      // item is after or equal to last

                return 0;      // item is between first and last.
            };
        }

        /// <summary>
        /// Inclusive enumeratable of the given range. Enumerates the items in order.
        /// </summary>
        /// <param name="first">Left bound of the enumerate range</param>
        /// <param name="last">Right bound of the enumerate range</param>
        /// <returns>Enumerable that can be used in a foreach loop</returns>
        public IEnumerable<Range> EnumerateRange(int first, int last)
        {
            RangeTester rangeTester = DoubleBoundedRangeTester(first, last);
            return EnumerateRange(rangeTester);
        }

        /// <summary>
        /// Insert a new node into the tree, maintaining the red-black invariants.
        /// </summary>
        /// <remarks>Algorithm from Sedgewick, "Algorithms".</remarks>
        /// <param name="item">The new item to insert</param>
        /// <returns>false if duplicate exists, otherwise true.</returns>
        public void Insert(Range item)
        {
            Node node = root;
            Node parent = null, gparent = null, ggparent = null;  // parent, grand, a great-grantparent of node.
            bool wentLeft = false, wentRight = false;        // direction from parent to node.
            bool rotated;

            // The tree may be changed.
            StopEnumerations();

            while (node != null)
            {
                // If we find a node with two red children, split it so it doesn't cause problems
                // when inserting a node.
                if (node.left != null && node.left.IsRed && node.right != null && node.right.IsRed)
                {
                    node = InsertSplit(ggparent, gparent, parent, node, out rotated);
                }

                // Keep track of parent, grandparent, great-grand parent.
                ggparent = gparent; gparent = parent; parent = node;

                // Compare the key and the node.
                int compare = item.CompareTo(node.item);

                // Overlapping ranges are not allowed!
                Debug.Assert(compare != 0);

                node.IncrementCount();

                // Move to the left or right as needed to find the insertion point.
                if (compare < 0)
                {
                    node = node.left;
                    wentLeft = true; wentRight = false;
                }
                else
                {
                    node = node.right;
                    wentRight = true; wentLeft = false;
                }
            }

            // Create a new node.
            node = new Node();
            node.item = item;
            node.Count = 1;

            // Link the node into the tree.
            if (wentLeft)
                parent.left = node;
            else if (wentRight)
                parent.right = node;
            else
            {
                Debug.Assert(root == null);
                root = node;
            }

            // Maintain the red-black policy.
            InsertSplit(ggparent, gparent, parent, node, out rotated);

            // We've added a node to the tree, so update the count.
            count += 1;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (Range range in this)
            {
                sb.Append(range.ToString());
                if (range.End != -1)
                {
                    sb.Append("->");
                }
            }

            return sb.ToString();
        }
    }
}
