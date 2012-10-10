#region File Description
//-----------------------------------------------------------------------------
// InstancedModelAnimation.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using Microsoft.Xna.Framework;

namespace TWEngine.InstancedModels
{
    // 1/9/2009
    
    /*public class InstancedModelAnimation
    {
        // What ItemType and Instance this animation belongs to.
        //public ItemType ItemType;
        //public int ItemInstanceKey;
        public InstancedItemData InstancedItemData;

        // 2/6/2009 - SceneItemOwner dead?
        public bool IsDead;

        // Animation Type (1 = continuous or 2 = random)
        public int Bone1AnimationType = 1;
        public int Bone2AnimationType = 1;

        // Animation Atts for a specific bone
        // Bone-1
        public string Bone1Name;
        public int Bone1RotateOnAxis = 2; // 1="X", 2="Y", 3="Z".
        public float Bone1RotationSpeed = 1.0f; // Ex: 4.0f or 0.2f.
        // Bone-2
        public bool Bone2Animates;
        public string Bone2Name;
        public int Bone2RotateOnAxis = 2; // 1="X", 2="Y", 3="Z".
        public float Bone2RotationSpeed = 1.0f; // Ex: 4.0f or 0.2f.

        private float _rotTime;
        private Matrix _doTranslation;

        private readonly TimeSpan _resetTimeSpan = new TimeSpan(0, 0, 10);

        private readonly Random _rndGenerator = new Random();
        // Bone-1
        private float _bone1FacingDirection, _bone1OldFacingDirection;
        private TimeSpan _bone1TimeToRandomAngleMove = new TimeSpan(0, 0, 10);        
        private float _bone1DesiredAngle;
        // Bone-2
        private float _bone2FacingDirection, _bone2OldFacingDirection;
        private TimeSpan _bone2TimeToRandomAngleMove = new TimeSpan(0, 0, 10);
        private float _bone2DesiredAngle;

        // Updates the animation for the SceneItemOwner.
        public void UpdateAnimation(GameTime gameTime)
        {
            _rotTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

            // 2/6/2009 - Skip if SceneItemOwner just killed.
            if (IsDead)
                return;

            //CalculateAnimation_Bone1(ref tmpElapsedTime);

            //CalculateAnimation_Bone2(ref tmpElapsedTime);
        }


        /// <summary>
        /// Calculates the Animation for Bone-1
        /// </summary>
        private void CalculateAnimation_Bone1(ref TimeSpan elapsedTime)
        {
            // What animation type to calc?
            switch (Bone1AnimationType)
            {
                case 1:
                    CalculateMatrixTransform(_rotTime, Bone1RotateOnAxis, Bone1RotationSpeed, out _doTranslation);
                    InstancedItem.SetAdjustingBoneTransform(ref InstancedItemData, Bone1Name, ref _doTranslation);
                    break;
                case 2:
                    _bone1TimeToRandomAngleMove -= elapsedTime;
                    if (_bone1TimeToRandomAngleMove.Milliseconds <= 0.0f)
                    {
                        // Get new angle to move fan base to                
                        _bone1DesiredAngle = MathHelper.ToRadians(_rndGenerator.Next(-180, 180));
                        // Reset timer
                        _bone1TimeToRandomAngleMove = _resetTimeSpan;
                    } // End if Time Expired
                    _bone1FacingDirection = TurnToFace(_bone1DesiredAngle, _bone1FacingDirection, Bone1RotationSpeed);
                    if (Math.Abs(_bone1FacingDirection - _bone1OldFacingDirection) > 0.01f)
                    {
                        // Calc the Matrix rotation Transform for bone-1.
                        CalculateMatrixTransform(_bone1FacingDirection, Bone1RotateOnAxis, 1, out _doTranslation);
                        // Set Adjusting Bone Transform
                        InstancedItem.SetAdjustingBoneTransform(ref InstancedItemData, Bone1Name, ref _doTranslation);

                        _bone1OldFacingDirection = _bone1FacingDirection;
                    }
                    break;
            }
        }

        /// <summary>
        /// Calculates the Animation for Bone-2
        /// </summary>
        private void CalculateAnimation_Bone2(ref TimeSpan elapsedTime)
        {
            // Check if Bone-2 Animation
            if (!Bone2Animates) return;

            // What animation type to calc?
            switch (Bone2AnimationType)
            {
                case 1:
                    CalculateMatrixTransform(_rotTime, Bone2RotateOnAxis, Bone2RotationSpeed, out _doTranslation);
                    InstancedItem.SetAdjustingBoneTransform(ref InstancedItemData, Bone2Name, ref _doTranslation);
                    break;
                case 2:
                    _bone2TimeToRandomAngleMove -= elapsedTime;
                    if (_bone2TimeToRandomAngleMove.Milliseconds <= 0.0f)
                    {
                        // Get new angle to move fan base to                
                        _bone2DesiredAngle = MathHelper.ToRadians(_rndGenerator.Next(-180, 180));
                        // Reset timer
                        _bone2TimeToRandomAngleMove = _resetTimeSpan;   // was TimeSpan.FromSeconds(10);
                    } // End if Time Expired
                    _bone2FacingDirection = TurnToFace(_bone2DesiredAngle, _bone2FacingDirection, Bone2RotationSpeed);
                    if (Math.Abs(_bone2FacingDirection - _bone2OldFacingDirection) > 0.01f)
                    {
                        // Calc the Matrix rotation Transform for bone-1.
                        CalculateMatrixTransform(_bone2FacingDirection, Bone2RotateOnAxis, 1, out _doTranslation);
                        // Set Adjusting Bone Transform
                        InstancedItem.SetAdjustingBoneTransform(ref InstancedItemData, Bone2Name, ref _doTranslation);

                        _bone2OldFacingDirection = _bone2FacingDirection;
                    }
                    break;
            }
        }

        /// <summary>
        /// Calculates the Matrix Transform needed to perform the given rotation request.
        /// </summary>
        /// <param name="rotTime"></param>
        /// <param name="rotateOnAxis"></param>
        /// <param name="rotationSpeed"></param>
        /// <param name="rotTransform"></param>
        public static void CalculateMatrixTransform(float rotTime, int rotateOnAxis, float rotationSpeed, out Matrix rotTransform)
        {
            rotTransform = Matrix.Identity;

            // Which Axis Rotating on?
            switch (rotateOnAxis)
            {
                case 1: // X
                    Matrix.CreateRotationX(rotTime * rotationSpeed, out rotTransform);
                    break;
                case 2: // Y
                    Matrix.CreateRotationY(rotTime * rotationSpeed, out rotTransform);
                    break;
                case 3: // Z
                    Matrix.CreateRotationZ(rotTime * rotationSpeed, out rotTransform);
                    break;
                default:
                    break;
            } // End Switch
        }


        #region TurnToFace Helper for Animation method

        /// <summary>
        /// Calculates the angle that an object should face, given its Position, its
        /// target's Position, its current angle, and its maximum turning speed.
        /// </summary>
        //float difference, currentAngleDiff, newDirection;
        static float TurnToFace(float desiredAngle, float currentAngle, float turnSpeed)
        {
            // so now we know where we WANT to be facing, and where we ARE facing...
            // if we weren't constrained by turnSpeed, this would be easy: we'd just 
            // return desiredAngle.
            // instead, we have to calculate how much we WANT to turn, and then make
            // sure that's not more than turnSpeed.

            // first, figure out how much we want to turn, using WrapAngle to get our
            // result from -Pi to Pi ( -180 degrees to 180 degrees )
            float currentAngleDiff = desiredAngle - currentAngle;
            float difference = MathHelper.WrapAngle(currentAngleDiff);

            // clamp that between -turnSpeed and turnSpeed.
            difference = MathHelper.Clamp(difference, -turnSpeed, turnSpeed);

            // so, the closest we can get to our target is currentAngle + difference.
            // return that, using WrapAngle again.
            currentAngleDiff = currentAngle + difference;
            float newDirection = MathHelper.WrapAngle(currentAngleDiff);

            return newDirection;
        }

        #endregion
    }*/
}
