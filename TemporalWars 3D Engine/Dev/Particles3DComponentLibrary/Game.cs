#region File Description
//-----------------------------------------------------------------------------
// Game.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements

using Microsoft.Xna.Framework;

#endregion

namespace ImageNexus.BenScharbach.TWTools.Particles3DComponentLibrary
{
    /// <summary>
    /// Sample showing how to implement a particle system entirely
    /// on the GPU, using the vertex shader to animate particles.
    /// </summary>
    public class Particle3DSampleGame : DrawableGameComponent
    {
       

        #region Initialization

        /// <summary>
        /// Constructor.
        /// </summary>
        public Particle3DSampleGame(Game game) 
            : base(game)
        {
        }

        /// <summary>
        /// Load your graphics content.
        /// </summary>
        protected override void LoadContent()
        {
        }

        #endregion

        #region Update and Draw

        /// <summary>
        /// Allows the game to run logic.
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        // 3/24/2011
        /// <summary>
        /// Adds the <see cref="ParticleSystem"/> to the <see cref="DrawableGameComponent"/> collection.
        /// </summary>
        public void AddParticleSystemToDraw(ParticleSystem particleSystem)
        {
            // Register the particle system components.
            Game.Components.Add(particleSystem);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            // This will draw the particle system components.
            base.Draw(gameTime);
        }

        #endregion

    }


   
}
