﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ProBuilder.Core;
using UnityEditor;
using UnityEngine;

// todo
using UnityEditor.GuidRemap;

namespace ProBuilder.EditorCore
{
	class pb_RemapGuids : EditorWindow
	{
		TextAsset m_RemapFile = null;

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Repair/Convert to Package Manager")]
		static void MenuInitRemapGuidEditor()
		{
			GetWindow<pb_RemapGuids>(true, "Package Manager Conversion Utility", true);
		}

		void OnGUI()
		{
			GUILayout.Label("Abandon hope, all ye who enter", EditorStyles.boldLabel);

			m_RemapFile = (TextAsset) EditorGUILayout.ObjectField("Remap File", m_RemapFile, typeof(TextAsset), false);

			SerializationMode serializationMode = EditorSettings.serializationMode;

			GUI.enabled = serializationMode == SerializationMode.ForceText;

			if (GUILayout.Button("Convert to Package Manager"))
				DoIt();

			GUI.enabled = true;
		}

		void DoIt()
		{
			GuidRemapObject remapObject = new GuidRemapObject();
			JsonUtility.FromJsonOverwrite(m_RemapFile.text, remapObject);

			string[] extensionsToScanForGuidRemap = new string[]
			{
				"*.meta",
				"*.asset",
				"*.mat",
				"*.unity",
			};

			string assetStoreProBuilderDirectory = pb_FileUtil.FindAssetStoreProBuilderInstall();

			if (string.IsNullOrEmpty(assetStoreProBuilderDirectory))
			{
				// todo pop up modal dialog asking user to point to ProBuilder directory (and validate before proceeding)
				Debug.LogWarning("Couldn't find an Asset Store install of ProBuilder. Aborting conversion process.");
				return;
			}

			foreach (string extension in extensionsToScanForGuidRemap)
			{
				foreach (string str in Directory.GetFiles("Assets", extension, SearchOption.AllDirectories))
					DoAssetIdentifierRemap(str, remapObject.map);
			}
		}

		static void DoAssetIdentifierRemap(string path, IEnumerable<AssetIdentifierTuple> map)
		{
			var sr = new StreamReader(path);
			var sw = new StreamWriter(path + ".remap", false);

			List<pb_Tuple<string, string>> replace = new List<pb_Tuple<string, string>>();

			// order is important - {fileId, guid} in asset files needs to be applied first
			IEnumerable<AssetIdentifierTuple> assetIdentifierTuples = map as AssetIdentifierTuple[] ?? map.ToArray();

			foreach(var kvp in assetIdentifierTuples)
				replace.Add( new pb_Tuple<string, string>(
					string.Format("{{fileId: {0}, guid: {1}, type:", kvp.source.fileId, kvp.source.guid),
					string.Format("{{fileId: {0}, guid: {1}, type:", kvp.destination.fileId, kvp.destination.guid)));

			foreach(var kvp in assetIdentifierTuples)
				replace.Add(new pb_Tuple<string, string>(
					string.Format("guid: {0}", kvp.source.guid),
					string.Format("guid: {0}", kvp.destination.guid)));

			int modified = 0;

			while (sr.Peek() > -1)
			{
				var line = sr.ReadLine();

				foreach (var kvp in replace)
				{
					if (line.Contains(kvp.Item1))
					{
						modified++;
						line = line.Replace(kvp.Item1, kvp.Item2);
						break;
					}
				}

				sw.WriteLine(line);
			}

			sr.Close();
			sw.Close();

			if (modified > 0)
			{
				File.Delete(path);
				File.Move(path + ".remap", path);
			}
		}
	}
}