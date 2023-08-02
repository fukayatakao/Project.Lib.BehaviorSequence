using System;
using System.Collections;
using System.Collections.Generic;


namespace Project.Lib {
    /// <summary>
    /// 判断と実行を優先度順に実行する
    /// </summary>
    public class DecisionList<T>
	{
		private List<Decision<T>> decisionList_ = new List<Decision<T>>();

		/// <summary>
		/// 追加
		/// </summary>
		public void Add(Decision<T> decision)
		{
			decisionList_.Add (decision);
		}

		/// <summary>
		/// 判定と実行
		/// </summary>
		public bool Execute(T entity)
		{
			int max = decisionList_.Count;
			//判定を順番に実行
			for(int i = 0; i < max; i++)
			{
				if (!decisionList_[i].Evaluate (entity, decisionList_[i].EvaluateArgs)) {
					continue;
				}
				//判定通過したら実行処理を行う
				decisionList_[i].Order (entity, decisionList_[i].OrderArgs);
				//制限フラグが立っている場合は判定終わり
				if(decisionList_[i].IsRestriction)
					return true;
			}
			return false;
		}
	}

}
