////////////////////////////////////////////////////////////////////////////////////////////////////
//
// オブジェクト用ピクセルシェーダー
//
////////////////////////////////////////////////////////////////////////////////////////////////////

#include "MikuMikuFlex.hlsli"
#include "DefaultVS_OUTPUT.hlsli"


SamplerState mySampler
{
    Filter = MIN_MAG_LINEAR_MIP_POINT;
    AddressU = WRAP;
    AddressV = WRAP;
};


float4 main(VS_OUTPUT IN) : SV_TARGET
{
    // 反射色計算
	
    float3 LightDirection = normalize(mul(Light1Direction, WorldMatrix * ViewMatrix)).xyz;
    float3 HalfVector = normalize(normalize(IN.Eye) - mul(float4(LightDirection, 0), WorldMatrix * ViewMatrix).xyz);
    float3 Specular = pow(max(0.00001, dot(HalfVector, normalize(IN.Normal))), SpecularPower) * SpecularColor.rgb;
    float4 Color = IN.Color;


	// テクスチャサンプリング

    if (UseTexture)
    {
        Color *= Texture.Sample(mySampler, IN.Tex);
    }

	// スフィアマップサンプリング

    if (UseSphereMap)
    {
        if (IsAddSphere)
        {
            Color.rgb += SphereTexture.Sample(mySampler, IN.SpTex).rgb; // 加算
        }
        else
        {
            Color.rgb *= SphereTexture.Sample(mySampler, IN.SpTex).rgb; // 乗算
        }
    }

	
	// シェーディング

    //float LightNormal = dot(IN.Normal, -mul(float4(LightDirection, 0), matWV).xyz);
    float LightNormal = dot(IN.Normal, -LightDirection.xyz);
    float shading = saturate(LightNormal); // 0～1 に丸める

	
	// トゥーンテクスチャサンプリング
	
    if (UseToonTextureMap)
    {
        float3 MaterialToon = ToonTexture.Sample(mySampler, float2(0, shading)).rgb;
        Color.rgb *= MaterialToon;
    }
    else
    {
        //float3 MaterialToon = 1.0f.xxx * shading;
        float3 MaterialToon = 1.0f.xxx * (0.85f + shading * 0.15f); // shading:0→1 のとき、MaerialToon: 0.85→1.0
        Color.rgb *= MaterialToon;
    }
    
    
    // 色に反射光を加算

    Color.rgb += Specular;

	
	// 色に環境光を加算

	//Color.rgb += AmbientColor.rgb * 0.2;	//TODO MMDのAmbientの係数がわからん・・・
    Color.rgb += AmbientColor.rgb * 0.005;

    return Color;
}
