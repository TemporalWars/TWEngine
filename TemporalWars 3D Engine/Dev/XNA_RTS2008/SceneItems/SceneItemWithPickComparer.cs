#region File Description
//-----------------------------------------------------------------------------
// SceneItemWithPickComparer.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System.Collections.Generic;

namespace TWEngine.SceneItems
{
    /// <summary>
    /// IComparer Class for the binarySearch.  Compares using the 'SceneItemNumber'.
    /// </summary>
    public class SceneItemWithPickComparer : IComparer<SceneItemWithPick>
    {
        /// <summary>
        /// Compare method, taking two SceneItemWithPick instances, and comparing
        /// their 'SceneItemNumber' to find a match.
        /// </summary> 
        /// <param name="item1">Instance of <see cref="SceneItemWithPick"/>.</param> 
        /// <param name="item2">Instance of <see cref="SceneItemWithPick"/>.</param>  
        /// <returns>Returns a result of the comparison: -1, 0, 1</returns>    
        public int Compare(SceneItemWithPick item1, SceneItemWithPick item2)
        {
            if (item1 == null)
            {
                if (item2 == null)
                {
                    // If item1 is null and item2 is null, they're
                    // equal. 
                    return 0;
                }

                // If item1 is null and item2 is not null, item2
                // is greater. 
                return -1;
            }

            // If item1 is not null...
            //
            return item2 == null ? 1 : item1.SceneItemNumber.CompareTo(item2.SceneItemNumber);
        }
    }
}