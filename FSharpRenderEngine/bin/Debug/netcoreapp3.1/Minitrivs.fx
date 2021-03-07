struct VS_IN
{
	float4 pos : POSITION0;
	float4 col : COLOR0;
};

struct PS_IN
{
	float4 pos : SV_POSITION;
	float4 col : COLOR0;
};

PS_IN main( VS_IN input )
{
	PS_IN output = (PS_IN)0;
	
	output.pos = input.pos;
	output.col = input.col;
	
	return output;
}
