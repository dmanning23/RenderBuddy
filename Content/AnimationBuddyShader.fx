#if OPENGL
#define SV_POSITION POSITION
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

sampler TextureSampler : register(s0);
sampler NormalSampler : register(s1)
{
	Texture = (NormalTexture);
};
sampler ColorMaskSampler : register(s2)
{
	Texture = (ColorMaskTexture);
};

float4 main(float4 color : COLOR0, float2 texCoord : TEXCOORD0) : COLOR0
{
	//Look up the texture value
	float4 tex = tex2D(TextureSampler, texCoord);

	//the final color we are going to use, either primary or secondary
	float4 texColor = color;

	if (tex.a > 0.0)
	{
		//If there is a palette swap, add it to the texture color
		if (HasColorMask == true)
		{
			//Get the texture from the palette
			float4 paletteSwap = tex2D(ColorMaskSampler, texCoord);
			if (paletteSwap.a > 0.0)
			{
				texColor = (texColor * (1.0 - paletteSwap.a)) + (paletteSwap * ColorMask);
			}
		}

		//Dont do these calculations if the alpha channel is empty
		if (HasNormal == true)
		{
			//the final light value that will be added to the texture color. Don't allow light level to go below the ambient light level
			float3 lightColor = AmbientColor;

			//Look up the normalmap value
			float4 normal = 2.0 * tex2D(NormalSampler, texCoord) - 1.0;

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

				if (FlipHorizontal == true)
				{
					rotatedLight.x *= -1.0;
				}

				//Compute lighting.
				float lightAmount = max(dot(normal.xyz, rotatedLight), 0.0);
				lightColor += (lightAmount * DirectionLightColors[i]);
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
