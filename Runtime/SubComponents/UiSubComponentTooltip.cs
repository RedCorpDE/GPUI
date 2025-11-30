using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Localization;

namespace GPUI.SubComponents
{
    public class UiSubComponentTooltip : UiSubComponentBase
    {
        
        public LocalizedString tooltipText;

        public bool overrideDirection = false;

        enum Direction
        {
            Left,
            Right,
            Top,
            Bottom
        }
        
        [SerializeField, ShowIf("overrideDirection")]
        Direction overridenDirection = Direction.Top;
        Direction usedDirection = Direction.Top;
        

        private void Start()
        {
            
            OnInitialize();
            
        }

        protected override void OnInitialize()
        {

            this.Element.onEnter.AddListener(delegate
            {
                
                UiTooltip tooltip = FindAnyObjectByType<UiTooltip>();

                if (tooltip == null)
                    return;

                if (overrideDirection)
                {

                    switch (overridenDirection)
                    {
                        case Direction.Left:
                            tooltip.rectTransform.position = new Vector2(this.Element.rectTransform.position.x - this.Element.rectTransform.rect.width / 2, this.Element.rectTransform.position.y + this.Element.rectTransform.rect.height / 2);
                            break;
                        case Direction.Right:
                            tooltip.rectTransform.position = new Vector2(this.Element.rectTransform.position.x - this.Element.rectTransform.rect.width / 2, this.Element.rectTransform.position.y + this.Element.rectTransform.rect.height / 2);
                            break;
                        case Direction.Top:
                            tooltip.rectTransform.position = new Vector2(this.Element.rectTransform.position.x - this.Element.rectTransform.rect.width / 2, this.Element.rectTransform.position.y + this.Element.rectTransform.rect.height / 2);
                            break;
                        case Direction.Bottom:
                            tooltip.rectTransform.position = new Vector2(this.Element.rectTransform.position.x - this.Element.rectTransform.rect.width / 2, this.Element.rectTransform.position.y + this.Element.rectTransform.rect.height / 2);
                            break;
                    }
                    
                }
                else
                {
                    
                    switch (usedDirection)
                    {
                        case Direction.Left:
                            tooltip.rectTransform.position = new Vector2(this.Element.rectTransform.position.x - this.Element.rectTransform.rect.width / 2, this.Element.rectTransform.position.y + this.Element.rectTransform.rect.height / 2);
                            break;
                        case Direction.Right:
                            tooltip.rectTransform.position = new Vector2(this.Element.rectTransform.position.x - this.Element.rectTransform.rect.width / 2, this.Element.rectTransform.position.y + this.Element.rectTransform.rect.height / 2);
                            break;
                        case Direction.Top:
                            tooltip.rectTransform.position = new Vector2(this.Element.rectTransform.position.x - this.Element.rectTransform.rect.width / 2, this.Element.rectTransform.position.y + this.Element.rectTransform.rect.height / 2);
                            break;
                        case Direction.Bottom:
                            tooltip.rectTransform.position = new Vector2(this.Element.rectTransform.position.x - this.Element.rectTransform.rect.width / 2, this.Element.rectTransform.position.y + this.Element.rectTransform.rect.height / 2);
                            break;
                    }
                    
                }
                
                tooltip.tooltipText.LocalizedString = tooltipText;
                tooltip.FadeElement(true);
                
            }); 
            
            this.Element.onExit.AddListener(delegate
            {
                
                UiTooltip tooltip = FindAnyObjectByType<UiTooltip>();
                
                if (tooltip == null)
                    return;    
                
                tooltip.FadeElement(false);
                
            }); 

        }
        
    }
}