#region File Description
//-----------------------------------------------------------------------------
// ClosestItemParallelFor.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using System.Collections.Generic;
using System.Threading;
using ParallelTasksComponent;
using TWEngine.SceneItems;
using TWEngine.Shapes;

namespace TWEngine.ParallelTasks
{
    ///<summary>
    /// The <see cref="ClosestItemParallelFor"/> class, threads the calling of the player logic, 
    /// for each <see cref="SceneItemWithPick"/>, into 4 parallize processors.
    ///</summary>
    public class ClosestItemParallelFor : AbstractParallelFor
    {
        // Collection of items to iterate
        private SceneItemWithPick[] _selectableItems = new SceneItemWithPick[1];

        // 2/18/2010: Updated following to be STATIC to force sharing between threads.
        // stores the closest index found
        private static float _closestIntersection;
        // stores the closest Model Index found
        private static int _closestModelIndex;

        // 5/28/2010
        /// <summary>
        /// Constructor; can use either the custom <see cref="MyThreadPool"/>, or the .Net Framework <see cref="ThreadPool"/>.
        /// </summary>
        /// <param name="useDotNetThreadPool">Use the .Net Framework <see cref="ThreadPool"/>?</param>
        public ClosestItemParallelFor(bool useDotNetThreadPool)
        {
            // save
            UseDotNetThreadPool = useDotNetThreadPool;
        }

        /// <summary>
        /// Used to parallize a 'For-Loop', to run on 4 separate processors.
        /// </summary>
        /// <param name="selectableItems">List of <see cref="SceneItemWithPick"/> to check</param>
        /// <param name="inclusiveLowerBound">Starting index of For-Loop</param>
        /// <param name="exclusiveUpperBound">Ending index of For-Loop</param>
        /// <returns>index in array of closest match</returns>
        public int ParallelFor(IList<SceneItemWithPick> selectableItems, int inclusiveLowerBound, int exclusiveUpperBound)
        {
            // 6/18/2010 - Make local copy to avoid ArgOutOfRange Thread sync errors.
            if (_selectableItems.Length < selectableItems.Count)
                Array.Resize(ref _selectableItems, selectableItems.Count);
            selectableItems.CopyTo(_selectableItems, 0);

            // Reset Max value
            _closestModelIndex = -1;
            _closestIntersection = float.MaxValue;

            // Start Parallel For-Loop process
            ParallelFor(this, inclusiveLowerBound, exclusiveUpperBound);

            // If all are still -1, then nothing was found, so return -1.
            if (_closestModelIndex == -1) 
                return -1;

            return _closestModelIndex;

        }

        /// <summary>
        /// Core method for the <see cref="LoopBody"/> of the 'For-Loop'.  Inheriting classes
        /// MUST override and provide the core <see cref="LoopBody"/> to the 'For-Loop' logic.
        /// </summary>
        /// <param name="index">Index of the current iteration of the collection.</param>
        protected override void LoopBody(int index)
        {
            // 2/15/2010 - Check if index outside array range; since threaded method.
            DoLoopBody(this, index);
            return;
        }

        // 5/24/2010
        /// <summary>
        /// Helper method, which does the core 'For-Loop' logic.
        /// </summary>
        /// <param name="closestItemParallelFor">this instance of <see cref="ClosestItemParallelFor"/></param>
        /// <param name="index">'For-Loop' index value</param>
        private static void DoLoopBody(ClosestItemParallelFor closestItemParallelFor, int index)
        {
            float? intersection;
            if (!GetIntersection(closestItemParallelFor, index, out intersection)) return;

            // Skip if null.
            if (intersection == null) return;

            // If so, is it closer than any other model we might have
            // previously intersected?
            if (intersection >= _closestIntersection) return;

            // Store information about this model.
            _closestIntersection = intersection.Value;
            _closestModelIndex = index;
        }


        // 2/15/2010; 5/24/2010: Updated to be STATIC method.
        /// <summary>
        /// Helper method, used to find the closest intersection point on an item.
        /// </summary>
        /// <param name="closestItemParallelFor"></param>
        /// <param name="index">Index in array of closest intersection</param>
        /// <param name="intersection">(OUT) intersection</param>
        /// <returns>True/False if found intersection</returns>
        private static bool GetIntersection(ClosestItemParallelFor closestItemParallelFor, int index, out float? intersection)
        {
            intersection = null;

            if (index >= closestItemParallelFor._selectableItems.Length)
                return false;

            // 8/12/2009 - Cache
            SceneItemWithPick selectableItem;
            try // 6/18/2010 - Handle ArgOutOfRange error.
            {
                selectableItem = closestItemParallelFor._selectableItems[index];
            }
            catch (ArgumentOutOfRangeException)
            {
                return false;
            }

            // 5/25/2009
            if (selectableItem == null) return false;

            // 11/13/2008 - Only Check Alive objects
            if (!selectableItem.IsAlive) return false;

            // 1/13/2011 - Only Check IsSelectable marked objects
            if (!selectableItem.ItemSelectable) return false;

            var shapeWithPick = selectableItem.ShapeItem as ShapeWithPick; // Cast up
            if (shapeWithPick == null) return false;

            // 2/2/2010 - Reset the PickedHovered to false.
            selectableItem.PickHovered = false;

            // If Picked, then return index to caller 
            return shapeWithPick.IsMeshPicked(out intersection);
        }
    }
}