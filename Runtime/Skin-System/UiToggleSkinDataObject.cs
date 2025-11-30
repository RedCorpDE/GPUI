using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace GPUI
{
    [System.Serializable]
        [Sirenix.OdinInspector.InlineEditor]
        [CreateAssetMenu(menuName = "Skin/Toggle DataObject")]

    public class UiToggleSkinDataObject : ComponentSkinDataObject
    {

        [TabGroup("Settings/Color/Colors", "Background")] [BoxGroup("Settings/Color/Colors/Background/Pressed")]
        public ColorBlock pressedBackgroundColor;

        [TabGroup("Settings/Color/Colors", "Background")] [BoxGroup("Settings/Color/Colors/Background/Pressed")]
        public Sprite pressedBackgroundSprite;

        [TabGroup("Settings/Color/Colors", "Outline")] [BoxGroup("Settings/Color/Colors/Outline/Pressed")]
        public ColorBlock pressedOutlineColor;

        [TabGroup("Settings/Color/Colors", "Outline")]
        [BoxGroup("Settings/Color/Colors/Outline/Pressed")]
        [Range(0, 100)]
        public int pressedOutlineWidth;

        [TabGroup("Settings/Color/Colors", "Detail")] [BoxGroup("Settings/Color/Colors/Detail/Pressed")]
        public ColorBlock pressedDetailColor;

    }
}