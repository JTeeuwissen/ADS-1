using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace VaccinationScheduling.Online.Machine
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
        public bool Find(int key, [MaybeNullWhen(false)] out Range item)
        {
            Node? current = root; // current search location in the tree
            Node? found = null; // last node found with the key, or null if none.

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

            item = new Range(-1, -1);
            return false;
        }

        /// <summary>
        /// Finds the key in the tree. If the key was not found it returns the previous items
        /// </summary>
        /// <param name="key">Key to search for.</param>
        /// <param name="item">Returns the found item, which is the previous item in case if the key was not found</param>
        /// <returns>True if the key was found. False if it returns the item previous to the searched key</returns>
        public bool FindOrPrevious(int key, out Range item)
        {
            Node? current = root; // current search location in the tree
            Node? found = null; // last node found with the key, or null if none.
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

            item = previous.item;
            return false;
        }

        /// <summary>
        /// Gets a range tester that defines a range by first and last items.
        /// </summary>
        /// <param name="first">The lower bound.</param>
        /// <param name="last">The upper bound.</param>
        /// <returns>A RangeTester delegate that tests for an item in the given range.</returns>
        private RangeTester DoubleBoundedRangeTester(int first, int last)
        {
            return delegate(Range item)
            {
                if (item.CompareTo(first) < 0)
                    return -1; // item is before or equal to first.

                if (last == -1)
                    return 0;

                // ReSharper disable once ConvertIfStatementToReturnStatement
                if (item.CompareTo(last) > 0)
                    return 1; // item is after or equal to last

                return 0; // item is between first and last.
            };
        }

        /// <summary>
        /// Inclusive enumerable of the given range. Enumerates the items in order.
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
        /// Inclusive enumerable of the given range. Enumerates the items in order.
        /// </summary>
        /// <param name="first">Left bound of the enumerate range</param>
        /// <param name="last">Right bound of the enumerate range</param>
        /// <returns>Enumerable that can be used in a foreach loop</returns>
        public IEnumerable<Range> FastEnumerateRange(int first, int last)
        {
            RangeTester rangeTester = DoubleBoundedRangeTester(first, last);
            return FastEnumerateRangeInOrder(rangeTester, root);
        }

        /// <summary>
        /// Insert a new node into the tree, maintaining the red-black invariants.
        /// </summary>
        /// <remarks>Algorithm from Sedgewick, "Algorithms".</remarks>
        /// <param name="item">The new item to insert</param>
        /// <returns>false if duplicate exists, otherwise true.</returns>
        public void Insert(Range item)
        {
            Node? node = root;
            Node? parent = null, gparent = null, ggparent = null; // parent, grand, a great-grandparent of node.
            bool wentLeft = false, wentRight = false; // direction from parent to node.
            bool rotated; //TODO gerard, why is dit?

            // The tree may be changed.
            StopEnumerations();

            while (node != null)
            {
                // If we find a node with two red children, split it so it doesn't cause problems
                // when inserting a node.
                if (node.left is { IsRed: true } && node.right is { IsRed: true })
                {
                    node = InsertSplit(ggparent, gparent, parent, node, out rotated);
                }

                // Keep track of parent, grandparent, great-grand parent.
                ggparent = gparent;
                gparent = parent;
                parent = node;

                // Compare the key and the node.
                int compare = item.CompareTo(node.item);

                // Overlapping ranges are not allowed!
                //Debug.Assert(compare != 0);

                node.IncrementCount();

                // Move to the left or right as needed to find the insertion point.
                if (compare < 0)
                {
                    node = node.left;
                    wentLeft = true;
                    wentRight = false;
                }
                else
                {
                    node = node.right;
                    wentRight = true;
                    wentLeft = false;
                }
            }

            // Create a new node.
            node = new Node { item = item, Count = 1 };

            // Link the node into the tree.
            if (wentLeft)
                parent.left = node;
            else if (wentRight)
                parent.right = node;
            else
                root = node;

            // Maintain the red-black policy.
            InsertSplit(ggparent, gparent, parent, node, out rotated);

            // We've added a node to the tree, so update the count.
            count += 1;
        }

        /// <summary>
        /// Enumerate all the items in a custom range, under and including node, in-order.
        /// </summary>
        /// <param name="rangeTester">Tests an item against the custom range.</param>
        /// <param name="node">Node to begin enumeration. May be null.</param>
        /// <returns>An enumerable of the items.</returns>
        /// <exception cref="InvalidOperationException">The tree has an item added or deleted during the enumeration.</exception>
        private IEnumerable<Range> FastEnumerateRangeInOrder(RangeTester rangeTester, Node root)
        {
            Stack<(CommandType, Node)> stack = new Stack<(CommandType, Node)>();
            Node current = root;
            int compare = 1;

            // Find the highest parent that overlaps with the range
            while (current != null)
            {
                compare = rangeTester(current.item);

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
                    break;
                }
            }

            // No node found that overlaps the range
            if (current == null)
            {
                yield break;
            }

            stack.Push((CommandType.ExpandAndYield, current));
            CommandType commandType;
            // Now we can enumerate the stack left from the root
            while (stack.Count != 0)
            {
                (commandType, current) = stack.Pop();

                // Current is null
                if (current == null)
                {
                    continue;
                }

                // We have to yield the current item
                if (commandType == CommandType.Yield)
                {
                    yield return current.item;
                }
                // We need to expand to get the next item
                else
                {
                    if (current.right != null)
                    {
                        compare = rangeTester(current.right.item);
                        if (compare == 0)
                        {
                            stack.Push((CommandType.ExpandAndYield, current.right));
                        }
                        // Right item is not too small
                        else if (compare < 0)
                        {
                            stack.Push((CommandType.Expand, current.right));
                        }
                    }

                    if (commandType == CommandType.ExpandAndYield)
                        stack.Push((CommandType.Yield, current));

                    if (current.left != null)
                    {
                        compare = rangeTester(current.left.item);
                        if (compare == 0)
                        {
                            stack.Push((CommandType.ExpandAndYield, current.left));
                        }
                        // Left item is not too big
                        else if (compare > 0)
                        {
                            // We still want to check
                            stack.Push((CommandType.Expand, current.left));
                        }
                    }
                }
            }
        }

        /*/// <summary>
        /// Enumerate all the items in a custom range, under and including node, in-order.
        /// </summary>
        /// <param name="rangeTester">Tests an item against the custom range.</param>
        /// <param name="node">Node to begin enumeration. May be null.</param>
        /// <returns>An enumerable of the items.</returns>
        /// <exception cref="InvalidOperationException">The tree has an item added or deleted during the enumeration.</exception>
        private IEnumerable<Range> FastEnumerateRangeInOrder(RangeTester rangeTester, Node root)
        {
            Stack<Node> stack = new Stack<Node>();
            Node current = root;
            int compare = 1;

            // Find the highest parent that overlaps with the range
            while (current != null)
            {
                compare = rangeTester(current.item);

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
                    break;
                }
            }

            // Go to the most left item containing the lower range
            expandStackLeft(stack, current, rangeTester);

            bool enumeratingStarted = true;
            bool goingLeft = true;
            bool wentLeft = false;
            bool goingDown = true;
            bool pushed = false;

            // Now we can enumerate the stack left from the root
            while (stack.Count != 0)
            {
                if (goingLeft)
                {
                    wentLeft = false;
                    compare = rangeTester(current.item);
                    // Too high
                    if (compare > 0)
                    {
                        goingLeft = false;
                    }
                    else
                    {
                        goingLeft = true;
                        stack.Push(current);
                        current = current.left;
                        continue;
                    }
                    //current = stack.Pop();
                    //yield return current.item;
                }
                else if (!goingLeft)
                {
                    current = stack.Pop();
                    if (!wentLeft)
                    {

                    }
                }
            }
        }*/

        private void expandStackLeft(Stack<Node> stack, Node node, RangeTester rangeTester)
        {
            int compare = 0;
            while (node != null)
            {
                compare = rangeTester(node.item);
                if (compare == 0)
                {
                    stack.Push(node);
                    node = node.left;
                }
                // Too low
                else if (compare < 0)
                {
                    node = node.right;
                }
                // If we went too high there is no more items to ad to the stack
                else
                {
                    break;
                }
            }
        }

        private void expandStackRight(Stack<Node> stack, Node node, RangeTester rangeTester)
        {
            int compare = 0;
            while (node != null)
            {
                compare = rangeTester(node.item);
                if (compare == 0)
                {
                    stack.Push(node);
                    node = node.right;
                }
                // Too high, see if the left branch does still contain some range
                else if (compare > 0)
                {
                    node = node.left;
                }
                // Cannot find an item that is too
                else
                {
                    break;
                }
            }
        }

        /*public override string ToString()
        {
            StringBuilder sb = new();
            foreach (Range range in this)
            {
                sb.Append(range);
                if (range.End != -1) sb.Append("->");
            }

            return sb.ToString();
        }*/

        public override string ToString()
        {
            StringBuilder sb = new();
            //foreach (Range range in FastEnumerateRange(0, -1))
            foreach (Range range in EnumerateRange(0, -1))
            {
                sb.Append(range);
                if (range.End != -1) sb.Append("->");
            }

            return sb.ToString();
        }
    }
}
