using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;


namespace Project.Lib {
	public class CheckMethod : IDecisionMethod {
		CheckFunctionAttribute checkFunctionAttribute_;
		/// <summary>
		/// コンストラクタ
		/// </summary>
		public CheckMethod(System.Reflection.MethodInfo method) {
			checkFunctionAttribute_ = Attribute.GetCustomAttribute(method, typeof(CheckFunctionAttribute)) as CheckFunctionAttribute;
		}
		/// <summary>
		/// 関数説明テキストを取得
		/// </summary>
		public string GetText() {
			return checkFunctionAttribute_.Negative;
		}

		/// <summary>
		/// 引数が列挙型か判定
		/// </summary>
		public bool IsEnumArg(int index) {
			return false;
		}


		/// <summary>
		/// 関数の説明に引数を埋め込む
		/// </summary>
		public string DecorateText(object[] args) {
			//データがおかしい時の保険を付ける
			if (args == null || args.Length == 0 || !(args[0] is bool))
				return GetText();
			if ((bool)args[0]) {
				return checkFunctionAttribute_.Affiermative;
			} else {
				return checkFunctionAttribute_.Negative;
			}
		}


		/// <summary>
		/// GUI表示
		/// </summary>
		public bool DrawGUI(ref object[] args) {
			if (args == null)
				return false;
			//関数に変更があったなどで引数が一致しない場合
			if (args.Length != 1) {
				Debug.LogError("引数の数が一致しないのでデフォルト値で初期化します:" + GetText());
				args = CreateDefaultValueArray();
			}
			EditorGUI.BeginChangeCheck();
			for (int i = 0; i < args.Length; i++) {
				using (new GUILayout.HorizontalScope()) {
					try {
						EditorGUILayout.LabelField("真偽値", GUILayout.MaxWidth(128f));
						args[i] = EditorGUILayout.Toggle((bool)args[i], GUILayout.MaxWidth(32f));
					} catch (Exception e) {
						Debug.LogError(e);
						Debug.LogError("引数のキャストエラーが発生したのでデフォルト値を代入します:" + GetText());

						args = CreateDefaultValueArray();

						return false;
					}

				}
			}
			//変更有ったのでtrueを返す
			if (EditorGUI.EndChangeCheck()) {
				return true;
			} else {
				return false;
			}
		}
		/// <summary>
		/// デフォルト値配列を生成
		/// </summary>
		public object[] CreateDefaultValueArray() {
			return new object[] { false };
		}
	}

}
