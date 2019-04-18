

★ このプロジェクトについて


このプロジェクトは、各 .hlsl ファイルをビルドして、生成された .cso ファイルを
以下のフォルダにコピーします。

　　$(SolutionDir)MikuMikuFlex3\Resources\Shaders\

.hlsl ファイルのビルドは C++ プロジェクトでしか行えないので、
MikuMikuFlex とは別のプロジェクトになりました。



★ シェーダーファイル(.hlsl)とそれがインクルードするファイル(.hlsli)の対応



スキニング用コンピュートシェーダー							; リソースバインディング

DefaultSkinningComputeShader.hlsl
	Skinning.hlsli	... 入出力定義（変更不可）				; b1, b2, b3, t0, u0
	VS_INPUT.hlsli	... 出力定義（変更不可）


頂点シェーダー

DefaultVertexShaderForObject.hlsl
DefaultVertexShaderForEdge.hlsl
	GlobalParameters.hlsli	... 入力定義（変更不可）		; b0
	VS_INPUT.hlsli			... 入力定義（変更不可）
	DefaultVS_OUTPUT.hlsli	... 出力定義


ハルシェーダー

DefaultHullShader.hlsl
	GlobalParameters.hlsli			... 入力定義（変更不可）; b0
	DefaultVS_OUTPUT.hlsli			... 入出力定義
	DefaultCONSTANT_HS_OUT.hlsli	... 出力定義


ドメインシェーダー

DefaultDomainShader.hlsl
	GlobalParameters.hlsli			... 入力定義（変更不可）; b0
	DefaultVS_OUTPUT.hlsli			... 入出力定義
	DefaultCONSTANT_HS_OUT.hlsli	... 入力定義


ジオメトリシェーダー

DefaultGeometryShader.hlsl
	GlobalParameters.hlsli	... 入力定義（変更不可）		; b0
	DefaultGS_OUTPUT.hlsli	... 出力定義


ピクセルシェーダー

DefaultPixelShaderForObject.hlsl
DefaultPixelShaderForEdge.hlsl
	GlobalParameters.hlsli	... 入力定義（変更不可）		; b0
	MaterialTexture.hlsli	... 入力定義（変更不可）		; t0, t1, t2
	DefaultVS_OUTPUT.hlsli	... 入力定義



！！注意！！

上記で「変更不可」と書かれているファイルは、プログラムにハードコーディングされている
内容と一致させる必要がありますので、変更しないでください。
