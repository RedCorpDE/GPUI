#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using UnityEngine;
using UnityEditor;
using System.Linq;
using Sirenix.Utilities;
using System.Collections.Generic;
using Sirenix.Utilities.Editor;
using System.IO;
using Sirenix.OdinInspector;

namespace GPUI.Editor
{
    [System.Serializable]

    public static class UiDefaultPaletteGenerator
    {

        public static List<ComponentSkinDataObject> skinDataObjects = new List<ComponentSkinDataObject>();

        [Button]
        public static void GenerateDefaultPaletteComponents(string paletteName)
        {

            string folder = Application.dataPath + "/Scripts/GUI/Resources/";
            DirectoryInfo d = new DirectoryInfo(folder);

            AssetDatabase.CreateFolder("Assets/Content/GUI/Styles", paletteName);
            AssetDatabase.Refresh();

            foreach (var file in d.GetFiles("*.json"))
            {

                string fileData = File.ReadAllText($"{file.FullName}");
                string newName = $"{file.Name.Replace("Generic", paletteName).Replace(".json", "")}";


                ComponentSkinDataObject componentSkinData =
                    ComponentSkinDataObject.CreateInstance<ComponentSkinDataObject>();
                JsonUtility.FromJsonOverwrite(fileData, componentSkinData);



                AssetDatabase.CreateAsset(componentSkinData,
                    $"Assets/Content/GUI/Styles/{paletteName}/{newName}.asset");
                AssetDatabase.SaveAssets();


            }

            AssetDatabase.Refresh();

        }

        [Button]
        public static void Save()
        {

            string parentFolder = Application.dataPath + "/Content/GUI/Styles/Generic/";
            string saveFolder = Application.dataPath + "/Scripts/GUI/Resources/";
            var subFolders = AssetDatabase.GetSubFolders("Assets/Content/GUI/Styles/Generic");



            Debug.Log($"parentFolder: {parentFolder} || saveFolder: {saveFolder} || subFolders: {subFolders.Length}");

            foreach (var subDir in subFolders)
            {

                Debug.Log(subDir);

                DirectoryInfo d = new DirectoryInfo(subDir);

                foreach (var file in d.GetFiles("*.asset"))
                {

                    ComponentSkinDataObject dataObject =
                        AssetDatabase.LoadAssetAtPath($"{subDir}/{file.Name}", typeof(ComponentSkinDataObject)) as
                            ComponentSkinDataObject;

                    skinDataObjects.Add(dataObject);

                }

            }

            Debug.Log($"skinDataObjects.Count: {skinDataObjects.Count}");

            for (int i = 0; i < skinDataObjects.Count; i++)
            {

                Debug.Log($"skinDataObjects[i]: {skinDataObjects[i].name}");

                string json = JsonUtility.ToJson(skinDataObjects[i], true);

                File.WriteAllText($"{saveFolder}{skinDataObjects[i].name}.json", json);

            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

        }

    }
}

#endif