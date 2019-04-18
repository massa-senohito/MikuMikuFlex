///////////////////////////////////////////////////////////////////////////////////////////////////
//
// Skinning.hlsli
// 　スキニングで使用されるデータの定義。
// 　これらは固定形式データとしてプログラム内にハードコーディングされているので、
// 　プログラムとの連携なしに改変してはならない。
//
////////////////////////////////////////////////////////////////////////////////////////////////////

// スキニングで使用できるボーン変形の種別
#define DEFORM_BDEF1    0
#define DEFORM_BDEF2    1
#define DEFORM_BDEF4    2
#define DEFORM_SDEF     3
#define DEFORM_QDEF     4

// スキニング用コンピュートシェーダーが扱えるボーン数の最大数
#define MAX_BONE    768


// スキニング用コンピュートシェーダーの入力：定数バッファ(1) ボーンのモデルポーズ行列の配列
cbuffer BoneTransBuffer : register(b1)
{
	float4x4 g_BoneTrans[MAX_BONE];
}

// スキニング用コンピュートシェーダーの入力：定数バッファ(2) ボーンのローカル位置の配列（SDEFで使用）
cbuffer BoneLocalPositionBuffer : register(b2)
{
	float4 g_BoneLocalPosition[MAX_BONE];
}

// スキニング用コンピュートシェーダーの入力：定数バッファ(3) ボーンの回転（クォータニオン）の配列（SDEFでのみ使用する）
cbuffer BoneQuaternionBuffer : register(b3)
{
	float4 g_BoneQuaternion[MAX_BONE];
}


// スキニング用コンピュートシェーダーの入力：頂点の構造
struct CS_BDEF_INPUT
{
	float4 Position;
	float BoneWeight1;
	float BoneWeight2;
	float BoneWeight3;
	float BoneWeight4;
	uint BoneIndex1;
	uint BoneIndex2;
	uint BoneIndex3;
	uint BoneIndex4;
	float3 Normal;
	float2 Tex;
	float4 AddUV1;
	float4 AddUV2;
	float4 AddUV3;
	float4 AddUV4;
	float4 Sdef_C;
	float3 Sdef_R0;
	float3 Sdef_R1;
	float EdgeWeight;
	uint Index;
	uint Deform;
};

// スキニング用コンピュートシェーダーの入力：頂点の構造化バッファ
StructuredBuffer<CS_BDEF_INPUT> g_CSBDEFBuffer : register(t0);


// スキニング用コンピュートシェーダーの出力：頂点バッファ（RWバイトアドレスバッファ）
// → このバッファは、そのまま頂点シェーダの入力（頂点バッファ）として使用される。
RWByteAddressBuffer g_VSBuffer : register(u0);

