
Texture2D input : register(t0);

RWTexture2D<float4> output : register(u0);


[numthreads(8, 8, 1)]
void main(uint2 id : SV_DispatchThreadID)
{
	output[id] = float4(0.5, 1, 1, 1);
}
