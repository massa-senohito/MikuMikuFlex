
Texture2D<float4> input : register(t0);

RWTexture2D<float4> output : register(u0);


[numthreads(8, 8, 1)]
void main(uint2 id : SV_DispatchThreadID)
{
	float4 col = input[id];
	output[id] = float4(col.b, col.g, col.r, col.a);	// �ԂƗ΂𔽑΂�
}
