using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace GPUI
{
    [System.Serializable]
        [Sirenix.OdinInspector.InlineEditor]
        [CreateAssetMenu(menuName = "Skin/Toast SkinDataObject")]

    public class UiToastElementSkinDataObject : ComponentSkinDataObject
    {

        [TabGroup("Settings/Color/Colors", "Custom", SdfIconType.Plus)]
        public Sprite toastIcon;

        [HideInTables] public ComponentSkinDataObject closeButtonSkinData;
        [HideInTables] public ComponentSkinDataObject additionalButtonSkinData;

    }
}
