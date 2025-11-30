using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GPUI
{
    [System.Serializable]
        [Sirenix.OdinInspector.InlineEditor]
        [CreateAssetMenu(menuName = "Skin/UI-Window (Modal) DataObject")]

    public class UiWindowModalSkinDataObject : UiWindowSkinDataObject
    {

        [HideInTables] public ComponentSkinDataObject closeButtonSkinData;

    }
}