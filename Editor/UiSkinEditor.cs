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
using GPUI;

namespace GPUI.Editor
{
    public class UiSkinEditor : OdinMenuEditorWindow
    {

        [MenuItem("Window/UI Editor")]
        private static void OpenWindow()
        {
            var window = GetWindow<UiSkinEditor>();
            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(800, 600);
        }

        protected override void OnBeginDrawEditors()
        {

            OdinMenuTreeSelection selection = this.MenuTree.Selection;

            SirenixEditorGUI.BeginHorizontalToolbar();
            {

                if (SirenixEditorGUI.ToolbarButton(SdfIconType.Bug, true))
                {
                    Application.OpenURL(
                        "https://studiob12.atlassian.net/jira/software/projects/UDEV/boards/146?issueParent=32818");
                }

                if (SirenixEditorGUI.ToolbarButton("Online Documentation", true))
                {
                    Application.OpenURL(
                        "https://studiob12.atlassian.net/wiki/spaces/GPUI/overview?homepageId=1269956842");
                }

                GUILayout.FlexibleSpace();

                if (SirenixEditorGUI.ToolbarButton(SdfIconType.Trash, true))
                {

                    Object asset = selection.SelectedValue as Object;

                    //Debug.Log(assets.Length);
                    //
                    //foreach (Object obj in assets)
                    //{
//
                    //    Debug.Log(obj.name);
                    //    
                    //    string path = AssetDatabase.GetAssetPath(obj);
//
                    //    AssetDatabase.DeleteAsset(path);
                    //    
                    //}

                    string path = AssetDatabase.GetAssetPath(asset);
                    AssetDatabase.DeleteAsset(path);

                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();

                }

            }
            SirenixEditorGUI.EndHorizontalToolbar();

        }

        protected override OdinMenuTree BuildMenuTree()
        {
            var tree = new OdinMenuTree(true);

            var customMenuStyle = new OdinMenuStyle
            {
                BorderPadding = 0f,
                AlignTriangleLeft = true,
                TriangleSize = 16f,
                TrianglePadding = 0f,
                Offset = 20f,
                Height = 48,
                IconPadding = 0f,
                BorderAlpha = 0.323f
            };
            tree.DefaultMenuStyle = customMenuStyle;
            tree.Config.DrawSearchToolbar = true;

            CreateNewPaletteData tutorialData = new CreateNewPaletteData();
            CreateNewPaletteData newPaletteData = new CreateNewPaletteData();

            tree.Add("Editor Tutorial", tutorialData);
            tree.Add("Create New Palette", newPaletteData);

            tree.Add("Palettes", new DisplayAllPalettes());

            tree.AddAllAssetsAtPath("Palettes", $"{UiSettings.instance.PalettePath.Replace("Assets/", "")}/",
                typeof(UiSkinPalette), true, true);



            foreach (var t in tree.MenuItems[2].ChildMenuItems)
            {

                tree.AddAllAssetsAtPath($"Palettes/{t.Name}",
                    $"Content/GUI/Styles/{(t.Value as UiSkinPalette).displayName}", typeof(ComponentSkinDataObject),
                    true);

            }



            tree.EnumerateTree()
                .AddThumbnailIcons()
                .SortMenuItemsByName();

            return tree;
        }

        private class PaletteMenuItem : OdinMenuItem
        {
            private readonly UiSkinPalette instance;

            public PaletteMenuItem(OdinMenuTree tree, UiSkinPalette instance) : base(tree, instance.displayName,
                instance)
            {
                this.instance = instance;
            }

            protected override void OnDrawMenuItem(Rect rect, Rect labelRect)
            {

            }

            public override string SmartName
            {
                get { return this.instance.displayName; }
            }
        }

        private class CreateNewPaletteData
        {

            [InlineEditor(InlineEditorObjectFieldModes.Hidden)]
            public UiSkinPalette paletteData;

            public CreateNewPaletteData()
            {

                paletteData = ScriptableObject.CreateInstance<UiSkinPalette>();
                paletteData.name = "SKN_UI_Palette_";

            }

            [Button]
            public void SavePalette()
            {

                paletteData.name = $"SKN_UI_Palette_{paletteData.displayName}";

                if (!Directory.Exists($"Assets/Content/GUI/Styles/{paletteData.displayName}"))
                    Directory.CreateDirectory($"Assets/Content/GUI/Styles/{paletteData.displayName}");

                AssetDatabase.CreateAsset(paletteData,
                    $"Assets/Content/GUI/Styles/{paletteData.displayName}/{paletteData.name}.asset");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

            }

            //[Button]
            public void CreateBaseComponentSkins()
            {

                UiDefaultPaletteGenerator.GenerateDefaultPaletteComponents(paletteData.displayName);

                SavePalette();

            }

        }

        private class ComponentSkinDataObjectEditor
        {



        }

        private class DisplayAllPalettes
        {

            [TableList(AlwaysExpanded = true)] public List<UiSkinPalette> allPalettes = new List<UiSkinPalette>();

            public DisplayAllPalettes()
            {

                allPalettes = AssetDatabase
                    .FindAssets($"t:{typeof(UiSkinPalette).Name}")
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .Select(AssetDatabase.LoadAssetAtPath<UiSkinPalette>).ToList();

            }

        }

    }
}

#endif