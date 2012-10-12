float4x4 World;
float4x4 ViewXProjection;
Texture InputTexture;
sampler TextureSampler = sampler_state
{
	Texture = <InputTexture>;
	MipFilter = LINEAR;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    AddressU  = WRAP;
	AddressV  = WRAP; 
};
Texture ColorMapTexture;  //Texture containing the colors to draw or interpolate
sampler ColorMapSampler = sampler_state
{
	Texture = <ColorMapTexture>;
	MipFilter = LINEAR;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    AddressU  = WRAP;
	AddressV  = WRAP; 
};
float TextureAlphaThreshold;  //Determines the alpha cut-off

//-----"Textured" Technique-----
//Applies a texture to a model based on given UV coordinates
//Uses 'InputTexture' as a group of X, Y, and alpha values

struct VSOut
{
	float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
};

VSOut TexturedVS(
	float4 Position : POSITION0
	, float2 TexCoord : TEXCOORD0)
{
	VSOut output = (VSOut) 0;
	output.Position = mul (mul (Position, World), ViewXProjection);
	output.TexCoord = TexCoord;
	
	return output;
}

float4 TexturedPS(VSOut input) : COLOR0
{
	float4 data = tex2D(TextureSampler, input.TexCoord);
	float4 color = tex2D(ColorMapSampler, float2(data.r, data.g));
	if (TextureAlphaThreshold != 0)
	{
		color.a = (data.b - TextureAlphaThreshold)/(1.0f - TextureAlphaThreshold);
	}
	
	return color;
}

technique Textured
{
	pass Pass1
	{
		VertexShader = compile vs_2_0 TexturedVS();
		PixelShader = compile ps_2_0 TexturedPS();
	}
}