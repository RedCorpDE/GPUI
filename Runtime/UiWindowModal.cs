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
    public class UiWindowModal : UiWindow
    {

        [TabGroup("Tabs", "UI Elements")] 
        public UiButton closeButton;
        
        [TabGroup("Events", "OnCloseButton")]
        public UnityEvent onCloseButton;
        
        #region Unity Lifecycle

#if UNITY_EDITOR
        protected override void Reset()
        {
            
            base.Reset();
            
            
        }
#endif

        protected override void Awake()
        {
            
            base.Awake();
            
        }

        protected override void OnEnable()
        {
            
            base.OnEnable();
            
            onCloseButton.AddListener(() =>
            {

                if (UiManager.Instance != null && useManagerForClosing)
                    UiManager.Instance.GoToLastWindow();
                else if (UiManager.Instance == null || !useManagerForClosing)
                    FadeElement();


            });

            closeButton?.onClick?.AddListener(() => onCloseButton?.Invoke());
            
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

            if (closeButton != null)
            {
                
                if (skinData is UiWindowModalSkinDataObject)
                {

                    if ((skinData as UiWindowModalSkinDataObject).windowHeaderSkinData != null)
                    {
                        
                        closeButton.skinData = (skinData as UiWindowModalSkinDataObject).windowHeaderSkinData;
                        closeButton.ApplySkinData();

                    }
                    else
                        closeButton.backgroundGraphic.color = 
                            (skinData as ComponentSkinDataObject).detailColor.normalColor;
                    
                }
                else if (skinData is ComponentSkinDataObject)
                {
                    
                    closeButton.backgroundGraphic.color = 
                        (skinData as ComponentSkinDataObject).detailColor.normalColor;
                    
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