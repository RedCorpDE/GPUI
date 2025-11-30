using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GPUI
{

    public class UiButton : UiElementExtended
    {
        
        public enum ButtonBehavior
        {
            Normal,
            PressOnce,
        }
        
        [TabGroup("Tabs", "Settings", SdfIconType.HandIndex)]
        [SerializeField]
        private ButtonBehavior behavior = ButtonBehavior.Normal;

        [TabGroup("Tabs", "Settings", SdfIconType.HandIndex)]
        [ShowInInspector, ReadOnly, LabelText("Has Been Pressed")]
        private bool hasBeenPressed;
        
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

        public override void OnPointerClick(PointerEventData eventData)
        {
            
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            if (!IsActive() || !IsInteractable())
                return;

            HandleClickBehavior();

            base.OnPointerClick(eventData);
            
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
            
            bool hadState = hasBeenPressed;

            hasBeenPressed = false;
                
            interactable = true;

            if (hadState)
                onReset?.Invoke();
            
            base.OnResetElement();
            
        }
        
        #endregion

        protected virtual void HandleClickBehavior()
        {
            
            switch (behavior)
            {
                case ButtonBehavior.Normal:
                    onClick?.Invoke();
                    break;

                case ButtonBehavior.PressOnce:
                    if (hasBeenPressed)
                        return;

                    hasBeenPressed = true;
                    onClick?.Invoke();
                    interactable = false;
                    break;
            }
            
        }

    }
}