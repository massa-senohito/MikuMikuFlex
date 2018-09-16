using System;
using System.Collections.Generic;
using System.Linq;
using MikuMikuFlex.モデル;
using MikuMikuFlex.モデル.PMX;
using SharpDX;
using SharpDX.Direct3D11;

namespace MikuMikuFlex.エフェクト変数管理
{
    internal sealed class CONTROLOBJECT変数 : 変数管理
    {
        public override string セマンティクス => "CONTROLOBJECT";

        public override 変数型[] 使える型の配列
            => new[] { 変数型.Bool, 変数型.Float, 変数型.Float3, 変数型.Float4, 変数型.Float4x4 };


        private CONTROLOBJECT変数( 変数型 type, string itemName, string name, TargetObject target, bool isSelf )
        {
            _variableType = type;
            _itemName = itemName;
            _name = name;
            _target = target;
            _isSelf = isSelf;
        }

        public CONTROLOBJECT変数()
        {
        }

        public override 変数管理 変数登録インスタンスを生成して返す( EffectVariable variable, エフェクト effect, int semanticIndex )
        {
            変数型 type = 0;
            TargetObject target = TargetObject.UnUsed;
            string itemName = null;
            string typeName = variable.TypeInfo.Description.TypeName.ToLower();

            switch( typeName )
            {
                case "bool":
                    type = 変数型.Bool;
                    break;

                case "float":
                    type = 変数型.Float;
                    break;

                case "float3":
                    type = 変数型.Float3;
                    break;

                case "float4":
                    type = 変数型.Float4;
                    break;

                case "float4x4":
                    type = 変数型.Float4x4;
                    break;

                default:
                    break;
            }

            EffectVariable nameVariable = EffectParseHelper.アノテーションを取得する( variable, "name", "string" );

            if( nameVariable == null )
                throw new InvalidMMEEffectShader例外( $"定義済みセマンティクス「CONTROLOBJECT」の適用されている変数「{typeName} {variable.Description.Name}:CONTROLOBJECT」に対してはアノテーション「string name」は必須ですが、指定されませんでした。" );

            string name = nameVariable.AsString().GetString();

            // Selfの場合はターゲットは自分自身となる

            if( name.ToLower().Equals( "(self)" ) )
            {
                _isSelf = true;
            }
            else
            {
                // TODO: (OffscreenOwner)がnameに指定されたときの対応
            }


            EffectVariable itemVariable = EffectParseHelper.アノテーションを取得する( variable, "item", "string" );

            if( itemVariable != null )
            {
                itemName = itemVariable.AsString().GetString();

                switch( itemName.ToLower() )
                {
                    case "x":
                        target = TargetObject.X;
                        break;
                    case "y":
                        target = TargetObject.Y;
                        break;
                    case "z":
                        target = TargetObject.Z;
                        break;
                    case "xyz":
                        target = TargetObject.XYZ;
                        break;
                    case "rx":
                        target = TargetObject.Rx;
                        break;
                    case "ry":
                        target = TargetObject.Ry;
                        break;
                    case "rz":
                        target = TargetObject.Rz;
                        break;
                    case "rxyz":
                        target = TargetObject.Rxyz;
                        break;
                    case "si":
                        target = TargetObject.Si;
                        break;
                    case "tr":
                        target = TargetObject.Tr;
                        break;
                    default:
                        target = type == 変数型.Float ? TargetObject.FaceName : TargetObject.BoneName;
                        break;
                }

                if( NeedFloat.Contains( target ) && type != 変数型.Float )
                    throw new InvalidMMEEffectShader例外( $"定義済みセマンティクス「CONTROLOBJECT」の適用されている変数「{typeName} {variable.Description.Name}:CONTROLOBJECT」にはアノテーション「string item=\"{itemName}\"」が適用されていますが、「{itemName}」の場合は「float {variable.Description.Name}:CONTROLOBJECT」である必要があります。" );

                if( NeedFloat3.Contains( target ) && type != 変数型.Float3 )
                    throw new InvalidMMEEffectShader例外( $"定義済みセマンティクス「CONTROLOBJECT」の適用されている変数「{typeName} {variable.Description.Name}:CONTROLOBJECT」にはアノテーション「string item=\"{itemName}\"」が適用されていますが、「{itemName}」の場合は「float3 {variable.Description.Name}:CONTROLOBJECT」である必要があります。" );
            }

            return new CONTROLOBJECT変数( type, itemName, name, target, _isSelf );
        }

        public override void 変数を更新する( EffectVariable 変数, 変数更新時引数 引数 )
        {
            var currentModel = RenderContext.Instance.描画ターゲットコンテキスト.ワールド空間.Drawableを取得する( _name );

            if( currentModel == null )
                return;

            IDrawable targetDrawable = ( _isSelf ) ? 引数.モデル : currentModel;

            if( _target == TargetObject.UnUsed )
            {
                switch( _variableType )
                {
                    case 変数型.Float4x4:
                        変数.AsMatrix().SetMatrix( RenderContext.Instance.行列管理.ワールド行列管理.モデルのワールド変換行列を作成して返す( targetDrawable ) );
                        break;
                    case 変数型.Float3:
                        変数.AsVector().Set( targetDrawable.モデル状態.位置 );
                        break;
                    case 変数型.Float4:
                        変数.AsVector().Set( new Vector4( targetDrawable.モデル状態.位置, 1f ) );
                        break;
                    case 変数型.Float:
                        変数.AsScalar().Set( targetDrawable.モデル状態.倍率.Length() );
                        break;
                    case 変数型.Bool:
                        変数.AsScalar().Set( targetDrawable.表示中 );
                        break;
                    default:
                        break;
                }
            }
            else if( _target == TargetObject.BoneName )
            {
                IEnumerable<PMXボーン> targetBone = ( from bone in ( (PMXModel) targetDrawable ).スキニング.ボーン配列 where bone.ボーン名 == _itemName select bone );

                foreach( var bone in targetBone )
                {
                    Matrix mat = bone.モデルポーズ行列 * RenderContext.Instance.行列管理.ワールド行列管理.モデルのワールド変換行列を作成して返す( targetDrawable );

                    switch( _variableType )
                    {
                        case 変数型.Float4x4:
                            変数.AsMatrix().SetMatrix( mat );
                            break;
                        case 変数型.Float3:
                            変数.AsVector().Set( Vector3.TransformCoordinate( bone.ローカル位置, mat ) );
                            break;
                        case 変数型.Float4:
                            変数.AsVector().Set( new Vector4( Vector3.TransformCoordinate( bone.ローカル位置, mat ), 1f ) );
                            break;
                        default:
                            break;
                    }
                    break;
                }
            }
            else if( _target == TargetObject.FaceName )
            {
                モーフ管理 morphManager = ( (PMXModel) targetDrawable ).モーフ管理;

                変数.AsScalar().Set( morphManager.モーフの進捗率を返す( _name ) );
            }
            else
            {
                switch( _target )
                {
                    case TargetObject.X:
                        変数.AsScalar().Set( targetDrawable.モデル状態.位置.X );
                        break;
                    case TargetObject.Y:
                        変数.AsScalar().Set( targetDrawable.モデル状態.位置.Y );
                        break;
                    case TargetObject.Z:
                        変数.AsScalar().Set( targetDrawable.モデル状態.位置.Z );
                        break;
                    case TargetObject.XYZ:
                        変数.AsVector().Set( targetDrawable.モデル状態.位置 );
                        break;
                    case TargetObject.Rx:
                    case TargetObject.Ry:
                    case TargetObject.Rz:
                    case TargetObject.Rxyz:
                        float xRotation, yRotation, zRotation; //X,Y,Z軸回転量に変換する。
                                                               //int type = 0; //分解パターン
                        if( !CGHelper.クォータニオンをXYZ回転に分解する( targetDrawable.モデル状態.回転, out xRotation, out yRotation, out zRotation ) )
                        {
                            if( !CGHelper.クォータニオンをYZX回転に分解する( targetDrawable.モデル状態.回転, out yRotation, out zRotation, out xRotation ) )
                            {
                                CGHelper.クォータニオンをZXY回転に分解する( targetDrawable.モデル状態.回転, out zRotation, out xRotation, out yRotation );
                                //		type = 2;
                            }
                            else
                            {
                                //		type = 1;
                            }
                        }
                        else
                        {
                            //	type = 0;
                        }

                        if( _target == TargetObject.Rx )
                        {
                            変数.AsScalar().Set( xRotation );
                        }
                        else if( _target == TargetObject.Ry )
                        {
                            変数.AsScalar().Set( yRotation );
                        }
                        else if( _target == TargetObject.Rz )
                        {
                            変数.AsScalar().Set( zRotation );
                        }
                        else
                        {
                            変数.AsVector().Set( new Vector3( xRotation, yRotation, zRotation ) );
                        }
                        break;

                    case TargetObject.Si:
                        変数.AsScalar().Set( targetDrawable.モデル状態.倍率.Length() );
                        break;

                    case TargetObject.Tr:
                        // TODO: Trへの対応
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }


        private 変数型 _variableType;

        private string _itemName;

        private string _name;

        private TargetObject _target;

        private bool _isSelf;

        private static TargetObject[] NeedFloat = {
            TargetObject.X,
            TargetObject.Y,
            TargetObject.Z,
            TargetObject.Rx,
            TargetObject.Ry,
            TargetObject.Rz,
            TargetObject.Si,
            TargetObject.Tr,
            TargetObject.FaceName,
        };

        private static TargetObject[] NeedFloat3 = {
            TargetObject.XYZ,
            TargetObject.Rxyz,
        };
    }
}
