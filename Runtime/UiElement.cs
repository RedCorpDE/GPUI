using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine.Events;
using UnityEngine.EventSystems;


namespace GPUI
{
    
    /// <summary>
    /// Base UI element for GPUI. 
    /// Handles:
    /// - Skin application (colors, sprite, material, outline, shadow)
    /// - Hover / reset events
    /// - State-based color transitions via Selectable
    /// 
    /// Derive from this for buttons, toggles, sliders, etc.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class UiElement : Selectable
    {

        [HideInInspector] public CanvasGroup canvasGroup;

        [HideInInspector] public RectTransform rectTransform;

        [TabGroup("Tabs", "UI Skin", SdfIconType.FileEarmarkMedical)]
        [AssetSelector(Paths = "Assets/Content/GUI/Styles/")]
        public SimpleComponentSkinDataObject skinData;

        [TabGroup("Tabs", "UI Elements", SdfIconType.Stickies)]
        [Tooltip(
            "This should be the Background Graphic of the UI Element.<br>Example: Background of a Button, not the Icon.")]
        public Graphic backgroundGraphic;

        [TabGroup("Tabs", "Animations", SdfIconType.SkipEnd)]
        [InfoBox("If no animations are set, the default fade-in/fade-out are used!")]
        [TabGroup("Tabs", "Animations")]
        [OdinSerialize, NonSerialized]
        public UiAnimationData setActiveAnimation;

        [TabGroup("Tabs", "Animations")] [OdinSerialize, NonSerialized]
        public UiAnimationData setInactiveAnimation;
        
        [TabGroup("Events", "OnClick")]
        public UnityEvent onClick;
        
        [TabGroup("Events", "OnEnter")]
        public UnityEvent onEnter;
        
        [TabGroup("Events", "OnExit")]
        public UnityEvent onExit;
        
        [TabGroup("Events", "OnReset")]
        public UnityEvent onReset;
        
        #region Unity Lifecycle

#if UNITY_EDITOR
        protected override void Reset()
        {
            
            base.Reset();
            
            CacheComponents();
            
        }
#endif

        protected override void Awake()
        {
            
            base.Awake();
            
            CacheComponents();
            ApplySkinData();
            
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            // Ensure targetGraphic is set for Selectable color transitions
            if (targetGraphic == null && backgroundGraphic != null)
                targetGraphic = backgroundGraphic;

            if (UiManager.Instance != null)
            {

                //Load desired SkinData
                UiManager.Instance.OnSkinChanged.AddListener(delegate
                {

                    UiManager.Instance.currentPalette.GetSkinData(out skinData, skinData);
                    ApplySkinData();

                });

            }
            else
                ApplySkinData();

            // Make sure visuals reflect current selection state
            DoStateTransition(currentSelectionState, true);
            
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
            
            CacheComponents();
            ApplySkinData();
            
        }
#endif

        #endregion
        
        #region Component Caching
        
        protected virtual void CacheComponents()
        {
            if (!backgroundGraphic)
                backgroundGraphic = GetComponent<Graphic>();

            // By default, use our background image as the target graphic
            if (backgroundGraphic && targetGraphic == null)
                targetGraphic = backgroundGraphic;
            
            if(!canvasGroup)
                canvasGroup = GetComponent<CanvasGroup>();
            
            if(!rectTransform)
                rectTransform = GetComponent<RectTransform>();
            
        }

        #endregion
        
        #region Apply Skin
        
        /// <summary>
        /// Assigns skin values to this element.
        /// </summary>
        public virtual void ApplySkinData()
        {

            if (skinData == null)
                return;

            if (backgroundGraphic)
            {

                if (skinData.backgroundMaterial != null)
                    backgroundGraphic.material = skinData.backgroundMaterial;
                
                if (backgroundGraphic is UiShapeRect)
                {

                    (backgroundGraphic as UiShapeRect).FillColor = skinData.backgroundColor.normalColor;
                    (backgroundGraphic as UiShapeRect).OutlineColor = skinData.outlineColor.normalColor;

                    //if (skinData.backgroundSprite != null)
                    //    (backgroundGraphic as UiShapeRect).Sprite = skinData.backgroundSprite;

                    (backgroundGraphic as UiShapeRect).UseOutline = true;
                    (backgroundGraphic as UiShapeRect).OutlineThickness = skinData.outlineWidth;

                    (backgroundGraphic as UiShapeRect).UseShadow = true;
                    (backgroundGraphic as UiShapeRect).ShadowCol = skinData.shadowColor;
                    (backgroundGraphic as UiShapeRect).ShadowOffset = skinData.offset;
                    (backgroundGraphic as UiShapeRect).ShadowSize = skinData.size;
                    (backgroundGraphic as UiShapeRect).ShadowSoftness = skinData.softness;

                }

                LayoutRebuilder.ForceRebuildLayoutImmediate(this.GetComponent<RectTransform>());
                LayoutRebuilder.ForceRebuildLayoutImmediate(backgroundGraphic.rectTransform);

            }
            
            ApplyShape();
            ApplyLayout(rectTransform, skinData.layoutOptions);

        }

        /// <summary>
        /// Hook for shape components (rounded corners, etc).
        /// Implement in subclasses or attach custom components that read from the skin.
        /// </summary>
        public virtual void ApplyShape()
        {
            
            if (backgroundGraphic is UiShapeRect)
            {

                if (skinData.useMaxRadius)
                {
                    


                }
                else
                {
                    
                    (backgroundGraphic as UiShapeRect).CornerRadius = new Vector4(skinData.backgroundRadiusTL, 
                                                                        skinData.backgroundRadiusTR, 
                                                                        skinData.backgroundRadiusBR, 
                                                                        skinData.backgroundRadiusBL);

                }

                (backgroundGraphic as UiShapeRect).ShapeRoundness = skinData.shapeRoundness;

                backgroundGraphic.SetAllDirty();

            }
            
        }
        
        public virtual void ApplyLayout(RectTransform layoutRectTransform, SimpleComponentSkinDataObject.UiElementLayoutOptions layoutOptions)
        {

            if (!skinData)
                return;
            
            if (!layoutRectTransform.TryGetComponent<UiCombiLayoutGroup>(out UiCombiLayoutGroup layoutGroup))
                layoutGroup = layoutRectTransform.gameObject.AddComponent<UiCombiLayoutGroup>();
            

            
        }
        
        /// <summary>
        /// Override default state transition to also update outline based on skin.
        /// </summary>
        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            base.DoStateTransition(state, instant);

            if (skinData == null)
                return;
            
            Color targetFill = skinData.backgroundColor.normalColor;
            Color targetOutline = skinData.outlineColor.normalColor;

            switch (state)
            {
                case SelectionState.Normal:
                    targetFill = skinData.backgroundColor.normalColor;
                    targetOutline = skinData.outlineColor.normalColor;
                    break;
                case SelectionState.Highlighted:
                    targetFill = skinData.backgroundColor.highlightedColor;
                    targetOutline = skinData.outlineColor.highlightedColor;
                    break;
                case SelectionState.Pressed:
                    targetFill = skinData.backgroundColor.pressedColor;
                    targetOutline = skinData.outlineColor.pressedColor;
                    break;
                case SelectionState.Selected:
                    targetFill = skinData.backgroundColor.selectedColor;
                    targetOutline = skinData.outlineColor.selectedColor;
                    break;
                case SelectionState.Disabled:
                    targetFill = skinData.backgroundColor.disabledColor;
                    targetOutline = skinData.outlineColor.disabledColor;
                    break;
            }

            if (backgroundGraphic is UiShapeRect)
            {
                (backgroundGraphic as UiShapeRect).FillColor = targetFill;
                (backgroundGraphic as UiShapeRect).OutlineColor = targetOutline;
                
            }
        }
        
        #endregion
        
        #region API
        
        public virtual void FadeElement(bool fadeIn = false)
        {

            if (fadeIn)
            {

                if (setActiveAnimation == null || setActiveAnimation.animation.GetType() == typeof(UiAnimationBase))
                {

                    LMotion.Create(canvasGroup.alpha, 1f, .5f)
                        .WithOnComplete(() =>
                        {
                            canvasGroup.blocksRaycasts = true;
                            canvasGroup.interactable = true;
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
                        })
                        .BindToAlpha(canvasGroup);

                }
                else
                    setInactiveAnimation.Play();

            }

        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            
            if (!IsActive() || !IsInteractable()) 
                return;

            onClick?.Invoke();
            
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            
            base.OnPointerEnter(eventData);

            if (!IsActive() || !IsInteractable()) 
                return;

            onEnter?.Invoke();
            
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            
            base.OnPointerExit(eventData);

            if (!IsActive()) 
                return;

            onExit?.Invoke();
         
        }

        public virtual void OnResetElement()
        {
            
            interactable = true;
            DoStateTransition(SelectionState.Normal, true);
            onReset?.Invoke();
            
        }
        
        #endregion
        
    }
}