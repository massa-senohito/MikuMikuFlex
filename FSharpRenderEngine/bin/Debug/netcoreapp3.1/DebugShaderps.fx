Texture2D  texDiffuse  : register( t0 );
SamplerState samDiffuse  : register( s0 );

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float2 TextureCoordinates : TEXCOORD0;
};
float4 main(VertexShaderOutput input): SV_TARGET
{

    return texDiffuse.Sample( samDiffuse, input.TextureCoordinates );
}
