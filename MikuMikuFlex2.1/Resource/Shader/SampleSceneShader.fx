//
// サンプルシーンエフェクト
//


// Script 宣言 ///////////////////////////////////////////


float Script : STANDARDSGLOBAL <
	string ScriptClass="sceneorobject"; // 行列関連のセマンティクスは Object、CURRENTSCENE*セマンティクスは Scene
	string Script="";
> = 0.8;


// グローバル変数 ///////////////////////////////////////////

float2   viewportSize  : VIEWPORTPIXELSIZE;
Texture2D SceneTexture : CURRENTSCENECOLOR;          // シーン用テクスチャ

SamplerState SceneSampler
{
    Filter = MIN_MAG_MIP_LINEAR;
    AddressU = WRAP;
    AddressV = WRAP;
};


// シーン描画 ////////////////////////////////////////////////


// 頂点シェーダ出力

struct SCENE_VS_OUTPUT
{
    float4 Position : SV_POSITION; // 頂点座標（射影座標系）
    float2 Tex      : TEXCOORD0;   // テクスチャ座標
};

// 頂点シェーダー

SCENE_VS_OUTPUT VS_Scene( uint vID : SV_VertexID )
{
    SCENE_VS_OUTPUT vt;
    
    // 頂点座標（射影座標系）の自動生成
    // プリミティブ型として TriangleStrip が設定されている前提なので注意。
    float z = 0;
    switch (vID)
    {
        case 0:
            vt.Position = float4(-1, 1, z, 1.0); // 左上
            vt.Tex = float2(0, 0);
            break;
        case 1:
            vt.Position = float4(1, 1, z, 1.0); // 右上
            vt.Tex = float2(1, 0);
            break;
        case 2:
            vt.Position = float4(-1, -1, z, 1.0); // 左下
            vt.Tex = float2(0, 1);
            break;
        case 3:
            vt.Position = float4(1, -1, z, 1.0); // 右下
            vt.Tex = float2(1, 1);
            break;
    }

    return vt;
}


// ピクセルシェーダー

float4 PS_Scene( SCENE_VS_OUTPUT input ) : SV_Target
{
    // CURRENTSCENECOLOR セマンティクスが付与された Texture2D 変数には、
    // 現在までの描画内容（バックバッファのコピー）が格納されている。

    float4 texCol = SceneTexture.Sample(SceneSampler, input.Tex);
    texCol.a = 1;

    //return texCol;    // 何も加工しない例
    return saturate(texCol * float4(input.Tex.x, input.Tex.y, 0.5, 1)); // グラデを乗じる例
}


// テクニックとパス

technique11 DefaultScene < string MMDPass = "scene"; >
{
    pass DefaultPass
    {
        SetVertexShader(CompileShader(vs_5_0, VS_Scene()));
        SetPixelShader(CompileShader(ps_5_0, PS_Scene()));
    }
}
