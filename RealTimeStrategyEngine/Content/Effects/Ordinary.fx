float4x4 World;
float4x4 ViewXProjection;
float OrdinaryTransparency;  //Used by OrdinaryAlpha, OrdinaryTint, PointSpriteAuto, and Formation
float4 OrdinaryColor;  //Used by OrdinaryTint, BruteForceColor, and Formation

//-----"Ordinary" Technique-----
//Displays things as they are
struct VSOut
{
    float4 Position : POSITION0;
    float4 Color	: COLOR0;
};

VSOut OrdinaryVS(
	float4 Position : POSITION0
	, float4 Color : COLOR0)
{
	VSOut output = (VSOut) 0;
    output.Position = mul (mul (Position, World), ViewXProjection);
    output.Color = Color;
    
    return output;
}

float4 OrdinaryPS(VSOut input) : COLOR0
{
    return input.Color;
}

technique Ordinary
{
    pass Pass1
    {
        VertexShader = compile vs_1_1 OrdinaryVS();
        PixelShader = compile ps_1_1 OrdinaryPS();
    }
}

//-----"OrdinaryAlpha" Technique-----
//Displays things as they are


float4 OrdinaryAlphaPS(VSOut input) : COLOR0
{
	input.Color.a *= OrdinaryTransparency;
    return input.Color;
}

technique OrdinaryAlpha
{
    pass Pass1
    {
        VertexShader = compile vs_1_1 OrdinaryVS();
        PixelShader = compile ps_1_1 OrdinaryAlphaPS();
    }
}

//-----"OrdinaryTint" Technique-----
//Tints models to a set color parameter

float4 OrdinaryTintPS(VSOut input) : COLOR0
{
    input.Color.r *= OrdinaryColor.r;
    input.Color.g *= OrdinaryColor.g;
    input.Color.b *= OrdinaryColor.b;
    input.Color.a *= OrdinaryTransparency;
	return input.Color;
}

technique OrdinaryTint
{
    pass Pass1
    {
        VertexShader = compile vs_1_1 OrdinaryVS();
        PixelShader = compile ps_1_1 OrdinaryTintPS();
    }
}

//-----"BruteForceColor" Technique-----
//Displays things with only one specified color

float4 BruteForcePS(VSOut input) : COLOR0
{
	input.Color = OrdinaryColor;
    return input.Color;
}

technique BruteForceColor
{
    pass Pass1
    {
        VertexShader = compile vs_1_1 OrdinaryVS();
        PixelShader = compile ps_1_1 BruteForcePS();
    }
}