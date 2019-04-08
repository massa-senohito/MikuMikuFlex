////////////////////////////////////////////////////////////////////////////////////////////////////
//
// ジオメトリシェーダー
//
////////////////////////////////////////////////////////////////////////////////////////////////////

#include "DefaultGS_OUTPUT.hlsli"
#include "GlobalParameters.hlsli"

[maxvertexcount(3)]
void main(
	triangle float4 input[3] : SV_POSITION,
	inout TriangleStream<GS_OUTPUT> output
)
{
    for (uint i = 0; i < 3; i++)
    {
        GS_OUTPUT element;
        element.pos = input[i];
        output.Append(element);
    }
}
