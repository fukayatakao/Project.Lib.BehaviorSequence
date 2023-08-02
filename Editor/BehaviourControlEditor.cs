using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace Project.Lib {
	public static class BehaviorSequenceEditorSetting {
		public static GUIStyle POPUP_STYLE;

		public const int FONT_SIZE = 16;
		public const float POPUP_SIZE = FONT_SIZE * 1.25f;
	}

	public class BehaviorSequenceEditor : EditorWindow {
		/// <summary>
		/// 編集ダイアログのオープン
		/// </summary>
		[MenuItem("Editor/BehaviourEditor", false, 98)]
		private static void Open() {
			BehaviorSequenceEditor window = EditorWindow.GetWindow<BehaviorSequenceEditor>(false, "BehaviourEditor");
			window.Init();
		}
		/// <summary>
		/// ウィンドウがアクティブになったとき
		/// </summary>
		private void OnEnable() {
			Init();
		}

		private class BehaviourType {
			public System.Type stateType;
			public System.Type entityType;
			public System.Type evaluateClass;
			public System.Type orderClass;

			public string assetPath;
		}

		static Dictionary<string, BehaviourType> behaviourEditDict_ = new Dictionary<string, BehaviourType>()
		{
			{ "character", new BehaviourType{
				stateType = Type.GetType("Project.Game.CharacterThink+State, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"),
				entityType =Type.GetType("Project.Game.CharacterEntity, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"),
				evaluateClass =Type.GetType("Project.Game.CharacterEvaluate, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"),
				orderClass =Type.GetType("Project.Game.CharacterOrder, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"),
				assetPath = "Assets/Application/Addressable/Character/AI/",
			}},
			{ "platoon", new BehaviourType{
				stateType =Type.GetType("Project.Game.PlatoonThink+State, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"),
				entityType =Type.GetType("Project.Game.PlatoonEntity, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"),
				evaluateClass =Type.GetType("Project.Game.PlatoonEvaluate, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"),
				orderClass =Type.GetType("Project.Game.PlatoonOrder, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"),
				assetPath = "Assets/Application/Addressable/Platoon/AI/",
			}},
		};
		Vector2 scroll_;
		int selectIndex_ = 0;


		private List<DecisionMethod> evaluate_;
		private string[] evaluateText_;

		private List<DecisionMethod> order_;
		private string[] orderText_;

		//DecisionMethodをScriptableObjectのロード時に使うとUnityが落ちる（原因不明、Attributeを使っているからか？
		//関数名のリストだけ別に保持してindexの算出に使う
		private List<string> evalueFuncName_;
		private List<string> orderFuncName_;
		/// <summary>
		/// 関数リストを取得
		/// </summary>
		private static List<DecisionMethod> CreateMethodList(System.Type entityType, Type EventClass, out List<string> methodList) {
			methodList = new List<string>();
			List<DecisionMethod> method = new List<DecisionMethod>();
			System.Reflection.MethodInfo[] list = EventClass.GetMethods();
			//関数の引数をチェック
			for (int i = 0, max = list.Length; i < max; i++) {
				System.Reflection.ParameterInfo[] param = list[i].GetParameters();
				//引数は２つで１つ目にEntity、２つ目にobject配列のもののみ正規の関数とみなす
				if (param.Length == 2 && param[0].ParameterType == entityType && param[1].ParameterType == typeof(object[])) {
					methodList.Add(list[i].Name);
					method.Add(new DecisionMethod(list[i]));
				}
			}
			return method;
		}

		BehaviourType currentEditType_;
		int currentEditState_ = 0;
		int copySourceState_ = 0;
		bool editFlag_ = false;
		int startStateNo_ = 0;
		Dictionary<int, BehaviourDecisionState> editData_;


		/// <summary>
		/// 初期化
		/// </summary>
		private void Init() {
			editData_ = new Dictionary<int, BehaviourDecisionState>();

			Setting("character");
		}

		/// <summary>
		/// Entity切り替えしたときの処理
		/// </summary>
		private void Setting(string entityType) {
			//現在選択中のentityの設定を取得する
			currentEditType_ = behaviourEditDict_[entityType];
			//popup用のindexも更新する
			int count = 0;
			foreach(string key in behaviourEditDict_.Keys) {
				if(key == entityType) {
					selectIndex_ = count;
					break;
				}
				count++;
			}

			//評価・実行関数の情報を取得
			evaluate_ = CreateMethodList(currentEditType_.entityType, currentEditType_.evaluateClass, out evalueFuncName_);
			order_ = CreateMethodList(currentEditType_.entityType, currentEditType_.orderClass, out orderFuncName_);

			evaluateText_ = new string[evaluate_.Count];
			for (int i = 0, max = evaluate_.Count; i < max; i++) {
				evaluateText_[i] = evaluate_[i].GetText();
			}
			orderText_ = new string[order_.Count];
			for (int i = 0, max = order_.Count; i < max; i++) {
				orderText_[i] = order_[i].GetText();
			}
		}


		/// <summary>
		/// 表示処理
		/// </summary>
		private void OnGUI() {
			GUI.skin.label.fontSize = BehaviorSequenceEditorSetting.FONT_SIZE;

			BehaviorSequenceEditorSetting.POPUP_STYLE = new GUIStyle (GUI.skin.button);
			BehaviorSequenceEditorSetting.POPUP_STYLE.fontSize = BehaviorSequenceEditorSetting.FONT_SIZE;
			BehaviorSequenceEditorSetting.POPUP_STYLE.alignment = TextAnchor.MiddleLeft;

			GUILayout.Space(5);
			DrawHeaderGUI();
			GUILayout.Space(5);
			using (new GUILayout.HorizontalScope())
			{
				EditorGUI.BeginChangeCheck();
				currentEditState_ = EditorGUILayout.Popup("State Select", currentEditState_, FieldAttribute.GetFields(currentEditType_.stateType));
				if (EditorGUI.EndChangeCheck()) {
					copySourceState_ = currentEditState_;
				}
				EditorGUI.BeginChangeCheck();
				GUILayout.Toggle(currentEditState_ == startStateNo_, "");
				if (EditorGUI.EndChangeCheck())
				{
					startStateNo_ = currentEditState_;
					editFlag_ = true;
				}

				GUILayout.Label("←", GUILayout.Width(32));
				copySourceState_ = EditorGUILayout.Popup(copySourceState_, FieldAttribute.GetFields(currentEditType_.stateType), GUILayout.Width(160));
				if (GUILayout.Button("copy", GUILayout.Width(160)))
				{
					if (editData_.ContainsKey(copySourceState_))
					{
						editData_[currentEditState_] = editData_[copySourceState_].Clone();
					}
				}
			}
			scroll_ = EditorGUILayout.BeginScrollView(scroll_);
			EditorGUI.BeginChangeCheck();
			//選択Stateのリストが未作成の場合はインスタンスを作る
			BehaviourDecisionState state;
			if (!editData_.ContainsKey(currentEditState_)) {
				state = new BehaviourDecisionState();
				editData_[currentEditState_] = state;
			}
			//選択中のstateを取得
			state = editData_[currentEditState_];

			//enter decisionを設定
			state.EnterFolding = DrawSeparater("Enter", state.EnterFolding, state.EnterDecision);
			if (!state.EnterFolding) {
				DrawStateGUI(state.EnterDecision);
			}
			//execute decisionを設定
			state.ExecuteFolding = DrawSeparater("Execute", state.ExecuteFolding, state.ExecuteDecision);
			if (!state.ExecuteFolding) {
				DrawStateGUI(state.ExecuteDecision);
			}

			//exit decisionを設定
			state.ExitFolding = DrawSeparater("Exit", state.ExitFolding, state.ExitDecision);
			if (!state.ExitFolding) {
				DrawStateGUI(state.ExitDecision);
			}

			if (EditorGUI.EndChangeCheck()) {
				editFlag_ = true;
			}

			EditorGUILayout.EndScrollView();

			DrawFooter();

			GUI.skin.label.fontSize = 0;
		}
		/// <summary>
		/// 表示処理
		/// </summary>
		void DrawHeaderGUI() {
			//Entityタイプの設定
			EditorGUI.BeginChangeCheck();

			int oldSelect = selectIndex_;
			selectIndex_ = EditorGUILayout.Popup("Entity", selectIndex_, new List<BehaviourType>(behaviourEditDict_.Values).ConvertAll(x => x.entityType.Name).ToArray());
			//変更有った時
			if (EditorGUI.EndChangeCheck()) {
				if (!editFlag_ || EditorUtility.DisplayDialog("", "データが消える可能性があります", "ok", "cancel")) {
					Setting(new List<string>(behaviourEditDict_.Keys)[selectIndex_]);
					currentEditState_ = 0;
					copySourceState_ = 0;


					editData_ = new Dictionary<int, BehaviourDecisionState>();
					editFlag_ = false;
				} else {
					selectIndex_ = oldSelect;
				}
			}
		}

		/// <summary>
		/// enter,execute,exitを分けるためのセパレータ
		/// </summary>
		bool DrawSeparater(string label, bool folding, List<BehaviourDecision> decisionList) {
			//ちょっと隙間を作る
			GUILayout.Space(5);
			using (new GUILayout.HorizontalScope()) {
				if (GUILayout.Button(folding ? "∨" : "∧", GUILayout.MaxWidth(20f))) {
					folding = !folding;
				}
				GUILayout.Label(label, GUILayout.MaxWidth(80f));

				Color orgColor = GUI.color;
				GUI.color = new Color(0.5f, 0.5f, 0.7f);
				if (GUILayout.Button("<", GUILayout.MaxWidth(20f))) {
					for (int i = 0, max = decisionList.Count; i < max; i++) {
						decisionList[i].EditMode = false;
					}
				}
				GUI.color = new Color(0.2f, 1f, 0.7f);
				if (GUILayout.Button(">", GUILayout.MaxWidth(20f))) {
					for (int i = 0, max = decisionList.Count; i < max; i++) {
						decisionList[i].EditMode = true;
					}
				}
				GUI.color = orgColor;
				EditorUtil.DrawLine();
			}

			return folding;
		}

		BehaviourDecision CreateBehaviourDecision(int evalueIndex = 0, int orderIndex = 0) {
			return BehaviourDecision.Create(evalueIndex, evaluate_[evalueIndex].CreateDefaultValueArray(), orderIndex, order_[orderIndex].CreateDefaultValueArray());
		}
		/// <summary>
		/// Decision一覧表示
		/// </summary>
		void DrawStateGUI(List<BehaviourDecision> decisionList) {
			if (decisionList.Count == 0) {
				if (GUILayout.Button("create", GUILayout.MaxWidth(120f))) {
					decisionList.Add(CreateBehaviourDecision());
				}
				return;
			}
			for (int i = 0, max = decisionList.Count; i < max; i++) {
				BehaviourDecision decision = decisionList[i];
				using (new GUILayout.HorizontalScope()) {
					Color tempColor = GUI.color;
					GUI.color = Color.yellow;
					//リストに対する操作が行われたらループ終わらせる
					//一つ後ろにDecision追加
					if (GUILayout.Button("＋", GUILayout.MaxWidth(20f))) {
						decisionList.Insert(i + 1, CreateBehaviourDecision());
						return;
					}
					//現在のDecision削除
					if (GUILayout.Button("－", GUILayout.MaxWidth(20f))) {
						decisionList.RemoveAt(i);
						return;
					}
					//一つ上に移動
					if (GUILayout.Button("↑", GUILayout.MaxWidth(20f))) {
						if (i > 0) {
							BehaviourDecision d = decisionList[i - 1];
							decisionList[i - 1] = decisionList[i];
							decisionList[i] = d;
							return;
						}
					}
					//一つ下に移動
					if (GUILayout.Button("↓", GUILayout.MaxWidth(20f))) {
						if (i < max - 1) {
							BehaviourDecision d = decisionList[i + 1];
							decisionList[i + 1] = decisionList[i];
							decisionList[i] = d;
							return;
						}
					}
					GUILayout.Label("", GUILayout.MaxWidth(20f));
					GUI.color = decision.EditMode ? new Color(0.5f, 0.5f, 0.7f) : new Color(0.2f, 1f, 0.7f);
					//詳細設定モード
					if (GUILayout.Button(decision.EditMode ? "<" : ">", GUILayout.MaxWidth(20f))) {
						decision.EditMode = !decision.EditMode;
					}
					GUI.color = tempColor;
					//評価関数を設定
					using (new GUILayout.VerticalScope(GUILayout.MaxWidth(400f))) {
						GUILayout.Space(3);
						EditorGUI.BeginChangeCheck();

						//現在選択中の関数説明だけ引数を埋め込む
						int replaceIndex = decision.EvaluateIndex;

						//関数のフォーマットを引数で展開して表示用文字列を生成
						evaluateText_[replaceIndex] = evaluate_[replaceIndex].DecorateText(decision.EvaluateArgs);

						decision.EvaluateIndex = EditorGUILayout.Popup(decision.EvaluateIndex, evaluateText_, BehaviorSequenceEditorSetting.POPUP_STYLE, GUILayout.MaxHeight(BehaviorSequenceEditorSetting.POPUP_SIZE));
						//変更した文字列をオリジナルに戻す
						evaluateText_[replaceIndex] = evaluate_[replaceIndex].GetText();
						//変更があったら現在の入力値を破棄してデフォルト引数を設定する
						if (EditorGUI.EndChangeCheck()) {
							decision.EvaluateArgs = evaluate_[decision.EvaluateIndex].CreateDefaultValueArray();
						}
						//引数の編集モード
						if (decision.EditMode) {
							evaluate_[decision.EvaluateIndex].DrawGUI(ref decision.EvaluateArgs);
						}
					}
					//実行関数を設定
					using (new GUILayout.VerticalScope(GUILayout.MaxWidth(400f))) {
						GUILayout.Space(3);
						EditorGUI.BeginChangeCheck();

						//現在選択中の関数説明だけ引数を埋め込む
						int replaceIndex = decision.OrderIndex;

						//関数のフォーマットを引数で展開して表示用文字列を生成
						orderText_[replaceIndex] = order_[replaceIndex].DecorateText(decision.OrderArgs);

						decision.OrderIndex = EditorGUILayout.Popup(decision.OrderIndex, orderText_, BehaviorSequenceEditorSetting.POPUP_STYLE, GUILayout.MaxHeight(BehaviorSequenceEditorSetting.POPUP_SIZE));
						//変更した文字列をオリジナルに戻す
						orderText_[replaceIndex] = order_[replaceIndex].GetText();
						//変更があったら現在の入力値を破棄してデフォルト引数を設定する
						if (EditorGUI.EndChangeCheck()) {
							decision.OrderArgs = order_[decision.OrderIndex].CreateDefaultValueArray();
						}

						//引数の編集モード
						if (decision.EditMode) {
							order_[decision.OrderIndex].DrawGUI(ref decision.OrderArgs);
						}
					}
					if (GUILayout.Button(decision.IsRestriction ? EditorGUIContent.StopContent() : EditorGUIContent.ThroughContent(), GUILayout.MaxWidth(40f))) {
						decision.IsRestriction = !decision.IsRestriction;
					}
				}
			}
		}
		/// <summary>
		/// 下部のGUI表示
		/// </summary>
		private void DrawFooter() {
			string path = currentEditType_.assetPath;

			//一番下の段でボタン類を表示
			using (new GUILayout.HorizontalScope()) {
				if (GUILayout.Button("Create", GUILayout.Width(160))) {
					if (!editFlag_ || EditorUtility.DisplayDialog("", "データが消える可能性があります", "ok", "cancel")) {
						editData_ = new Dictionary<int, BehaviourDecisionState>();
						editFlag_ = false;
					}

				}
				if (GUILayout.Button("Load", GUILayout.Width(160))) {
					//選択したファイルを取得
					string file = EditorUtility.OpenFilePanelWithFilters("select file", Application.dataPath + path.Substring("Assets".Length), new string[] { "asset", "asset" });
					//ファイルを選んでいたら場合は先頭のプロジェクトまでのパスを削除して"Assets"を入れる
					if (!string.IsNullOrEmpty(file)) {
						string assetFile = "Assets" + file.Substring(Application.dataPath.Length);

						//データをロード
						BehaviorSequenceData data = AssetDatabase.LoadAssetAtPath(assetFile, typeof(ScriptableObject)) as BehaviorSequenceData;
						if (data == null)
							return;

						Load(data);
						editFlag_ = false;
					}
				}
				if (GUILayout.Button("Save", GUILayout.Width(160))) {
					//データをセーブ
					string file = EditorUtility.SaveFilePanel("save file", Application.dataPath + path.Substring("Assets".Length), "new asset", "asset");
					if (string.IsNullOrEmpty(file))
						return;
					Save(file);
					editFlag_ = false;
				}
			}
		}

		/// <summary>
		/// ScriptableObjectから編集用データを生成
		/// </summary>
		private void Load(BehaviorSequenceData data) {
			Setting(data.EntityType);

			editData_ = new Dictionary<int, BehaviourDecisionState>();
			string[] names = System.Enum.GetNames(currentEditType_.stateType);

			for (int i = 0, max = data.StateDataList.Count; i < max; i++) {
				BehaviourDecisionState s = new BehaviourDecisionState();

				s.EnterDecision = ConvertAssetToEdit(data.StateDataList[i].EnterDecision);
				s.ExecuteDecision = ConvertAssetToEdit(data.StateDataList[i].ExecuteDecision);
				s.ExitDecision = ConvertAssetToEdit(data.StateDataList[i].ExitDecision);

				int stateNo = CalcStateIndex(data.StateDataList[i].StateNo, names);
				editData_[stateNo] = s;
			}

			startStateNo_ = CalcStateIndex(data.StartStateNo, names);
			currentEditState_ = startStateNo_;
			copySourceState_ = currentEditState_;
		}
		/// <summary>
		/// アセットデータを編集用データに変換
		/// </summary>
		private List<BehaviourDecision> ConvertAssetToEdit(List<BehaviorSequenceData.DecisionData> decisionAsset) {
			List<BehaviourDecision> decisionList = new List<BehaviourDecision>();
			for (int i = 0, max = decisionAsset.Count; i < max; i++) {
				int evalateIndex = CalcMethodIndex(decisionAsset[i].EvaluateFuncNo, evalueFuncName_);
				object[] evalateArgs = MethodArgs.DeserializeArgs(decisionAsset[i].EvaluateArgs);

				int orderIndex = CalcMethodIndex(decisionAsset[i].OrderFuncNo, orderFuncName_);
				object[] orderArgs = MethodArgs.DeserializeArgs(decisionAsset[i].OrderArgs);
				decisionList.Add(BehaviourDecision.Create(evalateIndex, evalateArgs, orderIndex, orderArgs, decisionAsset[i].RestrictionFlag));
			}

			return decisionList;
		}



		/// <summary>
		/// 編集用データからScriptableObjectにして保存
		/// </summary>
		private void Save(string path) {
			
			BehaviorSequenceData data = ScriptableObject.CreateInstance<BehaviorSequenceData>();
			data.StateDataList = new List<BehaviorSequenceData.DecisionStateData>();
			string[] names = System.Enum.GetNames(currentEditType_.stateType);
			foreach (var stateEdit in editData_) {
				BehaviorSequenceData.DecisionStateData stateAsset = new BehaviorSequenceData.DecisionStateData();

				stateAsset.StateNo = names[stateEdit.Key];
				stateAsset.EnterDecision = ConvertEditToAsset(stateEdit.Value.EnterDecision);
				stateAsset.ExecuteDecision = ConvertEditToAsset(stateEdit.Value.ExecuteDecision);
				stateAsset.ExitDecision = ConvertEditToAsset(stateEdit.Value.ExitDecision);

				data.StateDataList.Add(stateAsset);
			}
			data.StartStateNo = names[startStateNo_];
			data.EntityType = new List<string>(behaviourEditDict_.Keys).ToArray()[selectIndex_];
			//ActEventData data = CreateActEventData(editData);
			//セーブ
			EditorUtility.SetDirty(data);
			string assetFile = "Assets" + path.Substring(Application.dataPath.Length);
			AssetDatabase.CreateAsset(data, assetFile);
			AssetDatabase.SaveAssets();
		}

		/// <summary>
		/// 編集用データからアセットデータへ変換
		/// </summary>
		private List<BehaviorSequenceData.DecisionData> ConvertEditToAsset(List<BehaviourDecision> decisionEdit) {
			List<BehaviorSequenceData.DecisionData> decisionList = new List<BehaviorSequenceData.DecisionData>();
			for (int i = 0, max = decisionEdit.Count; i < max; i++) {
				string evaluateFunc = evalueFuncName_[decisionEdit[i].EvaluateIndex];

				string orderFunc = orderFuncName_[decisionEdit[i].OrderIndex];


				decisionList.Add(new BehaviorSequenceData.DecisionData(evaluateFunc, decisionEdit[i].EvaluateArgs, orderFunc, decisionEdit[i].OrderArgs, decisionEdit[i].IsRestriction));
			}

			return decisionList;
		}

		/// <summary>
		/// State名からIndexを計算
		/// </summary>
		private int CalcStateIndex(string state, string[] names) {
			for (int i = 0, max = names.Length; i < max; i++) {
				if (state == names[i]) {
					return i;
				}
			}
			return -1;
		}
		private int CalcMethodIndex(string method, List<string> methodList) {
			for (int i = 0, max = methodList.Count; i < max; i++) {
				if (method == methodList[i]) {
					return i;
				}
			}
			//関数を削除したときなどで関数名が見つからなかった場合
			Debug.Assert(false, "not found method:" + method);
			return 0;
		}
	}
}
