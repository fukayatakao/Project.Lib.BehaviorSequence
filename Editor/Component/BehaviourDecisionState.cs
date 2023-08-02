using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;


namespace Project.Lib {
	public class BehaviourDecisionState {
		public bool EnterFolding = false;
		public bool ExecuteFolding = false;
		public bool ExitFolding = false;

		public List<BehaviourDecision> EnterDecision = new List<BehaviourDecision>();
		public List<BehaviourDecision> ExecuteDecision = new List<BehaviourDecision>();
		public List<BehaviourDecision> ExitDecision = new List<BehaviourDecision>();

		/// <summary>
		/// ディープコピーする
		/// </summary>
		public BehaviourDecisionState Clone() {
			BehaviourDecisionState instance = new BehaviourDecisionState();
			instance.EnterFolding = EnterFolding;
			instance.ExecuteFolding = ExecuteFolding;
			instance.ExitFolding = ExitFolding;

			//リストの要素をコピーコンストラクタを使ってディープコピー
			for(int i = 0, max = EnterDecision.Count; i < max; i++) {
				instance.EnterDecision.Add(new BehaviourDecision(EnterDecision[i]));
			}
			for (int i = 0, max = ExecuteDecision.Count; i < max; i++) {
				instance.ExecuteDecision.Add(new BehaviourDecision(ExecuteDecision[i]));
			}
			for (int i = 0, max = ExitDecision.Count; i < max; i++) {
				instance.ExitDecision.Add(new BehaviourDecision(ExitDecision[i]));
			}

			return instance;
		}
	}
}
