using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


namespace Project.Lib {
	/// <summary>
	/// BehaviorSequenceで使用する関数キャッシュ
	/// </summary>
	public class BehaviorSequenceCache<T>
	{
		//チェック用関数の一覧
		Dictionary<string, Decision<T>.EvaluateFunc> evaluateMethods_;
		//指示用関数の一覧
		Dictionary<string, Decision<T>.OrderFunc> orderMethods_;
		/// <summary>
		/// クラス内にある関数のキャッシュを作る
		/// </summary>
		public BehaviorSequenceCache(Type EvaluateFuncClass, Type OrderFuncClass)
		{
			evaluateMethods_ = new Dictionary<string, Decision<T>.EvaluateFunc> ();
			orderMethods_ = new Dictionary<string, Decision<T>.OrderFunc> ();
			//この辺は重くなりそうなとAIすべてで共通なので全体で１回だけ行って結果を保持する
			{
				System.Reflection.MethodInfo[] list = EvaluateFuncClass.GetMethods ();
				for (int i = 0, max = list.Length; i < max; i++) {
					System.Reflection.ParameterInfo[] param = list[i].GetParameters();
					if(param.Length == 2 && param[0].ParameterType == typeof(T) && param[1].ParameterType == typeof(object[])){
						//delegate関数を作ってDictionaryにキャッシュする
						Decision<T>.EvaluateFunc evaluate = (Decision<T>.EvaluateFunc)Delegate.CreateDelegate (typeof(Decision<T>.EvaluateFunc), list[i]);
						evaluateMethods_.Add (list [i].Name, evaluate);
					}
				}
			}
			{
				System.Reflection.MethodInfo[] list = OrderFuncClass.GetMethods ();
				for (int i = 0, max = list.Length; i < max; i++) {
					System.Reflection.ParameterInfo[] param = list[i].GetParameters();
					if (param.Length == 2 && param[0].ParameterType == typeof(T) && param[1].ParameterType == typeof(object[])) {
						Decision<T>.OrderFunc order = (Decision<T>.OrderFunc)Delegate.CreateDelegate(typeof(Decision<T>.OrderFunc), list[i]);
						orderMethods_.Add(list[i].Name, order);
					}
				}
			}

		}
		/// <summary>
		/// Decisionのリストデータを作成
		/// </summary>
		public DecisionList<T> CreateDecisionList(List<BehaviorSequenceData.DecisionData> decisionData)
		{
			//状態開始時に行う処理
			DecisionList<T> list = new DecisionList<T> ();
			for (int i = 0, max = decisionData.Count; i < max; i++) {
				//Dictionaryのkeyに登録されてなかったら何が登録されてなかったかを明示的にエラーを出す(Dictionaryのエラーだけだと何がエラーを起こしてるか分からないので)
				Debug.Assert (evaluateMethods_.ContainsKey (decisionData [i].EvaluateFuncNo), "dictionary key not found: " + decisionData [i].EvaluateFuncNo.ToString ());
				Debug.Assert (orderMethods_.ContainsKey (decisionData [i].OrderFuncNo), "dictionary key not found: " + decisionData [i].OrderFuncNo.ToString ());

				Decision<T>.EvaluateFunc evaluate = evaluateMethods_ [decisionData [i].EvaluateFuncNo];
				object[] evaluateArgs = MethodArgs.DeserializeArgs(decisionData [i].EvaluateArgs);

				Decision<T>.OrderFunc order = orderMethods_ [decisionData [i].OrderFuncNo];
				object[] orderArgs = MethodArgs.DeserializeArgs(decisionData [i].OrderArgs);

				list.Add (new Decision<T> (evaluate, evaluateArgs, order, orderArgs, decisionData [i].RestrictionFlag));
			}

			return list;
		}


	}
}
