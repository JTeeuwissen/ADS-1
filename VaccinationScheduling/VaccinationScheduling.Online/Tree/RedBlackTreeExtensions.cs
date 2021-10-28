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
        /// <returns>Returns item</returns>
        public Range Find(int key)
        {
            Node current = root; // current search location in the tree

            if (key < -1)
            {
                throw new Exception("Key cannot be negative");
            }

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
                    break;
                }
            }
            return current.item;
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
        public IEnumerable<Range> FastEnumerateRange(int first, int last)
        {
            return FastEnumerateRangeInOrder(first, last, root);
        }

        public void RemoveRange(int tStartRange, int tEndRange, int machineNr)
        {
            bool foundBeforeStart = false;
            bool mergeRangeAfter = false;

            IEnumerator<Range> rangesEnum = FastEnumerateRange(tStartRange, tEndRange).GetEnumerator();
            List<Range> ranges = new List<Range>();
            while(rangesEnum.MoveNext())
            {
                ranges.Add(rangesEnum.Current);
            }
            int initialRangeStart = ranges[0].Start;
            int initialRangeEnd = tEndRange;

            Range range = null;
            for (int i = 0; i < ranges.Count; i++)
            {
                range = ranges[i];
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
                        MergeRangeBefore(range);
                    }
                    // tree -------------INF
                    // job    -----
                    // converted to:
                    //      --
                    //        -----(Not x)
                    //             ------INF
                    else if (tStartRange > range.Start)
                    {
                        range.EndMaybe = tStartRange - 1;
                        SetList sl = new SetList(machineNr);
                        Insert(new Range(tStartRange, tEndRange, sl));
                        Insert(new Range(tEndRange + 1, null));
                    }
                    break;
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
                    if (!foundBeforeStart)
                    {
                        foundBeforeStart = true;
                        MergeRangeAfter(newRange);
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
                    int oldStart = range.Start;
                    int? oldEnd = range.EndMaybe;
                    range.Start = rangeStart;
                    range.EndMaybe = rangeEnd;
                    Insert(new Range(oldStart, rangeStart - 1, range.NotList.Clone()));
                    Insert(new Range(rangeEnd + 1, oldEnd, range.NotList.Clone()));
                    range.NotList.Add(machineNr);
                    mergeRangeAfter = false;
                }
            }

            // The there is no range after the current one that needs to get updated
            if (range != null || mergeRangeAfter)
            {
                MergeRangeAfter(range);
            }

            //UpdateInbetweeenScoresOfRange(initialRangeStart, initialRangeEnd);
            //verifyTree();
        }

        private void UpdateInbetweeenScoresOfRange(int rangeStart, int rangeEnd)
        {
            int leftRange = findTwoLeft(rangeStart).Start;
            int rightRange = findTwoRight(rangeEnd).Start;

            // We always need to know the neighbours too
            Range first = null, second = null, third = null;

            IEnumerator<Range> ranges = FastEnumerateRange(leftRange, rightRange).GetEnumerator();
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
                else
                {
                    if (second.Start == second.EndMaybe)
                    {
                        second.MachineNrInBothNeighbours = second.NotList.FindItemInBothNeighbours(first.NotList, third.NotList);
                    }
                    else
                    {
                        second.MachineNrInBothNeighbours = null;
                    }
                    second.InLeftItem = second.NotList.FindFirstUniqueInOtherSet(first.NotList);
                    second.InRightItem = second.NotList.FindFirstUniqueInOtherSet(third.NotList);
                }
            }

            if (third.EndMaybe == null)
            {
                third.MachineNrInBothNeighbours = null;
                third.InLeftItem = third.NotList.FindFirstUniqueInOtherSet(second.NotList);
                third.InRightItem = null;
            }
            if (second.Start == 0)
            {
                second.MachineNrInBothNeighbours = null;
                second.InLeftItem = null;
                second.InRightItem = second.NotList.FindFirstUniqueInOtherSet(third.NotList);
            }
            else if (first.Start == 0)
            {
                first.MachineNrInBothNeighbours = null;
                first.InLeftItem = null;
                first.InRightItem = first.NotList.FindFirstUniqueInOtherSet(second.NotList);
            }
        }

        private Range findTwoLeft(int tStart)
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

        private Range findTwoRight(int tEnd)
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

        private void MergeRangeBefore(Range range)
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
            if (range.NotList.Equals(beforeRange.NotList))
            {
                // Delete range and update the previous range
                bool deleted = Delete(beforeRange, true, out beforeRange);
                //Debug.Assert(deleted);
                range.Start = beforeRange.Start;
            }
        }

        private void MergeRangeAfter(Range range)
        {
            // tree ((Not x)--------)(----------(Not x))
            // converts to: --------------------(Not x)

            // There is no range after the current range
            if (range.EndMaybe == null)
            {
                return;
            }

            // Find the range before
            Range afterRange = Find((int)range.EndMaybe + 1);
            if (range.NotList.Equals(afterRange.NotList))
            {
                // Delete range and update the previous range
                bool deleted = Delete(range, true, out range);
                //Debug.Assert(deleted);
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
            Stack<(CommandType, Node, bool, bool, bool)> stack = new Stack<(CommandType, Node, bool, bool, bool)>();
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

            stack.Push((CommandType.ExpandAndYield, current, true, true, true));
            CommandType commandType;
            bool parentInRange;
            bool isLeftChild;
            bool isRoot;
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
                else if (commandType == CommandType.FreeExpand)
                {
                    if (current.right != null)
                    {
                        stack.Push((CommandType.ExpandAndYield, current.right, true, false, false));
                    }
                    stack.Push((CommandType.Yield, current, true, true, false));
                    if (current.left != null)
                    {
                        stack.Push((CommandType.ExpandAndYield, current.left, true, false, false));
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
                            if (parentInRange && isLeftChild && !isRoot)
                                stack.Push((CommandType.FreeExpand, current.right, true, false, false));
                            else
                                stack.Push((CommandType.ExpandAndYield, current.right, true, false, false));
                        }
                        // Right item is too big
                        else if (compare > 0)
                        {
                            stack.Push((CommandType.ExpandLeft, current.right, true, false, false));
                        }
                    }

                    stack.Push((CommandType.Yield, current, true, true, false));

                    if (current.left != null)
                    {
                        compare = rangeTester(current.left.item);
                        if (compare == 0)
                        {
                            if (parentInRange && !isLeftChild && !isRoot)
                                stack.Push((CommandType.FreeExpand, current.left, true, true, false));
                            else
                                stack.Push((CommandType.ExpandAndYield, current.left, true, true, false));
                        }
                        // Left item is too small
                        else if (compare < 0)
                        {
                            // We still want to check
                            stack.Push((CommandType.ExpandRight, current.left, true, true, false));
                        }
                    }
                }
                else if (commandType == CommandType.ExpandRight)
                {
                    if (current.right != null)
                    {
                        if (current.right.item.CompareTo(first) < 0)
                        {
                            stack.Push((CommandType.ExpandRight, current.right, false, false, false));
                        }
                        else
                        {
                            stack.Push((CommandType.ExpandAndYield, current.right, false, false, false));
                        }
                    }
                }
                else
                {
                    if (current.left != null)
                    {
                        if (current.left.item.CompareTo(last) > 0)
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

        public void verifyTree()
        {
            IEnumerator<Range> rangesEnum = FastEnumerateRange(0, int.MaxValue).GetEnumerator();
            List<Range> ranges = new List<Range>();
            while (rangesEnum.MoveNext())
            {
                ranges.Add(rangesEnum.Current);
            }

            if (ranges[0].Start != 0)
            {
                throw new Exception("Range has to start with 0");
            }
            for (int i = 1; i < ranges.Count; i++)
            {
                Range range = ranges[i];
                if (ranges[i].Start - 1 != ranges[i-1].EndMaybe)
                {
                    throw new Exception($"Ranges do not match at {range.Start}");
                }
                if (ranges[i-1].EndMaybe == null)
                {
                    throw new Exception($"Several infinities");
                }
                if (ranges[i].NotList.Equals(ranges[i-1].NotList))
                {
                    throw new Exception("Neighbouring sets are not allowed to be equal");
                }
                if (ranges[i].MachineNrInBothNeighbours != null)
                {
                    int machineNr = (int)ranges[i].MachineNrInBothNeighbours;
                    if (ranges[i].Start != ranges[i].EndMaybe)
                    {
                        throw new Exception("Can only be flush when the range length is 1");
                    }
                    bool isValid = !ranges[i].NotList.Contains(machineNr) && ranges[i-1].NotList.Contains(machineNr) && ranges[i+1].NotList.Contains(machineNr);
                    if (!isValid)
                    {
                        throw new Exception("Range is found in neighbouring list or in current list");
                    }
                }
                else if (i + 1 < ranges.Count && ranges[i].Start == ranges[i].EndMaybe)
                {
                    if (ranges[i].NotList.FindItemInBothNeighbours(ranges[i-1].NotList, ranges[i+1].NotList) != null)
                    {
                        throw new Exception("There was a wrongly identified unique");
                    }
                }
                if (ranges[i].InLeftItem != null)
                {
                    int machineNr = (int)ranges[i].InLeftItem;
                    bool isValid = !ranges[i].NotList.Contains(machineNr) && ranges[i - 1].NotList.Contains(machineNr);
                    if (!isValid)
                    {
                        throw new Exception("left item is on in left list or present in current list");
                    }
                }
                else if (ranges[i].NotList.FindFirstUniqueInOtherSet(ranges[i - 1].NotList) != null)
                {
                    throw new Exception("misidentified left list");
                }
                if (ranges[i].InRightItem != null)
                {
                    int machineNr = (int)ranges[i].InRightItem;
                    bool isValid = !ranges[i].NotList.Contains(machineNr) && ranges[i + 1].NotList.Contains(machineNr);
                    if (!isValid)
                    {
                        throw new Exception("right item is not in right item or is present in current list");
                    }
                }
                else if (i + 1 < ranges.Count)
                {
                    if (ranges[i].NotList.FindFirstUniqueInOtherSet(ranges[i + 1].NotList) != null)
                    {
                        throw new Exception("misidentified right list");
                    }
                }
            }
            if (ranges[ranges.Count - 1].EndMaybe != null)
            {
                throw new Exception($"Last item must be null (aka. Infinity)");
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new();
            foreach (Range range in FastEnumerateRange(0, int.MaxValue))
            {
                sb.Append(range);
                if (range.EndMaybe != null) sb.Append("->");
            }

            return sb.ToString();
        }
    }
}
