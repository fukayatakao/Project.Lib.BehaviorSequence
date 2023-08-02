using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

namespace Project.Lib {
    /// <summary>
    /// 共用の行動評価関
    /// </summary>
    public static class CommonEvaluate {
		[Function("記録した時間から一定時間過ぎた")]
		[Arg(0, typeof(float), "時間(s)", 0f)]
		public static bool OverTime(CommonBlackboard mem, object[] args) {
			float t = (float)args[0];
			if (mem.TimeStamp + t <= Time.time)
				return true;
			else
				return false;
		}
		[Function("現在時間が記録した時間から一定時間以内")]
		[Arg(0, typeof(float), "時間(s)", 0f)]
		public static bool UnderTime(CommonBlackboard mem, object[] args) {
			float t = (float)args[0];
			if (mem.TimeStamp + t >= Time.time)
				return true;
			else
				return false;
		}
		[Function("フラグが立っているか")]
		[Arg(0, typeof(int), "フラグ番号", 0f)]
		public static bool AffirmativeLocalFlag(CommonBlackboard mem, object[] args) {
			int index = (int)args[0];

			return (mem.LocalFlag & (1 << index)) != 0;
		}
		[Function("フラグが立っていないか")]
		[Arg(0, typeof(int), "フラグ番号", 0f)]
		public static bool NegativeLocalFlag(CommonBlackboard mem, object[] args) {
			int index = (int)args[0];

			return (mem.LocalFlag & (1 << index)) == 0;
		}
	}
}
