//Used by the following Post Process shaders
sampler SceneSampler : register(s0);

//-----"GenerateNoise" Technique-----
//Generates a bunch of noise from

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
float NoiseShift;  //Should be between 0.0f and 1.0f
float Sharpness;  //Lower for more sharpness

float4 GenerateNoisePS (float2 TexCoord : TEXCOORD0) : COLOR0
{
	float2 shift = float2(1, 1);
	float4 color = tex2D(TextureSampler, fmod(TexCoord + NoiseShift * shift, shift)) / 2.0f;
    color += tex2D(TextureSampler, fmod(TexCoord * 2 + NoiseShift * shift, shift)) / 4.0f;
    color += tex2D(TextureSampler, fmod(TexCoord * 4 + NoiseShift * shift, shift)) / 8.0f;
    color += tex2D(TextureSampler, fmod(TexCoord * 8 + NoiseShift * shift, shift)) / 16.0f;
    color += tex2D(TextureSampler, fmod(TexCoord * 16 + NoiseShift * shift, shift)) / 32.0f;
    color += tex2D(TextureSampler, fmod(TexCoord * 32 + NoiseShift * shift, shift)) / 32.0f;
    color = pow(color, Sharpness);
	return color;
}

technique GenerateNoise
{
	pass Pass1
	{
		PixelShader = compile ps_2_0 GenerateNoisePS();
	}
}