using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace GPUI
{
    public class UiTextSkinDataObject : ComponentSkinDataObject
    {

        FieldInfo finfoTMPStyleSheet_m_StyleList;
        
        [TabGroup("Settings/Color/Colors", "Custom", SdfIconType.Plus)]
        public TMP_StyleSheet styleSheet;
        [HideInTables]
        List<TMP_Style> lstStyleList;

        [HideInTables]
        [ValueDropdown("GetStyleList")]
        [TabGroup("Settings/Color/Colors", "Custom", SdfIconType.Plus)]
        public string usedTextStyle;
        
        private void OnValidate()
        {
            
            finfoTMPStyleSheet_m_StyleList = typeof(TMP_StyleSheet).GetField("m_StyleList", BindingFlags.Instance | BindingFlags.NonPublic);
            
            var tmpDefaultStyleSheet = TMP_Settings.GetStyleSheet();
            lstStyleList = (List< TMP_Style >) finfoTMPStyleSheet_m_StyleList.GetValue(styleSheet);

            //lstStyleList.Clear();
            //styleSheet.RefreshStyles();
            
        }

        IEnumerable<string> GetStyleList()
        {
            
            return lstStyleList.Select(x => x.name);

        }
        
    }
}
