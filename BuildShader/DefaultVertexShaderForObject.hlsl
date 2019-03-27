////////////////////////////////////////////////////////////////////////////////////////////////////
//
// オブジェクト用頂点シェーダー
//
////////////////////////////////////////////////////////////////////////////////////////////////////

#include "MikuMikuFlex.hlsli"
#include "DefaultVS_OUTPUT.hlsli"

VS_OUTPUT main(VS_INPUT input)
{
    VS_OUTPUT Out = (VS_OUTPUT) 0;

	// 頂点法線
    Out.Normal = normalize(mul(input.Normal, (float3x3) WorldMatrix));

	// 位置
    Out.Position = mul(input.Position, WorldMatrix); // ワールド変換

	// カメラとの相対位置
    Out.Eye = (CameraPosition - mul(input.Position, WorldMatrix)).xyz;

	// ディフューズ色計算
    Out.Color.rgb = DiffuseColor.rgb;
    Out.Color.a = DiffuseColor.a;
    Out.Color = saturate(Out.Color); // 0～1 に丸める

    Out.Tex = input.Tex;

    if (UseSphereMap)
    {
		// スフィアマップテクスチャ座標
        float2 NormalWV = mul(float4(Out.Normal, 0), ViewMatrix).xy;
        Out.SpTex.x = NormalWV.x * 0.5f + 0.5f;
        Out.SpTex.y = NormalWV.y * -0.5f + 0.5f;
    }
    else
    {
        Out.SpTex.x = 0.0f;
        Out.SpTex.y = 0.0f;
    }

    return Out;
}
