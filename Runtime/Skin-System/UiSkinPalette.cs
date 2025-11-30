using System;
using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using System.Linq;
using System.Reflection;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GPUI
{
    [CreateAssetMenu(fileName = "", menuName = "Skin/Skin Palette")]

    public class UiSkinPalette : ScriptableObject
    {

        [HorizontalGroup("$displayName")] [VerticalGroup("$displayName/2")]
        public string displayName;

        [VerticalGroup("$displayName/2")] public TMP_StyleSheet textStyleSheet;
        
        FieldInfo finfoTMPStyleSheet_m_StyleList;
        [VerticalGroup("$displayName/2")]
        public List<TMP_Style> textStyleSheets = new List<TMP_Style>();

        [VerticalGroup("$displayName/2")] [BoxGroup("$displayName/2/General Settings")] [Range(1, 10)]
        public int shapeRoundnessFactor = 10;

        [VerticalGroup("$displayName/2"), SerializeField] [BoxGroup("$displayName/2/General Settings")]
        private UiSwatchesContainer swatchesContainer;
        
        [VerticalGroup("$displayName/2"), SerializeField] [BoxGroup("$displayName/2/General Settings")]
        public List<Sprite> cursorSprites = new List<Sprite>();

        [VerticalGroup("$displayName/3")] [ReadOnly, ShowInInspector]
        List<string> skinCategoryNames = new List<string>();

        List<UiSkinCategory> skinCategories = new List<UiSkinCategory>();

        [TableList(AlwaysExpanded = true), PropertyOrder(5000)]
        public List<SimpleComponentSkinDataObject> skinDataObjects = new List<SimpleComponentSkinDataObject>();
        



        private void OnValidate()
        {

#if UNITY_EDITOR

            FindAllSkinDataObjects();

#endif

            skinCategoryNames.Clear();
            skinCategories.Clear();

            for (int i = 0; i < skinDataObjects.Count; i++)
            {

                string category;

                GenerateCategoryName(out category, skinDataObjects[i].name);

                if (skinCategories.Contains(skinCategories.Find(x => x.name == category)))
                {

                    int index = skinCategories.IndexOf(skinCategories.Find(x => x.name == category));

                    if (index != -1)
                        skinCategories[index].skinDataObjects.Add(skinDataObjects[i]);

                }
                else
                {

                    UiSkinCategory uiSkinCategory = new UiSkinCategory() { name = category };
                    uiSkinCategory.skinDataObjects.Add(skinDataObjects[i]);

                    skinCategories.Add(uiSkinCategory);
                    skinCategoryNames.Add(uiSkinCategory.name);

                }

            }

            if (textStyleSheet != null)
            {
                
                finfoTMPStyleSheet_m_StyleList = typeof(TMP_StyleSheet).GetField("m_StyleList", BindingFlags.Instance | BindingFlags.NonPublic);
                
                textStyleSheets = (List< TMP_Style >) finfoTMPStyleSheet_m_StyleList.GetValue(textStyleSheet);
                //textStyleSheets.Clear();
                //textStyleSheet.RefreshStyles();
                
            }

        }

        void FindAllSkinDataObjects()
        {

            skinDataObjects.Clear();

#if UNITY_EDITOR
            
            List<SimpleComponentSkinDataObject> allSkinComponentDataObjects =
                AssetDatabase.FindAssets($"t:{typeof(SimpleComponentSkinDataObject).Name}")
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .Select(AssetDatabase.LoadAssetAtPath<SimpleComponentSkinDataObject>).ToList();

            skinDataObjects = (from componentSkinDataObject in allSkinComponentDataObjects
                where componentSkinDataObject.name.Contains(displayName)
                select componentSkinDataObject).ToList();
            
#endif

        }

        string GenerateCategoryName(out string categoryName, string fullName)
        {

            string[] splitName = fullName.Split('_');

            if (splitName.Length > 3)
                categoryName = splitName[3];
            else
                categoryName = fullName;

            return categoryName;

        }

        string GenerateTypeName(out string typeName, string fullName)
        {

            string[] splitName = fullName.Split('_');

            if (splitName.Length > 4)
                typeName = splitName[4];
            else
                typeName = fullName;

            return typeName;

        }

        public void GetSkinData(out SimpleComponentSkinDataObject returnedSkinData, SimpleComponentSkinDataObject currentSkinData)
        {

            returnedSkinData = null;

            if (currentSkinData == null)
                return;

            string categoryName;
            GenerateCategoryName(out categoryName, currentSkinData.name);

            string typeName;
            GenerateTypeName(out typeName, currentSkinData.name);

            UiSkinCategory foundCategory = null;

            foundCategory = skinCategories.Find(x => x.name.Contains(categoryName));

            if (foundCategory != null)
            {

                returnedSkinData = foundCategory.skinDataObjects
                    .Find(y => y.name.Contains(typeName));

                if (returnedSkinData == null)
                {

                    returnedSkinData = foundCategory.skinDataObjects.Find(y => y.name.Contains("Fallback"));

                    if (returnedSkinData == null)
                    {

                        foundCategory =
                            UiSettings.instance.DefaultPalette.skinCategories.Find(x => x.name.Contains(categoryName));

                        returnedSkinData = foundCategory.skinDataObjects.Find(y => y.name.Contains("Fallback"));

                    }

                }

            }
            else
            {

                foundCategory =
                    UiSettings.instance.DefaultPalette.skinCategories.Find(x => x.name.Contains(categoryName));

                returnedSkinData = foundCategory.skinDataObjects.Find(y => y.name.Contains("Fallback"));

            }

        }


        private class UiSkinCategory
        {

            public string name;

            public List<SimpleComponentSkinDataObject> skinDataObjects = new List<SimpleComponentSkinDataObject>();

        }

        [Serializable]
        private class UiSwatchesContainer
        {

            public List<Color> colors = new List<Color>();

        }

    }
}