using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


namespace Project.Lib {
    /// <summary>
    /// 行動制御のデータ
    /// </summary>
    public class BehaviorSequenceData : ScriptableObject {
		//適用するEntityの型情報（主に編集・確認用）
		public string EntityType;
		//最初の状態
		public string StartStateNo;
		//状態ごとのデータ
		public List<DecisionStateData> StateDataList;

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public BehaviorSequenceData() {
			StateDataList = new List<DecisionStateData>();
		}
		/// <summary>
		/// 判定と実行処理のデータ
		/// </summary>
		[Serializable]
		public class DecisionData
		{
			/// <summary>
			/// コンストラクタ
			/// </summary>
			public DecisionData(string evaluate, object[] evaluateArgs, string order, object[] orderArgs, bool stop)
			{
				EvaluateFuncNo = evaluate;
				if (evaluateArgs != null) {
					EvaluateArgs = MethodArgs.SerializeArgs(evaluateArgs);
				} else {
					EvaluateArgs = null;
				}
				OrderFuncNo = order;
				if (orderArgs != null) {
					OrderArgs = MethodArgs.SerializeArgs(orderArgs);
				} else {
					OrderArgs = null;
				}
				RestrictionFlag = stop;
			}
			//判定関数名
			public string EvaluateFuncNo;
			//判定関数引数
			public string[] EvaluateArgs;
			//命令関数名
			public string OrderFuncNo;
			//命令関数引数
			public string[] OrderArgs;
			//制限フラグ
			public bool RestrictionFlag;
		}

		/// <summary>
		/// 状態データ
		/// </summary>
		[Serializable]
		public class DecisionStateData
		{
			public DecisionStateData()
			{
				StateNo = null;
				EnterDecision = new List<DecisionData>();
				ExecuteDecision = new List<DecisionData>();
				ExitDecision = new List<DecisionData>();
			}
			public string StateNo;
			public List<DecisionData> EnterDecision;
			public List<DecisionData> ExecuteDecision;
			public List<DecisionData> ExitDecision;
		}

		public static DecisionData Create<T> (Decision<T>.EvaluateFunc evaluateFunc, Decision<T>.OrderFunc orderFunc, bool stop){
			return Create (evaluateFunc, null, orderFunc, null, stop);
		}

		public static DecisionData Create<T> (Decision<T>.EvaluateFunc evaluateFunc, object[] evaluateArgs, Decision<T>.OrderFunc orderFunc, bool stop){
			return Create (evaluateFunc, evaluateArgs, orderFunc, null, stop);
		}

		public static DecisionData Create<T> (Decision<T>.EvaluateFunc evaluateFunc, Decision<T>.OrderFunc orderFunc, object[] orderArgs, bool stop){
			return Create (evaluateFunc, null, orderFunc, orderArgs, stop);
		}

		public static DecisionData Create<T> (Decision<T>.EvaluateFunc evaluateFunc, object[] checkArgs, Decision<T>.OrderFunc orderFunc, object[] orderArgs, bool stop)
		{
			return new DecisionData (evaluateFunc.Method.Name, checkArgs, orderFunc.Method.Name, orderArgs, stop);
		}
	}

}
