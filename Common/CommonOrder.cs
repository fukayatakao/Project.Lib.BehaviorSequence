using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

namespace Project.Lib {
    /// <summary>
    /// 共用の行動評価関
    /// </summary>
    public static class CommonOrder {
		[Function("現在時間を記憶する")]
		public static void TimeStamp(CommonBlackboard mem, object[] args) {
			mem.TimeStamp = Time.time;
		}
		[Function("フラグを初期化")]
		public static void ResetLocalFlag(CommonBlackboard mem, object[] args) {
			mem.LocalFlag = 0;
		}
		[Function("フラグを立てる")]
		public static void SetLocalFlag(CommonBlackboard mem, object[] args) {
			int index = (int)args[0];

			mem.LocalFlag |= (1 << index);
		}
		[Function("フラグを降ろす")]
		public static void UnSetLocalFlag(CommonBlackboard mem, object[] args) {
			int index = (int)args[0];
			mem.LocalFlag &= ~(1 >> index);
		}
	}
}

