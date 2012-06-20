#region File Description
//-----------------------------------------------------------------------------
// ShapeWithPick.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using TWEngine.Interfaces;

namespace TWEngine.Shapes
{
    /// <summary>
    /// The <see cref="ShapeWithPick"/> class adds the ability to 'Pick' an item, which
    /// inherits from the <see cref="Shape"/> class.
    /// </summary>
    public class ShapeWithPick : Shape
    {    

        // The model (10/16/2008): Made Internal, so access given to SceneItemWithPick
        internal Model ModelInstance;

        /// <summary>
        /// The team this <see cref="ShapeWithPick"/> belongs to (MP).
        /// </summary>
        protected internal byte PlayerNumber; // 

        /// <summary>
        /// Cursor is a DrawableGameComponent that draws a Cursor on the screen. It works
        /// differently on Xbox and Windows. On windows, this will be a Cursor that is
        /// controlled using both the mouse and the gamepad. On Xbox, the Cursor will be
        /// controlled using only the gamepad.
        /// </summary>
        protected ICursor Cursor; 
     

        ///<summary>
        /// Constructor, which retrieves the <see cref="ICursor"/> service.
        ///</summary>
        ///<param name="game"><see cref="Game"/> instance</param>
        public ShapeWithPick(Game game)
            : base(game)
        {
            // 8/26/2008 - Get Cursor Interface Ref
            Cursor = (ICursor)game.Services.GetService(typeof(ICursor));
        }


        /// <summary>
        /// Creates the vertex buffers etc. This routine is called on object creation and on Device reset etc
        /// </summary>
        public override void Create()
        {
            return;
        }

        /// <summary>
        /// Called when a Device is created
        /// </summary>
        public override void OnCreateDevice()
        {
            return;
        }

        /// <summary>
        /// Renders the <see cref="ShapeWithPick"/>. 
        /// </summary>
        /// <remarks><see cref="ShapeWithPick"/> class does nothing.</remarks>
        public override void Render()
        {
            return;
        }

        Ray _cursorRay;
        Matrix[] _modelAbsoluteBoneTransforms;
       
        // 2/2/2010 - Updated to include (OUT) param for distance of ray hit.
        /// <summary>
        /// Overload v1: Checks if the mesh is picked, using a <see cref="Ray"/> from <see cref="ICursor"/>.
        /// </summary>
        /// <param name="intersectionDistance">(OUT) Intersection distance</param>
        /// <returns>True/Fale of result.</returns>
        public virtual bool IsMeshPicked(out float? intersectionDistance)
        {
            // If the Cursor is over a model, we'll draw its name. To figure out if
            // the Cursor is over a model, we'll use Cursor.CalculateCursorRay. That
            // function gives us a World space ray that starts at the "eye" of the
            // camera, and shoots out in the direction pointed to by the Cursor.

            // 2/2/2010
            intersectionDistance = null;

            // 5/5/2008 - Check if Custom Cursor exist; if so, then we will use the Custom Cursor Position for the Ray.            
            Common.Cursor.CalculateCursorRay(out _cursorRay);

            if (ModelInstance == null)
                return false;

            // create an array of matrices to hold the absolute bone transforms,
            // calculate them, and copy them in.
            if (_modelAbsoluteBoneTransforms == null)
                _modelAbsoluteBoneTransforms = new Matrix[ModelInstance.Bones.Count];

            ModelInstance.CopyAbsoluteBoneTransformsTo(_modelAbsoluteBoneTransforms);

            // check to see if the _cursorRay intersects the model....
            intersectionDistance = RayIntersectsModel(ref _cursorRay, ref ModelInstance, ref World, ref _modelAbsoluteBoneTransforms);

            // 2/2/2010
            return intersectionDistance != null;
        }

        // 2/2/2010 - Updated to include (OUT) param for distance of ray hit.
        /// <summary>
        /// Overload v2: Checks if the mesh is picked, using a <see cref="Ray"/> provided by caller.
        /// </summary>
        /// <param name="ray"><see cref="Ray"/> structure to use</param>
        /// <param name="intersectionDistance">(OUT) Intersection distance</param>
        /// <returns>True/Fale of result.</returns>
        public bool IsMeshPicked(ref Ray ray, out float? intersectionDistance)
        {
            // 2/2/2010
            intersectionDistance = null;

            if (ModelInstance == null) return false;

            // create an array of matrices to hold the absolute bone transforms,
            // calculate them, and copy them in. 
            if (_modelAbsoluteBoneTransforms == null)
                _modelAbsoluteBoneTransforms = new Matrix[ModelInstance.Bones.Count];

            ModelInstance.CopyAbsoluteBoneTransformsTo(_modelAbsoluteBoneTransforms);

            // check to see if the given Ray intersects the model....
            intersectionDistance = RayIntersectsModel(ref ray, ref ModelInstance, ref World, ref _modelAbsoluteBoneTransforms);

            // 2/2/2010
            return intersectionDistance != null;
        }

        static BoundingSphere _tmpMeshBoundingSphere;

        /// <summary>
        /// This helper function checks to see if a <see cref="Ray"/> will intersect with a <see cref="Model"/>.
        /// The model's bounding spheres are used, and the model is transformed using
        /// the matrix specified in the <paramref name="worldTransform"/> argument.
        /// </summary>
        /// <param name="ray"><see cref="Ray"/> structure to use</param>
        /// <param name="model"><see cref="Model"/> instance</param>
        /// <param name="worldTransform">World <see cref="Matrix"/> transform</param>
        /// <param name="absoluteBoneTransforms">Collection of <see cref="Matrix"/> absolute transo</param>
        /// <returns>true if the ray intersects the model.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="model"/> is null.</exception>
        private static float? RayIntersectsModel(ref Ray ray, ref Model model,
            ref Matrix worldTransform, ref Matrix[] absoluteBoneTransforms)
        {
            // 4/30/2010 - Check if model null.
            if (model == null)
                throw  new ArgumentNullException("model", @"The Model given is null, which is not allowed with the current method call.");

            // 4/30/2010 - Cache
            var modelMeshCollection = model.Meshes; 
            if (modelMeshCollection == null) return null;

            // Each ModelMesh in a Model has a bounding sphere, so to check for an
            // intersection in the Model, we have to check every mesh.
            // 8/26/2008: Updated to use For-Loop, rather than ForEach
            var count = modelMeshCollection.Count; // 4/30/2010 - Cache
            for (var i = 0; i < count; i++)
            {
                // 4/30/2010 - Cache
                var modelMesh = modelMeshCollection[i];
                if (modelMesh == null) continue;

                // the mesh's BoundingSphere is stored relative to the mesh itself.
                // (Mesh space). We want to get this BoundingSphere in terms of World
                // coordinates. To do this, we calculate a matrix that will Transform
                // from coordinates from mesh space into World space....
                var world = absoluteBoneTransforms[modelMesh.ParentBone.Index] * worldTransform;

                // ... and then Transform the BoundingSphere using that matrix.
                _tmpMeshBoundingSphere = modelMesh.BoundingSphere;
                var sphere = TransformBoundingSphere(ref _tmpMeshBoundingSphere, ref world);

                // now that the we have a sphere in World coordinates, we can just use
                // the BoundingSphere class's Intersects function. Intersects returns a
                // nullable float (float?). This value is the distance at which the ray
                // intersects the BoundingSphere, or null if there is no intersection.
                // so, if the value is not null, we have a collision.
                float? intersection;
                sphere.Intersects(ref ray, out intersection);
                if (intersection != null)
                {
                    return intersection;
                }
            }
         
            return null;
        }

        /// <summary>
        /// This helper function takes a <see cref="BoundingSphere"/> and a matrix <paramref name="transform"/>, then
        /// returns a transformed version of that <see cref="BoundingSphere"/>.
        /// </summary>
        /// <param name="sphere"><see cref="BoundingSphere"/> structure to transform</param>
        /// <param name="transform"><see cref="Matrix"/> transform to apply</param>
        /// <returns>The transformed <see cref="BoundingSphere"/></returns> 
        protected static BoundingSphere TransformBoundingSphere(ref BoundingSphere sphere,
                                                                ref Matrix transform)
        {
            BoundingSphere transformedSphere;

            // the Transform can contain different scales on the x, y, and z components.
            // this has the effect of stretching and squishing our bounding sphere along
            // different axes. Obviously, this is no good: a bounding sphere has to be a
            // SPHERE. so, the transformed sphere's radius must be the maximum of the 
            // scaled x, y, and z radii.

            // to calculate how the Transform matrix will affect the x, y, and z
            // components of the sphere, we'll create a vector3 with x y and z equal
            // to the sphere's radius...
            //Vector3 _scale3 = new Vector3(sphere.Radius, sphere.Radius, sphere.Radius);
            var scale3 = Vector3.Zero;
            scale3.X = sphere.Radius; scale3.Y = sphere.Radius; scale3.Z = sphere.Radius;

            // then Transform that vector using the Transform matrix. we use
            // TransformNormal because we don't want to take translation into account.
            //_scale3 = Vector3.TransformNormal(_scale3, Transform);
            Vector3.TransformNormal(ref scale3, ref transform, out scale3);

            // _scale3 contains the x, y, and z radii of a squished and stretched sphere.
            // we'll set the finished sphere's radius to the maximum of the x y and z
            // radii, creating a sphere that is large enough to contain the original 
            // squished sphere.
            transformedSphere.Radius = Math.Max(scale3.X, Math.Max(scale3.Y, scale3.Z));

            // transforming the center of the sphere is much easier. we can just use 
            // Vector3.Transform to Transform the center vector. notice that we're using
            // Transform instead of TransformNormal because in this case we DO want to 
            // take translation into account.
            var tmpCenter1 = sphere.Center;
            Vector3 tmpCenter2;
            //transformedSphere.Center = Vector3.Transform(sphere.Center, Transform);
            Vector3.Transform(ref tmpCenter1, ref transform, out tmpCenter2);
            transformedSphere.Center = tmpCenter2;

            return transformedSphere;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public override void Dispose()
        {
            Dispose(true);
            base.Dispose();
        }

        // 4/5/2009 - Dispose of resources
        /// <summary>
        /// Disposes of unmanaged resources.
        /// </summary>
        /// <param name="all">Is this final dispose?</param>
        private void Dispose(bool all)
        {
            if (!all) return;

            // Null Refs
            ModelInstance = null;
            Cursor = null;
            Game = null;

            // 
            // 1/5/2010 - Note: Up to this point, no InternalDriverError will be thrown in the SpriteBatch.
            //          - Note: Discovered, the error is coming from the call to 'Unload()' Content items below!


            // 1/5/2010
            // NOTE: Fix: Major; removed call to Content.Unload() methods below, to now be in Dispose of the main IN class!
            //       This removed the HUGE 'InternalDriverError' exception which will occur on the XBOX if left here!!!
            //
            // 11/17/2009 - Updated to use Unload, rather than Dispose.
            /*if (ImageNexusRTSGameEngine.Content != null)
                ImageNexusRTSGameEngine.Content.Unload();*/

            if (TemporalWars3DEngine.ContentGroundTextures != null)
                TemporalWars3DEngine.ContentGroundTextures.Unload();
                
            if (TemporalWars3DEngine.ContentMaps != null)
                TemporalWars3DEngine.ContentMaps.Unload();

           
        }      
                
    }
}
