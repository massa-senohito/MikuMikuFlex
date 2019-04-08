////////////////////////////////////////////////////////////////////////////////////////////////////
//
// GlobalParameters.hlsli
// 　シェーダー用グローバルパラメーター定数バッファ。
// 　プログラムからシェーダーに渡される一般的なパラメータ。
//
// 　これらは固定形式データとしてプログラム内にハードコーディングされているので、
// 　プログラムとの連携なしに改変してはならない。
//
// 備考：
// 　各パラメータは、設定・更新されるタイミングによって、以下のように分けられる。
//
// 　ステージ単位 …… ステージ内のすべてのモデルについて同一であるパラメータ。
// 　モデル単位 ……… モデル内のすべての材質について同一であるパラメータ。
// 　モデル単位 ……… モデル内の材質ごとに異なるパラメータ。
//
////////////////////////////////////////////////////////////////////////////////////////////////////


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

	// エッジの色。材質単位。
	float4 EdgeColor;
	
	// 反射係数。材質単位。
    float SpecularPower;

    // エッジの幅。材質単位。
    float EdgeWidth;

    // テッセレーション係数。モデル単位。
    float TessellationFactor;
}
