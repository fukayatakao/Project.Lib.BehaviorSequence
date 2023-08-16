#if UNITY_EDITOR
using System;


namespace Project.Lib {
	public interface IDecisionMethod {
		string GetText();
		bool IsEnumArg(int index);
		string DecorateText(object[] args);
		bool DrawGUI(ref object[] args);
		object[] CreateDefaultValueArray();
	}


	public class DecisionMethod {
		IDecisionMethod decisoinMethod_;

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public DecisionMethod(System.Reflection.MethodInfo method) {
			if (Attribute.GetCustomAttribute(method, typeof(CheckFunctionAttribute)) != null) {
				decisoinMethod_ = new CheckMethod(method);
			} else {
				decisoinMethod_ = new NormalMethod(method);
			}
		}
		/// <summary>
		/// 関数説明テキストを取得
		/// </summary>
		public string GetText() {
			return decisoinMethod_.GetText();
		}

		/// <summary>
		/// 引数が列挙型か判定
		/// </summary>
		public bool IsEnumArg(int index) {
			return decisoinMethod_.IsEnumArg(index);
		}


		/// <summary>
		/// 関数の説明に引数を埋め込む
		/// </summary>
		public string DecorateText(object[] args) {
			return decisoinMethod_.DecorateText(args);
		}


		/// <summary>
		/// GUI表示
		/// </summary>
		public bool DrawGUI(ref object[] args) {
			return decisoinMethod_.DrawGUI(ref args);
		}
		/// <summary>
		/// デフォルト値配列を生成
		/// </summary>
		public object[] CreateDefaultValueArray() {
			return decisoinMethod_.CreateDefaultValueArray();
		}
	}

}
#endif
