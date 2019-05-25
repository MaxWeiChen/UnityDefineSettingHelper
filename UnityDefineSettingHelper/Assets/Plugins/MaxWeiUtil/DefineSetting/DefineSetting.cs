using UnityEngine;

namespace MWUtil
{
	public class DefineSetting : ScriptableObject
	{
		[Header("遊戲所有的Defines"), SerializeField]
		public string[] gameDefines = new string[0];

		[SerializeField]
		public PlatformDefineSetting[] platformSetting = new PlatformDefineSetting[0];
	}

	[System.Serializable]
	public class PlatformDefineSetting
	{
		public string platform;
		public string description;
		public string[] defines;
	}
}
