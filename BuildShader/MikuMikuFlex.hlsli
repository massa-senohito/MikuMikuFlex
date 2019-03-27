////////////////////////////////////////////////////////////////////////////////////////////////////
//
// MikuMikuFlex.hlsli
// 固定されるデータの定義。
// 　これらは固定形式データとしてプログラム内にハードコーディングされているので、
// 　プログラムとの連携なしに改変してはならない。
//
////////////////////////////////////////////////////////////////////////////////////////////////////



////////////////////
//
// 固定データ(1) スキニング用コンピュートシェーダーの入力
//


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
    float4x4 BoneTrans[MAX_BONE];
}

// スキニング用コンピュートシェーダーの入力：定数バッファ(2) ボーンのローカル位置の配列（SDEFで使用）
cbuffer BoneLocalPositionBuffer : register(b2)
{
    float3 BoneLocalPosition[MAX_BONE];
}

// スキニング用コンピュートシェーダーの入力：定数バッファ(3) ボーンの回転（クォータニオン）の配列（SDEFでのみ使用する）
cbuffer BoneQuaternionBuffer : register(b3)
{
    float4 BoneQuaternion[MAX_BONE];
}

// スキニング用コンピュートシェーダーの入力：頂点の構造
struct CS_BDEF_INPUT
{
    float4 Position;
    float4 BoneWeight;
    uint4 BoneIndex;
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
StructuredBuffer<CS_BDEF_INPUT> CSBDEFBuffer : register(t0);



////////////////////
//
// 固定データ(2) スキニング用コンピュートシェーダーの出力（兼 頂点シェーダーの入力）
//


// スキニング用コンピュートシェーダーの出力：頂点バッファ（RWバイトアドレスバッファ）
// → このバッファは、そのまま頂点シェーダの入力（頂点バッファ）として使用される。
RWByteAddressBuffer VSBuffer : register(u0);

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



////////////////////
//
// 固定データ(3) テクスチャ
//


// ピクセルシェーダーの入力：通常テクスチャ
Texture2D Texture : register(t1);

// ピクセルシェーダーの入力：スフィアマップテクスチャ
Texture2D SphereTexture : register(t2);

// ピクセルシェーダーの入力：トゥーンテクスチャ
Texture2D ToonTexture : register(t3);




////////////////////
//
// 固定データ(4) シェーダー用グローバルパラメーター定数バッファ
// 　プログラムからシェーダーに渡される一般的なパラメータ。
//
// 備考：
// 　各パラメータは、設定・更新されるタイミングによって、以下のように分けられる。
//
// 　ステージ単位 …… ステージ内のすべてのモデルについて同一であるパラメータ。
// 　モデル単位 ……… モデル内のすべての材質について同一であるパラメータ。
// 　モデル単位 ……… モデル内の材質ごとに異なるパラメータ。
//

cbuffer GlobalParameters : register(b0)
{
    
    // コントロールフラグ


    // 描画中の材質がスフィアマップを使用するなら true。材質単位。
    // 　true の場合、SphereTexture オブジェクトが有効であること。
    bool UseSphereMap; // HLSLのboolは4byte

    // スフィアマップの種類。true なら加算スフィア、false なら乗算スフィア。材質単位。
    bool IsAddSphere;

    // 描画中の材質がテクスチャを使用するなら true。材質単位。
    // 　true の場合、Texture オブジェクトが有効であること。
    bool UseTexture;

    // 描画中の材質がトゥーンテクスチャを使用するなら true。材質単位。
    // 　true の場合、ToonTexture オブジェクトが有効であること。
    bool UseToonTextureMap;

    // 描画中の材質がセルフ影を使用するなら true。材質単位。
    bool UseSelfShadow;



    // ワールドビュー射影変換


    // ワールド変換行列。モデル単位。
    float4x4 WorldMatrix;

    // ビュー変換行列。ステージ単位。
    float4x4 ViewMatrix;

    // 射影変換行列。ステージ単位。
    float4x4 ProjectionMatrix;



    // カメラ


    // カメラの位置。ステージ単位。
    float4 CameraPosition;

    // カメラの注視点。ステージ単位。
    float4 CameraTargetPosition;

    // カメラの上方向を示すベクトル。ステージ単位。
    float4 CameraUp;



    // 照明
    // 　※MMMでは照明１～３を同時に使用可能。（MMDでは照明１のみ）

    // 照明１の色。ステージ単位。
    float4 Light1Color;

    // 照明１の方向。ステージ単位。
    float4 Light1Direction;

    // 照明２の色。ステージ単位。
    float4 Light2Color;

    // 照明２の方向。ステージ単位。
    float4 Light2Direction;

    // 照明３の色。ステージ単位。
    float4 Light3Color;

    // 照明３の方向。ステージ単位。
    float4 Light3Direction;



    // 材質


    // 環境光。材質単位。
    float4 AmbientColor;

    // 拡散色。材質単位。
    float4 DiffuseColor;

    // 反射色。材質単位。
    float4 SpecularColor;

    // 反射係数。材質単位。
    float SpecularPower;

    // エッジの色。材質単位。
    float4 EdgeColor;

    // エッジの幅。材質単位。
    float EdgeWidth;

    // テッセレーション係数。モデル単位。
    float TessellationFactor;
}
