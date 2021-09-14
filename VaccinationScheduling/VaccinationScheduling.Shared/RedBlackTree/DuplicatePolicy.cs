//******************************
// Written by Peter Golde
// Copyright (c) 2004-2007, Wintellect
//
// Use and restribution of this code is subject to the license agreement
// contained in the file "License.txt" accompanying this file.
//******************************

// License link: https://www.nuget.org/packages/SoftUni.Wintellect.PowerCollections/2.0.0

namespace VaccinationScheduling.Shared.RedBlackTree
{
    /// <summary>
    /// Describes what to do if a key is already in the tree when doing an
    /// insertion.
    /// </summary>
    public enum DuplicatePolicy
    {
        InsertFirst,               // Insert a new node before duplicates
        InsertLast,               // Insert a new node after duplicates
        ReplaceFirst,            // Replace the first of the duplicate nodes
        ReplaceLast,            // Replace the last of the duplicate nodes
        DoNothing                // Do nothing to the tree
    };
}
