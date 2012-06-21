// 1/27/2009
// Shadow Methods
// Ben Scharbach

float g_fSlopeBias = 4.0;

//
// 6/30/2009 - Misc Variance functions
//

float linstep(float min, float max, float v)
{
    return clamp((v - min) / (max - min), 0, 1);
}

// Rescale into [0, 1]
float RescaleDistToLight(float Distance)
{
    return linstep(0, 2500, Distance);
}

// 5/31/2010 - Helper method, to calculate the distance.
float CalculateDistance(float4 ShadowTexC)
{
	float distance = (ShadowTexC.z / ShadowTexC.w);	
	float ddistdx = ddx(distance);
	float ddistdy = ddy(distance);
	distance += g_fSlopeBias * abs(ddistdx);
	distance += g_fSlopeBias * abs(ddistdy);
	
	return distance;
}

// SHADOW METHODS

float SimpleShadowingMethod(float4 ShadowTexC, sampler2D ShadowMapSampler, float depthBias, float shadowDarkness)
{
	float depthStoredInShadowMap = tex2D(ShadowMapSampler, ShadowTexC.xy).r;
	//float depthStoredInShadowMap = tex2Dproj(ShadowMapSampler, ShadowTexC).r;	
	
    // 5/31/2010 - Refactored out into new helper method.
	float distance = CalculateDistance(ShadowTexC);
    
    // Check to see if this pixel is in front or behind the value in the shadow map
    return depthStoredInShadowMap < (distance - depthBias) ? shadowDarkness : 1.0f;
    
}


// 10/27/2009: Updated to include new param 'shadowDarkness'.
// 2/16/2009 - PCF Shadow Sampling Method.
float PCFShadowSampleMethod(float4 ShadowTexC, float2 PCFSamples[9], sampler2D ShadowMapSampler, float depthBias, float shadowDarkness)
{
	const int iterations = 9.0;
	float4 result = {1,1,1,1};
    
    // 5/31/2010 - Refactored out into new helper method.
	float distance = CalculateDistance(ShadowTexC);
	
	result = float4(0, 0, 0, 0);
	
	float shadowTerms[iterations];
	float shadowTerm = 0.0f;
	
	#ifdef XBOX
	[unroll]
	#endif
	for( int i = 0; i < iterations; i++ )
	{
		float4 ShadowTexCWithPCF = float4(ShadowTexC.xy + PCFSamples[i].xy, ShadowTexC.zw);
		float depthStoredInShadowMap = tex2D(ShadowMapSampler, ShadowTexCWithPCF).r;		
		
		// Texel is shadowed
		shadowTerms[i] = depthStoredInShadowMap < (distance - depthBias) ? shadowDarkness : 1.0f;
		shadowTerm += shadowTerms[i];		
	}	
	
	shadowTerm /= iterations;	
	  
    return shadowTerm;

}

// 5/31/2010 - GPU Gems - Chap-11 optimization for PCF.
float3 offset_lookup(sampler2D ShadowMapSampler, float4 ShadowTexC, float2 offset, float2 halfPixels)
{
	return tex2D(ShadowMapSampler, float4(ShadowTexC.xy + offset * halfPixels * ShadowTexC.w, 
					ShadowTexC.z, ShadowTexC.w));
}

// 5/31/2010 - GPU Gems - Chap-11 optimization for PCF.
float PCFShadowSampleMethod2(float4 ShadowTexC, float2 position, float2 halfPixels, sampler2D ShadowMapSampler, float depthBias, float shadowDarkness)
{

	// 5/31/2010 - Refactored out into new helper method.
	float distance = CalculateDistance(ShadowTexC);
		
	float shadowTerm = 0.0f;
	
	float2 offset = (float)(frac(position.xy * 0.5) > 0.25);  // mod
	offset.y += offset.x;  // y ^= x in floating point

	if (offset.y > 1.1)	offset.y = 0;

	float depthStoredInShadowMap = (offset_lookup(ShadowMapSampler, ShadowTexC, offset +
                             float2(-1.5, 0.5), halfPixels) +
               offset_lookup(ShadowMapSampler, ShadowTexC, offset +
                             float2(0.5, 0.5), halfPixels) +
               offset_lookup(ShadowMapSampler, ShadowTexC, offset +
                             float2(-1.5, -1.5), halfPixels) +
               offset_lookup(ShadowMapSampler, ShadowTexC, offset +
                             float2(0.5, -1.5), halfPixels) ) * 0.25;	
	
	// Texel is shadowed
	shadowTerm = depthStoredInShadowMap < (distance - depthBias) ? shadowDarkness : 1.0f;
	  
    return shadowTerm;

}
 
// 12/12/2009: Updated to include new param 'shadowDarkness'.
float VarianceShadowingMethod(float4 ShadowTexC, sampler2D ShadowMapSampler, float depthBias, float lightDistance, float shadowDarkness)
{	
	//float2 moments = tex2Dproj(ShadowMapSampler, ShadowTexC);		
	float2 moments = tex2D(ShadowMapSampler, ShadowTexC.xy);
	
	float distance;
	[branch]
	if (lightDistance = -1) // 6/30/2009
		distance = (ShadowTexC.z / ShadowTexC.w); 
	else
		distance = RescaleDistToLight(lightDistance);	
	
	float ddistdx = ddx(distance);
	float ddistdy = ddy(distance);
	distance += g_fSlopeBias * abs(ddistdx);
	distance += g_fSlopeBias * abs(ddistdy);
	
	// standard shadow mnap comparison
	float lit_factor = (distance <= moments.x);
		
    float E_x2 = moments.y;
    float Ex_2 = moments.x * moments.x;    
    float variance = min(max(E_x2 - Ex_2, 0.0f) + depthBias, 1.0); // 0.000005f ; depthBias
        
    float m_d = (moments.x - distance);    
    float p = (variance / (variance + m_d * m_d));  // Chebychev's inequality    	
   	
   	// 12/12/2009 - Apply ShadowDarkness
   	p *= 0.50f; // Required in order to see any shadow at all!
   	p *= shadowDarkness; 
   	
   	float shadowTerm = max(lit_factor, p); 
   	return shadowTerm;
	//return 1.0f * shadowTerm < 0.5 ? 0.5f : 1.0f * shadowTerm;
	
	//return max(step(distance, moments.x), p); // (White RT?)
	//return max(step(moments.x, distance), p); // (Black RT)
	
}

// 6/4/2009 - Calculates a ShadowMap's TextureCoords.
 float4 CalculateShadowTexCoords(float4 worldPosition, float4x4 lightViewProjection, float2 halfPixel)
 {
		// Find the position of this pixel in light space
		float4 lightingPosition = mul(worldPosition, lightViewProjection);	
					
    
		// Calculate the texture projection texcoords, we divide by 2 
		// and add 0.5 because we need to convert the coordinates 
		// from a -1.0 - 1.0 range into the 0.0 - 1.0 range	
		// 7/14/2009: Note that there is no need to divide by w for othogonal light sources	
		float4 ShadowTexC;		
		ShadowTexC.x = (lightingPosition.x / 2.0f) + 0.5f; // was - (lightingPosition.x / lightingPosition.w / 2.0f) + 0.5
		ShadowTexC.y = (-lightingPosition.y / 2.0f) + 0.5f; 		
		ShadowTexC.z = lightingPosition.z;
		ShadowTexC.w = lightingPosition.w; 		
		
		return ShadowTexC; 
 }
 
 // 10/27/2009: Updated to include new param 'shadowDarkness'.
 // 6/4/2009 - Calculate the Total Light Factor for a ShadowMap coordinates.
 float CalculatePCFShadowMapLightFactor(float vTotalLightDiffuse, float4 ShadowTexC, float2 PCFSamples[9], sampler2D ShadowMapSampler, float depthBias, float shadowDarkness)
 {		
		if ((saturate(ShadowTexC.x) == ShadowTexC.x) && (saturate(ShadowTexC.y) == ShadowTexC.y))
		{	
			vTotalLightDiffuse = PCFShadowSampleMethod(ShadowTexC, PCFSamples, ShadowMapSampler, depthBias, shadowDarkness);		
		}
		 
		return vTotalLightDiffuse;
 }
 
 // 5/31/2010 - GPU Gems - Chap-11 optimization for PCF.
 float CalculatePCFShadowMapLightFactor2(float vTotalLightDiffuse, float4 ShadowTexC, float2 position, float2 halfPixels, sampler2D ShadowMapSampler, float depthBias, float shadowDarkness)
 {		
		if ((saturate(ShadowTexC.x) == ShadowTexC.x) && (saturate(ShadowTexC.y) == ShadowTexC.y))
		{	
			vTotalLightDiffuse = PCFShadowSampleMethod2(ShadowTexC, position, halfPixels, ShadowMapSampler, depthBias, shadowDarkness);		
		}
		 
		return vTotalLightDiffuse;
 }
 
 // 6/4/2009 - Calculate the Total Light Factor for a ShadowMap coordinates.
 float CalculateVarianceShadowMapLightFactor(float vTotalLightDiffuse, float4 ShadowTexC, sampler2D ShadowMapSampler, float depthBias, float lightDistance, float shadowDarkness)
 {		
		if ((saturate(ShadowTexC.x) == ShadowTexC.x) && (saturate(ShadowTexC.y) == ShadowTexC.y))
		{			
			vTotalLightDiffuse = VarianceShadowingMethod(ShadowTexC, ShadowMapSampler, depthBias, lightDistance, shadowDarkness);					
		}
		 
		return vTotalLightDiffuse;
 }
 
 // 12/12/2009: Updated to include new param 'shadowDarkness'.
 // 6/4/2009 - Calculate the Total Light Factor for a ShadowMap coordinates.
 float CalculateSimpleShadowMapLightFactor(float vTotalLightDiffuse, float4 ShadowTexC, sampler2D ShadowMapSampler, float depthBias, float shadowDarkness)
 {		
		if ((saturate(ShadowTexC.x) == ShadowTexC.x) && (saturate(ShadowTexC.y) == ShadowTexC.y))
		{	
			vTotalLightDiffuse = SimpleShadowingMethod(ShadowTexC, ShadowMapSampler, depthBias, shadowDarkness);				
		}
		 
		return vTotalLightDiffuse;
 } 
 
 
 
 




