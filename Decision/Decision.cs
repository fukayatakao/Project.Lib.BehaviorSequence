using System;
using System.Collections;
using System.Collections.Generic;


namespace Project.Lib {

    /// <summary>
    /// 決定処理
    /// </summary>
    /// <remarks>
    /// 判定と命令関数をセットにしたクラス
    /// </remarks>
    public class Decision<T>
	{
		public delegate bool EvaluateFunc(T entity, object[] args); 
		public delegate void OrderFunc(T entity, object[] args); 

		//判定が真だったときに命令を行った後ほかの決定を制限するか
		public bool IsRestriction;
		//判定関数
		public EvaluateFunc Evaluate;
		//判定関数引数
		public object[] EvaluateArgs;
		//命令関数
		public OrderFunc Order;
		//命令関数引数
		public object[] OrderArgs;
		/// <summary>
		/// コンストラクタ
		/// </summary>
		public Decision(EvaluateFunc e, object[] ea, OrderFunc o, object[] oa, bool r)
		{
			Evaluate = e;
			EvaluateArgs = ea;
			Order = o;
			OrderArgs = oa;
			IsRestriction = r;
		}
		/// <summary>
		/// コンストラクタ
		/// </summary>
		public Decision(EvaluateFunc e, OrderFunc o, bool r)
		{
			Evaluate = e;
			EvaluateArgs = null;
			Order = o;
			OrderArgs = null;
			IsRestriction = r;
		}
		/// <summary>
		/// コンストラクタ
		/// </summary>
		public Decision(EvaluateFunc e, object[] ea, OrderFunc o, bool r)
		{
			Evaluate = e;
			EvaluateArgs = ea;
			Order = o;
			OrderArgs = null;
			IsRestriction = r;
		}
		/// <summary>
		/// コンストラクタ
		/// </summary>
		public Decision(EvaluateFunc e, OrderFunc o, object[] oa, bool r)
		{
			Evaluate = e;
			EvaluateArgs = null;
			Order = o;
			OrderArgs = oa;
			IsRestriction = r;
		}
	};



}
