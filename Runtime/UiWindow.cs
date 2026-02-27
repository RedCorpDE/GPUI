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
    public class UiWindow : UiElement
    {

        [TabGroup("Tabs", "Settings")] public bool useManagerForClosing = false;

        [TabGroup("Tabs", "UI Elements")] 
        public UiText windowTitle;
        
        [TabGroup("Tabs", "UI Elements")] 
        public LocalizedString windowTitleText;

        [TabGroup("Tabs", "UI Elements")] 
        public RectTransform windowTitleGroup;
        
        [TabGroup("Tabs", "UI Elements")] 
        public RectTransform windowBody;
        
        [TabGroup("Events", "OnSetActive")]
        public UnityEvent onSetActive;
        
        [TabGroup("Events", "OnSetInactive")]
        public UnityEvent onSetInactive;
        
        
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

                if (skinData is UiWindowSkinDataObject)
                {

                    if ((skinData as UiWindowSkinDataObject).windowHeaderSkinData != null)
                    {
                        
                        windowTitle.skinData = (skinData as UiWindowSkinDataObject).windowHeaderSkinData;
                        windowTitle.ApplySkinData();

                    }
                    else
                        windowTitle.backgroundGraphic.color = 
                            (skinData as ComponentSkinDataObject).detailColor.normalColor;
                    
                }
                else if (skinData is ComponentSkinDataObject)
                {
                    
                    windowTitle.backgroundGraphic.color = 
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

            if (fadeIn)
            {

                if (setActiveAnimation == null || setActiveAnimation.animation.GetType() == typeof(UiAnimationBase))
                {

                    LMotion.Create(canvasGroup.alpha, 1f, .5f)
                        .WithOnComplete(() =>
                        {
                            canvasGroup.blocksRaycasts = true;
                            canvasGroup.interactable = true;

                            onSetActive?.Invoke();

                        })
                        .BindToAlpha(canvasGroup);

                }
                else
                    setActiveAnimation.Play();

            }
            else
            {

                if (setInactiveAnimation == null || setInactiveAnimation.animation.GetType() == typeof(UiAnimationBase))
                {

                    LMotion.Create(canvasGroup.alpha, 0f, .5f)
                        .WithOnComplete(() =>
                        {
                            canvasGroup.blocksRaycasts = false;
                            canvasGroup.interactable = false;
                            
                            onSetInactive?.Invoke();
                            
                        })
                        .BindToAlpha(canvasGroup);

                }
                else
                    setInactiveAnimation.Play();

            }

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