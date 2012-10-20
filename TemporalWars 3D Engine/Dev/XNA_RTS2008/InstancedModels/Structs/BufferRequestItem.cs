#region File Description
//-----------------------------------------------------------------------------
// ChangeRequestItem.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Runtime.InteropServices;
using ImageNexus.BenScharbach.TWEngine.InstancedModels.Enums;

namespace ImageNexus.BenScharbach.TWEngine.InstancedModels.Structs
{
    // 3/10/2010: NOTE: In order to give the namespace the XML doc, must do it this way;
    /// <summary>
    /// The <see cref="TWEngine.InstancedModels.Structs"/> namespace contains the structures
    /// which make up the entire <see cref="ImageNexus.BenScharbach.TWEngine.InstancedModels.Structs" />.
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {
    }
    
    // 10/16/2012 - Optimization: In order to reduce the memory footprint of this structure, the .Net enumeration LayoutKind
    //                            will be used to reorder the memory explicitily.
    //               | BufRq | PlayerNumberAndMaterialId | Transform                                            |
    //               -------------------------------------------------------------------------------------------------------
    //               |0      |2                          |4                         |                           |68
    //
    // byte  = 1 byte
    // short = 2 bytes
    // int   = 4 bytes
    // float = 4 bytes ?

    // 7/21/2009 - BufferRequest Struct SceneItemOwner.
    ///<summary>
    /// <see cref="BufferRequestItem"/> structure stores the necessary data, to complete
    /// a change request for the given <see cref="InstancedModelPart"/>.
    ///</summary>
    //[StructLayout(LayoutKind.Explicit, Size = 68)]
    public struct BufferRequestItem
    {
        ///<summary>
        /// <see cref="BufferRequest"/> Enum type for this request.
        ///</summary>
        public BufferRequest BufferRequest;

        /// <summary>
        /// Stores PlayerNumber as Frac, and MaterialId as Int; so 1.02, would be 1 = MaterialId#1, and .02 = team#2.
        /// </summary>
        internal float PlayerNumberAndMaterialId;

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

        // 6/6/2010; 10/16/2012 - Removed obsolete explosion code.
        /*///<summary>
        /// Stores the projectiles' velocity, used specifically for explosions.
        ///</summary>
        public Vector3 ProjectileVelocity;*/
    }
}


