using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


namespace Project.Lib {
    /// <summary>
    /// 決定状態クラス
    /// </summary>
    public class DecisionState<T> : IState<T>
	{
#if DEVELOP_BUILD
		//状態の名称表示用
		[SerializeField]
        string StateName;
#endif
		//状態に入ったときに一回だけ処理するリスト
		DecisionList<T> dicisionEnterList_;
		//常時処理するリスト
		DecisionList<T> dicisionExecuteList_;
		//状態から抜けるとき一回だけ処理するリスト
		DecisionList<T> dicisionExitList_;

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public void Init(DecisionList<T> enterList, DecisionList<T> executeList, DecisionList<T> exitList)
		{
			dicisionEnterList_ = enterList;
			dicisionExecuteList_ = executeList;
			dicisionExitList_ = exitList;
        }
        /// <summary>
        /// デバッグ表示用
        /// </summary>
        [System.Diagnostics.Conditional("DEVELOP_BUILD")]
        public void SetStateName(string stateName) {
 #if DEVELOP_BUILD
           StateName = stateName;
#endif
        }
		/// <summary>
		/// 状態に入ったときに実行
		/// </summary>
		public override void Enter(T entity)
		{
			dicisionEnterList_.Execute(entity);
		}
		/// <summary>
		/// 常時実行
		/// </summary>
		public override void Execute(T entity)
		{
			dicisionExecuteList_.Execute(entity);
		}
		/// <summary>
		/// 状態から抜けるときに実行
		/// </summary>
		public override void Exit(T entity)
		{
			dicisionExitList_.Execute(entity);
		}
	}

}
