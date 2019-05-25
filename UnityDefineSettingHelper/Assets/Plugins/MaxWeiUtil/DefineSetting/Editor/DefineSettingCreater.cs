using UnityEditor;

namespace MWUtil
{
	public class DefineSettingCreater
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
	}
}
