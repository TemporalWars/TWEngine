#region File Description
//-----------------------------------------------------------------------------
// CinematicNode.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using Microsoft.Xna.Framework;

namespace ImageNexus.BenScharbach.TWEngine.GameCamera.Structs
{
    // 3/10/2010: NOTE: In order to give the namespace the XML doc, must do it this way;
    /// <summary>
    /// The <see cref="TWEngine.GameCamera.Structs"/> namespace contains the common structures
    /// which make up the entire <see cref="TWEngine.GameCamera"/> component.
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {
    }

    ///<summary>
    /// The <see cref="CinematicNode"/> movement structure is used for the
    /// <see cref="CameraCinematics"/>. (Scripting Purposes)
    ///</summary>
    public struct CinematicNode
    {
        ///<summary>
        /// Cinematic node position
        ///</summary>
        public Vector3 Position;

        ///<summary>
        /// Enter value 0-1.0, representing the relative height
        /// above ground level, at the given <see cref="Position"/>.
        ///</summary>
        public float HeightZoom;

        ///<summary>
        /// Time to complete value, used to interpolate
        /// the speed of movement when transitioning between
        /// <see cref="CinematicNode"/> structures.
        ///</summary>
        public int TimeToCompleteInMilliSeconds;
    }
}