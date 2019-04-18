////////////////////////////////////////////////////////////////////////////////////////////////////
//
// エッジ用ピクセルシェーダー
//
////////////////////////////////////////////////////////////////////////////////////////////////////

#include "MaterialTexture.hlsli"
#include "DefaultVS_OUTPUT.hlsli"
#include "GlobalParameters.hlsli"


SamplerState mySampler : register(s0);


float4 main(VS_OUTPUT IN) : SV_TARGET
{
	float4 Color = g_EdgeColor;

	if (g_UseTexture)
	{
		// 透明度はテクスチャに合わせる
		Color.a = g_Texture.Sample(mySampler, IN.Tex).a;
	}

	return Color;
}
