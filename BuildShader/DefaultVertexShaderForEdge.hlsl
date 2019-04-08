////////////////////////////////////////////////////////////////////////////////////////////////////
//
// エッジ用頂点シェーダー
//
////////////////////////////////////////////////////////////////////////////////////////////////////

#include "VS_INPUT.hlsli"
#include "DefaultVS_OUTPUT.hlsli"
#include "GlobalParameters.hlsli"

VS_OUTPUT main(VS_INPUT input)
{
    VS_OUTPUT Out = (VS_OUTPUT) 0;

	// 頂点法線
    Out.Normal = normalize(mul(input.Normal, (float3x3) WorldMatrix));

	// 位置
    float4 position = input.Position;
    // 法線方向に膨らませる。
    position = input.Position + float4(Out.Normal, 0) * EdgeWidth * input.EdgeWeight * distance(input.Position.xyz, CameraPosition.xyz) * 0.0005;
    Out.Position = mul(position, WorldMatrix); // ワールド変換

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
