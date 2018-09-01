
// クォータニオン /////////////////////////////////////////

#define QUATERNION_IDENTITY float4(0, 0, 0, 1)

float4 q_slerp(float4 a, float4 b, float t)
{
    // if either input is zero, return the other.
    if (length(a) == 0.0)
    {
        if (length(b) == 0.0)
        {
            return QUATERNION_IDENTITY;
        }
        return b;
    }
    else if (length(b) == 0.0)
    {
        return a;
    }

    float cosHalfAngle = a.w * b.w + dot(a.xyz, b.xyz);

    if (cosHalfAngle >= 1.0 || cosHalfAngle <= -1.0)
    {
        return a;
    }
    else if (cosHalfAngle < 0.0)
    {
        b.xyz = -b.xyz;
        b.w = -b.w;
        cosHalfAngle = -cosHalfAngle;
    }

    float blendA;
    float blendB;
    if (cosHalfAngle < 0.99)
    {
        // do proper slerp for big angles
        float halfAngle = acos(cosHalfAngle);
        float sinHalfAngle = sin(halfAngle);
        float oneOverSinHalfAngle = 1.0 / sinHalfAngle;
        blendA = sin(halfAngle * (1.0 - t)) * oneOverSinHalfAngle;
        blendB = sin(halfAngle * t) * oneOverSinHalfAngle;
    }
    else
    {
        // do lerp if angle is really small.
        blendA = 1.0 - t;
        blendB = t;
    }

    float4 result = float4(blendA * a.xyz + blendB * b.xyz, blendA * a.w + blendB * b.w);
    if (length(result) > 0.0)
    {
        return normalize(result);
    }
    return QUATERNION_IDENTITY;
}

float4x4 quaternion_to_matrix(float4 quat)
{
    float4x4 m = float4x4(float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0));

    float x = quat.x, y = quat.y, z = quat.z, w = quat.w;
    float x2 = x + x, y2 = y + y, z2 = z + z;
    float xx = x * x2, xy = x * y2, xz = x * z2;
    float yy = y * y2, yz = y * z2, zz = z * z2;
    float wx = w * x2, wy = w * y2, wz = w * z2;

    m[0][0] = 1.0 - (yy + zz);
    m[0][1] = xy - wz;
    m[0][2] = xz + wy;

    m[1][0] = xy + wz;
    m[1][1] = 1.0 - (xx + zz);
    m[1][2] = yz - wx;

    m[2][0] = xz - wy;
    m[2][1] = yz + wx;
    m[2][2] = 1.0 - (xx + yy);

    m[3][3] = 1.0;

    return m;
}


// Script 宣言 ///////////////////////////////////////////


float Script : STANDARDSGLOBAL <
	string ScriptClass="object";
	string Script="";
> = 0.8;


// グローバル変数 ///////////////////////////////////////////


float4 EdgeColor : EDGECOLOR; // エッジの色 
float EdgeWidth : EDGEWIDTH; // エッジの幅

float4x4 matWVP       : WORLDVIEWPROJECTION < string Object="Camera"; >;
float4x4 matWV        : WORLDVIEW < string Object = "Camera"; >;
float4x4 WorldMatrix  : WORLD;
float4x4 ViewMatrix   : VIEW;
float2   viewportSize : VIEWPORTPIXELSIZE;
float4   ViewPointPosition : POSITION < string object="camera"; >;	// カメラ位置（float4）
float4   LightPointPosition : POSITION < string object="light"; >;	// 光源位置
float3	 CameraPosition		: POSITION < string Object = "Camera"; > ;	// カメラ位置（float3）

Texture2D Texture : MATERIALTEXTURE;			// サンプリング用テクスチャ
Texture2D SphereTexture : MATERIALSPHEREMAP;	// サンプリング用スフィアマップテクスチャ
Texture2D Toon : MATERIALTOONTEXTURE;			// サンプリング用トゥーンテクスチャ


// グローバル変数；特殊パラメータ

bool use_spheremap;			// 描画中の材質がスフィアマップを使用するなら true
bool spadd;					// スフィアマップを使う場合のオプション。（true: 加算スフィア、false: 乗算スフィア）
bool use_texture;			// 描画中の材質がテクスチャを使用するなら true
bool use_toontexturemap;	// 描画中の材質がトゥーンテクスチャを使用するなら true
bool use_selfshadow;		// 描画中の材質がセルフ影を使用するなら true


// 定数バッファ；材質単位で変わらないもの

cbuffer BasicMaterialConstant   // cbuffer にはセマンティックを付けられないので、名前によって識別され、アプリからデータが書き込まれる。
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


// 既定の入出力定義 ///////////////////////////////////////


// コンピュートシェーダ入力

#define MAX_BONE    768

cbuffer BoneTransBuffer
{
    float4x4 BoneTrans[MAX_BONE]; // ボーンのモデルポーズの配列
}
cbuffer BoneLocalPositionBuffer
{
    float3 BoneLocalPosition[MAX_BONE]; // ボーンのローカル位置の配列（SDEFで使用）
}
cbuffer BoneQuaternionBuffer
{
    float4 BoneQuaternion[MAX_BONE]; // ボーンの回転（クォータニオン）の配列（SDEFで使用）
}

struct CS_INPUT
{
    float4 Position;
    float4 BoneWeight;
    uint4  BoneIndex;
    float3 Normal;
    float2 Tex;
    float4 AddUV1;
    float4 AddUV2;
    float4 AddUV3;
    float4 AddUV4;
    float4 Sdef_C;
    float3 Sdef_R0;
    float3 Sdef_R1;
    float  EdgeWeight;
    uint   Index;
    uint   Deform;
};

StructuredBuffer<CS_INPUT> CSBuffer : register(t0);

#define DEFORM_BDEF1    0
#define DEFORM_BDEF2    1
#define DEFORM_BDEF4    2
#define DEFORM_SDEF     3
#define DEFORM_QDEF     4


// 頂点シェーダ入力

struct VS_INPUT
{
	float4 Position   : POSITION;      // スキニング後の座標（ローカル座標）
	float3 Normal     : NORMAL;        // 法線（ローカル座標）
	float2 Tex        : TEXCOORD0;     // UV
	float4 AddUV1     : TEXCOORD1;     // 追加UV1
	float4 AddUV2     : TEXCOORD2;     // 追加UV2
	float4 AddUV3     : TEXCOORD3;     // 追加UV3
	float4 AddUV4     : TEXCOORD4;     // 追加UV4
	float  EdgeWeight : EDGEWEIGHT;    // エッジウェイト
	uint   Index      : PSIZE15;       // 頂点インデックス値
};

#define VS_INPUT_SIZE  ((4+3+2+4+4+4+4+1+1)*4)

RWByteAddressBuffer VSBuffer : register(u0);


// 頂点シェーダ出力

struct VS_OUTPUT
{
	float4 Position   : SV_POSITION;	// 座標（射影座標）
	float3 Normal	  : NORMAL;			// 法線
	float2 Tex		  : TEXCOORD1;		// テクスチャ
	float3 Eye		  : TEXCOORD3;		// カメラとの相対位置
	float2 SpTex	  : TEXCOORD4;		// スフィアマップテクスチャ座標
	float4 Color	  : COLOR0;			// ディフューズ色
};


// スキニング /////////////////////////////////////////////////

void BDEF(CS_INPUT input, out float4 position, out float3 normal)
{
    float4x4 bt =
        BoneTrans[input.BoneIndex[0]] * input.BoneWeight[0] +
        BoneTrans[input.BoneIndex[1]] * input.BoneWeight[1] +
        BoneTrans[input.BoneIndex[2]] * input.BoneWeight[2] +
        BoneTrans[input.BoneIndex[3]] * input.BoneWeight[3];

    position = mul(input.Position, bt);
    normal = normalize(mul(float4(input.Normal, 0), bt)).xyz;
}

void SDEF(CS_INPUT input, out float4 position, out float3 normal)
{
    // 参考: 
    // 自分用メモ「PMXのスフィリカルデフォームのコードっぽいもの」（sma42氏）
    // https://www.pixiv.net/member_illust.php?mode=medium&illust_id=60755964

    float w0 = 0.0f; // 固定値であるSDEFパラメータにのみ依存するので、これらの値も固定値。
    float w1 = 0.0f; //

    float L0 = length(input.Sdef_R0 - (float3) BoneLocalPosition[input.BoneIndex[1]]); // 子ボーンからR0までの距離
    float L1 = length(input.Sdef_R1 - (float3) BoneLocalPosition[input.BoneIndex[1]]); // 子ボーンからR1までの距離

    if (abs(L0 - L1) < 0.0001f)
    {
        w0 = 0.5f;
    }
    else
    {
        w0 = saturate(L0 / (L0 + L1));
    }
    w1 = 1.0f - w0;

    float4x4 modelPoseL = BoneTrans[input.BoneIndex[0]] * input.BoneWeight[0];
    float4x4 modelPoseR = BoneTrans[input.BoneIndex[1]] * input.BoneWeight[1];
    float4x4 modelPoseC = modelPoseL + modelPoseR;

    float4 Cpos = mul(input.Sdef_C, modelPoseC); // BDEF2で計算された点Cの位置
    float4 Ppos = mul(input.Position, modelPoseC); // BDEF2で計算された頂点の位置

    float4 qp = q_slerp(
        BoneQuaternion[input.BoneWeight[0]] * input.BoneWeight[0],
        BoneQuaternion[input.BoneWeight[1]] * input.BoneWeight[1],
        input.BoneWeight[0]);
    float4x4 qpm = quaternion_to_matrix(qp);

    float4 R0pos = mul(float4(input.Sdef_R0, 1.0f), (modelPoseL + (modelPoseC * -input.BoneWeight[0])));
    float4 R1pos = mul(float4(input.Sdef_R1, 1.0f), (modelPoseR + (modelPoseC * -input.BoneWeight[1])));
    Cpos += (R0pos * w0) + (R1pos * w1); // 膨らみすぎ防止

    Ppos -= Cpos; // 頂点を点Cが中心になるよう移動して
    Ppos = mul(Ppos, qpm); // 回転して
    Ppos += Cpos; // 元の位置へ

    position = Ppos;
    normal = normalize(mul(float4(input.Normal, 0), qpm)).xyz;
}

void QDEF(CS_INPUT input, out float4 position, out float3 normal)
{
    // TODO: QDEF の実装に変更する。

    float4x4 bt =
        BoneTrans[input.BoneIndex[0]] * input.BoneWeight[0] +
        BoneTrans[input.BoneIndex[1]] * input.BoneWeight[1] +
        BoneTrans[input.BoneIndex[2]] * input.BoneWeight[2] +
        BoneTrans[input.BoneIndex[3]] * input.BoneWeight[3];

    position = mul(input.Position, bt);
    normal = normalize(mul(float4(input.Normal, 0), bt)).xyz;
}


// コンピュートシェーダー
// 　Xしか扱わないので、Dispach は (頂点数/64+1, 1, 1) とすること。
// 　例: 頂点数が 130 なら Dispach( 3, 1, 1 )
[numthreads(64,1,1)]
void CS_Skinning( uint3 id : SV_DispatchThreadID )
{
    uint csIndex = id.x; // 頂点番号（0～頂点数-1）
    uint vsIndex = csIndex * VS_INPUT_SIZE; // 出力位置[byte単位（必ず4の倍数であること）]

    CS_INPUT input = CSBuffer[csIndex];

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

    VSBuffer.Store4(vsIndex + 0, asuint(position));
    VSBuffer.Store3(vsIndex + 16, asuint(normal));
    VSBuffer.Store2(vsIndex + 28, asuint(input.Tex));
    VSBuffer.Store4(vsIndex + 36, asuint(input.AddUV1));
    VSBuffer.Store4(vsIndex + 52, asuint(input.AddUV2));
    VSBuffer.Store4(vsIndex + 68, asuint(input.AddUV3));
    VSBuffer.Store4(vsIndex + 84, asuint(input.AddUV4));
    VSBuffer.Store(vsIndex + 100, asuint(input.EdgeWeight));
    VSBuffer.Store(vsIndex + 104, asuint(input.Index));
}

// テクニックとパス

technique11 DefaultSkinning < string MMDPass = "skinning"; >
{
    pass DefaultPass
    {
        SetComputeShader(CompileShader(cs_5_0, CS_Skinning()));
    }
}

// オブジェクト描画 ///////////////////////////////////////////


// 頂点シェーダ

VS_OUTPUT VS_Object(VS_INPUT input)
{
    VS_OUTPUT Out = (VS_OUTPUT) 0;

	// 位置（ワールドビュー射影変換）
    Out.Position = mul(input.Position, matWVP);

	// カメラとの相対位置
    Out.Eye = (ViewPointPosition - mul(input.Position, WorldMatrix)).xyz;

	// 頂点法線
    Out.Normal = normalize(mul(input.Normal, (float3x3) WorldMatrix));

	// ディフューズ色計算
    Out.Color.rgb = DiffuseColor.rgb;
    Out.Color.a = DiffuseColor.a;
    Out.Color = saturate(Out.Color); // 0～1 に丸める

    Out.Tex = input.Tex;

    if (use_spheremap)
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


// ピクセルシェーダ

float4 PS_Object( VS_OUTPUT IN ) : SV_TARGET
{
    // 反射色計算
	
    float3 LightDirection = -normalize(mul(LightPointPosition, matWV)).xyz;
    float3 HalfVector = normalize(normalize(IN.Eye) - mul(float4(LightDirection, 0), matWV).xyz);
    float3 Specular = pow(max(0.00001, dot(HalfVector, normalize(IN.Normal))), SpecularPower) * SpecularColor.rgb;
    float4 Color = IN.Color;


	// テクスチャサンプリング

    if (use_texture)
    {
        Color *= Texture.Sample(mySampler, IN.Tex);
    }

	// スフィアマップサンプリング

    if (use_spheremap)
    {
        if (spadd)
        {
            Color.rgb += SphereTexture.Sample(mySampler, IN.SpTex).rgb; // 加算
        }
        else
        {
            Color.rgb *= SphereTexture.Sample(mySampler, IN.SpTex).rgb; // 乗算
        }
    }

	
	// シェーディング

    float LightNormal = dot(IN.Normal, -mul(float4(LightDirection, 0), matWV).xyz);
    float shading = saturate(LightNormal); // 0～1 に丸める
    
	
	// トゥーンテクスチャサンプリング
	
    if (use_toontexturemap)
    {
        float3 MaterialToon = Toon.Sample(mySampler, float2(0, shading)).rgb;
        Color.rgb *= MaterialToon;
    }
    else
    {
        float3 MaterialToon = 1.0f.xxx * shading;
        Color.rgb *= MaterialToon;
    }
    
    
    // 色に反射光を加算

    Color.rgb += Specular;

	
	// 色に環境校を加算

	//Color.rgb += AmbientColor.rgb * 0.2;	//TODO MMDのAmbientの係数がわからん・・・
    Color.rgb += AmbientColor.rgb * 0.005;

    return Color;
}


// テクニックとパス

technique11 DefaultObject < string MMDPass = "object"; >
{
    pass DefaultPass
    {
        SetVertexShader(CompileShader(vs_5_0, VS_Object()));
        SetPixelShader(CompileShader(ps_5_0, PS_Object()));
    }
}


// エッジ描画 ////////////////////////////////////////////////


// 頂点シェーダ

VS_OUTPUT VS_Edge(VS_INPUT IN)
{
    VS_OUTPUT Out = (VS_OUTPUT) 0;

    Out.Normal = normalize(mul(IN.Normal, (float3x3) WorldMatrix)); // 頂点法線

	// 位置（ローカル座標）を、法線方向に膨らませてから、ワールドビュー射影変換する。
    float4 position = IN.Position + float4(Out.Normal, 0) * EdgeWidth * IN.EdgeWeight * distance(IN.Position.xyz, CameraPosition) * 0.0005;
    Out.Position = mul(position, matWVP);

    Out.Eye = (ViewPointPosition - mul(IN.Position, WorldMatrix)).xyz; // カメラとの相対位置
    Out.Tex = IN.Tex; // テクスチャ

    return Out;
}


// ピクセルシェーダ

float4 PS_Edge( VS_OUTPUT IN ) : SV_TARGET
{
    return EdgeColor;
}


// テクニックとパス

technique11 DefaultEdge < string MMDPass = "edge"; >
{
    pass DefaultPass
    {
        SetVertexShader(CompileShader(vs_5_0, VS_Edge()));
        SetPixelShader(CompileShader(ps_5_0, PS_Edge()));
    }
}
