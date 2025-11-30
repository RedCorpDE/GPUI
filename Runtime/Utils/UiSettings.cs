using Hextant;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Sirenix.Serialization;
using Sirenix.OdinInspector;
using NUnit.Framework.Internal;
using System;
using UnityEngine.Serialization;

#if UNITY_EDITOR
using Hextant.Editor;
using UnityEditor;
#endif

namespace GPUI
{
    [Settings(SettingsUsage.RuntimeProject, "GPUI Settings")]

    public sealed class UiSettings : Settings<UiSettings>
    {

#if UNITY_EDITOR
        [SettingsProvider]
        public static SettingsProvider GetSettingsProvider() => instance.GetSettingsProvider();
#endif

        [SerializeField] private UiSkinPalette defaultPalette;

        public UiSkinPalette DefaultPalette
        {
            get
            {

                //If null, check for default palette

                return defaultPalette;
            }
            private set { defaultPalette = value; }
        }

        [FolderPath(ParentFolder = "Assets/Content/GUI/Styles")] [SerializeField]
        private string palettePath;


        public string PalettePath
        {
            get { return palettePath; }
            private set { palettePath = value; }
        }


        private void OnEnable()
        {

#if UNITY_EDITOR
            SaveSettingsToJson();
#else
        if (File.Exists($"{Application.persistentDataPath}/GPUI-Settings.json"))
        {

            string jsonString = File.ReadAllText($"{Application.persistentDataPath}/GPUI-Settings.json");

            JsonUtility.FromJsonOverwrite(jsonString, UiSettings.instance);

        }
        else
        {

            SaveSettingsToJson();

        }
#endif

        }

        [Button("Save Settings")]
        public static void SaveSettingsToJson()
        {

            Debug.Log("Saving Settings");

            string jsonString = JsonUtility.ToJson(instance, true);

            File.WriteAllText($"{Application.persistentDataPath}/GPUI-Settings.json", jsonString);

        }


    }
}