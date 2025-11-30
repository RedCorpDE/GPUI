using UnityEngine;
using TMPro;

namespace GPUI
{
    [System.Serializable]
        [Sirenix.OdinInspector.InlineEditor]
        [CreateAssetMenu(menuName = "Skin/Inputfield DataObject")]

    public class UiInputfieldSkinDataObject : ComponentSkinDataObject
    {

        public TMP_InputField.ContentType contentType;

        public int characterLimit;

        public ComponentSkinDataObject placeholderTextSkinDataObject;
        public ComponentSkinDataObject textSkinDataObject;
        public ComponentSkinDataObject helperSkinDataObject;

        public ComponentSkinDataObject icon1SkinDataObject;
        public ComponentSkinDataObject icon2SkinDataObject;

        public ComponentSkinDataObject caretSkinDataObject;

    }
}