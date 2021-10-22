using System.Collections.Generic;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System;
using VaccinationScheduling.Online.List;

namespace VaccinationScheduling.Online.Tree
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
        private RangeTester DoubleBoundedRangeTester(int first, int? last)
        {
            return delegate(Range item)
            {
                if (item.CompareTo(first) < 0)
                    return -1; // item is before or equal to first.

                if (last == null)
                    return 0;

                // ReSharper disable once ConvertIfStatementToReturnStatement
                if (item.CompareTo((int)last) > 0)
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
        public IEnumerable<Range> EnumerateRange(int first, int? last)
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
            return FastEnumerateRangeInOrder(first, last, root);
        }

        public void RemoveRange(int tStartRange, int tEndRange, int machineNr)
        {
            bool foundBeforeStart = false;
            bool mergeRangeAfter = false;

            // tree --3 4-6-8 -10-- 13-- ------ -----
            IEnumerator<Range> ranges = FastEnumerateRange(tStartRange, tEndRange).GetEnumerator();
            Range range = null;
            while (ranges.MoveNext())
            {
                range = ranges.Current;
                if (range.NotList.Contains(machineNr))
                {
                    MergeRangeBefore(range);
                    continue;
                }
                // Last range item
                if (range.EndMaybe == null)
                {
                    // tree -------------INF
                    // job  -------
                    // converted to:
                    //      -------(Not x)
                    //             ------INF
                    if (tStartRange <= range.Start)
                    {
                        range.EndMaybe = tEndRange;
                        range.NotList.Add(machineNr);
                        Insert(new Range(tEndRange + 1, null));
                        if (!foundBeforeStart)
                        {
                            foundBeforeStart = true;
                            MergeRangeBefore(range);
                        }
                    }
                    // tree -------------INF
                    // job    -----
                    // converted to:
                    //      --
                    //        -----(Not x)
                    //             ------INF
                    if (tStartRange > range.Start)
                    {
                        range.EndMaybe = tStartRange - 1;
                        SetList sl = new SetList(machineNr);
                        Insert(new Range(tStartRange, tEndRange, sl));
                        Insert(new Range(tEndRange + 1, null));
                    }
                    return;
                }
                int rangeStart = Math.Max(tStartRange, range.Start);
                int rangeEnd = Math.Min(tEndRange, (int)range.EndMaybe);
                // Range item inbetween
                // tree ---------
                // job  ---------
                // converts to:
                //      ---------(Not x)
                // Begin + Einde
                if (rangeStart == range.Start && rangeEnd == range.EndMaybe)
                {
                    range.NotList.Add(machineNr);
                    if (!foundBeforeStart)
                    {
                        foundBeforeStart = true;
                        MergeRangeBefore(range);
                    }
                    mergeRangeAfter = true;
                }
                // tree ---------
                // job  -------
                // Converts to:
                //      -------(Not x)
                //             --
                // Begin
                else if(rangeStart == range.Start)
                {
                    int oldEnd = (int)range.EndMaybe;
                    range.EndMaybe = rangeEnd;
                    Insert(new Range(rangeEnd + 1, oldEnd, range.NotList.Clone()));
                    range.NotList.Add(machineNr);
                    if (!foundBeforeStart)
                    {
                        foundBeforeStart = true;
                        MergeRangeBefore(range);
                    }
                    mergeRangeAfter = false;
                }
                // tree ---------
                // job    -------
                // Converts to:
                //      --
                //        -------(Not x)
                // Eind
                else if (rangeEnd == range.EndMaybe)
                {
                    int oldEnd = (int)range.EndMaybe;
                    range.EndMaybe = rangeStart - 1;
                    Range newRange = new(rangeStart, oldEnd, range.NotList.Clone());
                    newRange.NotList.Add(machineNr);
                    Insert(newRange);
                    mergeRangeAfter = true;
                }
                // tree ---------
                // job    -----
                // Converts to:
                //      --
                //        -----(Not x)
                //             --
                else
                {
                    int oldEnd = (int)range.EndMaybe;
                    range.EndMaybe = rangeStart - 1;
                    Insert(new Range(rangeEnd + 1, oldEnd, range.NotList.Clone()));
                    Range newRange = new(rangeStart, rangeEnd, range.NotList.Clone());
                    newRange.NotList.Add(machineNr);
                    Insert(newRange);
                    mergeRangeAfter = false;
                }
            }

            // The there is no range after the current one that needs to get updated
            if (range == null || !mergeRangeAfter)
            {
                return;
            }

            MergeRangeAfter(range);
        }

        public void MergeRangeBefore(Range range)
        {
            // tree ((Not x)--------)(----------(Not x))
            // converts to: --------------------(Not x)

            // There is no range before the current range
            if (range.Start == 0)
            {
                return;
            }

            // Find the range before
            Range beforeRange;
            Find(range.Start - 1, out beforeRange);
            if (range.NotList.AreEqual(beforeRange.NotList))
            {
                // Delete range and update the previous range
                bool deleted = Delete(beforeRange, true, out beforeRange);
                //Debug.Assert(deleted);
                range.Start = beforeRange.Start;
            }
        }

        public void MergeRangeAfter(Range range)
        {
            // tree ((Not x)--------)(----------(Not x))
            // converts to: --------------------(Not x)

            // There is no range after the current range
            if (range.EndMaybe == null)
            {
                return;
            }

            // Find the range before
            Range afterRange;
            Find((int)range.EndMaybe + 1, out afterRange);
            if (range.NotList.AreEqual(afterRange.NotList))
            {
                // Delete range and update the previous range
                bool deleted = Delete(afterRange, true, out afterRange);
                //Debug.Assert(deleted);
                range.EndMaybe = afterRange.EndMaybe;
            }
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
        private IEnumerable<Range> FastEnumerateRangeInOrder(int first, int last, Node root)
        {
            Stack<(CommandType, Node, bool, bool)> stack = new Stack<(CommandType, Node, bool, bool)>();
            RangeTester rangeTester = DoubleBoundedRangeTester(first, last);
            Node current = root;
            int compare = 1;

            // Find the highest parent that overlaps with the range
            while (current != null)
            {
                if (current.item.CompareTo(last) > 0)
                {
                    current = current.left;
                }
                else if (current.item.CompareTo(first) < 0)
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

            stack.Push((CommandType.ExpandAndYield, current, true, true));
            CommandType commandType;
            bool parentInRange;
            bool isLeftChild;
            // Now we can enumerate the stack left from the root
            while (stack.Count != 0)
            {
                (commandType, current, parentInRange, isLeftChild) = stack.Pop();

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
                else if (commandType == CommandType.FreeExpand)
                {
                    if (current.right != null)
                    {
                        stack.Push((CommandType.ExpandAndYield, current.right, true, false));
                    }
                    stack.Push((CommandType.Yield, current, true, true));
                    if (current.left != null)
                    {
                        stack.Push((CommandType.ExpandAndYield, current.left, true, false));
                    }
                }
                // We need to expand to get the next item
                else if (commandType == CommandType.ExpandAndYield)
                {
                    if (current.right != null)
                    {
                        compare = rangeTester(current.right.item);
                        if (compare == 0)
                        {
                            if (parentInRange && isLeftChild)
                                stack.Push((CommandType.FreeExpand, current.right, true, false));
                            else
                                stack.Push((CommandType.ExpandAndYield, current.right, true, false));
                        }
                        // Right item is too big
                        else if (compare > 0)
                        {
                            stack.Push((CommandType.ExpandLeft, current.right, true, false));
                        }
                    }

                    stack.Push((CommandType.Yield, current, true, true));

                    if (current.left != null)
                    {
                        compare = rangeTester(current.left.item);
                        if (compare == 0)
                        {
                            if (parentInRange && !isLeftChild)
                                stack.Push((CommandType.FreeExpand, current.left, true, true));
                            else
                                stack.Push((CommandType.ExpandAndYield, current.left, true, true));
                        }
                        // Left item is too small
                        else if (compare < 0)
                        {
                            // We still want to check
                            stack.Push((CommandType.ExpandRight, current.left, true, true));
                        }
                    }
                }
                else if (commandType == CommandType.ExpandRight)
                {
                    if (current.right != null)
                    {
                        if (current.right.item.CompareTo(first) < 0)
                        {
                            stack.Push((CommandType.ExpandRight, current.right, false, false));
                        }
                        else
                        {
                            stack.Push((CommandType.ExpandAndYield, current.right, false, false));
                        }
                    }
                }
                else
                {
                    if (current.left != null)
                    {
                        if (current.left.item.CompareTo(last) > 0)
                        {
                            stack.Push((CommandType.ExpandLeft, current.left, false, true));
                        }
                        else
                        {
                            stack.Push((CommandType.ExpandAndYield, current.left, false, true));
                        }
                    }
                }
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new();
            //foreach (Range range in FastEnumerateRange(0, -1))
            foreach (Range range in EnumerateRange(0, null))
            {
                sb.Append(range);
                if (range.EndMaybe != null) sb.Append("->");
            }

            return sb.ToString();
        }
    }
}
