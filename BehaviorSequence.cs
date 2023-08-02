using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


namespace Project.Lib {
	/// <summary>
	/// 行動制御をするクラス
	/// </summary>
	public class BehaviorSequence<T> {
		//関数デリゲートのキャッシュクラス
		static BehaviorSequenceCache<T> cache_;
		/// <summary>
		/// 関数情報をロードしてキャッシュ
		/// </summary>
		public static void Initiate(System.Type evaluate, System.Type order) {
			Debug.Assert(cache_ == null, "already alloc instance: BehaviorSequence<" + typeof(T) + ">");
			cache_ = new BehaviorSequenceCache<T>(evaluate, order);
		}
		/// <summary>
		/// 関数情報をクリア
		/// </summary>
		public static void Terminate() {
			Debug.Assert(cache_ != null, "already free instance: BehaviorSequence<" + typeof(T) + ">");
			cache_ = null;
		}

		StateMachine<T> stateMachine_ = null;

		public int CurrentState { get { return stateMachine_.CurrentStateNo; } }

		private GameObject gameObject_;
		/// <summary>
		/// コンストラクタ
		/// </summary>
		public BehaviorSequence(GameObject obj) {
			gameObject_ = obj;
		}
		/// <summary>
		/// state変更
		/// </summary>
		public void ChangeState(T owner, int state, bool force = false) {
			stateMachine_.ChangeState(owner, state, force);
		}
		/// <summary>
		/// 現在stateの一致チェック
		/// </summary>
		public bool IsCurrentState(int state) {
			return stateMachine_.CurrentStateNo == state;
		}

		/// <summary>
		/// データから初期化
		/// </summary>
		public void Create<U, V>(BehaviorSequenceData controlData)
			where U : struct
			where V : DecisionState<T>, new() {
			System.Type stateType = typeof(U);
			//UがEnum型でない場合にエラー出す
			Debug.Assert(stateType.IsEnum, "generic type error");
			//初期化されてないときにエラー出す
			Debug.Assert(cache_ != null, "initialize error");

			stateMachine_ = new StateMachine<T>(Enum.GetNames(stateType).Length);
			for (int i = 0, max = controlData.StateDataList.Count; i < max; i++) {

				BehaviorSequenceData.DecisionStateData stateData = controlData.StateDataList[i];

				DecisionList<T> enter = cache_.CreateDecisionList(stateData.EnterDecision);
				DecisionList<T> execute = cache_.CreateDecisionList(stateData.ExecuteDecision);
				DecisionList<T> exit = cache_.CreateDecisionList(stateData.ExitDecision);

				V state = MonoPretender.Create<V>(gameObject_);
				state.Init(enter, execute, exit);
				state.SetStateName(stateData.StateNo);
				stateMachine_.Register(state, (int)Enum.Parse(stateType, stateData.StateNo));
			}

			//最初の状態をセット
			stateMachine_.SetFirstState((int)Enum.Parse(stateType, controlData.StartStateNo));
		}
		/// <summary>
		/// データから初期化
		/// </summary>
		public void Destroy(){
			stateMachine_.UnRegisterAll();
		}

		/// <summary>
		/// イベントの呼び出し
		/// </summary>
		public void Execute(T entity) {
			if (stateMachine_ == null)
				return;
			stateMachine_.Execute(entity);
		}

	}
}
