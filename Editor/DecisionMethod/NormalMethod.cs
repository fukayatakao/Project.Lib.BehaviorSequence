#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;


namespace Project.Lib {
	public class NormalMethod : IDecisionMethod {
		FunctionAttribute funcAttributes_;
		ArgAttribute[] argAttributes_;
		Dictionary<int, string[]> argEnumNames_;
		/// <summary>
		/// コンストラクタ
		/// </summary>
		public NormalMethod(System.Reflection.MethodInfo method) {
			argEnumNames_ = new Dictionary<int, string[]>();
			//引数の属性を拾う
			argAttributes_ = CreateArgAttributeArray(method);
			funcAttributes_ = Attribute.GetCustomAttribute(method, typeof(FunctionAttribute)) as FunctionAttribute;

			for (int i = 0; i < argAttributes_.Length; i++) {
				argEnumNames_[i] = FieldAttribute.GetFields(argAttributes_[i].Type);
			}
		}
		/// <summary>
		/// 関数説明テキストを取得
		/// </summary>
		public string GetText() {
			return funcAttributes_.Text;
		}

		/// <summary>
		/// 引数が列挙型か判定
		/// </summary>
		public bool IsEnumArg(int index) {
			if (index >= argAttributes_.Length)
				return false;
			return argAttributes_[index].Type.IsEnum;
		}


		/// <summary>
		/// 関数の説明に引数を埋め込む
		/// </summary>
		public string DecorateText(object[] args) {
			if (args == null)
				return GetText();

			//argsをそのまま使うと引数が置き換わってしまうのでコピーをちゃんと作る
			object[] convArgs = new object[args.Length];
			for (int i = 0, max = args.Length; i < max; i++) {
				//列挙型はフィールドの文字列を入れる
				if (IsEnumArg(i)) {
					convArgs[i] = FieldAttribute.GetField(args[i]);
					//それ以外はそのまま
				} else {
					convArgs[i] = args[i];
				}
			}
			try {
				string txt = GetText();
				txt = txt.Replace("{", "[{");
				txt = txt.Replace("}", "}]");
				string result = string.Format(txt, convArgs);
				return result;
			} catch (Exception e) {
				Debug.LogError(e);
				return GetText();
			}

		}


		/// <summary>
		/// GUI表示
		/// </summary>
		public bool DrawGUI(ref object[] args) {
			if (args == null)
				return false;
			//関数に変更があったなどで引数が一致しない場合
			if (args.Length != argAttributes_.Length) {
				Debug.LogError("引数の数が一致しないのでデフォルト値で初期化します:" + GetText());
				args = CreateDefaultValueArray();
			}
			EditorGUI.BeginChangeCheck();
			for (int i = 0; i < args.Length; i++) {
				using (new GUILayout.HorizontalScope()) {
					try {
						EditorGUILayout.LabelField(argAttributes_[i].Text, GUILayout.MaxWidth(128f));
						if (argAttributes_[i].Type == typeof(string)) {
							args[i] = EditorGUILayout.TextField((string)args[i], GUILayout.MaxWidth(256f));
						} else if (argAttributes_[i].Type == typeof(int)) {
							args[i] = EditorGUILayout.IntField((int)args[i], GUILayout.MaxWidth(128f));
						} else if (argAttributes_[i].Type == typeof(long)) {
							args[i] = EditorGUILayout.LongField((long)args[i], GUILayout.MaxWidth(128f));
						} else if (argAttributes_[i].Type == typeof(float)) {
							args[i] = EditorGUILayout.FloatField((float)args[i], GUILayout.MaxWidth(128f));
						} else if (argAttributes_[i].Type == typeof(bool)) {
							args[i] = EditorGUILayout.Toggle((bool)args[i], GUILayout.MaxWidth(32f));
						} else if (argAttributes_[i].Type == typeof(Vector3)) {
							args[i] = EditorGUILayout.Vector3Field("", (Vector3)args[i], GUILayout.MaxWidth(192));
							//enumの場合popuplist使う
						} else if (argAttributes_[i].Type.IsEnum) {
							string[] names = Enum.GetNames(argAttributes_[i].Type);
							string str = Enum.ToObject(argAttributes_[i].Type, args[i]).ToString();


							int sel = Array.IndexOf(names, str);
							sel = EditorGUILayout.Popup(sel, argEnumNames_[i], GUILayout.MaxWidth(128f));
							args[i] = Enum.Parse(argAttributes_[i].Type, names[sel]);
						}
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
		/// 引数編集用のカスタム属性を取得する
		/// </summary>
		private static ArgAttribute[] CreateArgAttributeArray(System.Reflection.MethodInfo methodInfo) {
			//引数の属性を拾う
			Attribute[] attr = Attribute.GetCustomAttributes(methodInfo, typeof(ArgAttribute));
			ArgAttribute[] attributes = new ArgAttribute[attr.Length];

			//GetCustomAttributesしたときの配列の順番と引数の順番は一致しないので明示的に並び替える
			for (int i = 0; i < attr.Length; i++) {
				ArgAttribute temp = attr[i] as ArgAttribute;
				attributes[temp.Index] = temp;
			}

			for (int i = 0; i < attr.Length; i++) {
				Debug.Assert(attributes[i] != null, string.Format("method {0} arg index {1} is not found", methodInfo.Name, i));
			}

			return attributes;
		}

		/// <summary>
		/// デフォルト値配列を生成
		/// </summary>
		public object[] CreateDefaultValueArray() {
			object[] args = new object[argAttributes_.Length];
			for (int i = 0; i < argAttributes_.Length; i++) {
				args[argAttributes_[i].Index] = argAttributes_[i].Value;
			}
			return args;
		}
	}

}
#endif
