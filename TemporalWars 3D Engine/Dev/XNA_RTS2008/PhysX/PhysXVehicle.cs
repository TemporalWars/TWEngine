#region File Description
//-----------------------------------------------------------------------------
// PhysXVehicle.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using Microsoft.Xna.Framework;
#if !XBOX360 // 6/22/2009
using StillDesign.PhysX;

#endif

namespace TWEngine.PhysX
{
    // 6/23/2009
    class PhysXVehicle
    {
        private static Actor _vehicleBodyActor;
        private static Vector3 _forceToApply = Vector3.Zero;

		public static void CreateVehicle(ref Vector3 position, int mass)
		{
			// Create a 2 ton vehicle
			var bodyDesc = new BodyDescription
			                   {
                Mass = mass                
			};				

			var actorDesc = new ActorDescription
			                    {
				BodyDescription = bodyDesc,
				Shapes = { new BoxShapeDescription( 5, 3, 7 ) },
                GlobalPose = Matrix.CreateTranslation( position )
                
			};

			_vehicleBodyActor = PhysXEngine.PhysXScene.CreateActor( actorDesc );
            
			_vehicleBodyActor.SetCenterOfMassOffsetLocalPosition( new Vector3( 0, -1.5f, 0 ) ); // Move the COM to the bottom of the vehicle to stop it flipping over so much

			
		}

        // Updates Vehicle force; called from 'PhysxEngine' update method.
        internal static void UpdateForceForVehicle(GameTime gameTime)
        {
            if (_vehicleBodyActor == null)
                return;

            Vector3 forceToApplyElapsed;
            Vector3.Multiply(ref _forceToApply, (float)gameTime.ElapsedGameTime.TotalSeconds, out forceToApplyElapsed);

            _vehicleBodyActor.AddForce(forceToApplyElapsed, ForceMode.SmoothVelocityChange);
            _forceToApply = Vector3.Zero; // reset
        }

        public static void ApplyForceToVehicle(Vector3 force)
        {
            _forceToApply = force;
        }


        public static Vector3 GetNewVehiclePositionData()
        {
            return _vehicleBodyActor.GlobalPosition;
        }
		
    }
}
