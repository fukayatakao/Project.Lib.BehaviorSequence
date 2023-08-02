using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

namespace Project.Lib {
    /// <summary>
    /// 汎用で使う一時変数を入れるクラス
    /// </summary>
	public class CommonBlackboard : MonoPretender{
		//タイムスタンプ
		public float TimeStamp;
        //ローカルフラグ
		public int LocalFlag;
        //グローバルフラグ
		public int GlobalFlag;
	}
}
