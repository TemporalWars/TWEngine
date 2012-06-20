#region File Description
//-----------------------------------------------------------------------------
// ChangeRequestItem.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using Microsoft.Xna.Framework;
using TWEngine.InstancedModels.Enums;

namespace TWEngine.InstancedModels.Structs
{
    // 3/10/2010: NOTE: In order to give the namespace the XML doc, must do it this way;
    /// <summary>
    /// The <see cref="TWEngine.InstancedModels.Structs"/> namespace contains the structures
    /// which make up the entire <see cref="InstancedModels.Structs"/> component.
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {
    }
    
    
    // 7/21/2009 - ChangeRequest Struct SceneItemOwner.
    ///<summary>
    /// <see cref="ChangeRequestItem"/> structure stores the necessary data, to complete
    /// a change request for the given <see cref="InstancedModelPart"/>.
    ///</summary>
    public struct ChangeRequestItem
    {
        ///<summary>
        /// <see cref="ChangeRequest"/> Enum type for this request.
        ///</summary>
        public ChangeRequest ChangeRequest;
        ///<summary>
        /// <see cref="PartType"/> to affect.
        ///</summary>
        public PartType PartType;
        ///<summary>
        /// New <see cref="Matrix"/> transform to apply.
        ///</summary>
        public Matrix Transform;

        // 8/28/2009 - 
        /// <summary>
        /// Stores PlayerNumber as Frac, and MaterialId as Int; so 1.02, would be 1 = MaterialId#1, and .02 = team#2.
        /// </summary>
        internal float PlayerNumberAndMaterialId; 

        // 6/6/2010
        ///<summary>
        /// Stores the projectiles' velocity, used specifically for explosions.
        ///</summary>
        public Vector3 ProjectileVelocity;
        
        /// <summary>
        /// PlayerNumber this InstancedItem belongs to.
        /// </summary>
        public float PlayerNumber
        {
            set
            {
                // 1st - retrieve the Int part of the original stored value
                int integer;
                ModF(PlayerNumberAndMaterialId, out integer);

                // store new combine result
                PlayerNumberAndMaterialId = integer + ((value == 0) ? 0.01f : 0.02f);

            }
            get
            {
                // retrieve the Frac portion and return to caller
                int integer;
                return ModF(PlayerNumberAndMaterialId, out integer);
            }
        }

        // 2/3/2010 - Used to set the 'ProceduralMaterialId'.
        /// <summary>
        /// Sets the ProceduralMaterialId for an instanceItem.
        /// </summary>
        public int ProceduralMaterialId
        {
            set
            {
                // 1st - retrieve the Int part of the original stored value
                int integer;
                var frac = ModF(PlayerNumberAndMaterialId, out integer);

                // store new combine result
                PlayerNumberAndMaterialId = value + frac;
                
            }
            get
            {
                // retrieve the Integer portion and MaterialId to caller.
                int integer;
                ModF(PlayerNumberAndMaterialId, out integer);
                return integer;
            }
        }

        // 10/12/2009; 2/3/2010: Updated to just set the ProceduralMaterialId.
        /// <summary>
        /// When set, will make an instance Flash white.
        /// </summary>
        public bool ShowFlashWhite
        {
            get { return false; }
            set
            {
                // 1st - retrieve the Int part of the original stored value
                int integer;
                var frac = ModF(PlayerNumberAndMaterialId, out integer);

                // 2/3/2010
                if (!value) return;

                // for 'ShowFlashWhite', just need to set the MaterialId=6 to Flash white version.
                const int proceduralMaterialId = 6;
                PlayerNumberAndMaterialId = proceduralMaterialId + frac;
            }
        }

        /// <summary>
        /// Splits the value x (float), into fractional and integer parts, each
        /// of which has the same sign as x.  The signed fractional portion of x is
        /// returned.  The integer portion is stored in the output parameter.
        /// </summary>
        /// <param name="numberToSplit">Float to split</param>
        /// <param name="intergerPart">(OUT) Interger part</param>
        /// <returns>Fractional part</returns>
        private static float ModF(float numberToSplit, out int intergerPart)
        {
            return numberToSplit - (intergerPart = (int)Math.Floor(numberToSplit)); 
        }
    }
}


