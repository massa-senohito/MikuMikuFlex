////////////////////////////////////////////////////////////////////////////////////////////////////
//
// ボーン変形用コンピュートシェーダー
//
// 　モデルに対するスキニング（ボーンの位置に追従して頂点位置を移動（ボーン変形）する作業）を、
// 　コンピュートシェーダーを使って行う。
//
////////////////////////////////////////////////////////////////////////////////////////////////////

#include "VS_INPUT.hlsli"
#include "Skinning.hlsli"
#include "Quaternion.hlsli"


void BDEF(CS_BDEF_INPUT input, out float4 position, out float3 normal)
{
	float4x4 bt =
		mul(g_BoneTrans[input.BoneIndex1], input.BoneWeight1) +
		mul(g_BoneTrans[input.BoneIndex2], input.BoneWeight2) +
		mul(g_BoneTrans[input.BoneIndex3], input.BoneWeight3) +
		mul(g_BoneTrans[input.BoneIndex4], input.BoneWeight4);

    position = mul(input.Position, bt);
    normal = normalize(mul(input.Normal, (float3x3) bt));
}

void SDEF(CS_BDEF_INPUT input, out float4 position, out float3 normal)
{
    // 参考: 
    // 自分用メモ「PMXのスフィリカルデフォームのコードっぽいもの」（sma42氏）
    // https://www.pixiv.net/member_illust.php?mode=medium&illust_id=60755964

    float w0 = 0.0f; // 固定値であるSDEFパラメータにのみ依存するので、これらの値も実は固定値。
    float w1 = 0.0f; //

    float L0 = length(input.Sdef_R0 - g_BoneLocalPosition[input.BoneIndex2].xyz); // 子ボーンからR0までの距離
    float L1 = length(input.Sdef_R1 - g_BoneLocalPosition[input.BoneIndex2].xyz); // 子ボーンからR1までの距離

    if (abs(L0 - L1) < 0.0001f)
    {
        w0 = 0.5f;
    }
    else
    {
        w0 = saturate(L0 / (L0 + L1));
    }
    w1 = 1.0f - w0;

	float4x4 modelPoseL = mul(g_BoneTrans[input.BoneIndex1], input.BoneWeight1);
	float4x4 modelPoseR = mul(g_BoneTrans[input.BoneIndex2], input.BoneWeight2);
    float4x4 modelPoseC = modelPoseL + modelPoseR;

    float4 Cpos = mul(input.Sdef_C, modelPoseC); // BDEF2で計算された点Cの位置
    float4 Ppos = mul(input.Position, modelPoseC); // BDEF2で計算された頂点の位置

    float4 qp = quaternion_slerp(
		mul(g_BoneQuaternion[input.BoneIndex1], input.BoneWeight1),
		mul(g_BoneQuaternion[input.BoneIndex2], input.BoneWeight2),
        input.BoneWeight1);
    float4x4 qpm = quaternion_to_matrix(qp);

    float4 R0pos = mul(float4(input.Sdef_R0, 1.0f), (modelPoseL + mul(modelPoseC, -input.BoneWeight1)));
    float4 R1pos = mul(float4(input.Sdef_R1, 1.0f), (modelPoseR + mul(modelPoseC, -input.BoneWeight2)));
    Cpos += mul(R0pos, w0) + mul(R1pos, w1); // 膨らみすぎ防止？

    Ppos -= Cpos; // 頂点を点Cが中心になるよう移動して
    Ppos = mul(Ppos, qpm); // 回転して
    Ppos += Cpos; // 元の位置へ

    position = Ppos;
    normal = normalize(mul(float4(input.Normal, 0), qpm)).xyz;
}

void QDEF(CS_BDEF_INPUT input, out float4 position, out float3 normal)
{
    // TODO: QDEF の実装に変更する。（いまはBDEF4と同じ）

    float4x4 bt =
		mul(g_BoneTrans[input.BoneIndex1], input.BoneWeight1) +
		mul(g_BoneTrans[input.BoneIndex2], input.BoneWeight2) +
		mul(g_BoneTrans[input.BoneIndex3], input.BoneWeight3) +
		mul(g_BoneTrans[input.BoneIndex4], input.BoneWeight4);

    position = mul(input.Position, bt);
    normal = normalize(mul(float4(input.Normal, 0), bt)).xyz;
}


////////////////////
//
// コンピュートシェーダー
//
// 　Xしか扱わないので、Dispach は (頂点数/64+1, 1, 1) とすること。
// 　例: 頂点数が 130 なら Dispach( 3, 1, 1 )
//

[numthreads(64, 1, 1)]
void main(uint3 id : SV_DispatchThreadID)
{
    uint csIndex = id.x; // 頂点番号（0～頂点数-1）
    uint vsIndex = csIndex * VS_INPUT_SIZE; // 出力位置[byte単位（必ず4の倍数であること）]

    CS_BDEF_INPUT input = g_CSBDEFBuffer[csIndex];

    // ボーンウェイト変形を適用して、新しい位置と法線を求める。

    float4 position = input.Position;
    float3 normal = input.Normal;

    switch (input.Deform)
    {
        case DEFORM_BDEF1:
        case DEFORM_BDEF2:
        case DEFORM_BDEF4:
            BDEF(input, position, normal);
            break;

        case DEFORM_SDEF:
            SDEF(input, position, normal);
            break;

        case DEFORM_QDEF:
            QDEF(input, position, normal);
            break;
    }

    
    // 頂点バッファへ出力する。

	g_VSBuffer.Store4(vsIndex + 0, asuint(position));
	g_VSBuffer.Store3(vsIndex + 16, asuint(normal));
	g_VSBuffer.Store2(vsIndex + 28, asuint(input.Tex));
	g_VSBuffer.Store4(vsIndex + 36, asuint(input.AddUV1));
	g_VSBuffer.Store4(vsIndex + 52, asuint(input.AddUV2));
	g_VSBuffer.Store4(vsIndex + 68, asuint(input.AddUV3));
	g_VSBuffer.Store4(vsIndex + 84, asuint(input.AddUV4));
	g_VSBuffer.Store(vsIndex + 100, asuint(input.EdgeWeight));
	g_VSBuffer.Store(vsIndex + 104, asuint(input.Index));
}
