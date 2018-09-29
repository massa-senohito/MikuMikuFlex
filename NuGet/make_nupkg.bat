echo off
echo ------------------------------------------------------------
echo MikuMikuFlex の NuGet パッケージ (.nupkg) 作成バッチ
echo 事前に x86/Release  のビルドを完了しておくこと。(x64もビルドできるが、NuGetにはx86のみ公開する。）
echo また、NuGetパッケージの属性（バージョンなど）が変わったら、
echo MikuMikuFlex/MikuMikuFlex.nuspec を修正すること。
echo ------------------------------------------------------------

rem nuget pack ..\MikuMikuFlex\MikuMikuFlex.csproj -IncludeReferencedProjects -properties Configuration=Release;Platform=x86 -OutputDirectory nuget_packages
nuget pack ..\MikuMikuFlex\MikuMikuFlex.nuspec -OutputDirectory nuget_packages

echo ------------------------------------------------------------
echo 注意：
echo sharpdx_direct3d11_1_effects_[x68/x64].dll に対する警告 NU5100 が出た場合は*無視*してください。
echo SharpDX.Direct3D11.Effects パッケージの構造が特殊なため、その対策です。
echo ------------------------------------------------------------

pause