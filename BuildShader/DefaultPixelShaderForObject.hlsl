////////////////////////////////////////////////////////////////////////////////////////////////////
//
// オブジェクト用ピクセルシェーダー
//
////////////////////////////////////////////////////////////////////////////////////////////////////

#include "MaterialTexture.hlsli"
#include "DefaultVS_OUTPUT.hlsli"
#include "GlobalParameters.hlsli"

SamplerState mySampler : register(s0);

float4 main(VS_OUTPUT IN) : SV_TARGET
{
    // 反射色計算
	
    float3 LightDirection = normalize(mul(g_Light1Direction, g_WorldMatrix * g_ViewMatrix)).xyz;
    float3 HalfVector = normalize(normalize(IN.Eye) - mul(float4(LightDirection, 0), g_WorldMatrix * g_ViewMatrix).xyz);
    float3 Specular = pow(max(0.00001, dot(HalfVector, normalize(IN.Normal))), g_SpecularPower) * g_SpecularColor.rgb;
    float4 Color = IN.Color;


	// テクスチャサンプリング

    if (g_UseTexture)
    {
        Color *= g_Texture.Sample(mySampler, IN.Tex);
    }

	// スフィアマップサンプリング

    if (g_UseSphereMap)
    {
        if (g_IsAddSphere)
        {
            Color.rgb += g_SphereTexture.Sample(mySampler, IN.SpTex).rgb; // 加算
        }
        else
        {
            Color.rgb *= g_SphereTexture.Sample(mySampler, IN.SpTex).rgb; // 乗算
        }
    }

	
	// シェーディング

    //float LightNormal = dot(IN.Normal, -mul(float4(LightDirection, 0), matWV).xyz);
    float LightNormal = dot(IN.Normal, -LightDirection.xyz);
    float shading = saturate(LightNormal); // 0～1 に丸める

	shading = 0.85f + shading * 0.15f;	// そのままだと濃ゆいので薄くする(0～1 → 0.85～1)


	// トゥーンテクスチャサンプリング
	
    if (g_UseToonTextureMap)
    {
        float3 MaterialToon = g_ToonTexture.Sample(mySampler, float2(0, shading)).rgb;
        Color.rgb *= MaterialToon;
    }
    else
    {
        float3 MaterialToon = 1.0f.xxx * shading;
        Color.rgb *= MaterialToon;
    }
    
    
    // 色に反射光を加算

    Color.rgb += Specular;

	
	// 色に環境光を加算

	//Color.rgb += AmbientColor.rgb * 0.2;	//TODO MMDのAmbientの係数がわからん・・・
    Color.rgb += g_AmbientColor.rgb * 0.005;

    return Color;
}
