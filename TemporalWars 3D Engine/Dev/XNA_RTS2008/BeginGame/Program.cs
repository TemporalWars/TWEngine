#region File Description
//-----------------------------------------------------------------------------
// Program.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;

namespace TWEngine
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (var game = new TemporalWars3DEngine())
            {        
                game.Run();              
                
            }
        }
    }
}