///////////////////////////////////////////////////////////////////////////////////////////////////
//
// VS_INPUT.hlsli
// 　これらは固定形式データとしてプログラム内にハードコーディングされているので、
// 　プログラムとの連携なしに改変してはならない。
//
////////////////////////////////////////////////////////////////////////////////////////////////////

// 頂点シェーダーの入力（＝スキニング用コンピュートシェーダーの出力）：頂点の構造
struct VS_INPUT
{
	float4 Position : POSITION; // スキニング後の座標（ローカル座標）
	float3 Normal : NORMAL; // 法線（ローカル座標）
	float2 Tex : TEXCOORD0; // UV
	float4 AddUV1 : TEXCOORD1; // 追加UV1
	float4 AddUV2 : TEXCOORD2; // 追加UV2
	float4 AddUV3 : TEXCOORD3; // 追加UV3
	float4 AddUV4 : TEXCOORD4; // 追加UV4
	float EdgeWeight : EDGEWEIGHT; // エッジウェイト
	uint Index : PSIZE15; // 頂点インデックス値
};

// スキニング用コンピュートシェーダーが出力する頂点のサイズ
#define VS_INPUT_SIZE  ((4+3+2+4+4+4+4+1+1)*4)
