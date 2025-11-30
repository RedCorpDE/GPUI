using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GPUI
{
    public class UiElementExtended : UiElement
    {

        [TabGroup("Tabs", "UI Elements")] public Graphic detailGraphic;
        
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

            if (detailGraphic != null && skinData is ComponentSkinDataObject)
            {

                ComponentSkinDataObject detailedSkinData = skinData as ComponentSkinDataObject;
                detailGraphic.color = detailedSkinData.detailColor.normalColor;

                if (detailGraphic is Image)
                {

                    if (detailedSkinData.detailSprite != null)
                        (detailGraphic as Image).sprite = detailedSkinData.detailSprite;
                    if (detailedSkinData.detailMaterial != null)
                        detailGraphic.material = detailedSkinData.detailMaterial;

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
            
            Color targetDetailColor = (skinData as ComponentSkinDataObject).detailColor.normalColor;

            switch (state)
            {
                case SelectionState.Normal:
                    targetDetailColor = (skinData as ComponentSkinDataObject).detailColor.normalColor;
                    break;
                case SelectionState.Highlighted:
                    targetDetailColor = (skinData as ComponentSkinDataObject).detailColor.highlightedColor;
                    break;
                case SelectionState.Pressed:
                    targetDetailColor = (skinData as ComponentSkinDataObject).detailColor.pressedColor;
                    break;
                case SelectionState.Selected:
                    targetDetailColor = (skinData as ComponentSkinDataObject).detailColor.selectedColor;
                    break;
                case SelectionState.Disabled:
                    targetDetailColor = (skinData as ComponentSkinDataObject).detailColor.disabledColor;
                    break;
            }

            if (detailGraphic is Image)
            {
                (detailGraphic as Image).color = targetDetailColor;
                
            }
        }
        
        #endregion
        
        #region API
        
        public override void FadeElement(bool fadeIn = false)
        {

            base.FadeElement(fadeIn);

        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            
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
            
            base.OnResetElement();
            
        }
        
        #endregion
    }
}