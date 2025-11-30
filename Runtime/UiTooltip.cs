using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace GPUI
{
    /// <summary>
    /// Global tooltip view, based on UiElement.
    /// Put a single instance in your main Canvas.
    /// Other components (UiTooltipTrigger) call UiTooltip.Instance.Show/Hide/Move.
    /// </summary>
    [AddComponentMenu("GPUI/Ui Tooltip (View)")]
    [RequireComponent(typeof(RectTransform))]
    public class UiTooltip : UiElement
    {
        public static UiTooltip Instance { get; private set; }

        [TabGroup("Tabs", "Settings")]
        [Tooltip("Extra padding from the mouse position, in screen pixels.")]
        [SerializeField]
        private Vector2 defaultOffset = new Vector2(16f, -16f);

        [TabGroup("Tabs", "UI Elements")]
        [SerializeField]
        public UiText tooltipText;
        
        private Canvas canvas;
        
        
        protected override void Awake()
        {
            base.Awake();

            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("Multiple UiTooltip instances found.");
                Destroy(gameObject);
                return;
            }

            Instance = this;

            rectTransform = GetComponent<RectTransform>();

            canvas = GetComponentInParent<Canvas>();

            // Tooltip shouldn't be interactable or block clicks.
            interactable = false;
            transition = Transition.None;

            if (targetGraphic != null)
                targetGraphic.raycastTarget = false;

            // Also disable raycasts on any child graphics, just to be safe.
            foreach (var g in GetComponentsInChildren<Graphic>())
                g.raycastTarget = false;

            FadeElement();
            
        }
        
        protected override void CacheComponents()
        {
            
            base.CacheComponents();
            
            if (targetGraphic != null)
                targetGraphic.raycastTarget = false;
            
        }

        private Camera GetCanvasCamera()
        {
            if (canvas == null) return null;
            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                return null;
            return canvas.worldCamera;
        }

        /// <summary>
        /// Show the tooltip at a screen position.
        /// </summary>
        public void Show(string text, Vector2 screenPosition, Vector2? customOffset = null)
        {
            if (string.IsNullOrEmpty(text))
                return;

            if (tooltipText != null)
                tooltipText.Text = text;

            FadeElement(true);
            
            SetPosition(screenPosition + (customOffset ?? defaultOffset));
        }
        
        /// <summary>
        /// Show the tooltip at a screen position.
        /// </summary>
        public void Show(LocalizedString text, Vector2 screenPosition, Vector2? customOffset = null)
        {

            if (tooltipText != null)
                tooltipText.LocalizedString = text;

            FadeElement(true);
            
            SetPosition(screenPosition + (customOffset ?? defaultOffset));
        }

        /// <summary>
        /// Move tooltip to a new screen position (usually while following the mouse).
        /// </summary>
        public void Move(Vector2 screenPosition, Vector2? customOffset = null)
        {
            if (!gameObject.activeInHierarchy)
                return;

            SetPosition(screenPosition + (customOffset ?? defaultOffset));
        }

        private void SetPosition(Vector2 screenPosition)
        {
            if (canvas == null)
                return;

            RectTransform parentRect = canvas.transform as RectTransform;
            if (parentRect == null)
                return;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    parentRect,
                    screenPosition,
                    GetCanvasCamera(),
                    out var localPoint))
            {
                rectTransform.anchoredPosition = localPoint;

                // Simple clamping inside canvas
                Vector2 size = rectTransform.sizeDelta;
                Vector2 halfSize = size * 0.5f;
                Vector2 min = parentRect.rect.min + halfSize;
                Vector2 max = parentRect.rect.max - halfSize;

                Vector2 clamped = new Vector2(
                    Mathf.Clamp(rectTransform.anchoredPosition.x, min.x, max.x),
                    Mathf.Clamp(rectTransform.anchoredPosition.y, min.y, max.y));

                rectTransform.anchoredPosition = clamped;
            }
        }
    }
}
