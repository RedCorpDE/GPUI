using Sirenix.OdinInspector;
using System;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace GPUI
{
    [System.Serializable]
        [Sirenix.OdinInspector.InlineEditor(InlineEditorModes.GUIOnly)]
        [CreateAssetMenu(menuName = "Skin/Component DataObject")]

    public class ComponentSkinDataObject : SimpleComponentSkinDataObject
    {
        
        [TabGroup("Settings/Color/Colors", "Detail", SdfIconType.QuestionSquare)]
        public ColorBlock detailColor;

        [TabGroup("Settings/Color/Colors", "Detail")]
        public Sprite detailSprite;

        [TabGroup("Settings/Color/Colors", "Detail")]
        public Material detailMaterial;

    }
}