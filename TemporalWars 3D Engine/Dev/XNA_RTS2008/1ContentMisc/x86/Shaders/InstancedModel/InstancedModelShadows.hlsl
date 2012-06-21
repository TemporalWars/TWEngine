float4x4 xLightViewProjection22; // 3/24/2010 

// Shadow VS_Struct
struct VS_SHADOW_INPUT
{
	float4 Position			  : POSITION0;
	float2 TextureCoordinate  : TEXCOORD0;  // 6/3/2009
	float3 Velocity			  : TEXCOORD6; // 6/4/2010 - Used to animate explosions.
};
struct VS_SHADOW_OUTPUT
{
	float4 Position			  : POSITION;	
	float ShadowMapDepth	  : TEXCOORD0;		
	float2 TextureCoordinate  : TEXCOORD1;  // 6/3/2009	
	float4 WorldPos		      : TEXCOORD2; // 7/14/2009
	
};
// Shadow PS_Struct
struct PS_OUTPUT
{
    float4 Color : COLOR0;  // Pixel color    
};


//
// 7/22/2008 - Shadow VS & PS Shaders
//
VS_SHADOW_OUTPUT RenderShadowMap(VS_SHADOW_INPUT input, InstanceDataStruct instanceTransform)
{
	VS_SHADOW_OUTPUT Output = (VS_SHADOW_OUTPUT)0;
	
	float4 worldPosition, viewPosition;
    float3x3 rotMatrix;
 
// 8/28/2009 - WINDOWS Only section    
#ifdef XBOX360   
#else
    // 8/28/2009 - Do transpose here; used to be done when float4x4 was used in param.
    instanceTransform.Transform = transpose(instanceTransform.Transform);
#endif

	// 8/28/2009 - Split the Fractional 'PlayerNumber', into its Integer & Frac portions; the 
	//             Integer portion represents the ProceduralMaterialId, while the fractional is for teamColors.
	//
	//             Note: The ModF HLSL function will return frac strangely; for example, 1.02 value will 
	//                   be returned as 0.01999998.  Therefore, the value is being multiplied by 100, to 
	//                   give 1.999998, and then rounded, to get the correct value of 2!
	//	
	float materialId = 0.0f;
	float teamColor = round(modf(instanceTransform.PlayerNTime.x, materialId) * 100); // x channel holds player/colors.	
	
	// 7/14/2009 - Make sure channel M44 is 1.0, otherwise the Shadows will 'Peter-Pan' from
	//             their objects!
	instanceTransform.Transform[3][3] = 1.0f;
	
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
	
	// 6/11/2010 - Check if is ExplosionPiece, so position can be done.	
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
	
	Output.WorldPos = worldPosition;	
	Output.Position = mul(Output.WorldPos, xLightViewProjection22);		
	
	// 6/6/2009
	Output.ShadowMapDepth = (Output.Position.z / Output.Position.w);
	
	
	return Output;
}

// Used on Windows
VS_SHADOW_OUTPUT ShadowInstancingVS(VS_SHADOW_INPUT input, float instanceIndex : TEXCOORD1)
{
	VS_SHADOW_OUTPUT Output = (VS_SHADOW_OUTPUT)0;	
	
	// 8/28/2009 - Create custom 'InstanceDataStruct'.
    InstanceDataStruct instanceDataStruct = (InstanceDataStruct)0;
    instanceDataStruct.Transform = InstanceTransforms[instanceIndex];
    instanceDataStruct.PlayerNTime.x = InstanceTransforms_PlayerNTime[instanceIndex].x;// 6/6/2010
    instanceDataStruct.PlayerNTime.y = InstanceTransforms_PlayerNTime[instanceIndex].y; // 6/6/2010
    instanceDataStruct.PVelocity = InstanceTransforms_PVelocity[instanceIndex]; // 6/6/2010
		    
	Output = RenderShadowMap(input, instanceDataStruct);		
	
	// 6/3/2009 - Pass TextureCoordinates to PS
	Output.TextureCoordinate = input.TextureCoordinate;
   
    return Output;
}

#ifdef XBOX360

// 1/15/2009 - XBOX VFetch ShadowMap 
VS_SHADOW_OUTPUT VFetchShadowInstancingVS(int index : INDEX)
{
	VS_SHADOW_OUTPUT Output = (VS_SHADOW_OUTPUT)0;	
	
	int vertexIndex = 0;
	int instanceIndex = 0;
	float4 position, textureCoordinate, velocity; // 6/7/2010: Add velocity.
	
    vertexIndex = (index + 0.5) % VertexCount;
    instanceIndex = (index + 0.5) / VertexCount;    

    asm
    {
        vfetch position,          vertexIndex, position0           
        vfetch textureCoordinate, vertexIndex, texcoord0
        vfetch velocity,		  vertexIndex, texcoord6 // 6/7/2010: Add velocity.
    };

    VS_SHADOW_INPUT input = (VS_SHADOW_INPUT)0;   

    input.Position = position;   
    input.TextureCoordinate = textureCoordinate;
    input.Velocity = velocity;  // 6/7/2010: Add velocity.      

	
	 // 8/28/2009 - Create custom 'InstanceDataStruct'.
    InstanceDataStruct instanceDataStruct = (InstanceDataStruct)0;
    instanceDataStruct.Transform = InstanceTransforms[instanceIndex];
    instanceDataStruct.PlayerNTime.x = InstanceTransforms_PlayerNTime[instanceIndex].x;// 6/6/2010
    instanceDataStruct.PlayerNTime.y = InstanceTransforms_PlayerNTime[instanceIndex].y; // 6/6/2010
    instanceDataStruct.PVelocity = InstanceTransforms_PVelocity[instanceIndex]; // 6/6/2010
	 
	
	Output = RenderShadowMap(input, instanceDataStruct);			
	
	// 6/5/2009 - Pass TextureCoordinates to PS
	Output.TextureCoordinate = textureCoordinate;
   
    return Output;
}

#endif

// 12/1/2008 - Shadow Hardware Instancing version for Windows ONLY
VS_SHADOW_OUTPUT ShadowHWInstancingVS(VS_SHADOW_INPUT input, InstanceDataStruct instanceTransform : TEXCOORD1)
{
	VS_SHADOW_OUTPUT Output = (VS_SHADOW_OUTPUT)0;	
		
	Output = RenderShadowMap(input, instanceTransform);		
	
	// 6/3/2009 - Pass TextureCoordinates to PS
	Output.TextureCoordinate = input.TextureCoordinate;	
    
    return Output;
    
}


//--------------------------------------------------------------------------------------
// This shader outputs the pixel's color by modulating the texture's
//       color with diffuse material color
//--------------------------------------------------------------------------------------


float4 RenderShadowMapPS( VS_SHADOW_OUTPUT PSIn ) : Color
{
	// 8/13/2009 - Cache
	float depth = PSIn.ShadowMapDepth;
	
	return float4(depth, depth, depth, 1);	
	 
}

// 6/4/2009 - ShadowMap with Alpha testing for items like Trees and Plants.
float4 RenderShadowMapAlphaPS( VS_SHADOW_OUTPUT PSIn ) : Color
{
	float AlphaColor = tex2D(Sampler, PSIn.TextureCoordinate).a; // 6/3/2009
	
	
#ifdef XBOX360
#else
	// 7/12/2009 - For some card, like the Nvidia 7900 series, the AlphaTest
	//             for ShadowMaps doesn't seem to work correctly!  Therefore,
	//             the following line fixes this problem.
	//if (AlphaColor < 0.2f) discard;		
#endif	

	// 8/13/2009 - Cache
	float depth = PSIn.ShadowMapDepth;
	
	return float4(depth, depth, depth, AlphaColor);	
	
}

// Windows Only
technique ShadowMapRender 
{
	pass P0
	{
		// These render states are necessary to get a shadow map.
		// You should consider resetting CullMode and AlphaBlendEnable
		// before you render your main scene.		
		ZEnable = TRUE;
		ZWriteEnable = TRUE;
		//AlphaBlendEnable = FALSE; // XNA 4.0 Updates - Obsolete.
		//AlphaTestEnable = FALSE; // XNA 4.0 Updates - Obsolete.
		CullMode = NONE;
		
        VertexShader = compile vs_3_0 ShadowInstancingVS();
        PixelShader  = compile ps_3_0 RenderShadowMapPS();        
	
	}		
}

// 12/1/2008 - ShadowMap Hardware Instancing version for Windows only.
technique ShadowMapHWRender
{
	pass P0
	{
		// These render states are necessary to get a shadow map.
		// You should consider resetting CullMode and AlphaBlendEnable
		// before you render your main scene.		
		ZEnable = TRUE;
		ZWriteEnable = TRUE;
		//AlphaBlendEnable = FALSE; // XNA 4.0 Updates - Obsolete.
		//AlphaTestEnable = FALSE; // XNA 4.0 Updates - Obsolete.
		CullMode = NONE;
		
        VertexShader = compile vs_3_0 ShadowHWInstancingVS();
        PixelShader  = compile ps_3_0 RenderShadowMapPS();      
	
	}	
	
}

// 6/4/2009 - ShadowMap (Alpha) Hardware Instancing version for Windows only.
technique ShadowMapHWAlphaRender 
{
	pass P0
	{
		// These render states are necessary to get a shadow map.
		// You should consider resetting CullMode and AlphaBlendEnable
		// before you render your main scene.		
		ZEnable = TRUE;
		ZWriteEnable = TRUE;
		//AlphaBlendEnable = FALSE; // XNA 4.0 Updates - Obsolete.
		//AlphaTestEnable = TRUE; // XNA 4.0 Updates - Obsolete.
		//AlphaFunc = Greater; // XNA 4.0 Updates - Obsolete.
		//AlphaRef = 128; // 250 // XNA 4.0 Updates - Obsolete.
		SrcBlend = SrcColor;
		DestBlend = InvSrcAlpha;
		CullMode = NONE;
		
        VertexShader = compile vs_3_0 ShadowHWInstancingVS();
        PixelShader  = compile ps_3_0 RenderShadowMapAlphaPS(); // AlphaDraw PS.  
	
	}	
	
}

#ifdef XBOX360

// 1/15/2009 - XBOX ShadowMap technique
technique VFetchShadowMapRender 
{
	pass P0
	{
		// These render states are necessary to get a shadow map.
		// You should consider resetting CullMode and AlphaBlendEnable
		// before you render your main scene.		
		ZEnable = TRUE;
		ZWriteEnable = TRUE;
		//AlphaBlendEnable = FALSE; // XNA 4.0 Updates - Obsolete.
		//AlphaTestEnable = FALSE; // XNA 4.0 Updates - Obsolete.
		CullMode = NONE;
		
        VertexShader = compile vs_3_0 VFetchShadowInstancingVS();
        PixelShader  = compile ps_3_0 RenderShadowMapPS();        
	
	}
}

// 6/5/2009 - XBOX ShadowMap (Alpha) technique
technique VFetchShadowMapAlphaRender 
{
	pass P0
	{
		// These render states are necessary to get a shadow map.
		// You should consider resetting CullMode and AlphaBlendEnable
		// before you render your main scene.		
		ZEnable = TRUE;
		ZWriteEnable = TRUE;
		//AlphaBlendEnable = FALSE; // XNA 4.0 Updates - Obsolete.
		//AlphaTestEnable = TRUE; // XNA 4.0 Updates - Obsolete.
		//AlphaFunc = Greater; // XNA 4.0 Updates - Obsolete.
		//AlphaRef = 128; // 250 // XNA 4.0 Updates - Obsolete.
		SrcBlend = SrcColor;
		DestBlend = InvSrcAlpha;
		CullMode = NONE;
		
        VertexShader = compile vs_3_0 VFetchShadowInstancingVS();
        PixelShader  = compile ps_3_0 RenderShadowMapAlphaPS(); // AlphaDraw       
	
	}
}

#endif