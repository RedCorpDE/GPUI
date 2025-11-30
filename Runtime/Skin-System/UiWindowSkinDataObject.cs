using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GPUI
{
    [System.Serializable]
        [Sirenix.OdinInspector.InlineEditor]
        [CreateAssetMenu(menuName = "Skin/UI-Window DataObject")]

    public class UiWindowSkinDataObject : ComponentSkinDataObject
    {

        [HideInTables] public ComponentSkinDataObject windowHeaderSkinData;
        
        [TabGroup("Settings/Layout/LayoutSettings", "Body"), ShowIf("useLayoutOptions")]
        public UiElementLayoutOptions bodyLayoutOptions;
        
        
        [TabGroup("Settings/Layout/LayoutSettings", "Header"), ShowIf("useLayoutOptions")]
        public UiElementLayoutOptions headerLayoutOptions;


    }
}