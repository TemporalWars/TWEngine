//-----------------------------------------------------------------------------
// InstancedModelCommon.hlsl
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------


// The maximum number of instances we can support when using the VFetchInstancing
// or ShaderInstancing technique is limited by the number of vertex shader constant
// registers. Shader model 2.0 has 256 constant registers: after using a couple for
// the camera and light settings, this leaves enough for 60 4x4 matrices. You will
// need to decrease this value if you add other shader parameters that also require
// constant registers, or if you want to use the ShaderInstancing technique with
// vertex shader 1.1 (which only has 96 constant registers). If you change this value,
// you must also update the constant at the top of InstancedModelPart.cs to match!


#define MAX_SHADER_MATRICES 35 

// 8/28/2009
struct InstanceDataStruct
{	
	// XNA 4.0 Updates - Channel is now BlendWeight
	float4x4 Transform : BLENDWEIGHT; // Uses 1-4 TexCoord spaces, since 4x4 matrix.
	float2 PlayerNTime : TEXCOORD5; // Player = Integer portion represents some misc data, while the fractional is for teamColors; 6/6/2010 Time = Stores beg time.	
	float3 PVelocity   : TEXCOORD7; // 6/6/2010 - Stores Projectile's Velocity; using channel-7, since channel-6 already used for Velocity in main stream.	

};

// 8/28/2009 - XNA does not have the 'StructureMembers' working at all!  Therefore, need seperate
//             arrays of each 'InstanceDataStruct' to be set on the XBOX!
// Array of instance transforms used by the VFetch and ShaderInstancing techniques.
float4x4 InstanceTransforms[MAX_SHADER_MATRICES];
float2 InstanceTransforms_PlayerNTime[MAX_SHADER_MATRICES]; // 8/28/2009; 6/6/2010 Time = Stores beg time for Xbox.	
float3 InstanceTransforms_PVelocity[MAX_SHADER_MATRICES]; // 6/6/2010 - Stores Projectile's Velocity for Xbox.

// 6/4/2010 - Add Explosions helper.
// 6/4/2010 - Explosion attributes
const float ExpDuration = 5; // 6/5/2010 last for 5 seconds.
const float Gravity = 9.8f * 40; // 40 frames per second.
const float Friction = 0.2f;
bool xIsExplosionPiece = false;

// Single instance transform used by the NoInstancing technique.
InstanceDataStruct NoInstancingTransform;

// Camera settings.
shared float4x4 View;
shared float4x4 ViewInverse; // 1/21/2010
shared float4x4 Projection;
shared float4x4 xWorld;
shared float4x4 xWorldI; // 1/19/2010

// 5/23/2010 - Bloom Attributes
shared float BloomThreshold = 0.25f;
shared float BloomIntensity = 1.25f;
shared float BaseIntensity = 1.0f;
shared float BloomSaturation = 1.0f;
shared float BaseSaturation = 1.0f;

// 9/19/2008
float xSpecularPower = 30.0f; // was 128
float xLightStrength = 0.9f;
float3 oDiffuseColor = float3(0.4f, 0.4f, 0.4f); // 1/22/2010 - Connected to Opaque var
float3 oSpecularColor = float3(1, 1, 1); // 1/22/2010 - Connected to Opaque var
float oSpecularCPower  = 0.3f; // 1/22/2010 - Connected to Opaque var
float3 oAmbientColor = float3(0.07f, 0.07f, 0.07f); // 1/22/2010 - Connected to Opaque var
float xAmbientPower = 0.6f;
bool oUseNormalMap = false; // 9/22/2008; 1/21/2010 - Opaque Var
bool oUseIllumMap = false; // 2/1/2009; 1/21/2010 - Opaque Var
bool xOscillateIllum = false; // 2/2/2009
bool xOscillateSpeed = 1; // 2/2/2009
float4 xIllumColor = float4(0,0,0,0); // 2/2/2009
bool oUseSpecularMap = false; // 2/10/2009; 1/21/2010 - Opaque Var

shared float xAccumElapsedTime = 0; // 6/5/2010
shared float xTime = 0; // 2/1/2009

// 2/10/2009
bool xBoneRotates = false;
float3 xBoneRotationData = float3(0, 1, 0);

// Shadow Light variables
float3 xLightPos = float3(0,3000,0); // 8/4/2008
float xDepthBias = 0.01f; // 6/4/2009
float2 PCFSamples[9]; // 6/26/2009
float4x4 xLightViewProjection; // 6/4/2009 - ShadowMap LightView
float4x4 xLightViewProjection_Static; // 7/13/2009 - STATIC ShadowMap
float2 xHalfPixel; // 7/13/2009
bool xEnableShadows; // 1/16/2009

// 11/19/2008: Ben - Team-Color Atts
bool oShowTeamColor; // 1/21/2010 - Opaque Var
float4 xTeamColor1;
float4 xTeamColor2;

float3 DiffuseLight = 1.25;
float3 AmbientLight = 0.25;

//------ Instance Texture --------
texture Texture : DIFFUSEMAP;
sampler Sampler = sampler_state { Texture = (Texture); minFilter = Linear; magFilter = Linear;
													   mipFilter = Linear; addressU = Wrap;
													   addressV = Wrap; addressW  = Wrap;};

// 9/22/2008
//------ NormalMap Texture --------
texture NormalMapTexture : NORMAL;
sampler TextureBumpSampler = sampler_state { texture = <NormalMapTexture> ; magfilter = LINEAR; minfilter = LINEAR; 
                                                                         mipfilter=LINEAR; AddressU  = Wrap;
                                                                         AddressV  = Wrap; AddressW  = Wrap;};
                                                                         
// 2/1/2009
//------ IllumMap Texture --------
texture IllumMapTexture : ENVMAP;
sampler TextureIllumSampler = sampler_state { texture = <IllumMapTexture> ; magfilter = LINEAR; minfilter = LINEAR; 
                                                                         mipfilter=LINEAR; AddressU  = Wrap;
                                                                         AddressV  = Wrap; AddressW  = Wrap;};
                                                                         
// 2/10/2009
//------ SpecularMap Texture --------
texture SpecularMapTexture : SPECULARMAP;
sampler TextureSpecularSampler = sampler_state { texture = <SpecularMapTexture> ; magfilter = LINEAR; minfilter = LINEAR; 
                                                                         mipfilter=LINEAR; AddressU  = Wrap;
                                                                         AddressV  = Wrap; AddressW  = Wrap;};
                                                                         
// 1/16/2009
//------ ShadowMap Texture --------

// 6/4/2009 - ShadowMap
shared texture ShadowMapTexture;
sampler ShadowMapSampler = sampler_state { texture = <ShadowMapTexture> ; magfilter = POINT; minfilter = POINT; 
																				mipfilter=None; AddressU  = clamp;
																				AddressV  = clamp; AddressW  = clamp;};	
																				
// 7/13/2009 - STATIC ShadowMap
shared texture TerrainShadowMap;
sampler ShadowMapSamplerTerrain = sampler_state { texture = <TerrainShadowMap> ; magfilter = POINT; minfilter = POINT; 
																				mipfilter=None; AddressU  = clamp;
																				AddressV  = clamp; AddressW  = clamp;};																					
                                                                         
                                                                         

// 1/28/2010: Decided to remove the 'BiNormal' channel, since it can be recreated by simply
//            crossing the 'Normal' with 'Tangent'!
struct VertexShaderInput
{
    float4 Position			   : POSITION0;    
    float4 Normal			   : NORMAL0;    
    float4 Tangent             : TANGENT0; // 9/22/2008   
    float3 TextureCoordinate   : TEXCOORD0;    
    float3 Velocity			   : TEXCOORD6; // 6/4/2010 - Used to animate explosions.
};


struct VertexShaderOutput
{
    float4 Position             : POSITION0;    
    float4 TeamColor	        : Color1; // Ben: Added on 11/19/2008; 2/3/2010: (a) channel stores MatieralId. 
    float2 TextureCoordinate    : TEXCOORD0;
    float3 lightDirection	    : TEXCOORD1;
    float3 viewDirection	    : TEXCOORD2;    
    float3x3 tangentToWorld     : TEXCOORD3; //  BumpMapping    
    float4 WorldPos             : TEXCOORD6; // 6/4/09 
    
    // 2/3/2010 - Used to pass misc atts; like the wood grain coordinate system.  
    float4 MiscAtts	            : TEXCOORD7; 
   
};

struct VertexShaderOutput_Deferred
{
    float4 Position				: POSITION0;    
    float4 TeamColor			: TEXCOORD0; // Ben: Added on 11/19/2008 
    float2 TextureCoordinate	: TEXCOORD1;   
    float4 Position2D			: TEXCOORD2;       
    float3x3 tangentToWorld		: TEXCOORD3; //  BumpMapping 
    
};

// 2/9/2009
// Helper function to create a YawPitchRoll Matrix. 
// Used to rotate the verticies by the random rotational values generated in the 
// processor.
float4x4 CreateYawPitchRollMatrix(half x, half y, half z)
{
    float4x4 result;
        
    result[0][0] = cos(z)*cos(y) + sin(z)*sin(x)*sin(y);
    result[0][1] = -sin(z)*cos(y) + cos(z)*sin(x)*sin(y);
    result[0][2] = cos(x)*sin(y);
    result[0][3] = 0;
    
    result[1][0] = sin(z)*cos(x);
    result[1][1] = cos(z)*cos(x);
    result[1][2] = -sin(x);
    result[1][3] = 0;
    
    result[2][0] = cos(z)*-sin(y) + sin(z)*sin(x)*cos(y);
    result[2][1] = sin(z)*sin(y) + cos(z)*sin(x)*cos(y);
    result[2][2] = cos(x)*cos(y);
    result[2][3] = 0;
    
    result[3][0] = 0;
    result[3][1] = 0;
    result[3][2] = 0;
    result[3][3] = 1;    

    return result;
}


// 6/4/2010: Updated to now include the 'instanceIndex' param; will be used for the Explosions.  
// Vertex shader helper function shared between the different instancing techniques. 
VertexShaderOutput VertexShaderCommon(VertexShaderInput input, InstanceDataStruct instanceTransform)
{
    VertexShaderOutput output = (VertexShaderOutput)0; 
    
    float4 worldPosition;
    float3x3 rotMatrix;
 
	// 8/28/2009 - Do transpose here; used to be done when float4x4 was used in param.
    instanceTransform.Transform = transpose(instanceTransform.Transform);

	// 8/28/2009 - Split the Fractional 'PlayerNumber', into its Integer & Frac portions; the 
	//             Integer portion represents the ProceduralMaterialId, while the fractional is for teamColors.
	//
	//             Note: The ModF HLSL function will return frac strangely; for example, 1.02 value will 
	//                   be returned as 0.01999998.  Therefore, the value is being multiplied by 100, to 
	//                   give 1.999998, and then rounded, to get the correct value of 2!
	//	
	float materialId = 0.0f;
	float teamColor = round(modf(instanceTransform.PlayerNTime.x, materialId) * 100); // x channel holds player/colors.	
       
    
	// 8/28/2009 - Apply TeamColor by reading the TeamNumber channel.
	output.TeamColor = 0;
	
	if (teamColor == 1)
	{			
		output.TeamColor = xTeamColor1;	
	}
	else if (teamColor == 2)
	{			
		output.TeamColor = xTeamColor2;	
	}
	
	// 6/7/2010: Updated to NOT do the bone rotation, when an explosion piece, since it has its own rotation.
	if (xBoneRotates && !xIsExplosionPiece)
	{
		float3 rotSpeed = xBoneRotationData.xyz * xTime;
		float4x4 animRotMatrix = CreateYawPitchRollMatrix(rotSpeed.x, rotSpeed.y, rotSpeed.z);		
		
		input.Position = mul(input.Position, animRotMatrix);
		
		// Apply the instanceTransform to get the proper World Instance position
		worldPosition = mul(input.Position, instanceTransform.Transform);				
					
		rotMatrix = (float3x3)mul(animRotMatrix, instanceTransform.Transform);
		
	}
	else
	{
		// 6/7/2010 - Check if ExplosionPiece, so rotation can be done.
		if (xIsExplosionPiece)
		{			
			// Compute the age of the explosion piece.
			float age = xAccumElapsedTime - instanceTransform.PlayerNTime.y; // Y channel holds original time of inception.
			float4x4 animRotMatrix = CreateYawPitchRollMatrix(5.25f * age, 0.25f * age, 0);		
		
			input.Position = mul(input.Position, animRotMatrix);
		}
				
		// Apply the instanceTransform to get the proper World Instance position
		worldPosition = mul(input.Position, instanceTransform.Transform);	
		
		rotMatrix = (float3x3)instanceTransform.Transform;
	} 
	
	
	// 6/6/2010 - Check if is ExplosionPiece, so position can be done.	
	if (xIsExplosionPiece)
	{	 
		// Compute the age of the explosion piece.
		float age = xAccumElapsedTime - instanceTransform.PlayerNTime.y; // Y channel holds time.		
	    
		// Normalize the age into the range zero to one.
		float normalizedAge = saturate(age / ExpDuration);
		
		// Update Velocity with Projectile's velocity value.
		// Note: Height already has a 30x multiplier, so only affect channels x/z.
		input.Velocity.xz *= instanceTransform.PVelocity.xz;
		
		float startVelocity = length(input.Velocity);

		// Work out how fast the pieces should be moving at the end of its life,
		// by applying a constant scaling factor to its starting velocity.
		float endVelocity = startVelocity * 0; // was * EndVelocity 
	    
		// Our pieces have constant acceleration, so given a starting velocity
		// S and ending velocity E, at time T their velocity should be S + (E-S)*T.
		// The particle position is the sum of this velocity over the range 0 to T.
		// To compute the position directly, we must integrate the velocity
		// equation. Integrating S + (E-S)*T for T produces S*T + (E-S)*T*T/2.
		float velocityIntegral = startVelocity * normalizedAge +
								 (endVelocity - startVelocity) * normalizedAge *
																 normalizedAge / 2;	
		// Update position		
		worldPosition += float4(normalize(input.Velocity) * velocityIntegral * ExpDuration, 0);		
		
		// Update if above ground level.
		if (worldPosition.y > 0)
			worldPosition.y -= Gravity * age * normalizedAge; // Exp Velocity with gravity/roll.		
		
	}	
	
	
	if (oUseNormalMap)
	{     		
		// BumpMapping
		// calculate tangent space to world space matrix using the world space tangent,
		// binormal, and normal as basis vectors
		output.tangentToWorld[0] = mul(input.Tangent.xyz, rotMatrix);
		output.tangentToWorld[1] = mul(cross(input.Tangent.xyz, input.Normal.xyz), rotMatrix); // 1/28/2010: Removed 'input.Binormal.xyz'; instead just calc cross.
		output.tangentToWorld[2] = mul(input.Normal.xyz, rotMatrix); 	
		
		// 1/24/2010
		output.tangentToWorld[0] = mul(output.tangentToWorld[0], transpose(xWorldI));
		output.tangentToWorld[1] = mul(output.tangentToWorld[1], transpose(xWorldI)); 
		output.tangentToWorld[2] = mul(output.tangentToWorld[2], transpose(xWorldI)); 		 
		  
	} 
	else
	{
		// Normal needs to adjusted for rotation for proper lighting.			
		output.tangentToWorld[2] = mul(input.Normal, rotMatrix); 
	}
	
	// 6/4/2009 - Save WorldPos for Shadows
	output.WorldPos = worldPosition;
	
	// Apply the view and projection matrices to compute the output position.	
	float4 viewPosition = mul(worldPosition, View);
	output.Position = mul(viewPosition, Projection);	
    
	// calculate the light direction ( from the surface to the light ), which is not
	// normalized and is in world space
	output.lightDirection = xLightPos - worldPosition;				
        
	// similarly, calculate the view direction, from the eye to the surface.  not
	// normalized, in world space.
    float3 eyePosition = mul(-View._m30_m31_m32, transpose(View));    
    output.viewDirection = eyePosition - worldPosition;		// ViewInverse[3]
    
	// Copy across the input texture coordinate.
	output.TextureCoordinate = input.TextureCoordinate;
	
	// 2/12/2010 - Check if this is Wood material
#ifdef VS_MaterialWood

	 // yes, so do custom woodPos calculation (Saves to GBA, not RGB channels.
	 output.MiscAtts.gba = (_mpWoodScale*worldPosition.xyz) + _mpWoodOffset; // wood grain coordinate system

#endif      


    return output;
}

// 1/30/2010 - FogOfWar Empty VertexShader.
VertexShaderOutput VertexShaderCommon_FOW()
{
	 VertexShaderOutput output = (VertexShaderOutput)0; 
	 return output;
}

// Vertex shader helper function shared between the different instancing techniques. 
VertexShaderOutput_Deferred VertexShaderCommon_Deferred(VertexShaderInput input,
                                      InstanceDataStruct instanceTransform)
{
    VertexShaderOutput_Deferred output = (VertexShaderOutput_Deferred)0; 
    
    float4 worldPosition, viewPosition;
    float3x3 rotMatrix;
    
	// 8/28/2009 - Do transpose here; used to be done when float4x4 was used in param.
    instanceTransform.Transform = transpose(instanceTransform.Transform);

   // 8/28/2009 - Split the Fractional 'PlayerNumber', into its Integer & Frac portions; the 
	//             Integer portion represents the ProceduralMaterialId, while the fractional is for teamColors.
	//
	//             Note: The ModF HLSL function will return frac strangely; for example, 1.02 value will 
	//                   be returned as 0.01999998.  Therefore, the value is being multiplied by 100, to 
	//                   give 1.999998, and then rounded, to get the correct value of 2!
	//
	
	float materialId = 0.0f;
	float teamColor = round(modf(instanceTransform.PlayerNTime.x, materialId) * 100); // x channel holds player/colors.	  
    
    
	// 8/28/2009 - Apply TeamColor by reading the TeamNumber channel.
	output.TeamColor = 0;
	
	if (teamColor == 1)
	{			
		output.TeamColor = xTeamColor1;	
	}
	else if (teamColor == 2)
	{			
		output.TeamColor = xTeamColor2;	
	}	
	
	
	if (xBoneRotates)
	{
		float3 rotSpeed = xBoneRotationData.xyz * xTime;
		float4x4 animRotMatrix = CreateYawPitchRollMatrix(rotSpeed.x, rotSpeed.y, rotSpeed.z);		
		
		input.Position = mul(input.Position, animRotMatrix);
		
		// Apply the instanceTransform to get the proper World Instance position
		worldPosition = mul(input.Position, instanceTransform.Transform);	
		
		rotMatrix = (float3x3)mul(instanceTransform.Transform, animRotMatrix);
		
	}
	else
	{			
		// Apply the instanceTransform to get the proper World Instance position
		worldPosition = mul(input.Position, instanceTransform.Transform);	
		
		rotMatrix = (float3x3)instanceTransform.Transform;
	}	
	
	// Apply the view and projection matrices to compute the output position.
	viewPosition = mul(worldPosition, View);
	output.Position = mul(viewPosition, Projection);		
	output.Position2D = output.Position; 
	
	// BumpMapping
	// calculate tangent space to world space matrix using the world space tangent,
	// binormal, and normal as basis vectors
	output.tangentToWorld[0] = mul(input.Tangent, rotMatrix);
	output.tangentToWorld[1] = mul(cross(input.Tangent.xyz, input.Normal.xyz), rotMatrix); // 1/28/2010: Removed 'input.Binormal.xyz'; instead just calc cross.
	output.tangentToWorld[2] = mul(input.Normal, rotMatrix); 			
    
	// Copy across the input texture coordinate.
	output.TextureCoordinate = input.TextureCoordinate;
   

    return output;
}


// On either platform, when instancing is disabled we can read
// the world transform directly from an effect parameter.
VertexShaderOutput NoInstancingVertexShader(VertexShaderInput input)
{
    return VertexShaderCommon(input,  NoInstancingTransform);
}

// 3/23/2010 - Shadow code was here; now placed in InstancedModelShadows.fx file.

#ifdef XBOX360

int VertexCount = 0; 

// On Xbox, we can use the GPU "vfetch" instruction to implement
// instancing. We perform arithmetic on the input index to compute
// both the vertex and instance indices.
VertexShaderOutput VFetchInstancingVertexShader(int index : INDEX)
{ 	
	int vertexIndex = 0;
	int instanceIndex = 0;
	float4 position, normal, textureCoordinate, tangent, velocity; // 6/7/2010: Add tangent & velocity.

    vertexIndex = (index + 0.5) % VertexCount;
    instanceIndex = (index + 0.5) / VertexCount;    

    asm
    {
        vfetch position,          vertexIndex, position0
        vfetch normal,            vertexIndex, normal0  
        vfetch tangent,			  vertexIndex, tangent0 // 6/7/2010: Add tangent.       
        vfetch textureCoordinate, vertexIndex, texcoord0
        vfetch velocity,		  vertexIndex, texcoord6 // 6/7/2010: Add velocity.
    };

    VertexShaderInput input = (VertexShaderInput)0;

    input.Position = position;
    input.Normal = normal;    
    input.Tangent = tangent; // 6/7/2010: Add tangent
    input.TextureCoordinate = textureCoordinate;
    input.Velocity = velocity;  // 6/7/2010: Add velocity.
    
    // 8/28/2009 - Create custom 'InstanceDataStruct'.
    InstanceDataStruct instanceDataStruct = (InstanceDataStruct)0;
    instanceDataStruct.Transform = InstanceTransforms[instanceIndex];
    instanceDataStruct.PlayerNTime.x = InstanceTransforms_PlayerNTime[instanceIndex].x;// 6/6/2010
    instanceDataStruct.PlayerNTime.y = InstanceTransforms_PlayerNTime[instanceIndex].y; // 6/6/2010
    instanceDataStruct.PVelocity = InstanceTransforms_PVelocity[instanceIndex]; // 6/6/2010
	
    return VertexShaderCommon(input, instanceDataStruct); // was - InstanceTransforms[instanceIndex]
}


// On Xbox, we can use the GPU "vfetch" instruction to implement
// instancing. We perform arithmetic on the input index to compute
// both the vertex and instance indices.
VertexShaderOutput_Deferred VFetchInstancingVertexShader_Deferred(int index : INDEX)
{ 	
	int vertexIndex = 0;
	int instanceIndex = 0;
	float4 position, normal, textureCoordinate, tangent, velocity; // 6/7/2010: Add tangent & velocity.

    vertexIndex = (index + 0.5) % VertexCount;
    instanceIndex = (index + 0.5) / VertexCount;    

     asm
    {
        vfetch position,          vertexIndex, position0
        vfetch normal,            vertexIndex, normal0  
        vfetch tangent,			  vertexIndex, tangent0 // 6/7/2010: Add tangent.       
        vfetch textureCoordinate, vertexIndex, texcoord0
        vfetch velocity,		  vertexIndex, texcoord6 // 6/7/2010: Add velocity.
    };

    VertexShaderInput input = (VertexShaderInput)0;

    input.Position = position;
    input.Normal = normal;    
    input.Tangent = tangent; // 6/7/2010: Add tangent
    input.TextureCoordinate = textureCoordinate;
    input.Velocity = velocity;  // 6/7/2010: Add velocity.
    
     // 8/28/2009 - Create custom 'InstanceDataStruct'.
    InstanceDataStruct instanceDataStruct = (InstanceDataStruct)0;
    instanceDataStruct.Transform = InstanceTransforms[instanceIndex];
    instanceDataStruct.PlayerNTime.x = InstanceTransforms_PlayerNTime[instanceIndex].x;// 6/6/2010
    instanceDataStruct.PlayerNTime.y = InstanceTransforms_PlayerNTime[instanceIndex].y; // 6/6/2010
    instanceDataStruct.PVelocity = InstanceTransforms_PVelocity[instanceIndex]; // 6/6/2010

    return VertexShaderCommon_Deferred(input, instanceDataStruct); // was - InstanceTransforms[instanceIndex]
}

#else


// On Windows, we can use an array of shader constants to implement
// instancing. The instance index is passed in as part of the vertex
// buffer data, and we use that to decide which world transform should apply.
VertexShaderOutput ShaderInstancingVertexShader(VertexShaderInput input, uniform float4x4 View, uniform float4x4 ViewInverse, 
									 uniform float4x4 Projection, uniform float4x4 xWorld, uniform float4x4 xWorldI,
									  uniform bool xBoneRotates, uniform float3 XBoneRotationData, float instanceIndex : TEXCOORD1)
{
    // 8/28/2009 - Create custom 'InstanceDataStruct'.
    InstanceDataStruct instanceDataStruct = (InstanceDataStruct)0;
    instanceDataStruct.Transform = InstanceTransforms[instanceIndex];
    instanceDataStruct.PlayerNTime.x = InstanceTransforms_PlayerNTime[instanceIndex].x;// 6/6/2010
    instanceDataStruct.PlayerNTime.y = InstanceTransforms_PlayerNTime[instanceIndex].y; // 6/6/2010
    instanceDataStruct.PVelocity = InstanceTransforms_PVelocity[instanceIndex]; // 6/6/2010
	
    return VertexShaderCommon(input, instanceDataStruct); // was - InstanceTransforms[instanceIndex]
}

// On Windows, we can use an array of shader constants to implement
// instancing. The instance index is passed in as part of the vertex
// buffer data, and we use that to decide which world transform should apply.
VertexShaderOutput_Deferred ShaderInstancingVertexShader_Deferred(VertexShaderInput input,
                                                float instanceIndex : TEXCOORD1)
{
    // 8/28/2009 - Create custom 'InstanceDataStruct'.
    InstanceDataStruct instanceDataStruct = (InstanceDataStruct)0;
    instanceDataStruct.Transform = InstanceTransforms[instanceIndex];
    instanceDataStruct.PlayerNTime.x = InstanceTransforms_PlayerNTime[instanceIndex].x;// 6/6/2010
    instanceDataStruct.PlayerNTime.y = InstanceTransforms_PlayerNTime[instanceIndex].y; // 6/6/2010
    instanceDataStruct.PVelocity = InstanceTransforms_PVelocity[instanceIndex]; // 6/6/2010

    return VertexShaderCommon_Deferred(input, instanceDataStruct); // was - InstanceTransforms[instanceIndex]
}


// On Windows shader 3.0 cards, we can use hardware instancing, reading
// the per-instance world transform directly from a secondary vertex stream.
VertexShaderOutput HardwareInstancingVertexShader(VertexShaderInput input, InstanceDataStruct instanceTransform) // was InstanceDataStruct instanceTransform : TEXCOORD1
{
	
    return VertexShaderCommon(input, instanceTransform); // 8/28/2009 - was transpose(instanceTransform)
}

// On Windows shader 3.0 cards, we can use hardware instancing, reading
// the per-instance world transform directly from a secondary vertex stream.
VertexShaderOutput_Deferred HardwareInstancingVertexShader_Deferred(VertexShaderInput input, InstanceDataStruct instanceTransform) // was InstanceDataStruct instanceTransform : TEXCOORD1
{
    return VertexShaderCommon_Deferred(input, instanceTransform); // 8/28/2009 - was transpose(instanceTransform.Transform)
}

#endif

// 3/14/2009 - Deferred Rendering
struct PixelToFrameStruct
{
	float4 Color 			: COLOR0;
	float4 Color1 			: COLOR1; // Normal
	float4 Color2 			: COLOR2; // Depth
	float4 Color3		    : COLOR3; // Glow
};

#include <..\ShadowMethods.hlsl>
#include <InstancedModelShadows.hlsl> // 6/11/2010

// 6/11/2009 - Calculates the Self-Shadowing lighting contribution.
float CalculateShadowMapLight(VertexShaderOutput input)
{
	float vTotalLightDiffuse = 1.0f; 
	float vTotalLightDiffuse_Static = 1.0f; // 7/13/2009    
	const float shadowDarkness = 0.5f; // 10/27/2009
    	
	// Calculate ShadowMap coordinates		
	float4 ShadowTexC = CalculateShadowTexCoords(input.WorldPos, xLightViewProjection, xHalfPixel);		
	float4 ShadowTexC_Static = CalculateShadowTexCoords(input.WorldPos, xLightViewProjection_Static, xHalfPixel); // 7/13/2009	
	
	
	//vTotalLightDiffuse = CalculateVarianceShadowMapLightFactor(vTotalLightDiffuse, ShadowTexC, ShadowMapSampler, xDepthBias);	

	// Calculate the Shadow's Total Light Factor.     	
	//vTotalLightDiffuse = CalculateSimpleShadowMapLightFactor(vTotalLightDiffuse, ShadowTexC, ShadowMapSampler, xDepthBias);	
	vTotalLightDiffuse = CalculatePCFShadowMapLightFactor(vTotalLightDiffuse, ShadowTexC, PCFSamples, ShadowMapSampler, xDepthBias, shadowDarkness);		
		
	// 7/13/2009 - STATIC ShadowMap.		
	vTotalLightDiffuse_Static = CalculatePCFShadowMapLightFactor(vTotalLightDiffuse_Static, ShadowTexC_Static, 
															PCFSamples, ShadowMapSamplerTerrain, xDepthBias, shadowDarkness);
	
	
	return vTotalLightDiffuse * vTotalLightDiffuse_Static;

}

// 6/17/2009 - Calculate Phong Lighting
float CalculatePhongLighting(float3 normal, float3 lightDirection)
{
	// Calculate Phong Lighting
	return max(dot(normal, lightDirection), 0);
}

// 6/17/2009 - Calculate Specular Lighting
float CalculateSpecularLighting(float3 normal, float3 lightDirection, float3 viewDirection)
{
	const float Shininess = 0.4f;

	//reflection vector	
	float3 Reflect = reflect(lightDirection, normal);	
	
	// Total SpecularPower
	float totalSpecularPower = 128.0f;	
			
	//compute specular light	
	return pow(dot(Reflect, viewDirection), totalSpecularPower) * Shininess;
}

// 1/29/2010
// Helper method, which uncompresses the Packed DXT5-SP version
// of the NormalMap, where the Red channel is placed into the Alpha.
float4 UncompressDXT5_NM(float4 normPacked)
{
   float4 norm = float4( normPacked.a, normPacked.g, 0, 1);
   norm.z = sqrt( 1 - (norm.x * norm.x + norm.y * norm.y) );
   return norm;
}

// 6/17/2009 - Calculate Normal Lighting
float3 CalculateNormalLighting(float2 textureCoords, float3x3 TTW)
{
	// Bump Mapping
	//float3 BumpMapColor = (UncompressDXT5_NM(tex2D(TextureBumpSampler, textureCoords)) - 0.5f) * 2;
	float3 BumpMapColor = (tex2D(TextureBumpSampler, textureCoords) - 0.5f) * 2;
	
	// 1/24/2010 - create a per-pixel normal.
	//input the vectors required for tangent to world space transform
	float3 Nn = normalize(TTW[2]); 		 
	float3 Tn = normalize(TTW[0]); 
	float3 Bn = normalize(TTW[1]); 
	BumpMapColor = float3((Nn * BumpMapColor.z) + (BumpMapColor.x * Tn + BumpMapColor.y * -Bn)); //create a per-pixel normal for Y up
	//BumpMapColor = float3((Nn * BumpMapColor.z) + (BumpMapColor.x * Bn + BumpMapColor.y * -Tn)); //create a per-pixel normal for Z up
	
	return BumpMapColor;
}

// 5/23/2010 - Bloom 
// Helper for modifying the saturation of a color.
float4 AdjustSaturation(float4 color, float saturation)
{
    // The constants 0.3, 0.59, and 0.11 are chosen because the
    // human eye is more sensitive to green light, and less to blue.
    float grey = dot(color, float3(0.3, 0.59, 0.11));

    return lerp(grey, color, saturation);
}


// 2/11/2010
// All the different instancing techniques share this same pixel shader.
void PixelShaderForMaterials(VertexShaderOutput input, uniform float3 oDiffuseColor, uniform bool oUseNormalMap, uniform bool oUseSpecularMap,
						     uniform bool oUseIllumMap, uniform bool oShowTeamColor, out float3 Normal, out float3 viewDirection, out float3 lightDirection, 
						     out PixelToFrameStruct Output)
{
	Output = (PixelToFrameStruct)0;
	
	
	float4 diffuseTexture = tex2D(Sampler, input.TextureCoordinate);	
	Normal = 0;
	float specular = 0;
	float4 Color = 0;	
	
	viewDirection = normalize(input.viewDirection); //creating the eye vector  
    lightDirection = normalize(input.lightDirection); //creating the light vector

	
	// 11/19/2008 - Apply Team Colors	
	if (oShowTeamColor)
	{			
		float inv = 1.0f - diffuseTexture.a;
		Color = float4((diffuseTexture.rgb * inv) + (input.TeamColor.rgb * diffuseTexture.a), 1.0f);
    }
    else
		Color = diffuseTexture;		
    
    // BumpMapping Calculation   
    Normal = (oUseNormalMap) ? CalculateNormalLighting(input.TextureCoordinate, input.tangentToWorld) : input.tangentToWorld[2].xyz; // Normal Mapping 	
	Normal = normalize(Normal);  // Normalize Normal
	
		
	// 2/10/2009 - Is there a Specular Map we can use?
	float4 specColor = (oUseSpecularMap) ? 2 * tex2D(TextureSpecularSampler, input.TextureCoordinate) : float4(0,0,0,1);	
	Color.a = 1.0f;
	
	
	// 2/1/2009 - Illum Mapping Calculations	
	if (oUseIllumMap)		
	{
		float3 IllumMapColor = tex2D(TextureIllumSampler, input.TextureCoordinate);	 
		
		
		// 2/2/2009
		if (xOscillateIllum)
			IllumMapColor *= sin(xOscillateSpeed * xTime);			
		
		// Check if using IllumColor or TeamColor as default.
		if (xIllumColor.r == 0 && xIllumColor.g == 0 && xIllumColor.b == 0 && xIllumColor.a == 0)
		{
			// Apply Color
			float inv = 1.0f - IllumMapColor.r;
			Color.rgb = float4((Color.rgb * inv) + ((input.TeamColor.rgb * 5) * IllumMapColor.r), 125.0f); // 1/26/2010 - Test RT3
		}
		else
		{	
			// Apply Color
			float inv = 1.0f - IllumMapColor.r;
			Color.rgb = float4((Color.rgb * inv) + ((xIllumColor.rgb * 5) * IllumMapColor.r), 125.0f); // 1/26/2010 - Test RT3	 
		}
		
		// 2/18/2009
		float glow_intensity = saturate(dot(IllumMapColor.xyz, 1.0) + dot(specular, 1.0));
		Color.a = glow_intensity; // 1/26/2010 - Test RT3	
		
	}
	else
		Color.a = 0.0f; // 2/18/2009 - used by BlurManager for Glow; if no GlowMap, then set zero value!	
	
	
	// Shadow Mapping Section 
    if (xEnableShadows)
	{
		float vTotalLightDiffuse = CalculateShadowMapLight(input);
		Color.rgb *= (vTotalLightDiffuse < 0.3f) ? 0.3f : vTotalLightDiffuse; // 3/19/2011 - Updated keep at least 0.3f of ambient light.  
	}
    
    // 5/23/2010
	//-------------------
	// Bloom Effect
	//-------------------	
	{
		// 1st: Extract bloom.
		// Adjust it to keep only values brighter than the specified threshold.
		float4 bloomExtract = saturate((Color - BloomThreshold) / (1 - BloomThreshold));
	
		// 2nd: Calc final bloom.	    
		// Adjust color saturation and intensity.
		float4 bloom = AdjustSaturation(bloomExtract, BloomSaturation) * BloomIntensity;
		float4 base = AdjustSaturation(Color, BaseSaturation) * BaseIntensity;
	    
		// Darken down the base image in areas where there is a lot of bloom,
		// to prevent things looking excessively burned-out.
		base *= (1 - saturate(bloom));
		
		// Combine final pixel color.
		Color = base + bloom;		
	}
	
	Output.Color = Color; 	
    
}

// 1/30/2010 - Empty FogOfWar PS.
PixelToFrameStruct PixelShaderFunction_FOW()
{
	PixelToFrameStruct Output = (PixelToFrameStruct)0;
	return Output;
}

PixelToFrameStruct PixelShaderFunction_Deferred(VertexShaderOutput_Deferred input)
{
	PixelToFrameStruct Output = (PixelToFrameStruct)0;
	
	float4 diffuse = tex2D(Sampler, input.TextureCoordinate);	
	float4 DiffuseColor = 0;	
	float3 specular = 0;
	float4 Color = 0;
	const float Shininess = 0.3f;

	
	// 11/19/2008 - Apply Team Colors
	if (oShowTeamColor)
	{			
		float inv = 1.0f - diffuse.a;
		DiffuseColor = float4((diffuse.rgb * inv) + (input.TeamColor.rgb * diffuse.a), 1.0f);
    }
    else
		DiffuseColor = diffuse;
		
	float3 AmbientColor = (DiffuseColor * oAmbientColor);  
    Color.rgb = DiffuseColor.rgb + AmbientColor.rgb;
	
	
	// 2/1/2009 - Illum Mapping Calculations	
	Output.Color3 = 0;
	if (oUseIllumMap)		
	{
		float3 IllumMapColor = tex2D(TextureIllumSampler, input.TextureCoordinate);	 
		
		
		// 2/2/2009
		if (xOscillateIllum)
			IllumMapColor *= sin(xOscillateSpeed * xTime);			
		
		// Check if using IllumColor or TeamColor as default.
		if (xIllumColor.r == 0 && xIllumColor.g == 0 && xIllumColor.b == 0 && xIllumColor.a == 0)
		{
			// Apply Color
			float inv = 1.0f - IllumMapColor.r;
			Color.rgb = float4((Color.rgb * inv) + ((input.TeamColor.rgb * 5) * IllumMapColor.r), 125.0f); 
		}
		else
		{	
			// Apply Color
			float inv = 1.0f - IllumMapColor.r;
			Color.rgb = float4((Color.rgb * inv) + ((xIllumColor.rgb * 5) * IllumMapColor.r), 125.0f); 	
		}
		
		// 2/18/2009
		float glow_intensity = saturate(dot(IllumMapColor.xyz, 1.0) + dot(specular.xyz, 1.0));
		//Color.a = glow_intensity;
		
		// 3/17/2009 - Test 4th render target
		Output.Color3.rgb = Color.rgb;
		Output.Color3.a = glow_intensity;
		
	}
	else
		Color.a = 0.0f; // 2/18/2009 - used by BlurManager for Glow; if no GlowMap, then set zero value!	
	
	
	// Bump Mapping
	float3 BumpMapColor = (tex2D(TextureBumpSampler, input.TextureCoordinate) - 0.5f) * 2;
	float3 NormalW = mul(BumpMapColor, input.tangentToWorld);
	
	// 2/10/2009 - Specular Mapping
	if (oUseSpecularMap)
	{
		float4 specColor = 2 * tex2D(TextureSpecularSampler, input.TextureCoordinate);
		// Store Specular Intensity into the Normal's Alpha channel.
		Output.Color1.a = specColor.r;
	}
	else
		Output.Color1.a = 0.1f; // default
	
	
	// 3/14/2009 - Def Rendering test
	Output.Color = Color;	
	Output.Color1.xyz = NormalW/2.0f+0.5f;	
	Output.Color2 = input.Position2D.z/input.Position2D.w;	
	
	// 3/20/2009 - Test 2-RT Deferred Rendering
	/*float3 tmpNormal = NormalW/2.0f+0.5f;	
	Output.Normal.xy = tmpNormal.xy;	
	Output.Normal.z = input.Position2D.z / 256; 
	Output.Normal.w = input.Position2D.w / 256;*/
   
	return Output;		
    
}


// 2/14/2010: Updated to be a Shared method, called by the Materials.
// 6/17/2009: Updated to use Normal mapping and Phong lighting.
// 9/21/2008 - Created this seperate PS-Shader for the AlphaMap 2 Draw
//             Calls; otherwise, any models using the 2-draw method was not
//             showing up on screen when mixed with the 'Phong' lighting.
void PixelShaderFunctionAlphaDraw(VertexShaderOutput input, uniform bool useNormalMap, uniform bool useSpecularMap, out float3 normal, 
								  out float3 viewDirection, out float3 lightDirection, out float4 diffuseColor, out float4 specularColor) 
{	
	normal = float3(0,1,0);	
	
	// BumpMapping Calculation	
	normal = (useNormalMap) ? CalculateNormalLighting(input.TextureCoordinate, input.tangentToWorld) : input.tangentToWorld[2].xyz;	
	normal = normalize(normal);  // Normalize Normal
	
	viewDirection = normalize(input.viewDirection);	//creating the eye vector  
    lightDirection = normalize(input.lightDirection); //creating the light vector   
	
	diffuseColor = tex2D(Sampler, input.TextureCoordinate);					
	// 2/10/2009 - Is there a Specular Map we can use?
	specularColor = (useSpecularMap) ? 2 * tex2D(TextureSpecularSampler, input.TextureCoordinate) : float4(0.5f,0.5f,0.5f,1);	
    
}