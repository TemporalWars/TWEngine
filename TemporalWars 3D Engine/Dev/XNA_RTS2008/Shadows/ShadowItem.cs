#region File Description
//-----------------------------------------------------------------------------
// ShadowItem.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using TWEngine.GameCamera;
using TWEngine.Interfaces;
using TWEngine.SceneItems;

namespace TWEngine.Shadows
{
    // 3/10/2010: NOTE: In order to give the namespace the XML doc, must do it this way;
    /// <summary>
    /// The <see cref="TWEngine.Shadows"/> namespace contains the classes
    /// which make up the entire <see cref="Shadows"/> component.
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {
    }
    
    

    ///<summary>
    /// The <see cref="ShadowItem"/> is used to store a <see cref="SceneItem"/> shadowing
    /// data, like the effect to use, world position, and bone transforms.
    ///</summary>
    public struct ShadowItem : IShadowShapeItem, IDisposable
    {
        // 8/13/2008 - Add ContentManager Instance
        private static ContentManager _contentManager;

        // ShadowMap Shader
        private static Effect _shadowMapEffect;
        // Store Original BasicEffect for SceneItemOwner
        private List<BasicEffect> _basicEffect;
        // Store Original Effect for SceneItemOwner
        private List<Effect> _orgEffect;
        // Was BasicEffect
        private bool _isBasicEffect;
       
        ///<summary>
        /// Collection of <see cref="Matrix"/> as bone transforms.
        ///</summary>
        public Matrix[] BoneTransforms;
       
        private Matrix _world;       
        private Model _model;

        #region Properties     
        

        ///<summary>
        /// XNA type <see cref="Model"/>.
        ///</summary>
        public Model Model
        {
            get { return _model; }
            set { _model = value; }
        }

        ///<summary>
        /// <see cref="Matrix"/> world matrix.
        ///</summary>
        public Matrix WorldP
        {
            get { return _world; }
            set { _world = value; }
        }

        /// <summary>
        /// If <see cref="SceneItem"/> cast a shadow?
        /// </summary>
        public bool ModelCastShadow { get; set; }

        /// <summary>
        /// If model animates?
        /// </summary>
        public bool ModelAnimates { get; set; }

        /// <summary>
        /// In <see cref="Camera"/> Frustrum? 
        /// </summary>
        /// <remarks>Checked by <see cref="ShadowMap"/> Draw.</remarks>
        public bool InCameraFrustrum { get; set; }

        #endregion

        ///<summary>
        /// Constructor, which creates the internal <see cref="ContentManager"/>, loads the 'ShadowEffect' shader
        /// into memory, and creates the required collections.
        ///</summary>
        ///<param name="game"><see cref="Game"/> instance</param>
        ///<param name="modelAnimates">If model animates?</param>
        public ShadowItem(Game game, bool modelAnimates) : this()
        {          
            // 4/6/2010: Updated to use 'ContentMiscLoc' global var.
            // 8/13/2008
            if (_contentManager == null)
                _contentManager = new ContentManager(game.Services, TemporalWars3DEngine.ContentMiscLoc); // was "Content"

            // 1/20/2011 - TODO: Dictionary thread add/insert error.
            // 1/21/2011 - NOTE: Testing shows it seems to only occur on XBOX reloads, and 1 item.
            try 
            {
                // Load ShadowMap Shader
                if (_shadowMapEffect == null)
                    _shadowMapEffect = _contentManager.Load<Effect>(@"Shaders\ShadowEffect");

            }
            catch (Exception exp)
            {
                Debug.WriteLine(string.Format("ShadowItem constructor threw an exception {0}", exp.Message));
            }
           
            _basicEffect = new List<BasicEffect>();
            _orgEffect = new List<Effect>();

            _isBasicEffect = false;
            ModelCastShadow = false;
            InCameraFrustrum = true;

            BoneTransforms = new Matrix[1];
            _world = new Matrix();
            _model = null;

            ModelAnimates = modelAnimates;            

        }     
   
        // 8/14/2008
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            // Dispose of resources
            if (_shadowMapEffect != null)
                _shadowMapEffect.Dispose();

            // Arrays
            var count = _basicEffect.Count; // 4/30/2010
            for (var i = 0; i < count; i++)
            {
                // 4/30/2010 - Cache
                var basicEffect = _basicEffect[i];
                if (basicEffect == null) continue;

                basicEffect.Dispose();
                _basicEffect[i] = null;
            }
            _basicEffect.Clear();

            var count2 = _orgEffect.Count; // 4/30/2010
            for (var j = 0; j < count2; j++)
            {
                // 4/30/2010 - Cache
                var effect = _orgEffect[j];
                if (effect == null) continue;

                effect.Dispose();
                _orgEffect[j] = null;
            }
            _orgEffect.Clear();
            

            // Nulls Refs
            _model = null;
            BoneTransforms = null;
            _shadowMapEffect = null;
            _basicEffect = null;
            _orgEffect = null;

            if (_contentManager == null) return;

            _contentManager.Unload();
            _contentManager.Dispose();
            _contentManager = null;
        }

        
        // 7/8/2008
        /// <summary>
        /// Stores the Model's Original Effect, since it will be removed every
        /// cycle to apply the ShadowEffect Shader.
        /// </summary>
        /// <param name="model">Model to pass in</param>
        /// <param name="isBasicEffect">Is Basic Effect being Passed in</param>
        public void StoreModelEffect(ref Model model, bool isBasicEffect)
        {
            var count = model.Meshes.Count; // 4/30/2010
            if (isBasicEffect)
            {
                _isBasicEffect = true;
                // 8/27/2008: Updated to use ForLoop, rather than ForEach.
                for (var i = 0; i < count; i++)
                {
                    // 4/30/2010 - Cache
                    var modelMesh = model.Meshes[i];
                    if (modelMesh == null) continue;

                    // 4/30/2010 - Cache
                    var modelEffectCollection = modelMesh.Effects;
                    var effectsCount = modelEffectCollection.Count;
                    for (var j = 0; j < effectsCount; j++)
                    {
                        _basicEffect.Add((BasicEffect)modelEffectCollection[j]); 
                    }
                }
            }
            else
            {
                _isBasicEffect = false;
                // 8/27/2008: Updated to use ForLoop, rather than ForEach.
                for (var i = 0; i < count; i++)
                {
                    // 4/30/2010 - Cache
                    var modelMesh = model.Meshes[i];
                    if (modelMesh == null) continue;

                    // 4/30/2010 - Cache
                    var modelEffectCollection = modelMesh.Effects;
                    var effectsCount = modelEffectCollection.Count;
                    for (var j = 0; j < effectsCount; j++)
                    {
                        _orgEffect.Add(modelEffectCollection[j]);
                    }
                }
            }
        }
        
        ///<summary>
        /// Draws the <see cref="SceneItem"/> using the <see cref="ShadowMap"/> shader, which will project the shadow for this
        /// <see cref="SceneItem"/> onto the <see cref="ShadowMap"/>.
        ///</summary>
        ///<param name="lightView"><see cref="Matrix"/> as light view</param>
        ///<param name="lightProj"><see cref="Matrix"/> as light projection</param>
        public void DrawForShadowMap(ref Matrix lightView, ref Matrix lightProj)
        {
            // 4/30/2010 - Cache
            var model = _model;
            if (model == null) return;

            if (ModelAnimates)
                model.CopyAbsoluteBoneTransformsTo(BoneTransforms);

            // Remap _model to use the ShadowMap Shader
            RemapModel(model, _shadowMapEffect);

            // 4/30/2010
            var modelMeshCollection = model.Meshes;
            if (modelMeshCollection == null) return;

            // Draw Model using ShadowMap Shader
            // 8/27/2008: Updated to use For-Loop, rather than ForEach.

            var count = modelMeshCollection.Count; // 4/30/2010
            for (var i = 0; i < count; i++)
            {
                // 4/30/2010 - Cache
                var modelMesh = modelMeshCollection[i];
                if (modelMesh == null) continue;

                // 4/30/2010 - Cache
                var modelEffectCollection = modelMesh.Effects;
                var effectsCount = modelEffectCollection.Count;
                for (var j = 0; j < effectsCount; j++)
                {
                    // 4/30/2010 - Cache
                    var modelEffect = modelEffectCollection[j];
                    if (modelEffect == null) continue;

                    if (ModelAnimates)
                    {                      
                        modelEffect.Parameters["xWorld"].SetValue(BoneTransforms[modelMesh.ParentBone.Index] * WorldP);
                        modelEffect.Parameters["xLightView"].SetValue(lightView);
                        modelEffect.Parameters["xLightProjection"].SetValue(lightProj);
                    }
                    else
                    {                        
                        modelEffect.Parameters["xWorld"].SetValue(WorldP);
                        modelEffect.Parameters["xLightView"].SetValue(lightView);
                        modelEffect.Parameters["xLightProjection"].SetValue(lightProj);
                    }

                    modelEffect.CurrentTechnique = _shadowMapEffect.Techniques["ShadowMapRender"];   
                }
                modelMesh.Draw();
            }           

            // Remap _model back to BasicEffect or Custom Effect
            if (_isBasicEffect)
                RemapModelToBasic(model, _basicEffect);
            else
                RemapModelToCustom(model, _orgEffect);
               
        }
         

        // 3/7/2008 - 
        /// <summary>
        /// Helper Function to map the <see cref="Terrain"/> effect to the custom 
        /// 'MultiTexture' shader.
        /// </summary>
        /// <param name="model"><see cref="Model"/> instance</param>
        /// <param name="effect"><see cref="Effect"/> instance</param>
        private static void RemapModel(Model model, Effect effect)
        {
            // 10/19/2008
            if (model == null || effect == null) return;

            // 4/30/2010 - Cache
            var modelMeshCollection = model.Meshes;
            if (modelMeshCollection == null) return;

            // 8/27/2008: Updated to use ForLoop, rather than ForEach.
            var count = modelMeshCollection.Count; // 4/30/2010
            for (var i = 0; i < count; i++)
            {
                // 4/30/2010 - Cache
                var modelMesh = modelMeshCollection[i];
                if (modelMesh == null) continue;

                // 4/30/2010 - Cache
                var modelMeshPartCollection = modelMesh.MeshParts;
                if (modelMeshPartCollection == null) continue;

                var count1 = modelMeshPartCollection.Count; // 4/30/2010
                for (var j = 0; j < count1; j++)
                {
                    // 4/30/2010 - Cache
                    var modelMeshPart = modelMeshPartCollection[j];
                    if (modelMeshPart == null) continue;

                    modelMeshPart.Effect = effect;
                }
            }
        }

        // 6/3/2008 - 
        /// <summary>
        /// Helper Function to map the <see cref="BasicEffect"/> back to <paramref name="model"/>.   
        /// </summary>
        /// <param name="model"><see cref="Model"/> instance</param>
        /// <param name="basicEffect">Collection of <see cref="BasicEffect"/></param>
        private static void RemapModelToBasic(Model model, IList<BasicEffect> basicEffect)
        {
            // 10/19/2008
            if (model == null || basicEffect == null) return;

            // 4/30/2010 - Cache
            var modelMeshCollection = model.Meshes;
            if (modelMeshCollection == null) return;

            // 8/27/2008: Updated to use ForLoop, rather than ForEach.
            var beIndex = 0;

            var count = modelMeshCollection.Count; // 4/30/2010
            for (var i = 0; i < count; i++)
            {
                // 4/30/2010 - Cache
                var modelMesh = modelMeshCollection[i];
                if (modelMesh == null) continue;

                // 4/30/2010 - Cache
                var modelMeshPartCollection = modelMesh.MeshParts;
                if (modelMeshPartCollection == null) continue;

                var count1 = modelMeshPartCollection.Count; // 4/30/2010
                for (var j = 0; j < count1; j++)
                {
                    // 4/30/2010 - Cache
                    var modelMeshPart = modelMeshPartCollection[j];
                    if (modelMeshPart == null) continue;

                    modelMeshPart.Effect = basicEffect[beIndex];
                    beIndex++;
                }
            }
        }

        // 7/8/2008 - 
        /// <summary>
        /// Helper Function to map the Custom <paramref name="orgEffect"/> back to <see cref="model"/>
        /// </summary>
        /// <param name="model"><see cref="Model"/> instance</param>
        /// <param name="orgEffect">Collection of <see cref="Effect"/></param>
        private static void RemapModelToCustom(Model model, IList<Effect> orgEffect)
        {
            // 10/19/2008
            if (model == null || orgEffect == null) return;

            // 4/30/2010 - Cache
            var modelMeshCollection = model.Meshes;
            if (modelMeshCollection == null) return;

            // 8/27/2008: Updated to use ForLoop, rather than ForEach.
            var beIndex = 0;

            var count = modelMeshCollection.Count; // 4/30/2010
            for (var i = 0; i < count; i++)
            {
                // 4/30/2010 - Cache
                var modelMesh = modelMeshCollection[i];
                if (modelMesh == null) continue;

                // 4/30/2010 - Cache
                var modelMeshPartCollection = modelMesh.MeshParts;
                if (modelMeshPartCollection == null) continue;

                var count1 = modelMeshPartCollection.Count; // 4/30/2010
                for (var j = 0; j < count1; j++)
                {
                    // 4/30/2010 - Cache
                    var modelMeshPart = modelMeshPartCollection[j];
                    if (modelMeshPart == null) continue;

                    modelMeshPart.Effect = orgEffect[beIndex];
                    beIndex++;   
                }
            }
        }

        // NOTE: Use this as a test for someone.. :)
        /*private static void RemapModelToCustom(ref Model model, IList<Effect> orgEffect)
        {
            // 10/19/2008
            if (model == null)
                return;

            // 8/27/2008: Updated to use ForLoop, rather than ForEach.
            int beIndex = 0;
            for (int loop1 = 0; loop1 < model.Meshes.Count; loop1++)
            {
                for (int loop2 = 0; loop2 < model.Meshes[loop1].MeshParts.Count; loop2++)
                {
                    model.Meshes[loop1].MeshParts[loop2].Effect = orgEffect[beIndex];
                    beIndex++;
                }
            }

        }  */     
    }
}
