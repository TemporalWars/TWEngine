//**********************************************************************
// Vertex Shaders
//**********************************************************************

float4x4 Model : WORLD;
float4x4 View : VIEW;
float4x4 Projection : PROJECTION;

// Ben
float4x4 g_LightWorldViewProjection;  // Light's View/Projection Matrix


#define MaxBones 58
float4x4 Bones[MaxBones];
float4x4 skinTransform;
float4 weightedposition;
float3 N, T, B;
XSI_VertexToPixel VSSkinned(XSI_AppToVertex IN)
{
	XSI_VertexToPixel OUT;

	// Blend between the weighted bone matrices.
	skinTransform = 0;

	skinTransform += Bones[IN.BoneIndices.x] * IN.BoneWeights.x;
	skinTransform += Bones[IN.BoneIndices.y] * IN.BoneWeights.y;
	skinTransform += Bones[IN.BoneIndices.z] * IN.BoneWeights.z;
	skinTransform += Bones[IN.BoneIndices.w] * IN.BoneWeights.w;

	// Skin the vertex position.
 	weightedposition = mul(IN.position, skinTransform);

	// transform in screen space
	OUT.position = mul( mul(weightedposition, View), Projection );

	// position in global space is in TC4
	OUT.texcoord4 = weightedposition;
	
	// Tangent to world space is stored in TC5,6,7	
	N = IN.normal;
	T = (IN.color0 * 2) - 1;
	B = N.yzx * T.zxy;
	B = (-T.yzx * N.zxy) + B;
	
	N = mul(N, Model);
	T = mul(T, Model);
	B = mul(B, Model);
	
	OUT.texcoord5 = normalize(B).xyzz;
	OUT.texcoord6 = normalize(T).xyzz;
	OUT.texcoord7 = normalize(N).xyzz;
	
	// position in global space is stored in TC7
	
	// these texture coordinates are used as texture coordinates
	OUT.texcoord0 = IN.texcoord0;
	OUT.texcoord1 = IN.texcoord1;
	OUT.texcoord2 = IN.texcoord2;
	OUT.texcoord3 = IN.texcoord3;
	
	// leftovers
	OUT.color0 = IN.color0;
	OUT.color1 = IN.color1;

	return OUT;
}

float4 position;
XSI_VertexToPixel VSStatic(XSI_AppToVertex IN)
{
	XSI_VertexToPixel OUT;

 	position = mul(IN.position, Model);

	// transform in screen space
	OUT.position = mul( mul(position, View), Projection );

	// position in global space is in TC4
	OUT.texcoord4 = position;
	
	// Tangent to world space is stored in TC5,6,7	
	N = IN.normal;
	T = (IN.color0 * 2) - 1;
	B = N.yzx * T.zxy;
	B = (-T.yzx * N.zxy) + B;
	
	N = mul(N, Model);
	T = mul(T, Model);
	B = mul(B, Model);
	
	OUT.texcoord5 = normalize(B).xyzz;
	OUT.texcoord6 = normalize(T).xyzz;
	OUT.texcoord7 = normalize(N).xyzz;
	
	// position in global space is stored in TC7
	
	// these texture coordinates are used as texture coordinates
	OUT.texcoord0 = IN.texcoord0;
	OUT.texcoord1 = IN.texcoord1;
	OUT.texcoord2 = IN.texcoord2;
	OUT.texcoord3 = IN.texcoord3;
	
	// leftovers
	OUT.color0 = IN.color0;
	OUT.color1 = IN.color1;

	return OUT;
}

// Ben
XSI_ShadowVertexToPixel VSShadowSkinned(XSI_AppToVertex IN)
{
	XSI_ShadowVertexToPixel OUT = (XSI_ShadowVertexToPixel)0;

	// Blend between the weighted bone matrices.
	skinTransform = 0;

	skinTransform += Bones[IN.BoneIndices.x] * IN.BoneWeights.x;
	skinTransform += Bones[IN.BoneIndices.y] * IN.BoneWeights.y;
	skinTransform += Bones[IN.BoneIndices.z] * IN.BoneWeights.z;
	skinTransform += Bones[IN.BoneIndices.w] * IN.BoneWeights.w;

	// Skin the vertex position.
 	weightedposition = mul(IN.position, skinTransform);

	// transform in screen space
	OUT.position = mul(weightedposition, g_LightWorldViewProjection);
	
	// Calculate Depth
	OUT.Position2D.x = (OUT.position.z / OUT.position.w);
	OUT.Position2D.y = ((OUT.position.z / OUT.position.w) * (OUT.position.z / OUT.position.w));
	
	return OUT;
	
}


