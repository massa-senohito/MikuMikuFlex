using System;
using System.Linq;
using System.Text;
using SharpDX.Direct3D11;

namespace MikuMikuFlex.エフェクト
{
	/// <summary>
	///     エフェクトのパースのヘルパークラス
	/// </summary>
	public static class EffectParseHelper
	{
		/// <summary>
		///     アノテーション名の大文字小文字を無視して取得する
		/// </summary>
		/// <param name="variable">取得対象のエフェクト変数</param>
		/// <param name="target">アノテーション名</param>
		/// <param name="typeName">期待する型</param>
		/// <returns>アノテーション</returns>
		public static EffectVariable アノテーションを取得する( EffectVariable variable, string target, string typeName )
		{
			string name = target.ToLower();
			string[] valid = name.Split( '/' );
			for( int i = 0; i < variable.Description.AnnotationCount; i++ )
			{
				EffectVariable val = variable.GetAnnotationByIndex( i );
				string typeString = val.Description.Name.ToLower();
				if( typeString == name )
				{
					if( !valid.Contains( typeString ) && !String.IsNullOrWhiteSpace( typeString ) )
					{
						throw new InvalidMMEEffectShader例外(
							string.Format(
								"変数「{0} {1}:{2}」に適用されたアノテーション「{3} {4}」はアノテーションの型が正しくありません。期待した型は{5}でした。",
								variable.TypeInfo.Description.TypeName, variable.Description.Name,
								variable.Description.Semantic, val.TypeInfo.Description.TypeName,
								val.Description.Name, _期待される型の一覧を返す( valid, val.Description.Name ) ) );
					}
					return val;
				}
			}
			return null;
		}

		/// <summary>
		///     アノテーション名の大文字小文字を無視しして取得する
		/// </summary>
		/// <param name="pass">取得対象のパス</param>
		/// <param name="target">アノテーション名</param>
		/// <param name="typeName">期待する型</param>
		/// <returns>アノテーション</returns>
		public static EffectVariable アノテーションを取得する( EffectPass pass, string target, string typeName )
		{
			string name = target.ToLower();
			string[] valid = name.Split( '/' );
			for( int i = 0; i < pass.Description.AnnotationCount; i++ )
			{
				EffectVariable val = pass.GetAnnotationByIndex( i );
				string typeString = val.Description.Name.ToLower();
				if( typeString == name )
				{
					if( !valid.Contains( typeString ) && !String.IsNullOrWhiteSpace( typeString ) )
					{
						throw new InvalidMMEEffectShader例外(
							string.Format(
								"パス「{0}」に適用されたアノテーション「{1} {2}」はアノテーションの型が正しくありません。期待した型は{3}でした。",
								pass.Description.Name, typeString, val.Description.Name, _期待される型の一覧を返す( valid, val.Description.Name ) ) );
					}
					return val;
				}
			}
			return null;
		}

		/// <summary>
		///     アノテーション名の大文字小文字を無視しして取得する
		/// </summary>
		/// <param name="technique">取得対象のテクニック</param>
		/// <param name="target">アノテーション名</param>
		/// <param name="typeName">期待する型</param>
		/// <returns>アノテーション</returns>
		public static EffectVariable アノテーションを取得する( EffectTechnique technique, string target )
		{
			string name = target.ToLower();
			string[] valid = name.Split( '/' );
			for( int i = 0; i < technique.Description.AnnotationCount; i++ )
			{
				EffectVariable val = technique.GetAnnotationByIndex( i );
				string typeString = val.Description.Name.ToLower();
				if( typeString == name )
				{
					if( !valid.Contains( typeString ) && !String.IsNullOrWhiteSpace( typeString ) )
					{
						throw new InvalidMMEEffectShader例外(
							string.Format(
								"テクニック「{0}」に適用されたアノテーション「{1} {2}」はアノテーションの型が正しくありません。期待した型は{3}でした。",
								technique.Description.Name, typeString, val.Description.Name, _期待される型の一覧を返す( valid, val.Description.Name ) ) );
					}
					return val;
				}
			}
			return null;
		}

		public static EffectVariable アノテーションを取得する( EffectGroup group, string target )
		{
			string name = target.ToLower();
			string[] valid = name.Split( '/' );

            for( int i = 0; i < group.Description.Annotations; i++ )
			{
				EffectVariable val = group.GetAnnotationByIndex( i );
				string typeString = val.Description.Name.ToLower();
				if( typeString == name )
				{
					if(
						!valid.Contains( typeString ) && !String.IsNullOrWhiteSpace( typeString ) )
					{
						throw new InvalidMMEEffectShader例外(
							string.Format(
								"エフェクトグループ「{0}」に適用されたアノテーション「{1} {2}」はアノテーションの型が正しくありません。期待した型は{3}でした。",
								group.Description.Name, typeString, val.Description.Name, _期待される型の一覧を返す( valid, val.Description.Name ) ) );
					}
					return val;
				}
			}
			return null;
		}

		private static string _期待される型の一覧を返す( string[] types, string name )
		{
            var builder = new StringBuilder();

			foreach( string type in types )
			{
				if( String.IsNullOrWhiteSpace( type ) )
                    continue;

                if( builder.Length != 0 )
                    builder.Append( "," );

                builder.Append( $"「{type} {name}」" );
			}

			return builder.ToString();
		}
	}
}
