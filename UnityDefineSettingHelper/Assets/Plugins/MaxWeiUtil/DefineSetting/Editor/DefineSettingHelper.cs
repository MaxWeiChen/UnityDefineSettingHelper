using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MWUtil
{
	public class DefineSettingHelper
	{
		static private readonly string fileName = "DefineSetting";

		[MenuItem("MWUtil/DefineSetting", false, 0)]
		static public void CreateDefineSetting()
		{
			ScriptableCreateUtility.GetOrCreateScriptable<DefineSetting>(fileName);
		}

		static public DefineSetting FetchDefineSetting()
		{
			return ScriptableCreateUtility.GetScriptable<DefineSetting>(fileName);
		}

		static public void SetDefineFromCommandline()
		{
			Debug.Log("[DefineSettingHelper] SetDefineFromCommandline Start");

			if(!UnityEditorInternal.InternalEditorUtility.inBatchMode)
			{
				Debug.Log("[DefineSettingHelper] SetDefineFromCommandline Fail is not batchMode");
				return;
			}

			var defineSettingFile = FetchDefineSetting();
			if(defineSettingFile == null)
			{
				Debug.LogError("[DefineSettingHelper] SetDefineFromCommandline Fail no Find DefineSetting File");
				return;
			}

			var arguments = new List<string>(System.Environment.GetCommandLineArgs());

			int targetIndex = arguments.FindIndex(a => a == "-target");
			if(targetIndex < 0)
			{
				Debug.LogError("[DefineSettingHelper] SetDefineFromCommandline Fail no param -target");
				return;
			}

			var targetStr = arguments[targetIndex + 1];
			BuildTargetGroup buildTargetGroup = BuildTargetGroup.Standalone;
			if(!Enum.TryParse<BuildTargetGroup>(targetStr, out buildTargetGroup))
			{
				Debug.LogError("[DefineSettingHelper] SetDefineFromCommandline Fail -target: " + targetStr);
				return;
			}

			int definePlatformIndex = arguments.FindIndex(a => a == "-definePlatform");
			if(definePlatformIndex < 0)
			{
				Debug.LogError("[DefineSettingHelper] SetDefineFromCommandline Fail no param -definePlatform");
				return;
			}

			var definePlatformStr = arguments[definePlatformIndex + 1];
			var defineSetting = defineSettingFile.platformSetting.Where((arg) => arg.platform == definePlatformStr).First();
			if(defineSetting == null)
			{
				Debug.LogError("[DefineSettingHelper] SetDefineFromCommandline Fail no Find defineSetting: " + definePlatformStr);
				return;
			}

			SetDefineSymbols(buildTargetGroup, defineSetting);
			Debug.Log("[DefineSettingHelper] SetDefineFromCommandline SUCCESS");
		}

		static public void SetDefineSymbols(BuildTargetGroup buildTargetGroup, PlatformDefineSetting defineSetting)
		{
			string newDef = string.Empty;
			for(int i = 0; i < defineSetting.defines.Length; i++)
			{
				newDef += defineSetting.defines[i] + ";";
			}
			PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, newDef);
		}
	}
}
