using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditorInternal; // Reoderlist

namespace MWUtil
{
	[CustomEditor(typeof(DefineSetting))]
	public class DefineSettingInspector : Editor
	{
		private DefineSetting root = null;
		private string[] platformSettings = null;
		private int curretPlatformSettingIdx = 0;
		private bool[] originDefineToggles = null;
		private bool[] newDefineToggles = null;
		private PlatformDefineSetting selectedPlatformSetting = null;
		private BuildTargetGroup buildTargetGroup = BuildTargetGroup.Standalone;
		private ReorderableList gameDefineReorderList = null;
		private ReorderableList platformSettingReorderlist = null;
		private bool isShowSelectedPlatformSetting = true;
		private bool isShowSelectedBuildSetting = true;

		private void OnEnable()
		{
			this.root = target as DefineSetting;

			ResetPlatformSettingArray();
			ResetGameDefineToggles();

			var buildTarget = EditorUserBuildSettings.activeBuildTarget;
			buildTargetGroup = BuildPipeline.GetBuildTargetGroup(buildTarget);

			gameDefineReorderList = new ReorderableList(serializedObject, serializedObject.FindProperty("gameDefines"));
			gameDefineReorderList.drawHeaderCallback = (rect) =>
			{
				EditorGUI.LabelField(rect, "Defines of the game");
			};
			gameDefineReorderList.drawElementCallback = (Rect rect, int index, bool selected, bool focused) =>
			{
				SerializedProperty itemData = gameDefineReorderList.serializedProperty.GetArrayElementAtIndex(index);

				rect.y += 2;
				rect.height = EditorGUIUtility.singleLineHeight;
				EditorGUI.PropertyField(rect, itemData, GUIContent.none);
			};
			gameDefineReorderList.onReorderCallback = (list) =>
			{
				UpdateDefineToggles();
			};

			platformSettingReorderlist = new ReorderableList(serializedObject, serializedObject.FindProperty("platformSetting"));
			platformSettingReorderlist.drawHeaderCallback = (rect) =>
			{
				EditorGUI.LabelField(rect, "Multiple PlatformSetting of the game");
			};
			platformSettingReorderlist.drawElementCallback = (Rect rect, int index, bool selected, bool focused) =>
			{
				SerializedProperty itemData = platformSettingReorderlist.serializedProperty.GetArrayElementAtIndex(index);
				var platform = itemData.FindPropertyRelative("platform");

				rect.y += 2;
				rect.height = EditorGUIUtility.singleLineHeight;
				EditorGUI.PropertyField(rect, platform, GUIContent.none);
			};

			platformSettingReorderlist.onReorderCallback = (list) =>
			{
				ResetPlatformSettingArray();
			};

			platformSettingReorderlist.onSelectCallback = (list) =>
			{
				SetNewPlatformSetting(list.index);
			};

			platformSettingReorderlist.index = 0;
		}

		public override void OnInspectorGUI()
		{
			if(EditorApplication.isCompiling)
			{
				EditorGUILayout.LabelField("is Compiling...");
				return;
			}

			DrawGameDefines();
			EditorGUILayout.Separator();
			DrawPlatformSettings();
			EditorGUILayout.Separator();

			DrawSelectedPlatformSetting();
			EditorGUILayout.Separator();
			EditorGUILayout.Separator();
			EditorGUILayout.Separator();
			DrawSelectBuildSetting();
		}

		private void ResetGameDefineToggles()
		{
			originDefineToggles = new bool[root.gameDefines.Length];
			newDefineToggles = new bool[root.gameDefines.Length];
		}

		private void UpdateDefineToggles()
		{
			if(selectedPlatformSetting == null) return;

			for(int i = 0; i < originDefineToggles.Length; i++)
			{
				var def = root.gameDefines[i];
				originDefineToggles[i] = HasString(selectedPlatformSetting.defines, def);
				newDefineToggles[i] = originDefineToggles[i];
			}
		}

		private void ResetPlatformSettingArray()
		{
			curretPlatformSettingIdx = 0;
			platformSettings = new string[root.platformSetting.Length];
			for(int i = 0; i < platformSettings.Length; i++)
			{
				platformSettings[i] = root.platformSetting[i].platform;
			}
		}

		private void SetNewPlatformSetting(int index)
		{
			if(index >= root.platformSetting.Length) return;

			curretPlatformSettingIdx = index;
			var newPlatformSetting = root.platformSetting[index];
			if(selectedPlatformSetting == null || selectedPlatformSetting != newPlatformSetting)
			{
				// New Target Platform, Update information
				selectedPlatformSetting = newPlatformSetting;

				UpdateDefineToggles();
			}
		}

		private bool HasString(string[] strArray, string str)
		{
			for(int i = 0; i < strArray.Length; i++)
			{
				if(strArray[i] == str) return true;
			}

			return false;
		}

		private void DrapTitle(string str)
		{
			EditorGUILayout.Separator();
			EditorGUILayout.LabelField("----- " + str + " -----");
		}

		private void DrawGameDefines()
		{
			EditorGUI.BeginChangeCheck();
			serializedObject.Update();
			gameDefineReorderList.DoLayoutList();
			serializedObject.ApplyModifiedProperties();
			if(EditorGUI.EndChangeCheck())
			{
				ResetGameDefineToggles();
				UpdateDefineToggles();
			}
		}

		private void DrawPlatformSettings()
		{
			EditorGUI.BeginChangeCheck();
			serializedObject.Update();
			platformSettingReorderlist.DoLayoutList();
			serializedObject.ApplyModifiedProperties();
			if(EditorGUI.EndChangeCheck())
			{
				ResetPlatformSettingArray();
				UpdateDefineToggles();
			}
		}

		private void DrawSelectedPlatformSetting()
		{
			if(platformSettings.Length <= 0)
			{
				EditorGUILayout.LabelField("No any PlatformSettings, please add one");
				return;
			}

			EditorGUILayout.BeginHorizontal();
			isShowSelectedPlatformSetting = EditorGUILayout.Foldout(isShowSelectedPlatformSetting, "PlatformSetting:");
			var selectIndex = EditorGUILayout.Popup(curretPlatformSettingIdx, platformSettings);
			SetNewPlatformSetting(selectIndex);
			EditorGUILayout.EndHorizontal();

			if(!isShowSelectedPlatformSetting) return;
			if(selectedPlatformSetting == null)
			{
				EditorGUILayout.LabelField("Please select a PlatformSetting:");
				return;
			}

			EditorGUILayout.LabelField("Description:");
			selectedPlatformSetting.description = EditorGUILayout.TextArea(selectedPlatformSetting.description);

			DrapTitle("Defines of PlatformSetting");
			List<string> resultDefines = new List<string>();
			for(int i = 0; i < newDefineToggles.Length; i++)
			{
				var def = root.gameDefines[i];

				if(originDefineToggles[i] == newDefineToggles[i])
				{
					if(!originDefineToggles[i])
					{
						GUIStyle style = new GUIStyle(GUI.skin.label);
						style.normal.textColor = Color.gray;
						newDefineToggles[i] = EditorGUILayout.ToggleLeft(def, newDefineToggles[i], style);
					}
					else
					{
						newDefineToggles[i] = EditorGUILayout.ToggleLeft(def, newDefineToggles[i]);
					}
				}
				else
				{
					if(newDefineToggles[i])
					{
						GUIStyle style = new GUIStyle(GUI.skin.label);
						style.normal.textColor = Color.blue;
						newDefineToggles[i] = EditorGUILayout.ToggleLeft(def + " (Add)", newDefineToggles[i], style);
					}
					else
					{
						GUIStyle style = new GUIStyle(GUI.skin.label);
						style.normal.textColor = Color.red;
						newDefineToggles[i] = EditorGUILayout.ToggleLeft(def + " (Remove)", newDefineToggles[i], style);
					}
				}

				if(newDefineToggles[i])
				{
					resultDefines.Add(def);
				}
			}

			// Missing Define
			for(int i = 0; i < selectedPlatformSetting.defines.Length; i++)
			{
				var def = selectedPlatformSetting.defines[i];

				if(!HasString(root.gameDefines, def))
				{
					GUIStyle style = new GUIStyle(GUI.skin.label);
					style.normal.textColor = Color.magenta;
					EditorGUILayout.LabelField(def + " (Missing)", style);
				}
			}

			if(GUILayout.Button("Confirm to Save"))
			{
				selectedPlatformSetting.defines = resultDefines.ToArray();
				UpdateDefineToggles();
				EditorUtility.SetDirty(root);
			}
		}

		private void DrawSelectBuildSetting()
		{
			EditorGUILayout.BeginHorizontal();
			isShowSelectedBuildSetting = EditorGUILayout.Foldout(isShowSelectedBuildSetting, "BuildSetting:");
			buildTargetGroup = (BuildTargetGroup)EditorGUILayout.EnumPopup(buildTargetGroup);
			EditorGUILayout.EndHorizontal();

			if(!isShowSelectedBuildSetting) return;

			if(selectedPlatformSetting == null)
			{
				EditorGUILayout.LabelField("Please select a PlatformSetting:");
				return;
			}

			var buildDefStr = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
			var buildDefs = buildDefStr.Split(';');
			DrapTitle("Defines of BuildSetting");
			if(string.IsNullOrEmpty(buildDefStr))
			{
				EditorGUILayout.LabelField("No Define");
			}
			else
			{
				for(int i = 0; i < buildDefs.Length; i++)
				{
					var def = buildDefs[i];
					if(HasString(selectedPlatformSetting.defines, def))
					{
						EditorGUILayout.LabelField(buildDefs[i]);
					}
					else
					{
						GUIStyle style = new GUIStyle(GUI.skin.label);
						style.normal.textColor = Color.red;
						EditorGUILayout.LabelField(buildDefs[i] + " (remove)", style);
					}
				}
			}

			DrapTitle("After Update Defines");
			bool hasChange = false;
			if(selectedPlatformSetting.defines.Length <= 0)
			{
				EditorGUILayout.LabelField("No Define");
			}
			else
			{
				for(int i = 0; i < selectedPlatformSetting.defines.Length; i++)
				{
					var def = selectedPlatformSetting.defines[i];
					if(HasString(buildDefs, def))
					{
						EditorGUILayout.LabelField(selectedPlatformSetting.defines[i]);
					}
					else
					{
						GUIStyle style = new GUIStyle(GUI.skin.label);
						style.normal.textColor = Color.blue;
						EditorGUILayout.LabelField(selectedPlatformSetting.defines[i] + " (new)", style);
					}
				}
			}

			EditorGUI.BeginDisabledGroup(hasChange);
			if(GUILayout.Button("Update Defines"))
			{
				DefineSettingHelper.SetDefineSymbols(buildTargetGroup, selectedPlatformSetting);
			}
			EditorGUI.EndDisabledGroup();
		}
	}
}
