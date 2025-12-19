using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace GPUI
{

    public class UiToggle : UiElementExtended
    {
        
        public enum ToggleBehavior
        {
            Normal,
            ToggleButton,
        }
        
        [TabGroup("Tabs", "Settings")]
        [SerializeField]
        private ToggleBehavior behavior = ToggleBehavior.Normal;
        
        [TabGroup("Tabs", "Settings")]
        [Tooltip("Initial state when the toggle becomes active.")]
        [SerializeField]
        private bool startOn = false;

        [TabGroup("Tabs", "Settings")]
        [ShowInInspector, ReadOnly, LabelText("Is On")]
        private bool isOn;
        
        [TabGroup("Tabs", "Settings")]
        [Tooltip("Optional toggle group this toggle belongs to.")]
        [SerializeField]
        private UiToggleGroup toggleGroup;
        
        [TabGroup("Events", "OnClick")]
        [LabelText("On Value Changed (bool)")]
        public UnityEvent<bool> onValueChanged;

        [TabGroup("Events", "OnClick")]
        [LabelText("On Toggled On")]
        public UnityEvent onToggledOn;

        [TabGroup("Events", "OnClick")]
        [LabelText("On Toggled Off")]
        public UnityEvent onToggledOff;
        
        /// <summary>
        /// Public read-only access to the current state.
        /// </summary>
        public bool IsOn => isOn;
        
        public UiToggleGroup ToggleGroup
        {
            get => toggleGroup;
            set
            {
                if (toggleGroup == value)
                    return;

                // deregister from old
                if (toggleGroup != null)
                    toggleGroup.UnregisterToggle(this);

                toggleGroup = value;

                // register to new
                if (isActiveAndEnabled && toggleGroup != null)
                    toggleGroup.RegisterToggle(this);

                // ensure group constraints are respected
                if (toggleGroup != null && isOn)
                    toggleGroup.NotifyToggleOn(this);
            }
        }

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

            // Initialize state when the toggle becomes active
            SetIsOn(startOn, false);

            if (toggleGroup != null)
                toggleGroup.RegisterToggle(this);

            // If we're ON, notify group so it can turn others off.
            if (toggleGroup != null && isOn)
                toggleGroup.NotifyToggleOn(this);
            
        }

        protected override void OnDisable()
        {
            
            if (toggleGroup != null)
                toggleGroup.UnregisterToggle(this);

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
            
            //base.DoStateTransition(state, instant);

            if (skinData == null)
                return;
            
            Color targetBackgroundColor = (skinData as ComponentSkinDataObject).backgroundColor.normalColor;
            Color targetDetailColor = (skinData as ComponentSkinDataObject).detailColor.normalColor;

            if (skinData is UiToggleSkinDataObject)
            {
                
                switch (state)
                {
                    case SelectionState.Normal:

                        if (isOn)
                        {
                            targetBackgroundColor = (skinData as UiToggleSkinDataObject).pressedBackgroundColor.normalColor;
                            targetDetailColor = (skinData as UiToggleSkinDataObject).pressedDetailColor.normalColor;
                        }
                        else
                        {
                            targetBackgroundColor = (skinData as UiToggleSkinDataObject).backgroundColor.normalColor;
                            targetDetailColor = (skinData as UiToggleSkinDataObject).detailColor.normalColor;
                        }
                        
                        break;
                    case SelectionState.Highlighted:
                        
                        if (isOn)
                        {
                            targetBackgroundColor = (skinData as UiToggleSkinDataObject).pressedBackgroundColor.highlightedColor;
                            targetDetailColor = (skinData as UiToggleSkinDataObject).pressedDetailColor.highlightedColor;
                        }
                        else
                        {
                            targetBackgroundColor = (skinData as UiToggleSkinDataObject).backgroundColor.highlightedColor;
                            targetDetailColor = (skinData as UiToggleSkinDataObject).detailColor.highlightedColor;
                        }
                        
                        break;
                    case SelectionState.Pressed:
                        
                        if (isOn)
                        {
                            targetBackgroundColor = (skinData as UiToggleSkinDataObject).pressedBackgroundColor.pressedColor;
                            targetDetailColor = (skinData as UiToggleSkinDataObject).pressedDetailColor.pressedColor;
                        }
                        else
                        {
                            targetBackgroundColor = (skinData as UiToggleSkinDataObject).backgroundColor.pressedColor;
                            targetDetailColor = (skinData as UiToggleSkinDataObject).detailColor.pressedColor;
                        }
                        
                        break;
                    case SelectionState.Selected:
                        
                        if (isOn)
                        {
                            targetBackgroundColor = (skinData as UiToggleSkinDataObject).pressedBackgroundColor.selectedColor;
                            targetDetailColor = (skinData as UiToggleSkinDataObject).pressedDetailColor.selectedColor;
                        }
                        else
                        {
                            targetBackgroundColor = (skinData as UiToggleSkinDataObject).backgroundColor.selectedColor;
                            targetDetailColor = (skinData as UiToggleSkinDataObject).detailColor.selectedColor;
                        }
                        
                        break;
                    case SelectionState.Disabled:
                        
                        if (isOn)
                        {
                            targetBackgroundColor = (skinData as UiToggleSkinDataObject).pressedBackgroundColor.disabledColor;
                            targetDetailColor = (skinData as UiToggleSkinDataObject).pressedDetailColor.disabledColor;
                        }
                        else
                        {
                            targetBackgroundColor = (skinData as UiToggleSkinDataObject).backgroundColor.disabledColor;
                            targetDetailColor = (skinData as UiToggleSkinDataObject).detailColor.disabledColor;
                        }
                        
                        break;
                }
                
            }
            else
            {
                
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
                
            }

            backgroundGraphic.color = targetBackgroundColor;
            detailGraphic.color = targetDetailColor;
        }
        
        #endregion
        
        #region API
        
        public override void FadeElement(bool fadeIn = false)
        {

            base.FadeElement(fadeIn);

        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            if (!IsActive() || !IsInteractable())
                return;

            Toggle();

            // Let Selectable handle state transitions, etc.
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
        
        /// <summary>
        /// Toggles the current state and fires events.
        /// </summary>
        [TabGroup("Tabs", "Settings")]
        [Button(ButtonSizes.Small)]
        public virtual void Toggle()
        {
            
            bool newValue = !isOn;

            // If we're trying to switch OFF, ask the group first.
            if (!newValue && toggleGroup != null && !toggleGroup.CanSwitchOff(this))
                return;

            SetIsOn(newValue, true);
            
        }
        
        /// <summary>
        /// Sets the toggle state from code.
        /// </summary>
        /// <param name="value">New toggle state.</param>
        /// <param name="sendEvent">Whether to invoke events.</param>
        public virtual void SetIsOn(bool value, bool sendEvent = true)
        {
            
            if (isOn == value)
                return;

            isOn = value;

            if (sendEvent)
            {
                onValueChanged?.Invoke(isOn);
                if (isOn) onToggledOn?.Invoke();
                else      onToggledOff?.Invoke();
            }

            UpdateVisualState();

            // If we just turned ON, tell our group.
            if (isOn && toggleGroup != null)
            {
                toggleGroup.NotifyToggleOn(this);
            }
            
        }
        
        /// <summary>
        /// Updates the visuals to reflect current state.
        /// This is where we implement the two visual modes:
        ///  - ToggleButton
        ///  - Normal (just color swap)
        /// </summary>
        private void UpdateVisualState()
        {
            if (detailGraphic == null)
                return;

            switch (behavior)
            {
                case ToggleBehavior.ToggleButton:
                    detailGraphic.enabled = isOn;
                    break;

                case ToggleBehavior.Normal:
                    detailGraphic.enabled = true;
                    break;
            }
        }

        #endregion
    }
    
}