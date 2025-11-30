#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEngine;
using UnityEngine.UI;

namespace GPUI.Editor
{
    /// <summary>
    /// Ensures that all inherited Selectable fields show up inside
    /// the "Tabs/UI Skin" tab of UiElement's inspector.
    /// </summary>
    public class UiElementSelectableToTabProcessor : OdinAttributeProcessor<UiElement>
    {
        // Only care about child members that come from UnityEngine.UI.Selectable
        public override bool CanProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member)
        {
            return member.DeclaringType == typeof(Selectable);
        }

        public override void ProcessChildMemberAttributes(
            InspectorProperty parentProperty,
            MemberInfo member,
            List<Attribute> attributes)
        {
            if (member.DeclaringType != typeof(Selectable))
                return;
            
            switch (member.Name)
            {
                case "m_Interactable":
                    attributes.Add(new TabGroupAttribute("Tabs", "Settings"));
                    attributes.Add(new LabelTextAttribute("Interactable"));
                    break;

                case "m_TargetGraphic":
                    attributes.Add(new HideInInspector());
                    attributes.Add(new TabGroupAttribute("Tabs", "Settings"));
                    attributes.Add(new ShowIfAttribute("", false));
                    attributes.Add(new LabelTextAttribute("Target Graphic"));
                    break;

                case "m_Transition":
                    attributes.Add(new TabGroupAttribute("Tabs", "Settings"));
                    attributes.Add(new LabelTextAttribute("Transition"));
                    break;

                case "m_Colors":
                    attributes.Add(new HideInInspector());
                    attributes.Add(new TabGroupAttribute("Tabs", "Settings"));
                    attributes.Add(new LabelTextAttribute("Color Tint"));
                    attributes.Add(new ShowIfAttribute(
                        "@this.transition == UnityEngine.UI.Selectable.Transition.ColorTint"));
                    break;

                case "m_SpriteState":
                    attributes.Add(new TabGroupAttribute("Tabs", "Settings"));
                    attributes.Add(new LabelTextAttribute("Sprite Swap"));
                    attributes.Add(new ShowIfAttribute(
                        "@this.transition == UnityEngine.UI.Selectable.Transition.SpriteSwap"));
                    break;

                case "m_AnimationTriggers":
                    attributes.Add(new TabGroupAttribute("Tabs", "Settings"));
                    attributes.Add(new LabelTextAttribute("Animation Triggers"));
                    attributes.Add(new ShowIfAttribute(
                        "@this.transition == UnityEngine.UI.Selectable.Transition.Animation"));
                    break;

                case "m_Navigation":
                    attributes.Add(new TabGroupAttribute("Tabs", "Settings"));
                    attributes.Add(new LabelTextAttribute("Navigation"));
                    break;

                default:
                    break;
            }
        }
    }
}
#endif
