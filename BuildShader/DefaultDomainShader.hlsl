////////////////////////////////////////////////////////////////////////////////////////////////////
//
// ドメインシェーダー
//
////////////////////////////////////////////////////////////////////////////////////////////////////

#include "MikuMikuFlex.hlsli"
#include "DefaultVS_OUTPUT.hlsli"
#include "DefaultCONSTANT_HS_OUT.hlsli"


[domain("tri")]
VS_OUTPUT main(CONSTANT_HS_OUT In, float3 uvw : SV_DomainLocation, const OutputPatch<VS_OUTPUT, 3> patch)
{
    VS_OUTPUT Out = (VS_OUTPUT) 0;

    float U = uvw.x;
    float V = uvw.y;
    float W = uvw.z;
    float UU = U * U;
    float VV = V * V;
    float WW = W * W;
    float UUU = UU * U;
    float VVV = VV * V;
    float WWW = WW * W;

    // 頂点座標
    float3 Position = patch[2].Position.xyz * WWW +
                      patch[1].Position.xyz * UUU +
                      patch[0].Position.xyz * VVV +
                      In.B210 * WW * 3 * U +
                      In.B120 * W * UU * 3 +
                      In.B201 * WW * 3 * V +
                      In.B021 * UU * 3 * V +
                      In.B102 * W * VV * 3 +
                      In.B012 * U * VV * 3 +
                      In.B111 * 6 * W * U * V;
    Out.Position = mul(float4(Position, 1), mul(ViewMatrix, ProjectionMatrix));

    // カメラとの相対位置
    Out.Eye = CameraPosition.xyz - Position;

    // 法線ベクトル
    float3 Normal = patch[2].Normal * WW +
                    patch[1].Normal * UU +
                    patch[0].Normal * VV +
                    In.N110 * W * U +
                    In.N011 * U * V +
                    In.N101 * W * V;
    Out.Normal = normalize(Normal);

    // テクスチャ座標
    Out.Tex = patch[2].Tex * W + patch[1].Tex * U + patch[0].Tex * V;
  
    // その他
    Out.SpTex = patch[2].SpTex * W + patch[1].SpTex * U + patch[0].SpTex * V;
    Out.Color = patch[2].Color * W + patch[1].Color * U + patch[0].Color * V;

    return Out;
}
