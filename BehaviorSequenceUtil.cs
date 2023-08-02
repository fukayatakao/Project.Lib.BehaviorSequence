using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;


namespace Project.Lib {
#if UNITY_EDITOR
    /// <summary>
    /// 行動制御データを出力するときの便利関数
    /// </summary>
    public class BehaviorSequenceUtil{
        //デリゲート関数の定義
        private delegate BehaviorSequenceData.DecisionStateData CreateDicision();

        /// <summary>
        /// アセットを生成して出力
        /// </summary>
        public static void Output(string path, string filename, System.Type editClass, string startState) {
            List<CreateDicision> funcList = new List<CreateDicision>();
            //static関数をpublic, privateお構いなしに拾う
            System.Reflection.MethodInfo[] list = editClass.GetMethods(System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            for (int i = 0, max = list.Length; i < max; i++) {
                System.Type ret = list[i].ReturnType;
                System.Reflection.ParameterInfo[] param = list[i].GetParameters();
				//引数なしで返り値がBehaviorSequenceData.DecisionStateDataの関数を拾う
				if (param.Length == 0 && ret == typeof(BehaviorSequenceData.DecisionStateData)) {
                    //delegate関数を作ってDictionaryにキャッシュする
                    funcList.Add((CreateDicision)Delegate.CreateDelegate(typeof(CreateDicision), list[i]));
                }
            }



            //データ出力
            BehaviorSequenceData data = ScriptableObject.CreateInstance<BehaviorSequenceData>();
            data.StartStateNo = startState;
            for(int i = 0, max = funcList.Count; i < max; i++) {
                data.StateDataList.Add(funcList[i]());
            }
            //ステート名でソート
            data.StateDataList.Sort((a, b) =>
            {
                return string.Compare(a.StateNo, b.StateNo);
            });

            //同一のstateが追加されてたらassert出す
            for(int i = 0; i < data.StateDataList.Count; i++) {
                for (int j = i + 1; j < data.StateDataList.Count; j++) {
                    Debug.Assert(data.StateDataList[i].StateNo != data.StateDataList[j].StateNo, "duplicate state no");
                }
            }



            string p = path + filename + ".asset";
            EditorUtility.SetDirty(data);
            AssetDatabase.CreateAsset(data, p);
        }
    }
#endif
}
