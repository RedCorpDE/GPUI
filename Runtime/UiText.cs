using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace GPUI
{
    public class UiText : UiElement
    {
        
        [TabGroup("Tabs", "UI Elements")]
        [Tooltip("The TextMeshProUGUI component this element controls.")]
        [Required]
        [SerializeField]
        private TextMeshProUGUI textComponent;
        
        [TabGroup("Tabs", "Settings")]
        [Tooltip("Use Unity's LocalizedString to drive the label text.")]
        [SerializeField]
        private bool useLocalizedString = false;

        [TabGroup("Tabs", "Settings")]
        [ShowIf(nameof(useLocalizedString))]
        [Tooltip("Localized string reference (Unity Localization package).")]
        [SerializeField]
        private LocalizedString localizedString;
        
        
        #region Unity Lifecycle

        protected override void Reset()
        {
            
            base.Reset();
            
        }

        protected override void Awake()
        {
            
            base.Awake();
            
            if (targetGraphic == null && textComponent != null)
                targetGraphic = textComponent;
            
        }

        protected override void OnEnable()
        {
            
            base.OnEnable();
            
            if (useLocalizedString)
                RegisterLocalizedString();

            DoStateTransition(currentSelectionState, true);
            
        }
        
        protected override void OnDisable()
        {
            
            UnregisterLocalizedString();
            
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
            
            if (textComponent == null)
                textComponent = GetComponent<TextMeshProUGUI>();

            if (useLocalizedString && isActiveAndEnabled)
            {
                UnregisterLocalizedString();
                RegisterLocalizedString();
            }

            DoStateTransition(currentSelectionState, true);
            
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
            
            if (textComponent == null)
                return;
            
            if (skinData == null)
                return;
            
            Color stateColor = (skinData ? skinData.backgroundColor.normalColor : colors.normalColor);

            switch (state)
            {
                case SelectionState.Normal:
                    stateColor = (skinData ? skinData.backgroundColor.normalColor : colors.normalColor);
                    break;
                case SelectionState.Highlighted:
                    stateColor = (skinData ? skinData.backgroundColor.highlightedColor : colors.highlightedColor);
                    break;
                case SelectionState.Pressed:
                    stateColor = (skinData ? skinData.backgroundColor.pressedColor : colors.pressedColor);
                    break;
                case SelectionState.Selected:
                    stateColor = (skinData ? skinData.backgroundColor.selectedColor : colors.selectedColor);
                    break;
                case SelectionState.Disabled:
                    stateColor = (skinData ? skinData.backgroundColor.disabledColor : colors.disabledColor);
                    break;
            }
            
            textComponent.color = stateColor;
            
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
        
        /// <summary>
        /// The underlying TMP text component.
        /// </summary>
        public TextMeshProUGUI TextComponent
        {
            get => textComponent;
            set
            {
                textComponent = value;
                if (textComponent != null && targetGraphic == null)
                    targetGraphic = textComponent;

                DoStateTransition(currentSelectionState, true);
            }
        }
        
        /// <summary>
        /// Convenience access to the label text.
        /// If useLocalizedString is true, setting this will also disable localization.
        /// </summary>
        public string Text
        {
            get => textComponent != null ? textComponent.text : string.Empty;
            set
            {
                if (textComponent == null) return;

                // If user manually sets text, assume they want to override localization.
                useLocalizedString = false;
                UnregisterLocalizedString();

                textComponent.text = value;
            }
        }
        
        public bool UseLocalizedString
        {
            get => useLocalizedString;
            set
            {
                if (useLocalizedString == value)
                    return;

                useLocalizedString = value;

                if (isActiveAndEnabled)
                {
                    if (useLocalizedString)
                        RegisterLocalizedString();
                    else
                        UnregisterLocalizedString();
                }
            }
        }
        
        public LocalizedString LocalizedString
        {
            get => localizedString;
            set
            {
                // Swap the reference and re-register.
                UnregisterLocalizedString();
                localizedString = value;
                if (useLocalizedString && isActiveAndEnabled)
                    RegisterLocalizedString();
            }
        }
        
        #endregion

        #region Localization

        private void RegisterLocalizedString()
        {
            localizedString.StringChanged += OnLocalizedStringChanged;
            localizedString.RefreshString();
        }

        private void UnregisterLocalizedString()
        {
            localizedString.StringChanged -= OnLocalizedStringChanged;
        }

        private void OnLocalizedStringChanged(string value)
        {
            if (!useLocalizedString)
                return;

            if (textComponent != null)
                textComponent.text = value;
        }

        #endregion
    }
}