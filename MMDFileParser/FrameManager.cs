using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMDFileParser
{
    /// <summary>
    ///     フレームの管理。モーションで利用。
    /// </summary>
    public class FrameManager
    {
        public bool フレームは昇順に並んでいる
        {
            get
            {
                if( this._フレームデータリスト.Count <= 1 )
                    return true;

                for( int i = 1; i < _フレームデータリスト.Count; i++ )
                {
                    if( _フレームデータリスト[ i - 1 ].フレーム番号 > _フレームデータリスト[ i ].フレーム番号 )
                        return false;   // 昇順じゃない箇所があった
                }

                return true;
            }
        }


        public void フレームデータを追加する( IFrameData frameData )
        {
            _フレームデータリスト.Add( frameData );
        }

        public void フレームデータを昇順にソートする()
        {
            _フレームデータリスト.Sort();
        }

        public uint 最後のフレーム番号を返す()
        {
            return _フレームデータリスト.Last().フレーム番号;
        }

        public void 現在のフレームの前後のキーフレームを探して返す( float フレーム番号, out IFrameData ひとつ前のフレーム, out IFrameData ひとつ後ろのフレーム )
        {
            // 指定フレームが最初のキーフレームより前にある場合
            if( フレーム番号 <= _フレームデータリスト.First().フレーム番号 )
            {
                ひとつ前のフレーム =
                    ひとつ後ろのフレーム = _フレームデータリスト.First();
                return;
            }

            // 指定フレームが最後のキーフレームより後にある場合
            if( フレーム番号 >= _フレームデータリスト.Last().フレーム番号 )
            {
                ひとつ前のフレーム = 
                    ひとつ後ろのフレーム = _フレームデータリスト.Last();
                return;
            }

            // 指定フレームの後のキーフレームのインデックスを探す
            // 　高速化のため、指定フレームが前回の過去キーフレームより進んでいれば、前回の過去キーフレームから探す。

            int futureFrameIndex;

            if( _フレームデータリスト[ _前回のフレームの過去キーフレームのインデックス ].フレーム番号 < フレーム番号 )
            {
                futureFrameIndex = _フレームデータリスト.FindIndex( _前回のフレームの過去キーフレームのインデックス, ( b ) => ( b.フレーム番号 > フレーム番号 ) );
            }
            else
            {
                futureFrameIndex = _フレームデータリスト.FindIndex( ( b ) => ( b.フレーム番号 > フレーム番号 ) );
            }


            ひとつ前のフレーム = _フレームデータリスト[ futureFrameIndex - 1 ];
            ひとつ後ろのフレーム = _フレームデータリスト[ futureFrameIndex ];

            // 今回の過去キーフレームを記憶しておく

            _前回のフレームの過去キーフレームのインデックス = futureFrameIndex - 1;
        }


        private List<IFrameData> _フレームデータリスト = new List<IFrameData>();

        private int _前回のフレームの過去キーフレームのインデックス = 0;
    }
}
