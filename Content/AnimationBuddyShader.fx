#if OPENGL
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0
#define PS_SHADERMODEL ps_4_0
#endif

// Effect applies normalmapped lighting to a 2D sprite.

float3 AmbientColor = 0.35;
float4 ColorMask = 1.0;
float Rotation = 0.0;
bool HasNormal = false;
bool FlipHorizontal = false;
bool HasColorMask = false;

float3 DirectionLights[4];
float3 DirectionLightColors[4];
uint NumberOfDirectionLights = 0;

float3 PointLights[32];
float3 PointLightColors[32];
float PointLightBrightness[32];
uint NumberOfPointLights = 0;

sampler TextureSampler : register(s0);
sampler NormalSampler : register(s1)
{
	Texture = (NormalTexture);
};
sampler ColorMaskSampler : register(s2)
{
	Texture = (ColorMaskTexture);
};

float4 main(float4 position : SV_POSITION, float4 color : COLOR0, float2 texCoord : TEXCOORD0) : COLOR0
{
	//Look up the texture value
	float4 tex = tex2D(TextureSampler, texCoord);

	//the final color we are going to use, either primary or secondary
	float4 texColor = color;

	//Dont do these calculations if the alpha channel is empty
	if (tex.a > 0.0)
	{
		//If there is a palette swap, add it to the texture color
		if (HasColorMask == true)
		{
			//Get the color from the palette swap texture
			float4 paletteSwap = tex2D(ColorMaskSampler, texCoord);
			if (paletteSwap.a > 0.0)
			{
				texColor = (texColor * (1.0 - paletteSwap.a)) + (paletteSwap * ColorMask);
			}
		}

		//Dont do these calculations if there is no normal map
		if (HasNormal == true)
		{
			//the final light value that will be added to the texture color. Don't allow light level to go below the ambient light level
			float3 lightColor = AmbientColor;

			//Look up the normalmap value
			float4 normal = tex2D(NormalSampler, texCoord);
			if (FlipHorizontal == true)
			{
				//If we are drawing a flipped image, reverse the normal
				normal.x = 1 - normal.x;
			}
			normal = 2.0 * normal - 1.0;

			//Loop through all the directional lights. 
			[loop]
			for (uint i = 0; i < 4; i++)
			{
				if (i >= NumberOfDirectionLights)
				{
					//This is done in this goofy way because the GLSL transpiler is goofed
					break;
				}

				//compute the rotated light direction
				float3 rotatedLight = DirectionLights[i];
				if (Rotation != 0.0)
				{
					float cs = cos(-Rotation);
					float sn = sin(-Rotation);

					float px = rotatedLight.x * cs - rotatedLight.y * sn;
					float py = rotatedLight.x * sn + rotatedLight.y * cs;
					rotatedLight.x = px;
					rotatedLight.y = py;
				}

				//Compute lighting.
				float lightAmount = max(dot(normal.xyz, rotatedLight), 0.0);
				lightColor += (lightAmount * DirectionLightColors[i]);
			}

			//Loop through all the point lights
			[loop]
			for (uint i = 0; i < 32; i++)
			{
				if (i >= NumberOfPointLights)
				{
					//This is done in this goofy way because the GLSL transpiler is goofed
					break;
				}

				//Get the vector from the point light to the pixel position
				float3 rotatedLight = { PointLights[i].x - position.x, -1 * (PointLights[i].y - position.y), PointLights[i].z };
				rotatedLight = normalize(rotatedLight);

				//compute the rotated light direction
				if (Rotation != 0.0)
				{
					float cs = cos(-Rotation);
					float sn = sin(-Rotation);

					float px = rotatedLight.x * cs - rotatedLight.y * sn;
					float py = rotatedLight.x * sn + rotatedLight.y * cs;
					rotatedLight.x = px;
					rotatedLight.y = py;
				}

				//Compute lighting.
				float lightAmount = saturate(dot(normal.xyz, rotatedLight)) * PointLightBrightness[i];
				lightColor += (lightAmount * PointLightColors[i]);

				//if (lightAmount > 0.0)
				//{

				//	// Sample the pixel from the specular map texture.
				//	float specularIntensity = 1;

				//	// Calculate the reflection vector based on the light intensity, normal vector, and light direction.
				//	float3 reflection = normalize(2 * lightAmount * normal - rotatedLight);

				//	// Determine the amount of specular light based on the reflection vector, viewing direction, and specular power.
				//	float3 viewDirection = { 0,0,-1 };
				//	float specularPower = 16;
				//	float4 specular = pow(saturate(dot(reflection, viewDirection)), specularPower);

				//	// Use the specular map to determine the intensity of specular light at this pixel.
				//	specular = specular * specularIntensity;

				//	// Add the specular component last to the output color.
				//	lightColor = saturate(lightColor + specular);
				//}
			}

			texColor.rgb *= lightColor;
		}
	}

	return tex * texColor;
}

technique Normalmap
{
	pass Pass1
	{
		PixelShader = compile PS_SHADERMODEL main();
	}
}
