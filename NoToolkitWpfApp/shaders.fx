
cbuffer ConstantBuffer : register(b0)
{
	float4x4 WorldViewProj;
	matrix World;
	float4 LightDir;
	int Light;
}



struct VS_IN
{
	float4 pos : POSITION;
	float4 col : COLOR;
	float4 norm : NORMAL;
};

struct PS_IN
{
	float4 pos : SV_POSITION;
	float4 col : COLOR;
	float4 norm : NORMAL;
};



PS_IN VS(VS_IN input)
{
	PS_IN output = (PS_IN)0;

	output.pos = mul(WorldViewProj, input.pos);
	output.norm = mul(World, input.norm);

	output.col = input.col;

	return output;
}

float4 PS(PS_IN input) : SV_Target
{
	//return input.col;
	return 
		float4(
			saturate(dot(LightDir.xyz, input.norm.xyz) * input.col.xyz * 0.9 * Light + input.col * 0.1), 
			input.col.w
			);
}


