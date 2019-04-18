echo off
echo ------------------------------------------------------------
echo MikuMikuFlex の NuGet パッケージ (.nupkg) 作成バッチ
echo 事前に Release/x64  のビルドを完了しておくこと。
echo また、NuGetパッケージの属性（バージョンなど）が変わったら、
echo MikuMikuFlex3/MikuMikuFlex.nuspec を修正すること。
echo ------------------------------------------------------------

nuget pack ..\MikuMikuFlex3\MikuMikuFlex3.csproj -IncludeReferencedProjects -properties Configuration=Release;Platform=x64 -OutputDirectory nuget_packages
pause
