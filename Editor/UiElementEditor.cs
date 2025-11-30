#if UNITY_EDITOR
using UnityEditor;
using Sirenix.OdinInspector.Editor;

namespace GPUI.Editor
{
    [CustomEditor(typeof(UiElement), true)]
    [CanEditMultipleObjects]
    public class UiElementEditor : OdinEditor
    {

    }
}
#endif