#region File Description
//-----------------------------------------------------------------------------
// TerrainPerlinClouds.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using ImageNexus.BenScharbach.TWEngine.BeginGame;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace ImageNexus.BenScharbach.TWEngine.Terrain
{
    // 1/10/2011 - Updated to Static class.
    /// <summary>
    /// The <see cref="TerrainPerlinClouds"/> class is used to generate the PerlinNoise textures used to create
    /// the illusion of clouds on <see cref="TWEngine.Terrain"/>.
    /// </summary>
    public static class TerrainPerlinClouds
    {
        // Updated to GraphicsDevice.
        private static GraphicsDevice _graphicsDevice;

        private static readonly Game GameInstance;
        private static Effect _multiTerrainEffect; // 1/10/2011

        // 1/10/2011
        private static bool _enableClouds;
        private static Texture2D _permTexture;
        private static Texture2D _permTexture2D;
        private static Texture2D _permGradTexture;
        private static Texture2D _gradTexture4D;

        #region Properities

        // 12/2/2013 - AppSetting Override..
        /// <summary>
        /// Gets or sets to turn OFF the use of the PerlinNoise clouds on the terrain, which overrides the 'EnableClouds' setting; this is set
        /// from the App.Config xml file.
        /// </summary>
        public static bool AppSettingTurnOffClouds { get; set; }

        // 1/10/2011
        ///<summary>
        /// Gets or Sets to enable the use of the PerlinNoise clouds on the terrain.
        ///</summary>
        public static bool EnableClouds
        {
            get
            {
                // 12/2/2013 - Check for AppSetting Override.
                return !AppSettingTurnOffClouds && _enableClouds;
            }
            set
            {
                _enableClouds = value;

                // 1/10/2011 - Enable/Disable the Perlin Clouds.
                if (_multiTerrainEffect != null) _multiTerrainEffect.Parameters["xEnablePerlinNoiseClouds"].SetValue(value);
            }
        }
       
        internal static Texture2D PermTexture
        {
            get { return _permTexture; }
            private set
            {
                _permTexture = value;

                // 1/10/2011
                if (_multiTerrainEffect != null) _multiTerrainEffect.Parameters["permTexture"].SetValue(PermTexture);
            }
        }
        
        internal static Texture2D PermTexture2D
        {
            get { return _permTexture2D; }
            private set
            {
                _permTexture2D = value;

                // 1/10/2011
                if (_multiTerrainEffect != null) _multiTerrainEffect.Parameters["permTexture2d"].SetValue(PermTexture2D);
            }
        }
        
        internal static Texture2D PermGradTexture
        {
            get { return _permGradTexture; }
            private set
            {
                _permGradTexture = value;

                // 1/10/2011
                if (_multiTerrainEffect != null) _multiTerrainEffect.Parameters["permGradTexture"].SetValue(PermGradTexture);
            }
        }

        
        internal static Texture2D GradTexture4D
        {
            get { return _gradTexture4D; }
            private set
            {
                _gradTexture4D = value;

                // 1/10/2011
                if (_multiTerrainEffect != null) _multiTerrainEffect.Parameters["gradTexture4d"].SetValue(GradTexture4D);
            }
        }
      

        #endregion

        /// <summary>
        /// constructor
        /// </summary>
        static TerrainPerlinClouds()
        {
            // store locally
            _graphicsDevice = TemporalWars3DEngine.GameInstance.GraphicsDevice;

            // Save game ref
            GameInstance = TemporalWars3DEngine.GameInstance;
            
        }

        #region Arrays used for texture generation

        // gradients for 3d noise
        static readonly float[,] G3 =  
        {
            {1,1,0},
            {-1,1,0},
            {1,-1,0},
            {-1,-1,0},
            {1,0,1},
            {-1,0,1},
            {1,0,-1},
            {-1,0,-1}, 
            {0,1,1},
            {0,-1,1},
            {0,1,-1},
            {0,-1,-1},
            {1,1,0},
            {0,-1,1},
            {-1,1,0},
            {0,-1,-1}
        };

        // gradients for 4D noise
        static readonly float[,] G4 = 
        {
	        {0, -1, -1, -1},
	        {0, -1, -1, 1},
	        {0, -1, 1, -1},
	        {0, -1, 1, 1},
	        {0, 1, -1, -1},
	        {0, 1, -1, 1},
	        {0, 1, 1, -1},
	        {0, 1, 1, 1},
	        {-1, -1, 0, -1},
	        {-1, 1, 0, -1},
	        {1, -1, 0, -1},
	        {1, 1, 0, -1},
	        {-1, -1, 0, 1},
	        {-1, 1, 0, 1},
	        {1, -1, 0, 1},
	        {1, 1, 0, 1},
        	
	        {-1, 0, -1, -1},
	        {1, 0, -1, -1},
	        {-1, 0, -1, 1},
	        {1, 0, -1, 1},
	        {-1, 0, 1, -1},
	        {1, 0, 1, -1},
	        {-1, 0, 1, 1},
	        {1, 0, 1, 1},
	        {0, -1, -1, 0},
	        {0, -1, -1, 0},
	        {0, -1, 1, 0},
	        {0, -1, 1, 0},
	        {0, 1, -1, 0},
	        {0, 1, -1, 0},
	        {0, 1, 1, 0},
	        {0, 1, 1, 0}
        };

        static readonly int[] Perm = { 151, 160, 137, 91, 90, 15, 131, 13, 201, 95,
			    96, 53, 194, 233, 7, 225, 140, 36, 103, 30, 69, 142, 8, 99, 37,
			    240, 21, 10, 23, 190, 6, 148, 247, 120, 234, 75, 0, 26, 197, 62,
			    94, 252, 219, 203, 117, 35, 11, 32, 57, 177, 33, 88, 237, 149, 56,
			    87, 174, 20, 125, 136, 171, 168, 68, 175, 74, 165, 71, 134, 139,
			    48, 27, 166, 77, 146, 158, 231, 83, 111, 229, 122, 60, 211, 133,
			    230, 220, 105, 92, 41, 55, 46, 245, 40, 244, 102, 143, 54, 65, 25,
			    63, 161, 1, 216, 80, 73, 209, 76, 132, 187, 208, 89, 18, 169, 200,
			    196, 135, 130, 116, 188, 159, 86, 164, 100, 109, 198, 173, 186, 3,
			    64, 52, 217, 226, 250, 124, 123, 5, 202, 38, 147, 118, 126, 255,
			    82, 85, 212, 207, 206, 59, 227, 47, 16, 58, 17, 182, 189, 28, 42,
			    223, 183, 170, 213, 119, 248, 152, 2, 44, 154, 163, 70, 221, 153,
			    101, 155, 167, 43, 172, 9, 129, 22, 39, 253, 19, 98, 108, 110, 79,
			    113, 224, 232, 178, 185, 112, 104, 218, 246, 97, 228, 251, 34, 242,
			    193, 238, 210, 144, 12, 191, 179, 162, 241, 81, 51, 145, 235, 249,
			    14, 239, 107, 49, 192, 214, 31, 181, 199, 106, 157, 184, 84, 204,
			    176, 115, 121, 50, 45, 127, 4, 150, 254, 138, 236, 205, 93, 222,
			    114, 67, 29, 24, 72, 243, 141, 128, 195, 78, 66, 215, 61, 156, 180 };

       

        #endregion

        #region Texture generation methods

        private static void GeneratePermTexture()
        {
            // 3/9/2011
            if (_graphicsDevice == null)
                _graphicsDevice = TemporalWars3DEngine.GameInstance.GraphicsDevice;

            // XNA 4.0 Updates - Many SurfaceFormat's are not longer supported - Like 'Luminance8'; Suggested equivilent is 'Alpha8'.
            // NOTE: http://blogs.msdn.com/b/shawnhar/archive/2010/03/12/reach-vs-hidef.aspx?PageIndex=3
            //PermTexture = new Texture2D(_gameInstance.GraphicsDevice, 256, 1, 1, TextureUsage.None, SurfaceFormat.Luminance8);
            PermTexture = new Texture2D(_graphicsDevice, 256, 1, true, SurfaceFormat.Alpha8);

            var data = new byte[256*1];
            for (var x = 0; x < 256; x++)
                for (var y = 0; y < 1; y++)
                {
                    data[x + (y*256)] = (byte) (Perm[x]);
                }

            PermTexture.SetData(data);
        }

        private static int Perm2D(int i)
        {
            return Perm[i%256];
        }

        private static void GeneratePermTexture2D()
        {
            // XNA 4.0 Updates
            //PermTexture2D = new Texture2D(_gameInstance.GraphicsDevice, 256, 256, 1, TextureUsage.None, SurfaceFormat.Color);
            PermTexture2D = new Texture2D(_graphicsDevice, 256, 256, true, SurfaceFormat.Color);

            var data = new Color[256 * 256];
            for (var x = 0; x < 256; x++)
                for (var y = 0; y < 256; y++)
                {
                    var a = Perm2D(x) + y;
                    var aa = Perm2D(a);
                    var ab = Perm2D(a + 1);
                    var b = Perm2D(x + 1) + y;
                    var ba = Perm2D(b);
                    var bb = Perm2D(b + 1);
                    data[x + (y*256)] = new Color((byte) (aa), (byte) (ab),
                                                  (byte) (ba), (byte) (bb));
                }

            PermTexture2D.SetData(data);
        }

        private static void GeneratePermGradTexture()
        {
            // XNA 4.0 Updates
            //PermGradTexture = new Texture2D(_gameInstance.GraphicsDevice, 256, 1, 1, TextureUsage.None, SurfaceFormat.NormalizedByte4);
            PermGradTexture = new Texture2D(_graphicsDevice, 256, 1, true, SurfaceFormat.NormalizedByte4);
            
            var data = new NormalizedByte4[256 * 1];
            for (var x = 0; x < 256; x++)
                for (var y = 0; y < 1; y++)
                {
                    data[x + (y*256)] = new NormalizedByte4(G3[Perm[x]%16, 0], G3[Perm[x]%16, 1], G3[Perm[x]%16, 2], 1);
                }

            PermGradTexture.SetData(data);
        }

        private static void GenerateGradTexture4D()
        {
            // 9/22/2010 - XNA 4.0 Updates
            //GradTexture4D = new Texture2D(_gameInstance.GraphicsDevice, 32, 1, 1, TextureUsage.None, SurfaceFormat.NormalizedByte4);
            GradTexture4D = new Texture2D(_graphicsDevice, 32, 1, true, SurfaceFormat.NormalizedByte4);
            
            var data = new NormalizedByte4[32 * 1];
            for (var x = 0; x < 32; x++)
                for (var y = 0; y < 1; y++)
                {
                    data[x + (y*32)] = new NormalizedByte4(G4[x, 0], G4[x, 1], G4[x, 2], G4[x, 3]);
                }

            GradTexture4D.SetData(data);
        }

        /// <summary>
        /// Generates all of the needed textures on the CPU
        /// </summary>
        /// <param name="multiTerrainEffect">Instance of the MultiTerrain <see cref="Effect"/>.</param>
        internal static void GenerateTextures(Effect multiTerrainEffect)
        {
            // 1/10/2011 - Save instance of effect
            _multiTerrainEffect = multiTerrainEffect;

            // 2/3/2011 - Check if _enableClouds backing is TRUE.  
            // If true, then reset into property to set into shader.
            if (_enableClouds) EnableClouds = _enableClouds;

            GeneratePermTexture();
            GeneratePermTexture2D();
            GeneratePermGradTexture();
            GenerateGradTexture4D();
        }
        #endregion
    }
}
