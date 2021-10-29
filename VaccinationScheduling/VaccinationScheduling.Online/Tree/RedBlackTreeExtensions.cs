using System.Collections.Generic;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System;
using VaccinationScheduling.Online.List;
using System.Numerics;

namespace VaccinationScheduling.Online.Tree
{
    /// <summary>
    /// The extensions we made on the redblacktree are specified in this file
    /// </summary>
    public partial class RedBlackTree : IEnumerable<Range>
    {
        /// <summary>
        /// Length of the jab in the current tree.
        /// </summary>
        public BigInteger JabLength = 0;

        /// <summary>
        /// Finds the key in the tree. If multiple items in the tree have
        /// compare equal to the key, finds the first or last one. Optionally replaces the item
        /// with the one searched for.
        /// </summary>
        /// <param name="key">Key to search for.</param>
        /// <param name="item">Returns the found item, before replacing (if function returns true).</param>
        /// <returns>Returns item</returns>
        public Range Find(BigInteger key)
        {
            Node current = root; // current search location in the tree

            // Cannot find a key lower than 0
            if (key < 0)
            {
                throw new Exception("Key cannot be negative");
            }

            // Recursively go down the range until the key is found
            while (current != null)
            {
                int compare = current.item.CompareTo(key);

                // Current item is too high
                if (compare > 0)
                {
                    current = current.left;
                }
                // Current item is too low
                else if (compare < 0)
                {
                    current = current.right;
                }
                // Item got found
                else
                {
                    break;
                }
            }

            // Return the node
            return current.item;
        }

        /// <summary>
        /// Gets a range tester that defines a range by first and last items.
        /// </summary>
        /// <param name="first">The lower bound.</param>
        /// <param name="last">The upper bound.</param>
        /// <returns>A RangeTester delegate that tests for an item in the given range.</returns>
        private RangeTester DoubleBoundedRangeTester(BigInteger first, BigInteger? last)
        {
            return delegate(Range item)
            {
                if (item.CompareTo(first) < 0)
                    return -1; // item is before or equal to first.

                if (last == null)
                    return 0;

                // ReSharper disable once ConvertIfStatementToReturnStatement
                if (item.CompareTo((BigInteger)last) > 0)
                    return 1; // item is after or equal to last

                return 0; // item is between first and last.
            };
        }

        /// <summary>
        /// Marks the given range occupied by the machine
        /// </summary>
        /// <param name="leftBound">Leftbound of the range to mark occupied</param>
        /// <param name="rightBound">Rightbound of the range to mark occupied</param>
        /// <param name="machineNr">MachineNr that gets occupied in the given range</param>
        public void MarkRangeOccupied(BigInteger leftBound, BigInteger rightBound, int machineNr)
        {
            // Booleans to merge the first and last range item if needed.
            bool foundBeforeStart = false;
            bool mergeRangeAfter = false;

            // Enumerates the range and add it to a list. Ranges get changed and insertions are done so enumeration will break when the tree re-sorts during the loop.
            IEnumerator<Range> rangesEnum = FastEnumerateRangeInOrder(leftBound, rightBound).GetEnumerator();
            List<Range> ranges = new List<Range>();
            while(rangesEnum.MoveNext())
            {
                ranges.Add(rangesEnum.Current);
            }

            BigInteger initialRangeStart = ranges[0].Start;
            BigInteger initialRangeEnd = rightBound;

            Range range = null;
            // Go through each range item and update it according to how it overlaps.
            // Each case is showed by the minus signs and how it gets converted.
            for (int i = 0; i < ranges.Count; i++)
            {
                range = ranges[i];
                // The range already contains the machineNr
                if (range.OccupiedMachineNrs.Contains(machineNr))
                {
                    mergeWithRangeBefore(range);
                    continue;
                }
                // Item has no maximum, goes to inifinity.
                if (range.EndMaybe == null)
                {
                    // tree -------------INFINITY
                    // job  -------
                    // converted to:
                    //      -------(Not x)
                    //             ------INFINITY
                    if (leftBound <= range.Start)
                    {
                        range.EndMaybe = rightBound;
                        range.OccupiedMachineNrs.Add(machineNr);
                        Insert(new Range(rightBound + 1, null));
                        mergeWithRangeBefore(range);
                    }
                    // tree -------------INFINITY
                    // job    -----
                    // converted to:
                    //      --
                    //        -----(Not x)
                    //             ------INFINITY
                    else if (leftBound > range.Start)
                    {
                        range.EndMaybe = leftBound - 1;
                        CustomSet sl = new CustomSet(machineNr);
                        Insert(new Range(leftBound, rightBound, sl));
                        Insert(new Range(rightBound + 1, null));
                    }
                    break;
                }
                BigInteger rangeStart = BigInteger.Max(leftBound, range.Start);
                BigInteger rangeEnd = BigInteger.Min(rightBound, (BigInteger)range.EndMaybe);
                // Range item inbetween
                // tree ---------
                // job  ---------
                // converts to:
                //      ---------(Not x)
                // Begin + Einde
                if (rangeStart == range.Start && rangeEnd == range.EndMaybe)
                {
                    range.OccupiedMachineNrs.Add(machineNr);
                    if (!foundBeforeStart)
                    {
                        foundBeforeStart = true;
                        mergeWithRangeBefore(range);
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
                    BigInteger oldEnd = (BigInteger)range.EndMaybe;
                    range.EndMaybe = rangeEnd;
                    Insert(new Range(rangeEnd + 1, oldEnd, range.OccupiedMachineNrs.Clone()));
                    range.OccupiedMachineNrs.Add(machineNr);
                    if (!foundBeforeStart)
                    {
                        foundBeforeStart = true;
                        mergeWithRangeBefore(range);
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
                    BigInteger oldEnd = (BigInteger)range.EndMaybe;
                    range.EndMaybe = rangeStart - 1;
                    Range newRange = new(rangeStart, oldEnd, range.OccupiedMachineNrs.Clone());
                    newRange.OccupiedMachineNrs.Add(machineNr);
                    Insert(newRange);
                    if (!foundBeforeStart)
                    {
                        foundBeforeStart = true;
                        mergeWithNextRange(newRange);
                    }
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
                    BigInteger oldStart = range.Start;
                    BigInteger? oldEnd = range.EndMaybe;
                    range.Start = rangeStart;
                    range.EndMaybe = rangeEnd;
                    Insert(new Range(oldStart, rangeStart - 1, range.OccupiedMachineNrs.Clone()));
                    Insert(new Range(rangeEnd + 1, oldEnd, range.OccupiedMachineNrs.Clone()));
                    range.OccupiedMachineNrs.Add(machineNr);
                    mergeRangeAfter = false;
                }
            }

            // The there is no range after the current one that needs to get updated
            if (range != null || mergeRangeAfter)
            {
                mergeWithNextRange(range);
            }

            //UpdateUniqueItemsInNeighbours(initialRangeStart, initialRangeEnd);
        }

        /// <summary>
        /// Update the items that are not occupied in current item but are occupied in the left or right ranges.
        /// </summary>
        /// <param name="rangeStart"></param>
        /// <param name="rangeEnd"></param>
        private void UpdateUniqueItemsInNeighbours(BigInteger rangeStart, BigInteger rangeEnd)
        {
            // We need to go two to the left and right since we need to update the neighbours just outside the range too
            BigInteger leftRange = findTwoLeft(rangeStart).Start;
            BigInteger rightRange = findTwoRight(rangeEnd).Start;

            // We always need to know the neighbours too
            Range first = null, second = null, third = null;

            // Enumerate the ranges
            IEnumerator<Range> ranges = FastEnumerateRangeInOrder(leftRange, rightRange).GetEnumerator();
            while (ranges.MoveNext())
            {
                first = second;
                second = third;
                third = ranges.Current;

                // We don't need to update the middle one yet since there are no 3 items
                if (first == null)
                {
                    continue;
                }

                // The range has a length of 1, so it can be a perfect flush spot to put a job in.
                if (second.Start == second.EndMaybe)
                {
                    second.MachineNrInBothNeighbours = second.OccupiedMachineNrs.FindUniqueItemInBothNeighbours(first.OccupiedMachineNrs, third.OccupiedMachineNrs);
                }
                else
                {
                    second.MachineNrInBothNeighbours = null;
                }
                second.InLeftItem = second.OccupiedMachineNrs.FindFirstUniqueInOtherSet(first.OccupiedMachineNrs);
                second.InRightItem = second.OccupiedMachineNrs.FindFirstUniqueInOtherSet(third.OccupiedMachineNrs);
            }

            // The final range goes to infinity, so only the left item needs to get set.
            if (third.EndMaybe == null)
            {
                third.MachineNrInBothNeighbours = null;
                third.InLeftItem = third.OccupiedMachineNrs.FindFirstUniqueInOtherSet(second.OccupiedMachineNrs);
                third.InRightItem = null;
            }

            // For the starting item only the right item needs to get updated
            if (second.Start == 0)
            {
                second.MachineNrInBothNeighbours = null;
                second.InLeftItem = null;
                second.InRightItem = second.OccupiedMachineNrs.FindFirstUniqueInOtherSet(third.OccupiedMachineNrs);
            }
            else if (first.Start == 0)
            {
                first.MachineNrInBothNeighbours = null;
                first.InLeftItem = null;
                first.InRightItem = first.OccupiedMachineNrs.FindFirstUniqueInOtherSet(second.OccupiedMachineNrs);
            }
        }

        /// <summary>
        /// Find the item two to the left of the given time.
        /// </summary>
        /// <param name="tStart">Time to go left of the range item</param>
        /// <returns>Returns the range object of the</returns>
        private Range findTwoLeft(BigInteger tStart)
        {
            if (tStart == 0)
            {
                return Find(tStart);
            }
            Range result = Find(tStart - 1);
            if (result.Start == 0)
            {
                return result;
            }
            return Find(result.Start - 1);
        }

        /// <summary>
        /// Find the item two to the right of the given time
        /// </summary>
        /// <param name="tEnd">Endtime of the previous range item</param>
        /// <returns>The range item two to the right of the given time</returns>
        private Range findTwoRight(BigInteger tEnd)
        {
            Range result = Find(tEnd);
            if (result.EndMaybe == null)
            {
                return result;
            }
            result = Find((int)result.EndMaybe + 1);
            if (result.EndMaybe == null)
            {
                return result;
            }
            return Find((int)result.EndMaybe + 1);
        }

        /// <summary>
        /// Range the current range with the previous range if both have the same machines that are occupied at that range.
        /// </summary>
        /// <param name="range">Current range to check for if they can merge.</param>
        private void mergeWithRangeBefore(Range range)
        {
            // tree ((Not x)--------)(----------(Not x))
            // converts to: --------------------(Not x)

            // There is no range before the current range
            if (range.Start == 0)
            {
                return;
            }

            // Find the range before
            Range beforeRange = Find(range.Start - 1);
            // If occupied machines are equal, merge the range.
            if (range.OccupiedMachineNrs.Equals(beforeRange.OccupiedMachineNrs))
            {
                // Delete range and update the previous range
                Delete(beforeRange, true, out beforeRange);
                range.Start = beforeRange.Start;
            }
        }

        /// <summary>
        /// Range the current range with the next range if both have the same machines that are occupied at that range.
        /// </summary>
        /// <param name="range">Current range to check for if they can merge.</param>
        private void mergeWithNextRange(Range range)
        {
            // tree ((Not x)--------)(----------(Not x))
            // converts to: --------------------(Not x)

            // There is no range after the current range
            if (range.EndMaybe == null)
            {
                return;
            }

            // Find the range after the current range
            Range afterRange = Find((BigInteger)range.EndMaybe + 1);
            // If occupied machines are equal, merge the range.
            if (range.OccupiedMachineNrs.Equals(afterRange.OccupiedMachineNrs))
            {
                // Delete range and update the previous range
                Delete(range, true, out range);
                afterRange.Start = range.Start;
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
            bool rotated;

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
        /// Because we
        /// </summary>
        /// <param name="leftBound">Leftbound of the enumeration</param>
        /// <param name="rightBound">Rightbound of the enumeration</param>
        /// <returns>An enumerable of the items.</returns>
        public IEnumerable<Range> FastEnumerateRangeInOrder(BigInteger leftBound, BigInteger rightBound)
        {
            Stack<(CommandType, Node, bool, bool, bool)> stack = new Stack<(CommandType, Node, bool, bool, bool)>();
            RangeTester rangeTester = DoubleBoundedRangeTester(leftBound, rightBound);
            Node current = findHighestParentWithinRange(leftBound, rightBound);
            int compare = 1;

            // Initialize the stack with the highest node having to be yielded.
            stack.Push((CommandType.ExpandAndYield, current, true, true, true));

            CommandType commandType;
            bool parentInRange, isLeftChild, isRoot;

            // Now we can enumerate the stack left from the root
            while (stack.Count != 0)
            {
                (commandType, current, parentInRange, isLeftChild, isRoot) = stack.Pop();

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
                // The current node is marked as being free to expand, no checks are required anymore since they are guaranteed to be within the range.
                else if (commandType == CommandType.FreeExpand)
                {
                    if (current.right != null)
                    {
                        stack.Push((CommandType.FreeExpand, current.right, true, false, false));
                    }
                    stack.Push((CommandType.Yield, current, true, true, false));
                    if (current.left != null)
                    {
                        stack.Push((CommandType.FreeExpand, current.left, true, false, false));
                    }
                }
                // The current node is expanded meaning both left and right is checked
                else if (commandType == CommandType.ExpandAndYield)
                {
                    // We check the right child first since we use a stack. Meaning that we add right -> middle -> left
                    // but the items are evaluated from left to right when the stack is popped.
                    if (current.right != null)
                    {
                        compare = rangeTester(current.right.item);
                        if (compare == 0)
                        {
                            // The root cannot free-expand
                            if (parentInRange && isLeftChild && !isRoot)
                                stack.Push((CommandType.FreeExpand, current.right, true, false, false));
                            else
                                stack.Push((CommandType.ExpandAndYield, current.right, true, false, false));
                        }
                        // Right item is too big, maybe the left child of the right child is within the range again.
                        else if (compare > 0)
                        {
                            stack.Push((CommandType.ExpandLeft, current.right, true, false, false));
                        }
                    }

                    // Yield the current item.
                    stack.Push((CommandType.Yield, current, true, true, false));

                    if (current.left != null)
                    {
                        compare = rangeTester(current.left.item);
                        if (compare == 0)
                        {
                            // The root cannot be free-expanded. Its not a child
                            if (parentInRange && !isLeftChild && !isRoot)
                                stack.Push((CommandType.FreeExpand, current.left, true, true, false));
                            else
                                stack.Push((CommandType.ExpandAndYield, current.left, true, true, false));
                        }
                        // Left item is too small, but it can have a right child that is wihtin the range again.
                        else if (compare < 0)
                        {
                            stack.Push((CommandType.ExpandRight, current.left, true, true, false));
                        }
                    }
                }
                // In the tree we are on the left side. If the left item is too small we are forced to expand only right and not yield
                // Only if we find an Expand to the right of the current item that is within the range we start yielding items again.
                else if (commandType == CommandType.ExpandRight)
                {
                    if (current.right != null)
                    {
                        // The right
                        if (current.right.item.CompareTo(leftBound) < 0)
                        {
                            stack.Push((CommandType.ExpandRight, current.right, false, false, false));
                        }
                        else
                        {
                            stack.Push((CommandType.ExpandAndYield, current.right, false, false, false));
                        }
                    }
                }
                // Same logic as expand right but now for the mirrored case.
                else
                {
                    if (current.left != null)
                    {
                        if (current.left.item.CompareTo(rightBound) > 0)
                        {
                            stack.Push((CommandType.ExpandLeft, current.left, false, true, false));
                        }
                        else
                        {
                            stack.Push((CommandType.ExpandAndYield, current.left, false, true, false));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Searches for the highest parent that is within the range.
        /// </summary>
        /// <param name="leftBound">Leftbound of the range</param>
        /// <param name="rightBound">Rightbound of the range</param>
        /// <returns>Node that contains the highest node in the tree that corresponds with the range.</returns>
        private Node findHighestParentWithinRange(BigInteger leftBound, BigInteger rightBound)
        {
            // Start at the root
            Node current = root;

            // Find the highest parent that is within the bounds.
            while (current != null)
            {
                // Item too high, go left
                if (current.item.CompareTo(rightBound) > 0)
                {
                    current = current.left;
                }
                // Item too low, go right
                else if (current.item.CompareTo(leftBound) < 0)
                {
                    current = current.right;
                }
                // Found node.
                else
                {
                    break;
                }
            }

            // Return the node
            return current;
        }

        /// <summary>
        /// Method to print the tree into a linear format. Enumerates all items in the tree.
        /// </summary>
        /// <returns>String representation of the redblacktree</returns>
        public override string ToString()
        {
            // Use stringbuilder so concatinations are quick.
            StringBuilder sb = new();
            foreach (Range range in FastEnumerateRangeInOrder(0, int.MaxValue))
            {
                sb.Append(range);
                if (range.EndMaybe != null) sb.Append("->");
            }

            return sb.ToString();
        }
    }
}
