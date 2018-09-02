# NuGet でのインストール時、toon0～10.bmp のプロパティを変更する。
# 参考: http://garicchi.hatenablog.jp/entry/2015/06/01/185201

param($installPath, $toolsPath, $package, $project)

function Recurse($dir)
{
    foreach($i in $dir)
    {
        Recurse($i.ProjectItems)

        if(($i.Name -like "toon*.bmp") -Or ($i.Name -like "sharpdx_direct3d11_1_effects_*.dll"))
        {
			# 0:None, 1:Compile, 2:Content, 3:EmbeddedResource
            $i.Properties.Item("BuildAction").Value = [int]0
			
			# 0:DontCopy, 1:CopyAlways, 2:CopyIfNewer
            $i.Properties.Item("CopyToOutputDirectory").Value = [int]2
        }
    }
}

Recurse($project.ProjectItems);
