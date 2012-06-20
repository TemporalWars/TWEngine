#region File Description
//-----------------------------------------------------------------------------
// UserOutput.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using System.Collections.Generic;

using StillDesign.PhysX;

namespace StillDesign
{
	public class UserOutput : UserOutputStream
	{
		public override void Print( string message )
		{
			Console.WriteLine( "PhysX: " + message );
		}
		public override AssertResponse ReportAssertionViolation( string message, string file, int lineNumber )
		{
			Console.WriteLine( "PhysX: " + message );

			return AssertResponse.Continue;
		}
		public override void ReportError( ErrorCode errorCode, string message, string file, int lineNumber )
		{
			Console.WriteLine( "PhysX: " + message );
		}
	}
}