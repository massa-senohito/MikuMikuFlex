
float Script : STANDARDSGLOBAL <
	string ScriptClass="scene";
	string Script="";
> = 0.8;


// 変換行列系

float4x4 matWVP : WORLDVIEWPROJECTION < string Object="Camera"; >;
float4x4 matWV:WORLDVIEW < string Object = "Camera"; >;
float4x4 WorldMatrix : WORLD;
float4x4 ViewMatrix : VIEW;

float2 viewportSize : VIEWPORTPIXELSIZE;

float4 ViewPointPosition : POSITION < string object="camera"; >;
float4 LightPointPosition : POSITION < string object="light"; >;


// スキニング

float4x4 BoneTrans[768] : BONETRANS;


// 材質


// サンプリング用テクスチャ
Texture2D Texture : MATERIALTEXTURE;

// サンプリング用スフィアマップテクスチャ
Texture2D SphereTexture : MATERIALSPHEREMAP;

// サンプリング用トゥーンテクスチャ
Texture2D Toon : MATERIALTOONTEXTURE;

bool spadd;


// 材質単位で変わらないもの

cbuffer BasicMaterialConstant
{
	float4 AmbientColor:packoffset(c0);
	float4 DiffuseColor:packoffset(c1);
	float4 SpecularColor:packoffset(c2);
	float SpecularPower:packoffset(c3);
}


/////////////////////////////////////////////
// サンプラーステート

// テクスチャ、スフィアマップ、トゥーンテクスチャで共通
SamplerState mySampler
{
   Filter = MIN_MAG_LINEAR_MIP_POINT;
   AddressU = WRAP;
   AddressV = WRAP;
};

/////////////////////////////////////////////
// シェーダー入出力


// 頂点シェーダ入力（MMM準拠）
struct MMM_SKINNING_INPUT
{
	float4 Pos : POSITION;//頂点位置
	float4 BoneWeight : BLENDWEIGHT;
	uint4 BlendIndices : BLENDINDICES;
	float3 Normal : NORMAL;
	float2 Tex : TEXCOORD0;
	float4 AddUV1 : TEXCOORD1;
	float4 AddUV2 : TEXCOORD2;
	float4 AddUV3 : TEXCOORD3;
	float4 AddUV4 : TEXCOORD4;
	float4 SdefC : TEXCOORD5;
	float3 SdefR0 : TEXCOORD6;
	float3 SdefR1 : TEXCOORD7;
	float EdgeWeight : TEXCOORD8;
	uint Index : PSIZE15;
};

// 頂点シェーダ出力（＝ピクセルシェーダ入力）
struct VS_OUTPUT
{
	float4 Pos		: SV_Position;
    float2 Tex		: TEXCOORD1;   // テクスチャ
    float3 Normal	: TEXCOORD2;   // 法線
    float3 Eye		: TEXCOORD3;   // カメラとの相対位置
    float2 SpTex	: TEXCOORD4;   // スフィアマップテクスチャ座標
    float4 Color	: COLOR0;      // ディフューズ色
};


/////////////////////////////////////////////
// 頂点シェーダ実装

VS_OUTPUT VS_Main( MMM_SKINNING_INPUT input, uint vid:SV_VertexID, uniform bool useTexture, uniform bool useSphereMap, uniform bool useToon )
{    
	VS_OUTPUT Out;
	
	// スキンメッシュアニメーション
	float4x4 bt =
		BoneTrans[ input.BlendIndices[0] ] * input.BoneWeight[0] + 
		BoneTrans[ input.BlendIndices[1] ] * input.BoneWeight[1] + 
		BoneTrans[ input.BlendIndices[2] ] * input.BoneWeight[2] + 
		BoneTrans[ input.BlendIndices[3] ] * input.BoneWeight[3];

	// 位置（ワールドビュー射影変換）
	Out.Pos = mul( input.Pos, mul( bt, matWVP ) );
	
	// カメラとの相対位置
    Out.Eye = ViewPointPosition - mul( input.Pos, WorldMatrix );
	
	// 頂点法線
    Out.Normal = normalize( mul( input.Normal, (float3x3)WorldMatrix ) );
	
	// ディフューズ色＋アンビエント色 計算
    Out.Color.rgb = DiffuseColor.rgb;
	Out.Color.a = DiffuseColor.a;
	Out.Color = saturate( Out.Color );	// 0～1 に丸める
	
	Out.Tex = input.Tex;
	
    if ( useSphereMap )
	{
        // スフィアマップテクスチャ座標
        float2 NormalWV = mul( Out.Normal, (float3x3)ViewMatrix );
        Out.SpTex.x = NormalWV.x * 0.5f + 0.5f;
        Out.SpTex.y = NormalWV.y * -0.5f + 0.5f;
    }
    
	return Out;
}


/////////////////////////////////////////////
// ピクセルシェーダ実装

float4 PS_Main( VS_OUTPUT IN, uniform bool useTexture, uniform bool useSphereMap, uniform bool mulSphere, uniform bool useToon ) : SV_Target
{
    // 反射色計算
	
	float3 LightDirection = -normalize( mul( LightPointPosition, matWV ) );
	float3 HalfVector = normalize(normalize( IN.Eye ) + -mul( LightDirection, matWV ) );
    float3 Specular = pow( max( 0.00001, dot( HalfVector, normalize( IN.Normal ) ) ), SpecularPower ) * SpecularColor.rgb;
	float4 Color = IN.Color;


	// テクスチャサンプリング

	if( useTexture )
	{
		Color *= Texture.Sample( mySampler, IN.Tex );
	}


	// スフィアマップサンプリング

	if ( useSphereMap )
	{
		if( mulSphere )
		{
			Color.rgb *= SphereTexture.Sample( mySampler, IN.SpTex ).rgb;	// 乗算
		}
		else
		{
			Color.rgb += SphereTexture.Sample( mySampler, IN.SpTex ).rgb;	// 加算
		}
    }

	
	// シェーディング

	float LightNormal = dot( IN.Normal, -mul( LightDirection,matWV ) );
	float shading = saturate( LightNormal );	// 0～1 に丸める
    
	
	// トゥーンテクスチャサンプリング
	
	if ( useToon )
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


/////////////////////////////////////////////
//テクニック

technique10 TexturedObjectTechnique < 
	string MMDPass = "object"; 
	bool UseTexture = true;		// ON:  テクスチャ
	bool UseSphereMap = false;	// OFF: スフィアマップ
								// 無視: 乗算スフィア
	bool UseToon = true; >		// ON:  トゥーンテクスチャ使用
{
	pass Textured
	{		
		SetVertexShader( CompileShader( vs_4_0, VS_Main( true, false, true ) ) );
		SetPixelShader( CompileShader( ps_4_0, PS_Main( true, false, false, true ) ) );
	}
}

technique10 UnTexturedObjectTechnique <
	string MMDPass = "object";
	bool UseTexture = false;	// OFF: テクスチャ
	bool UseSphereMap = false;	// OFF: スフィアマップ
								// 無視: 乗算スフィア
	bool UseToon = true; >		// OFF: トゥーンテクスチャ
{
	pass UnTextured
	{
		SetVertexShader( CompileShader( vs_4_0, VS_Main( false, false, true ) ) );
		SetPixelShader( CompileShader( ps_4_0, PS_Main( false, false, false, true ) ) );
	}
}

technique10 SphereObjectTechnique < 
	string MMDPass = "object";
	bool UseTexture = true;		// ON:  テクスチャ
	bool UseSphereMap = true;	// ON:  スフィアマップ
	bool MulSphere = false;		// OFF:	乗算スフィア
	bool UseToon = true; >		// ON:  トゥーンテクスチャ
{
	pass TexturedAddSphere
	{
		SetVertexShader( CompileShader( vs_4_0, VS_Main( true, true, true ) ) );
		SetPixelShader( CompileShader( ps_4_0, PS_Main( true, true, false, true ) ) );
	}
}

technique10 SpheredUnTextureTechnique < 
	string MMDPass = "object";
	bool UseTexture = false;	// OFF: テクスチャ
	bool UseSphereMap = true;	// ON:  スフィアマップ
	bool MulSphere = false;		// OFF: 乗算スフィア
	bool UseToon = true; >		// ON:  トゥーンテクスチャ
{
	pass UnTexturedAddSphere
	{
		SetVertexShader( CompileShader( vs_4_0, VS_Main( false, true, true ) ) );
		SetPixelShader( CompileShader( ps_4_0, PS_Main( false, true, false, true ) ) );
	}
}

technique10 MulSphereObjectTechnique <
	string MMDPass = "object";
	bool UseTexture = true;		// ON:  テクスチャ
	bool UseSphereMap = true;	// ON:  スフィアマップ
	bool MulSphere = true;		// ON:  乗算スフィア
	bool UseToon = true; >		// ON:  トゥーンテクスチャ
{
    pass TexturedMulSphere
    {
		SetVertexShader( CompileShader( vs_4_0, VS_Main( true, true, true ) ) );
		SetPixelShader( CompileShader( ps_4_0, PS_Main( true, true, true, true ) ) );
    }
}

technique10 MulSpheredUnTextureTechnique <
	string MMDPass = "object";
	bool UseTexture = false;	// OFF: テクスチャ
	bool UseSphereMap = true;	// ON:	スフィアマップ
	bool MulSphere = true;		// ON:	乗算スフィア
	bool UseToon=true; >		// ON:	トゥーンテクスチャ
{
    pass UnTexturedMulSphere
    {
        SetVertexShader(CompileShader(vs_4_0, VS_Main(false, true, true)));
        SetPixelShader(CompileShader(ps_4_0, PS_Main(false, true, true, true)));
    }
}

technique10 TexturedObjectTechniqueUnToon <
	string MMDPass = "object";
	bool UseTexture = true;		// ON:	テクスチャ
	bool UseSphereMap = false;	// OFF:	スフィアマップ
								// 無視: 乗算スフィア
	bool UseToon = false; >		// OFF: トゥーンテクスチャ
{
	pass Textured
	{		
		SetVertexShader(CompileShader(vs_4_0, VS_Main(true,false,false)));
		SetPixelShader(CompileShader(ps_4_0, PS_Main(true,false,false,false)));
	}
}

technique10 UnTexturedObjectTechniqueUnToon <
	string MMDPass = "object";
	bool UseTexture = false;    // OFF: テクスチャ
	bool UseSphereMap = false;  // OFF: スフィアマップ
								// 無視: 乗算スフィア
	bool UseToon = false; >		// OFF: トゥーンテクスチャ
{
	pass UnTextured
	{		
		SetVertexShader(CompileShader(vs_4_0, VS_Main(false,false,false)));
		SetPixelShader(CompileShader(ps_4_0, PS_Main(false,false,false,false)));
	}
}

technique10 SphereObjectTechniqueUnToon <
	string MMDPass = "object";
	bool UseTexture = true;		// ON:  テクスチャ
	bool UseSphereMap = true;	// ON:	スフィアマップ
	bool MulSphere = false;		// OFF:	乗算スフィア
	bool UseToon = false; >		// OFF: トゥーンテクスチャ
{
	pass TexturedAddSphere
	{
		SetVertexShader(CompileShader(vs_4_0,VS_Main(true,true,false)));
		SetPixelShader(CompileShader(ps_4_0, PS_Main(true,true,false,false)));
	}
}

technique10 SpheredUnTextureTechniqueUnToon <
	string MMDPass = "object";
	bool UseTexture = false;	// OFF: テクスチャ
	bool UseSphereMap = true;	// ON:  スフィアマップ
	bool MulSphere = false;		// OFF: 乗算スフィア
	bool UseToon = false; >		// OFF: トゥーンテクスチャ
{
		pass UnTexturedAddSphere
	{
		SetVertexShader(CompileShader(vs_4_0,VS_Main(false,true,false)));
		SetPixelShader(CompileShader(ps_4_0, PS_Main(false,true,false,false)));
	}
}

technique10 MulSphereObjectTechniqueUnToon <
	string MMDPass = "object";
	bool UseTexture = true;		// ON:  テクスチャ
	bool UseSphereMap = true;	// ON:  スフィアマップ
	bool MulSphere = true;		// ON:  乗算スフィア
	bool UseToon = false; >		// OFF:	トゥーンテクスチャ
{
    pass TexturedMulSphere
    {
        SetVertexShader(CompileShader(vs_4_0, VS_Main(true, true, false)));
        SetPixelShader(CompileShader(ps_4_0, PS_Main(true, true, true, false)));
    }
}

technique10 MulSpheredUnTextureTechniqueUnToon <
	string MMDPass = "object";
	bool UseTexture = false;	// OFF: テクスチャ
	bool UseSphereMap = true;	// ON:  スフィアマップ
	bool MulSphere = true;		// ON:	乗算スフィア
	bool UseToon = false; >		// OFF:	トゥーンテクスチャ
{
    pass UnTexturedMulSphere
    {
        SetVertexShader(CompileShader(vs_4_0, VS_Main(false, true, false)));
        SetPixelShader(CompileShader(ps_4_0, PS_Main(false, true, true, false)));
    }
}
