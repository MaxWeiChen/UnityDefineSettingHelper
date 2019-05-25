using UnityEngine;
using UnityEditor;
using System.IO;

namespace MWUtil
{
	public class ScriptableCreateUtility
	{
		static public void CreateScriptableObject(System.Type type, string path, string fileName, string fileExtension = ".asset")
		{
			string filePath = string.Empty;
			int newIndex = 0;
			int limitCount = 10000;
			do
			{
				limitCount--;
				if(newIndex > 0)
				{
					filePath = Path.Combine(path, fileName + newIndex + fileExtension);
				}
				else
				{
					filePath = Path.Combine(path, fileName + fileExtension);
				}
				newIndex++;

			} while(System.IO.File.Exists(filePath) && limitCount > 0);

			if(System.IO.File.Exists(filePath))
			{
				Debug.LogError("CreateScriptableObject Fail");
				return;
			}

			var table = ScriptableObject.CreateInstance(type);

			AssetDatabase.CreateAsset(table, filePath);
			AssetDatabase.Refresh();
			AssetDatabase.SaveAssets();

			Selection.activeObject = table;
			Debug.Log("CreateScriptableObject Create Done: " + fileName, table);
		}

		static public void CreateScriptableObject<T>(string path, string fileName, string fileExtension = ".asset") where T : ScriptableObject
		{
			CreateScriptableObject(typeof(T), path, fileName, fileExtension);
		}

		static public void GetOrCreateScriptable<T>(string fileName) where T : ScriptableObject
		{
			var scriptable = GetScriptable<T>(fileName);

			if(scriptable != null)
			{
				Selection.activeObject = scriptable;
				Debug.Log("Already Has " + fileName, scriptable);
				return;
			}

			string rootPath = "Assets";
			CreateScriptableObject<T>(rootPath, fileName);
		}

		static public T GetScriptable<T>(string fileName) where T : ScriptableObject
		{
			var guids = AssetDatabase.FindAssets(fileName);

			if(guids.Length <= 0)
			{
				Debug.LogWarning("[GetScriptable] Get Fail, can't find");
				return null;
			}

			var path = AssetDatabase.GUIDToAssetPath(guids[0]);
			var target = AssetDatabase.LoadAssetAtPath<T>(path);
			return target;
		}
	}
}