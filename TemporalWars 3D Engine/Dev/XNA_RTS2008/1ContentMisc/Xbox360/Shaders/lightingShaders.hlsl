// 7/23/2009
// Lighting Shaders
// Ben Scharbach


// 7/23/2009 - Nvidia Plastic FN.
void plasticLighting(float3 normal, float3 lightDirection, float3 viewDirection, float4 DiffuseColor,			
					float3 SpecularLightColor, float3 AmbiColor, float Time, out float3 newDiffuse)
{
	// Constants
	const float Kd = 0.9f; // diffuse %
	const float Ks = 0.4f; // specular %
	const float SpecExpon = 30; // 1-128
	//const float3 AmbiColor = float3(0.07f, 0.07f, 0.07f);   
	
	// 1/20/2010
	float3 DiffuseContrib = 0;
	float3 SpecularContrib = 0; 
    
    float3 Hn = normalize(viewDirection + lightDirection);
    float4 litV = lit(dot(lightDirection, normal), dot(Hn, normal), SpecExpon);
    
    DiffuseContrib = litV.y * Kd * SpecularLightColor + AmbiColor;
    SpecularContrib = litV.z * Ks * SpecularLightColor;
    
    // 1/20/2010 - Create th final output value
    newDiffuse = (DiffuseColor * DiffuseContrib) + SpecularContrib; // Plastic/Blinn/Glossy
}

// 7/23/2009 - Nvidia Metal FN.
void metalLighting(float3 normal, float3 lightDirection, float3 viewDirection,	float4 DiffuseColor,		
				   float3 SpecularLightColor, float3 AmbiColor, float Time, out float3 newDiffuse)
{
	// Constants
	const float Kd = 0.1f; // diffuse %
	const float Ks = 0; // specular %
	const float SpecExpon = 12; // 1-128
	//const float3 AmbiColor = float3(0.07f, 0.07f, 0.07f);    
	
	// 1/20/2010
	float3 DiffuseContrib = 0;
	float3 SpecularContrib = 0;
    
    float3 Hn = normalize(viewDirection + lightDirection);
    float4 litV = lit(dot(lightDirection, normal), dot(Hn, normal), SpecExpon);
    
    DiffuseContrib = litV.y * Kd * SpecularLightColor + AmbiColor;
    SpecularContrib = litV.z * SpecularLightColor;
    
    // 1/20/2010 - Create th final output value
    newDiffuse = DiffuseColor * ((SpecularContrib * SpecularLightColor) + DiffuseContrib); // Metal		
    
}

// 7/23/2009 - Nvidia Blinn FN.
void blinnLighting(float3 normal, float3 lightDirection, float3 viewDirection,	float4 DiffuseColor,
				   float3 SpecularLightColor, float3 AmbiColor, float Time, out float3 newDiffuse) 
{
    // Constants
	const float Ks = 0.1f; // specular %
	const float Eccentricity = 0.3f; // 0 - 1.0
	
	// 1/20/2010
	float3 DiffuseContrib = 0;
	float3 SpecularContrib = 0;
    
    float3 Ln = lightDirection;
    float3 Vn = viewDirection;
    float3 Nn = normal;
    
    float3 Hn = normalize(Vn + Ln);
    float hdn = dot(Hn,Nn);
    float3 Rv = reflect(-Ln,Nn);
    float rdv = dot(Rv,Vn);
    rdv = max(rdv,0.001);
    float ldn=dot(Ln,Nn);
    ldn = max(ldn,0.0);
    float ndv = dot(Nn,Vn);
    float hdv = dot(Hn,Vn);
    float eSq = Eccentricity*Eccentricity;
    float distrib = eSq / (rdv * rdv * (eSq - 1.0) + 1.0);
    distrib = distrib * distrib;
    float Gb = 2.0 * hdn * ndv / hdv;
    float Gc = 2.0 * hdn * ldn / hdv;
    float Ga = min(1.0,min(Gb,Gc));
    float fresnelHack = 1.0 - pow(ndv,5.0);
    hdn = distrib * Ga * fresnelHack / ndv;
    
    DiffuseContrib = ldn * SpecularLightColor + AmbiColor;
    SpecularContrib = hdn * Ks * SpecularLightColor;  
    
    // 1/20/2010 - Create th final output value
    newDiffuse = (DiffuseColor * DiffuseContrib) + SpecularContrib; // Plastic/Blinn/Glossy 
    
}

// 7/23/2009 - Nvidia Glossy Lighting
float glossy_drop(float v,
		    uniform float top,
		    uniform float bot,
		    uniform float drop)
{
    return (drop+smoothstep(bot,top,v)*(1.0-drop));
}

void glossyLighting(float3 normal, float3 lightDirection, float3 viewDirection, float4 DiffuseColor,
				  float3 SpecularLightColor, float3 AmbiColor, float Time, out float3 newDiffuse)
{
	// Constants
	const float Ks = 0.5f; // specular %
	const float SpecExpon = 6; // 1-128
	const float GlossTopUI = 0.46f; // 0.2 - 1.0 (Bright Glossy Edge)
	const float GlossBotUI = 0.41f; // 0.05 - 0.95 (Dim Glossy Edge)
	const float GlossDrop = 0.25f; // 0 - 1.0 (Glossy Brightness Drop)
	const float3 SurfaceColor = float3(1,1,1);
	
	// 1/20/2010
	float3 DiffuseContrib = 0;
	float3 SpecularContrib = 0;

    float3 Ln = lightDirection;
    float3 Nn = normal;
    float3 Vn = viewDirection;
    float3 Hn = normalize(Vn + Ln);
    float4 litV = lit(dot(Ln,Nn),dot(Hn,Nn),SpecExpon);
    float spec = litV.y * litV.z;
    float GlossTop = max(GlossTopUI,GlossBotUI);
    float GlossBot = min(GlossTopUI,GlossBotUI);
    spec *= (Ks * glossy_drop(spec,GlossTop,GlossBot,GlossDrop));
    SpecularContrib = spec * SpecularLightColor;
    DiffuseContrib = litV.y * SurfaceColor + AmbiColor;
    
    // 1/20/2010 - Create th final output value
    newDiffuse = (DiffuseColor * DiffuseContrib) + SpecularContrib; // Plastic/Blinn/Glossy
    
}

// 1/22/2010 - Microsoft's Phong style material - Code comes from XNA sample 'NormalMapping'.
void PhongDefault(float3 normal, float3 lightDirection, float3 viewDirection, float4 DiffuseColor,
				  float3 SpecularLightColor, float3 AmbiColor, float Time, out float3 newDiffuse)
{
	// Constants
	const float Shininess = 0.3f;
	const float SpecularPower = 4.0f;
	

	// use the normal we looked up to do phong diffuse style lighting.    
    float nDotL = max(dot(normal, lightDirection), 0);
    float3 diffuse = SpecularLightColor * nDotL;
    
    // use phong to calculate specular highlights: reflect the incoming light
    // vector off the normal, and use a dot product to see how "similar"
    // the reflected vector is to the view vector.    
    float3 reflectedLight = reflect(lightDirection, normal);
    float rDotV = max(dot(reflectedLight, viewDirection), 0);
    float3 specular = Shininess * SpecularLightColor * pow(rDotV, SpecularPower);     
    
    // return the combined result.
    newDiffuse = (diffuse + AmbiColor) * DiffuseColor + specular;

}

// 2/2/2010 - Microsoft's Phong style material - This will set item to be all RED, used to show
//            'Picked' mesh items.
void PhongRed(float3 normal, float3 lightDirection, float3 viewDirection, float4 DiffuseColor,
				  float3 SpecularLightColor, float3 AmbiColor, float Time, out float3 newDiffuse)
{
    // 2/3/2010 - Use Original Phong first.
	PhongDefault(normal, lightDirection, viewDirection, DiffuseColor,
				 SpecularLightColor, AmbiColor, Time, newDiffuse);
    
    // 2/2/2010 - Set 100% Red channel, and reduce other 2 color channels by 50%.
    newDiffuse.r = 1.0f;
    newDiffuse.g = newDiffuse.g * 0.5f;
    newDiffuse.b = newDiffuse.b * 0.5f;

}

// 2/3/2010 - Microsoft's Phong style material - This will set to show the FlashWhite effect.
void PhongFlashWhite(float3 normal, float3 lightDirection, float3 viewDirection, float4 DiffuseColor,
				     float3 SpecularLightColor, float3 AmbiColor, float Time, out float3 newDiffuse)
{
	 // 2/3/2010 - Use Original Phong first.
	PhongDefault(normal, lightDirection, viewDirection, DiffuseColor,
				 SpecularLightColor, AmbiColor, Time, newDiffuse);
    
    // Do White Flash
    // (Scripting Purposes)
	newDiffuse.rgb += 255.0f * sin(Time * 7);	

}


// 2/3/2010: Updated to include the new 'Time' param into all signatures.
// 1/20/2010: Updated by adding 'DiffuseColor' param, & removing the (OUT) of SpecularContrib.
// 12/12/2009 - Use as a re-direct method, passing in the 'LightType' to use; then this method will
//              call the appropriate method below!
void DoLighting(int lightingType, float3 normal, float3 lightDirection, float3 viewDirection, float4 DiffuseColor,			
				float3 SpecularLightColor, float3 AmbiColor, float Time, out float3 newDiffuse)
{
	newDiffuse = 0;	
	
	// 1/20/2010: Tried Switch construct, but fails in shader code!
	// Which 'LightingType' user wants.
	if (lightingType == 0)
		plasticLighting(normal, lightDirection, viewDirection, DiffuseColor, SpecularLightColor, AmbiColor, Time, newDiffuse);
	else if (lightingType == 1)
		metalLighting(normal, lightDirection, viewDirection, DiffuseColor, SpecularLightColor, AmbiColor, Time, newDiffuse);
	else if (lightingType == 2)		
		blinnLighting(normal, lightDirection, viewDirection, DiffuseColor, SpecularLightColor, AmbiColor, Time, newDiffuse);	
	else if (lightingType == 3)
		glossyLighting(normal, lightDirection, viewDirection, DiffuseColor, SpecularLightColor, AmbiColor, Time, newDiffuse);
	else if (lightingType == 4) // 1/22/2010
		PhongDefault(normal, lightDirection, viewDirection, DiffuseColor, SpecularLightColor, AmbiColor, Time, newDiffuse);
	else if (lightingType == 5) // 2/2/2010 - Used when item is mesh picked by mouse ray.
		PhongRed(normal, lightDirection, viewDirection, DiffuseColor, SpecularLightColor, AmbiColor, Time, newDiffuse);
	else if (lightingType == 6) // 2/3/2010 - Used during scripting, to make an item Flash white.
		PhongFlashWhite(normal, lightDirection, viewDirection, DiffuseColor, SpecularLightColor, AmbiColor, Time, newDiffuse);
	
	
}



 
 
 




