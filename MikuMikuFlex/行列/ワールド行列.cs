using System;
using MikuMikuFlex.モデル;
using SharpDX;

namespace MikuMikuFlex.行列
{
    /// <summary>
    ///     一般的なワールド行列の管理クラス
    /// </summary>
    public class ワールド行列
    {
        public Vector3 拡大縮小
        {
            get => _拡大縮小;
            set
            {
                _拡大縮小 = value;

                _ワールド行列が変更されればイベントを発生する( new ワールド行列変更EventArgs( ワールド行列変数種別.Scaling ) );
            }
        }

        public Quaternion 回転
        {
            get => this._回転;
            set
            {
                this._回転 = value;

                this._ワールド行列が変更されればイベントを発生する( new ワールド行列変更EventArgs( ワールド行列変数種別.Rotation ) );
            }
        }

        public Vector3 移動
        {
            get => _移動;
            set
            {
                _移動 = value;

                this._ワールド行列が変更されればイベントを発生する( new ワールド行列変更EventArgs( ワールド行列変数種別.Translation ) );
            }
        }

        public event EventHandler<ワールド行列変更EventArgs> ワールド行列が変更された;


        public ワールド行列()
        {
            _拡大縮小 = new Vector3( 1.0f, 1.0f, 1.0f );
            _回転 = Quaternion.Identity;
            _移動 = Vector3.Zero;
        }

        public Matrix ローカル値とあわせたワールド変換行列を作成して返す( Vector3 ローカル拡大縮小, Quaternion ローカル回転, Vector3 ローカル移動 )
        {
            var ローカルスケーリング = new Vector3(
                拡大縮小.X * ローカル拡大縮小.X,
                拡大縮小.Y * ローカル拡大縮小.Y,
                拡大縮小.Z * ローカル拡大縮小.Z );

            //return 
            //    Matrix.Scaling( ローカルスケーリング ) *
            //    Matrix.RotationQuaternion( ローカル回転 * 回転 ) *
            //    Matrix.Translation( ローカル移動 + 移動 );
            return
                Matrix.Scaling( ローカルスケーリング ) *
                Matrix.RotationQuaternion( ローカル回転 ) *
                Matrix.Translation( ローカル移動 ) *
                Matrix.RotationQuaternion( 回転 ) *
                Matrix.Translation( 移動 );
        }

        public Matrix ローカル行列とあわせたワールド変換行列を作成して返す( Matrix ローカル行列 )
        {
            //return
            //    Matrix.Scaling( _拡大縮小 ) *
            //    Matrix.RotationQuaternion( _回転 ) *
            //    Matrix.Translation( _移動 ) *
            //    ローカル行列;
            return
                ローカル行列 *
                Matrix.Scaling( _拡大縮小 ) *
                Matrix.RotationQuaternion( _回転 ) *
                Matrix.Translation( _移動 );
        }

        public Matrix モデルのワールド変換行列を作成して返す( IDrawable drawable )
        {
            return ローカル行列とあわせたワールド変換行列を作成して返す( drawable.モデル状態.ローカル変換行列 );
        }


        private Quaternion _回転;

        private Vector3 _拡大縮小;

        private Vector3 _移動;


        private void _ワールド行列が変更されればイベントを発生する( ワールド行列変更EventArgs arg )
        {
            ワールド行列が変更された?.Invoke( this, arg );
        }
    }
}
