
/////////////////////////////////////////////
// Script 宣言

float Script : STANDARDSGLOBAL <
	string ScriptClass="scene";
	string Script="";
> = 0.8;


/////////////////////////////////////////////
// グローバル変数

float4x4 matWVP : WORLDVIEWPROJECTION < string Object="Camera"; >;
float4x4 matWV:WORLDVIEW < string Object = "Camera"; >;
float4x4 WorldMatrix : WORLD;
float4x4 ViewMatrix : VIEW;
float2   viewportSize : VIEWPORTPIXELSIZE;
float4   ViewPointPosition : POSITION < string object="camera"; >;	// カメラ位置（float4）
float4   LightPointPosition : POSITION < string object="light"; >;	// 光源位置
float3	 CameraPosition		: POSITION < string Object = "Camera"; > ;	// カメラ位置（float3）

Texture2D Texture : MATERIALTEXTURE;			// サンプリング用テクスチャ
Texture2D SphereTexture : MATERIALSPHEREMAP;	// サンプリング用スフィアマップテクスチャ
Texture2D Toon : MATERIALTOONTEXTURE;			// サンプリング用トゥーンテクスチャ

float4 EdgeColor : EDGECOLOR;	// エッジの色
float  EdgeWidth : EDGEWIDTH;	// エッジの幅


// グローバル変数；特殊パラメータ

bool use_spheremap;			// 描画中の材質がスフィアマップを使用するなら true
bool spadd;					// スフィアマップを使う場合のオプション。（true: 加算スフィア、false: 乗算スフィア）
bool use_texture;			// 描画中の材質がテクスチャを使用するなら true
bool use_toontexturemap;	// 描画中の材質がトゥーンテクスチャを使用するなら true
bool use_selfshadow;		// 描画中の材質がセルフ影を使用するなら true


// 定数バッファ；材質単位で変わらないもの

cbuffer BasicMaterialConstant
{
	float4 AmbientColor:packoffset(c0);
	float4 DiffuseColor:packoffset(c1);
	float4 SpecularColor:packoffset(c2);
	float SpecularPower:packoffset(c3);
}


// サンプラーステート; テクスチャ、スフィアマップ、トゥーンテクスチャで共通
SamplerState mySampler
{
   Filter = MIN_MAG_LINEAR_MIP_POINT;
   AddressU = WRAP;
   AddressV = WRAP;
};


/////////////////////////////////////////////
// 入出力定義


// 頂点シェーダ入力
struct SKINNING_OUTPUT
{
	float4 Position   : POSITION;      // スキニング後の座標（ローカル座標）
	float3 Normal     : NORMAL;        // 法線（ローカル座標）
	float2 Tex        : TEXCOORD0;     // UV
	float4 AddUV1     : TEXCOORD1;     // 追加UV1
	float4 AddUV2     : TEXCOORD2;     // 追加UV2
	float4 AddUV3     : TEXCOORD3;     // 追加UV3
	float4 AddUV4     : TEXCOORD4;     // 追加UV4
	float  EdgeWeight : EDGEWEIGHT;    // エッジウェイト
	float  Index      : PSIZE15;       // 頂点インデックス値
};

// 頂点シェーダ出力（＝ピクセルシェーダ入力）
struct VS_OUTPUT
{
	float4 Position   : SV_POSITION;	// 座標（射影座標）
	float3 Normal	  : NORMAL;			// 法線
	float2 Tex		  : TEXCOORD1;		// テクスチャ
	float3 Eye		  : TEXCOORD3;		// カメラとの相対位置
	float2 SpTex	  : TEXCOORD4;		// スフィアマップテクスチャ座標
	float4 Color	  : COLOR0;			// ディフューズ色
};


/////////////////////////////////////////////
// オブジェクト描画用シェーダ


// 頂点シェーダ

VS_OUTPUT VS_Main(SKINNING_OUTPUT input)
{
	VS_OUTPUT Out;

	// 位置（ワールドビュー射影変換）
	Out.Position = mul(input.Position, matWVP);

	// カメラとの相対位置
	Out.Eye = ViewPointPosition - mul(input.Position, WorldMatrix);

	// 頂点法線
	Out.Normal = normalize(mul(input.Normal, (float3x3)WorldMatrix));

	// ディフューズ色計算
	Out.Color.rgb = DiffuseColor.rgb;
	Out.Color.a = DiffuseColor.a;
	Out.Color = saturate(Out.Color);	// 0～1 に丸める

	Out.Tex = input.Tex;

	if (use_spheremap)
	{
		// スフィアマップテクスチャ座標
		float2 NormalWV = mul(Out.Normal, (float3x3)ViewMatrix);
		Out.SpTex.x = NormalWV.x * 0.5f + 0.5f;
		Out.SpTex.y = NormalWV.y * -0.5f + 0.5f;
	}

	return Out;
}


// ピクセルシェーダ

float4 PS_Main( VS_OUTPUT IN ) : SV_Target
{
    // 反射色計算
	
	float3 LightDirection = -normalize( mul( LightPointPosition, matWV ) );
	float3 HalfVector = normalize(normalize( IN.Eye ) + -mul( LightDirection, matWV ) );
    float3 Specular = pow( max( 0.00001, dot( HalfVector, normalize( IN.Normal ) ) ), SpecularPower ) * SpecularColor.rgb;
	float4 Color = IN.Color;


	// テクスチャサンプリング

	if( use_texture )
	{
		Color *= Texture.Sample( mySampler, IN.Tex );
	}


	// スフィアマップサンプリング

	if ( use_spheremap )
	{
		if( spadd )
		{
			Color.rgb += SphereTexture.Sample(mySampler, IN.SpTex).rgb;	// 加算
		}
		else
		{
			Color.rgb *= SphereTexture.Sample(mySampler, IN.SpTex).rgb;	// 乗算
		}
    }

	
	// シェーディング

	float LightNormal = dot( IN.Normal, -mul( LightDirection,matWV ) );
	float shading = saturate( LightNormal );	// 0～1 に丸める
    
	
	// トゥーンテクスチャサンプリング
	
	if ( use_toontexturemap )
	{
        float3 MaterialToon = Toon.Sample( mySampler, float2( 0, shading ) ).rgb;
        Color.rgb *= MaterialToon;
    }
	else
	{
        float3 MaterialToon = 1.0.xxx * shading;
        Color.rgb *= MaterialToon;
	}
    
    
    // 色に反射光を加算

    Color.rgb += Specular;

	
	// 色に環境校を加算

	//Color.rgb += AmbientColor.rgb * 0.2;	//TODO MMDのAmbientの係数がわからん・・・
	Color.rgb += AmbientColor.rgb * 0.05;

	return Color;
}


// テクニックとパス

technique11 DefaultTechnique < string MMDPass = "object"; >
{
	pass DefaultPass
	{
		SetVertexShader(CompileShader(vs_5_0, VS_Main()));
		SetPixelShader(CompileShader(ps_5_0, PS_Main()));
	}
}


/////////////////////////////////////////////
// エッジ描画用シェーダ


// 頂点シェーダ

VS_OUTPUT VS_Edge(SKINNING_OUTPUT IN)
{
	VS_OUTPUT Out;

	Out.Position = IN.Position;	// 位置（ローカル座標）
	Out.Eye = ViewPointPosition - mul(IN.Position, WorldMatrix);	// カメラとの相対位置
	Out.Normal = normalize(mul(IN.Normal, (float3x3)WorldMatrix));	// 頂点法線
	Out.Tex = IN.Tex;	// テクスチャ

	// 頂点を法線方向に膨らませる
	float4 position = Out.Position + float4(Out.Normal, 0) * EdgeWidth * IN.EdgeWeight * distance(Out.Position.xyz, CameraPosition) * 0.0005;

	// ワールドビュー射影変換
	Out.Position = mul(position, matWVP);

	return Out;
}


// ピクセルシェーダ

float4 PS_Edge( VS_OUTPUT IN ) : SV_Target
{
	return EdgeColor;
}


// テクニックとパス

BlendState NoBlend
{
	BlendEnable[0] = False;
};
technique11 DefaultEdge < string MMDPass = "edge"; >
{
	pass DefaultPass
	{
		SetBlendState( NoBlend, float4(0.0f,0.0f,0.0f,0.0f), 0xFFFFFFFF );	//AlphaBlendEnable = FALSE;
		//AlphaTestEnable = FALSE;	--> D3D10 以降は廃止

		SetVertexShader( CompileShader( vs_5_0, VS_Edge() ) );
		SetPixelShader( CompileShader( ps_5_0, PS_Edge() ) );
	}
}
