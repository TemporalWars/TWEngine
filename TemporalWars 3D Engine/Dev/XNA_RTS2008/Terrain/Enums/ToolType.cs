#region File Description
//-----------------------------------------------------------------------------
// ToolType.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
namespace ImageNexus.BenScharbach.TWEngine.Terrain.Enums
{
    ///<summary>
    /// The <see cref="ToolType"/> Enum is used to determine the <see cref="TWEngine.Terrain"/> Tool in use.
    ///</summary>
    public enum ToolType
    {
        ///<summary>
        /// No tool is being used.
        ///</summary>
        None,
        ///<summary>
        /// Height tool edit mode.
        ///</summary>
        HeightTool,
        ///<summary>
        /// Paint tool edit mode.
        ///</summary>
        PaintTool,
        ///<summary>
        /// Item tool edit mode.
        ///</summary>
        ItemTool,
        ///<summary>
        /// Properties tool edit mode.
        ///</summary>
        PropertiesTool,
        // 1/13/2011
        ///<summary>
        /// MainMenu tool editor.
        ///</summary>
        MainMenuTool
    }
}
