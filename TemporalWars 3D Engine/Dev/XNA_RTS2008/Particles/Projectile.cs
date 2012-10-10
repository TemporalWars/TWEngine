#region File Description
//-----------------------------------------------------------------------------
// Projectile.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using ImageNexus.BenScharbach.TWEngine.Common;
using ImageNexus.BenScharbach.TWEngine.Interfaces;
using ImageNexus.BenScharbach.TWEngine.ItemTypeAttributes.Structs;
using ImageNexus.BenScharbach.TWEngine.MemoryPool;
using ImageNexus.BenScharbach.TWEngine.Particles.Enums;
using ImageNexus.BenScharbach.TWEngine.SceneItems;
using ImageNexus.BenScharbach.TWTools.Particles3DComponentLibrary;
using Microsoft.Xna.Framework;

namespace ImageNexus.BenScharbach.TWEngine.Particles
{

    /// <summary>
    /// This class combines several different <see cref="ParticleSystem"/>
    /// to build up a more sophisticated composite effect. It implements a rocket
    /// projectile, which arcs up into the sky using a ParticleEmitter to leave a
    /// steady stream of trail particles behind it. After a while it explodes,
    /// creating a sudden burst of explosion and smoke particles.
    /// </summary>
    public class Projectile: ICurve3D, IDisposable
    {
        #region Constants
        
        const int NumExplosionParticles = 30;
        const int NumExplosionSmokeParticles = 50;        
        const float SidewaysVelocityRange = 60;
        const float VerticalVelocityRange = 40;
        const float Gravity = 15;
        // 10/24/2008 - Add Speed, used to figure out positions in Curve paths.
        float _speedPerSecond = 240;       

        #endregion

        #region Fields

        // 7/3/2008 - Add Curve3D Class for indirect Inheritance
        Curve3D _curve3D = new Curve3D();
        PathShape _pathShape = PathShape.Straight; // Default        
        float _curveMagnitude = 1.0f; // Default to 100%     
   
        // 12/30/2008 - Store who projectile is attacking!  This is to correct the error
        //              caused when a player then clicks on another SceneItemOwner to attack, which
        //              would then cause the AttackDamage to get applied to this new unit, rather
        //              than the original unit the projectile was intended for!
        internal SceneItem AttackSceneItem;

        

        double _time;

        // 5/13/2009 - Total Projectiles LifeSpan Allowed!
        const double LifeSpanTime = 5000; // default to 5 seconds tops!

        // 5/18/2009
        private bool _projectileDead;

        ParticleSystem _explosionParticles;
        ParticleSystem _explosionSmokeParticles;
        ParticleSystem _projectileTrailParticles;
        ParticleSystem _ballParticles; // 10/27/2008
        ParticleEmitter _trailEmitter;
        ParticleEmitter _ballEmitter; // 2/9/2009
        

        Vector3 _startPosition;
        Vector3 _targetPosition;
        Vector3 _currentPosition;
        Vector3 _velocity;

        Vector3 _tmpVelocity; // 2/9/2009

        // 11/14/2008 - 
        ///<summary>
        /// Primarily used to get the velocity of the projectile upon impact, which
        /// will influence the exploding animation.
        ///</summary>
        public Vector3 Velocity
        {
            get { return _velocity; }            
        }

        // 1/14/2011
        /// <summary>
        /// Stores the AttackDamage, since it can be different for each SpawnBullet Position!
        /// </summary>
        internal float AttackDamage { get; private set; }

        static Random _random = new Random();

        // 5/13/2009 - Ref to ProjectilePoolItem Wrapper class
        ///<summary>
        /// <see cref="ProjectilePoolItem"/> reference to wrapper class instance.
        ///</summary>
        public ProjectilePoolItem PoolItemWrapper;


        #endregion

        #region ICurve3D Interface Properties

        ///<summary>
        /// The <see cref="Curve"/> instance which represents the X axis.
        ///</summary>
        public Curve CurveX
        {
            get { return _curve3D.CurveX; }
            set { _curve3D.CurveX = value; }
        }

        ///<summary>
        /// The <see cref="Curve"/> instance which represents the Y axis.
        ///</summary>
        public Curve CurveY
        {
            get { return _curve3D.CurveY; }
            set { _curve3D.CurveY = value; }
        }

        ///<summary>
        /// The <see cref="Curve"/> instance which represents the Z axis.
        ///</summary>
        public Curve CurveZ
        {
            get { return _curve3D.CurveZ; }
            set { _curve3D.CurveZ = value; }
        }

       

        #endregion

       
        // 12/27/2008: Updated to include the 'ProjectileType' parameter.
        // 12/30/2008: Updated to change the 'targetPos' from Vector3 to the actual 'SceneItemWithPick' ref!
        // 2/4/2009: Updated to include the 'AttackDamage' parameter.
        // 2/9/2009: Updated to include the 'particlesPerSecond' parameter.
        /// <summary>
        /// Constructs a new <see cref="Projectile"/>.
        /// </summary>
        /// <param name="startPos">The starting position to spawn the bullet.</param>
        /// <param name="targetItem">The <see cref="SceneItem"/> being attacked.</param>
        /// <param name="playableItemTypeAttributes">Instance of the <see cref="PlayableItemTypeAttributes"/>.</param>
        /// <param name="index">The index value of the projectile to use.</param>
        public void ProjectileInitilization(ref Vector3 startPos, SceneItem targetItem, PlayableItemTypeAttributes playableItemTypeAttributes, int index)
        {
            // 5/13/2009 - Reset Time to zero
            _time = 0;
            _projectileDead = false; 

            // 5/18/2009
            if (targetItem == null)
                return;

            // 1/16/2010: Updated to use the new 'GetParticleSystem' method.
            // 10/7/2008 - Get ParticleSystem Services
            ParticlesManager.GetParticleSystem(ParticleSystemTypes.ExplosionParticleSystem, 0, out _explosionParticles);
            ParticlesManager.GetParticleSystem(ParticleSystemTypes.ExplosionParticleSystem, 0, out _explosionSmokeParticles); // TODO: Is this an error?
            ParticlesManager.GetParticleSystem(ParticleSystemTypes.ProjectileTrailParticleSystem, 0, out _projectileTrailParticles);
            /* _explosionParticles = Particles3D.ParticlesToDisplay[(int)ParticleSystemTypes.ExplosionParticleSystem];
            _explosionSmokeParticles = Particles3D.ParticlesToDisplay[(int)ParticleSystemTypes.ExplosionParticleSystem];
            _projectileTrailParticles = Particles3D.ParticlesToDisplay[(int)ParticleSystemTypes.ProjectileTrailParticleSystem];*/
            
            // Type of Projectile Bullet            
            switch (playableItemTypeAttributes.ProjectileTypeToUse[index])
            {
                case ProjectileType.WhiteBall:
                    ParticlesManager.GetParticleSystem(ParticleSystemTypes.BallParticleSystemWhite, 0, out _ballParticles); // 1/16/2010
                    break;
                case ProjectileType.RedBall:
                    ParticlesManager.GetParticleSystem(ParticleSystemTypes.BallParticleSystemRed, 0, out _ballParticles); // 1/16/2010
                    break;
                case ProjectileType.BlueBall:
                    ParticlesManager.GetParticleSystem(ParticleSystemTypes.BallParticleSystemBlue, 0, out _ballParticles); // 1/16/2010
                    break;
                case ProjectileType.OrangeBall:
                    ParticlesManager.GetParticleSystem(ParticleSystemTypes.BallParticleSystemOrange, 0, out _ballParticles); // 1/16/2010
                    break;
                default:
                    break;
            }

            // 7/3/2008 - Ben
            // Start & Target Position Settings.
            _startPosition = startPos;

            // 3/1/2009
            _targetPosition = targetItem.Position;

            // 12/30/2008 - Store ref to AttackSceneItem
            AttackSceneItem = targetItem;
            
            // 2/4/2009 - Store AttackDamage value
            AttackDamage = playableItemTypeAttributes.AttackDamage[index];
           
            // Elevate Target Positions a tad above ground           
            _targetPosition.Y += 25.0f;

            // Projectile Path Shape to use
            _pathShape = playableItemTypeAttributes.CurvePathShape[index];

            // Curve Magnitude % to apply
            _curveMagnitude = playableItemTypeAttributes.CurveMagnitude[index];

            // 10/27/2008 - Set Projectile's speed
            _speedPerSecond =  playableItemTypeAttributes.ProjectileSpeed[index];

            // Init Curve3D path
            InitCurve();

            // 12/9/2008 - Make sure Random is not null
            if (_random == null)
                _random = new Random();       
            

            _velocity.X = (float)(_random.NextDouble() - 0.5) * SidewaysVelocityRange;
            _velocity.Y = (float)(_random.NextDouble() + 0.5) * VerticalVelocityRange;
            _velocity.Z = (float)(_random.NextDouble() - 0.5) * SidewaysVelocityRange;

            // 2/9/2009 - Calculate the _velocity using the targetPos
            Vector3.Subtract(ref _targetPosition, ref _startPosition, out _tmpVelocity);
            if (!_tmpVelocity.Equals(Vector3.Zero)) _tmpVelocity.Normalize(); // 8/7/2009 - Avoid NaN errors, by not normalzing when zero value.
            Vector3.Multiply(ref _tmpVelocity, _speedPerSecond, out _tmpVelocity);

            // 5/13/2009 - Updated to only Create once, since this class is reused with the MemoryPool!
            // Use the particle emitter helper to output our trail particles.
            if (_trailEmitter == null)
                _trailEmitter = new ParticleEmitter(_projectileTrailParticles, playableItemTypeAttributes.ProjectileParticlesPerSecond[index], _startPosition);
            else
                _trailEmitter.ParticleEmitterInitialization(_projectileTrailParticles, playableItemTypeAttributes.ProjectileParticlesPerSecond[index], _startPosition);

            // 5/13/2009 - Updated to only Create once, since this class is reused with the MemoryPool!
            // 2/9/2009 - Use the particle emitter for our Ball Particles too.    
            if (_ballEmitter == null)
                _ballEmitter = new ParticleEmitter(_ballParticles, playableItemTypeAttributes.ProjectileParticlesPerSecond[index], _startPosition);
            else
                _ballEmitter.ParticleEmitterInitialization(_ballParticles, playableItemTypeAttributes.ProjectileParticlesPerSecond[index], _startPosition);

            // 2/9/2009
            // Create the particle.
            _ballParticles.AddParticle(_startPosition, _tmpVelocity);
            
            
        }        
       
        /// <summary>
        /// Updates the <see cref="Projectile"/>.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        /// <returns>True or False that the <see cref="Projectile"/> updated.</returns>        
        public bool Update(GameTime gameTime)
        {
            // 5/18/2009; 6/11/2010 - Check if emitters are null
            if (_projectileDead || _ballEmitter == null || _trailEmitter == null)
                return false;

            var elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // 7/3/2008
            // Calculate the Projectile's current Position using the Curve3D Class.
            var tmpTime = (float)_time;

            // 10/22/2009 - Now check if TRUE is returned, which means MaxTimeAllowed is up.
            if (GetPointOnCurve(tmpTime, out _currentPosition))
            {
                _projectileDead = true;
                return false;
            }

            _time += gameTime.ElapsedGameTime.TotalMilliseconds;

            // Simple projectile physics.            
            _velocity.Y -= elapsedTime * Gravity;

            // 2/9/2009 - Update the particle emitter, which will create our particle trail.           
            _ballEmitter.Update(gameTime, _currentPosition);

            // Update the particle emitter, which will create our particle trail.
            _trailEmitter.Update(gameTime, _currentPosition);
            
            // If within distance of target, explode! Note how we pass our _velocity
            // in to the AddParticle method: this lets the explosion be influenced
            // by the speed and direction of the projectile which created it.             
            //float result;
            //Vector3.Distance(ref _currentPosition, ref _targetPosition, out result);
            //if (result < _checkRadius) // 1/13/2009: was 10
            if (AttackSceneItem.WithinCollision(ref _currentPosition)) // 5/18/2009
            {
                for (var i = 0; i < NumExplosionParticles; i++)
                    _explosionParticles.AddParticle(_currentPosition, _velocity);

                for (var i = 0; i < NumExplosionSmokeParticles; i++)
                    _explosionSmokeParticles.AddParticle(_currentPosition, _velocity);                

                // 5/18/2009 - Place Particle below ground level, so it is out of sight until it dies!
                _currentPosition.Y = -1000;
                _ballEmitter.Update(gameTime, _currentPosition);
                _trailEmitter.Update(gameTime, _currentPosition);
                _projectileDead = true;

                return false;
            }

            // 5/13/2009 - Check if past Total LifeSpan
            if (_time >= LifeSpanTime)
            {
                _projectileDead = true;
                return false;
            }
                
            return true;
        }

        // 7/3/2008
        /// <summary>
        /// Initializes the <see cref="Curve3D"/> class for the Start and End positions for the <see cref="Projectile"/>.
        /// </summary>
        private void InitCurve()
        {
            // 5/18/2009 - Clear out any old Keys of Curve3D
            _curve3D.ClearAll();            

            // Choose Proper Curve3D Shape to use
            switch (_pathShape)
            {
                case PathShape.Straight:
                    CalculateStraightPath();
                    break;
                case PathShape.ArchUp:
                    CalculateYArchCurvePath();
                    break;
                case PathShape.ZigZagLeftRight:
                    CalculateLeftRightCurvePath();
                    break;
                default:
                    break;
            }            

        }

        // 5/13/2009
        /// <summary>
        /// Returns this instance back into the <see cref="PoolManager"/>, setting to not 'Active'.
        /// </summary>
        public void ReturnItemToPool()
        {
            // Return this instance to the PoolManager 
            //PoolItemWrapper.PoolManager.projectileItems.Return(PoolItemWrapper.PoolNode);

            // 6/29/2009
            if (PoolItemWrapper != null)
                PoolItemWrapper.PoolNode.ReturnToPool();

        }      

        #region Projectile Path Methods        
        /// <summary>
        /// Calculates the <see cref="Curve"/> path to be straight from Start to Target.
        /// </summary>
        private void CalculateStraightPath()
        {
            // 10/24/2008 - Calc Distance & Time Multiplier
            float distance;
            Vector3.Distance(ref _targetPosition, ref _startPosition, out distance);
            var timeMultiplier = distance / _speedPerSecond;

            // Add Start Point
            /*float timeAtPoint = 0;
            AddPoint(ref _startPosition, timeAtPoint);

            // End Point
            timeAtPoint += 1000 * timeMultiplier;
            AddPoint(ref _targetPosition, timeAtPoint);*/

            // 8/7/2009 - Use the new AddStraightPath version, to reduce memory garbage!
            _curve3D.AddStraightPath(timeMultiplier, ref _startPosition, ref _targetPosition);
           

            SetTangents();
        }

         
        /// <summary>
        /// Calculates the <see cref="Curve"/> path to Arch up on the Y-Axis between
        /// the Start and Target.
        /// </summary>
        private void CalculateYArchCurvePath()
        {
            // 10/24/2008 - Calc Distance
            float dist;
            Vector3.Distance(ref _targetPosition, ref _startPosition, out dist);
            var timeMultiplier = dist / _speedPerSecond;

            // Add Start Point
            float timeAtPoint = 0;
            AddPoint(ref _startPosition, timeAtPoint);

            // Calculate Middle Y-Arch Point           
            timeAtPoint += 1000 * (timeMultiplier / 2);
            var distance = (_targetPosition - _startPosition) / 2;
            var middlePosition = _startPosition + distance;
            middlePosition.Y += (100 * _curveMagnitude);
            AddPoint(ref middlePosition, timeAtPoint);

            // End Point
            timeAtPoint += 1000 * (timeMultiplier / 2);
            AddPoint(ref _targetPosition, timeAtPoint);

            // 10/22/2009 - Set MaxAllowedtime
            _curve3D.MaxTimeAllowed = timeAtPoint;

            SetTangents();

        }

        /// <summary>
        /// Calculates the <see cref="Curve"/> path to Zig-Zag left then right between
        /// the Start and Target.
        /// </summary>
        private void CalculateLeftRightCurvePath()
        {
            // 10/24/2008 - Calc Distance
            float dist;
            Vector3.Distance(ref _targetPosition, ref _startPosition, out dist);
            var timeMultiplier = dist / _speedPerSecond;

            // Add Start Point
            float timeAtPoint = 0;
            AddPoint(ref _startPosition, timeAtPoint);
            
            // Calculate 1st Middle ZigZag-Left Point
            timeAtPoint += 1000 * (timeMultiplier / 3);
            var distance = (_targetPosition - _startPosition) / 3;
            var middlePosition = _startPosition + distance;

            // Which Access to affect?
            // Let's take the Normalized Distance of the Vector,
            // and the compare the Absolute value to see which is
            // greater between X or Z; the greater value will be
            // the overall direction the projectile is traveling!
            // Therefore, if X is greater, then we know we should
            // Zig-Zag on the Z access! - Ben
            distance.Normalize();
            if (Math.Abs(distance.X) > Math.Abs(distance.Z))
                middlePosition.Z += (100 * _curveMagnitude);
            else
                middlePosition.X += (100 * _curveMagnitude);
            AddPoint(ref middlePosition, timeAtPoint);

            // Calculate 2nd Middle ZigZag-Right Point
            timeAtPoint += 1000 * (timeMultiplier / 3);
            distance = (_targetPosition - _startPosition) / 3;
            middlePosition = _startPosition + (distance * 2);

            // Which Access to affect?
            distance.Normalize();
            if (Math.Abs(distance.X) > Math.Abs(distance.Z))
                middlePosition.Z -= (100 * _curveMagnitude);
            else
                middlePosition.X -= (100 * _curveMagnitude);

            AddPoint(ref middlePosition, timeAtPoint);

            // End Point
            _time += 1000 * (timeMultiplier / 3);
            AddPoint(ref _targetPosition, timeAtPoint);

            // 10/22/2009 - Set MaxAllowedtime
            _curve3D.MaxTimeAllowed = timeAtPoint;

            SetTangents();

        }

        #endregion

        #region ICurve3D Interface Wrapper Methods

        /// <summary>
        /// Sets up the Tangents between the points on the <see cref="Curve"/>.
        /// </summary>
        public void SetTangents()
        {
            _curve3D.SetTangents();
        }

        /// <summary>
        /// Add points to create the <see cref="Curve"/> at a given <paramref name="time"/> interval.
        /// </summary>
        /// <param name="point"><see cref="Vector3"/> for Curve</param>
        /// <param name="time">Time at which we should be at this point</param>
        public void AddPoint(ref Vector3 point, float time)
        {
            _curve3D.AddPoint(ref point, time);
        }

        /// <summary>
        /// Retrieves the points, interpolated between the added points, at a given <paramref name="time"/>.
        /// </summary>
        /// <param name="time">The time to use.</param>
        /// <param name="point">(OUT) Returning a point as <see cref="Vector3"/>.</param>
        /// <returns>Vector3 Point on the Curve at the given _time.</returns>
        public bool GetPointOnCurve(float time, out Vector3 point)
        {
            return _curve3D.GetPointOnCurve(time, out point);
        }

        #endregion

        #region Dispose
        /// <summary>
        /// Disposes of unmanaged resources.
        /// </summary>
        /// <param name="disposing">Is this final dipose?</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;

            // 5/3/2009
            //if (_trailEmitter != null)
               // _trailEmitter.Dispose();

            //if (_ballEmitter != null)
                //_ballEmitter.Dispose();

            // dispose managed resources
            // Null Refs
            _curve3D = null;
            _explosionParticles = null;
            _explosionSmokeParticles = null;
            _trailEmitter = null;
            _ballEmitter = null;
            _random = null;
            _ballParticles = null;
            // free native resources
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion


    }
}


