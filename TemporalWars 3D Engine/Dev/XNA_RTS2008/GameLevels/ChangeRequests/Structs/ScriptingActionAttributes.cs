#region File Description
//-----------------------------------------------------------------------------
// ScriptingActionAttributes.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System.Collections.Generic;
using TWEngine.GameLevels.ChangeRequests.Enums;

namespace TWEngine.GameLevels.ChangeRequests.Structs
{
    /// <summary>
    /// The <see cref="ScriptingActionAttributes"/> structure holds the request for each movement request on a single edge.
    /// </summary>
    public struct ScriptingActionAttributes
    {
        public ScriptingActionChangeRequestAttributes ChangeRequestAttributes { get; set; }
        public ScriptingActionChangeRequestEnum ChangeRequestEnum { get; set; }
    }
}