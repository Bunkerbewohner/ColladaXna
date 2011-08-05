
//
// This is just a simple shader that is used as a custom shader in the COLLADA material definition
//

float4x4 WVP : WorldViewProjection;

float4 FilterColor = float4(1,1,1,1);

texture DiffuseMap;

sampler DiffuseMapSampler = sampler_state
{
	texture = <DiffuseMap>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = wrap;
	AddressV = wrap;
};

struct VertexShaderInput
{
	float4 Position : POSITION;
	float2 TexCoord : TEXCOORD0;
};

struct PixelShaderInput
{
	float4 Position : POSITION;
	float2 TexCoord : TEXCOORD0;
};

PixelShaderInput VertexShaderFunction(VertexShaderInput input)
{
	PixelShaderInput output = (PixelShaderInput)0;

	output.Position = mul(input.Position, WVP);
	output.TexCoord = input.TexCoord;

	return output;
}

float4 PixelShaderFunction(PixelShaderInput input) : COLOR
{
	float4 color = tex2D(DiffuseMapSampler, input.TexCoord) * FilterColor;
	return color;
}

technique SimpleTechnique
{
	pass Pass0
	{      
		VertexShader = compile vs_2_0 VertexShaderFunction();  
		PixelShader = compile ps_2_0 PixelShaderFunction();
	}
}