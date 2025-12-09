using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Localization;


namespace GPUI
{
    
    /// <summary>
    /// Custom Element for Windows and base for complex elements like modals. 
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class UiWindowModal : UiElement
    {

        [TabGroup("Tabs", "UI Elements")] 
        public UiText windowTitle;
        
        [TabGroup("Tabs", "UI Elements")] 
        public LocalizedString windowTitleText;
        
        
        [TabGroup("Tabs", "UI Skin", SdfIconType.FileEarmarkMedical)]
        [AssetSelector(Paths = "Assets/Content/GUI/Styles/")]
        public SimpleComponentSkinDataObject titleSkinDataObject;
        
        #region Unity Lifecycle

        protected override void Reset()
        {
            
            base.Reset();
            
        }

        protected override void Awake()
        {
            
            base.Awake();
            
        }

        protected override void OnEnable()
        {
            
            base.OnEnable();
            
        }
        
        protected override void OnDisable()
        {
            
            base.OnDisable();
            
        }
        
        protected override void Start()
        {
            
            base.Start();
            
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            
            base.OnValidate();
            
        }
#endif

        #endregion
        
        #region Component Caching
        
        protected override void CacheComponents()
        {
            
            base.CacheComponents();
            
        }

        #endregion
        
        #region Apply Skin
        
        /// <summary>
        /// Assigns skin values to this element.
        /// </summary>
        public override void ApplySkinData()
        {

            base.ApplySkinData();

            if (skinData == null)
                return;

            if (windowTitle != null)
            {
                
                windowTitle.LocalizedString = windowTitleText;

                if (titleSkinDataObject == null && skinData is ComponentSkinDataObject)
                {
                    
                    windowTitle.backgroundGraphic.color = (skinData as ComponentSkinDataObject).detailColor.normalColor;
                    
                }
                else if (titleSkinDataObject != null)
                {
                    
                    windowTitle.skinData = titleSkinDataObject;
                    windowTitle.ApplySkinData();
                    
                }

            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);

        }

        /// <summary>
        /// Hook for shape components (rounded corners, etc).
        /// Implement in subclasses or attach custom components that read from the skin.
        /// </summary>
        public override void ApplyShape()
        {
            
            base.ApplyShape();
            
        }
        
        public override void ApplyLayout(RectTransform layoutRectTransform, SimpleComponentSkinDataObject.UiElementLayoutOptions layoutOptions)
        {

           base.ApplyLayout(layoutRectTransform, layoutOptions);
            
        }
        
        /// <summary>
        /// Override default state transition to also update outline based on skin.
        /// </summary>
        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            
            base.DoStateTransition(state, instant);

            if (skinData == null)
                return;
            
            if(windowTitle == null)
                return;
            
            Color targetDetailColor = (skinData as ComponentSkinDataObject).detailColor.normalColor;

            switch (state)
            {
                case SelectionState.Normal:
                    
                    if(titleSkinDataObject == null)
                        targetDetailColor = (skinData as ComponentSkinDataObject).detailColor.normalColor;
                    else
                        targetDetailColor = titleSkinDataObject.backgroundColor.normalColor;
                    
                    break;
                case SelectionState.Highlighted:
                    
                    if(titleSkinDataObject == null)
                        targetDetailColor = (skinData as ComponentSkinDataObject).detailColor.highlightedColor;
                    else
                        targetDetailColor = titleSkinDataObject.backgroundColor.highlightedColor;
                    
                    break;
                case SelectionState.Pressed:
                    
                    if(titleSkinDataObject == null)
                        targetDetailColor = (skinData as ComponentSkinDataObject).detailColor.pressedColor;
                    else
                        targetDetailColor = titleSkinDataObject.backgroundColor.pressedColor;
                    
                    break;
                case SelectionState.Selected:
                   
                    if(titleSkinDataObject == null)
                        targetDetailColor = (skinData as ComponentSkinDataObject).detailColor.selectedColor;
                    else
                        targetDetailColor = titleSkinDataObject.backgroundColor.selectedColor;
                    
                    break;
                case SelectionState.Disabled:
                    
                    if(titleSkinDataObject == null)
                        targetDetailColor = (skinData as ComponentSkinDataObject).detailColor.disabledColor;
                    else
                        targetDetailColor = titleSkinDataObject.backgroundColor.disabledColor;
                    
                    break;
            }

            windowTitle.backgroundGraphic.color = targetDetailColor;
            windowTitle.LocalizedString = windowTitleText;

        }
        
        #endregion
        
        #region API
        
        public override void FadeElement(bool fadeIn = false)
        {

            base.FadeElement(fadeIn);

        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            
            base.OnPointerUp(eventData);
            
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            
            base.OnPointerEnter(eventData);
            
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            
            base.OnPointerExit(eventData);
         
        }

        public override void OnResetElement()
        {
            
            base.OnResetElement();
            
        }
        
        #endregion
        
    }
}