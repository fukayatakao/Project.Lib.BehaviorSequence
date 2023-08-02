using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;


namespace Project.Lib {
	public class BehaviourDecision {
		/// <summary>
		/// コンストラクタ（封印）
		/// </summary>
		private BehaviourDecision(){}
		/// <summary>
		/// コピーコンストラクタ
		/// </summary>
		public BehaviourDecision(BehaviourDecision source) {
			EditMode = source.EditMode;
			EvaluateIndex = source.EvaluateIndex;
			//一回シリアライズで文字列にしてからデシリアライズでインスタンスを作る
			EvaluateArgs = MethodArgs.DeserializeArgs(MethodArgs.SerializeArgs(source.EvaluateArgs));
			OrderIndex = source.OrderIndex;
			//一回シリアライズで文字列にしてからデシリアライズでインスタンスを作る
			OrderArgs = MethodArgs.DeserializeArgs(MethodArgs.SerializeArgs(source.OrderArgs));
			IsRestriction = source.IsRestriction;
		}

		/// <summary>
		/// インスタンス生成
		/// </summary>
		public static BehaviourDecision Create(int evIndex, object[] evArgs, int orIndex, object[] orArgs, bool restriction=false) {
			BehaviourDecision decision = new BehaviourDecision();
			decision.EvaluateIndex = evIndex;
			decision.EvaluateArgs = evArgs;
			decision.OrderIndex = orIndex;
			decision.OrderArgs = orArgs;
			decision.IsRestriction = restriction;
			return decision;
		}

		public bool EditMode;

		public int EvaluateIndex;
		public object[] EvaluateArgs;
		public int OrderIndex;
		public object[] OrderArgs;

		public bool IsRestriction;



	}
}
