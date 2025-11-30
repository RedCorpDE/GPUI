using System;
using Sirenix.OdinInspector;
using UnityEngine;


namespace GPUI.SubComponents
{
    public class UiSubComponentBase : MonoBehaviour
    {
        
        [ShowInInspector]
        protected UiElement Element => this.GetComponent<UiElement>();
        
        private void OnEnable()
        {
            
            OnInitialize();
            
        }

        protected virtual void OnInitialize()
        {
            
        }
        
    }
}