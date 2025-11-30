using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GPUI
{
    /// <summary>
    /// Group for UiToggle components.
    /// - Ensures at most one toggle is ON at a time (radio-like)
    /// - Optional: allow all toggles to be OFF.
    /// - UiToggle notifies this group when turned ON.
    /// </summary>
    [AddComponentMenu("GPUI/Ui Toggle Group")]
    public class UiToggleGroup : MonoBehaviour
    {
        [TabGroup("Settings", "Group")]
        [Tooltip("If false, at least one toggle in this group must remain ON.")]
        [SerializeField]
        private bool allowSwitchOff = false;

        [TabGroup("Settings", "Group")]
        [ShowInInspector, ReadOnly]
        private readonly List<UiToggle> toggles = new();

        /// <summary>
        /// If false, group will prevent the last ON toggle from being turned OFF.
        /// </summary>
        public bool AllowSwitchOff
        {
            get => allowSwitchOff;
            set => allowSwitchOff = value;
        }

        /// <summary>
        /// Register a toggle with this group.
        /// Called automatically from UiToggle.OnEnable.
        /// </summary>
        public void RegisterToggle(UiToggle toggle)
        {
            if (toggle == null || toggles.Contains(toggle))
                return;

            toggles.Add(toggle);
        }

        /// <summary>
        /// Unregister a toggle from this group.
        /// Called automatically from UiToggle.OnDisable.
        /// </summary>
        public void UnregisterToggle(UiToggle toggle)
        {
            if (toggle == null)
                return;

            toggles.Remove(toggle);
        }

        /// <summary>
        /// Called by a toggle when it is turned ON.
        /// Ensures all other toggles are set OFF.
        /// </summary>
        public void NotifyToggleOn(UiToggle toggle)
        {
            if (toggle == null)
                return;

            for (int i = 0; i < toggles.Count; i++)
            {
                var t = toggles[i];
                if (t != null && t != toggle && t.IsOn)
                {
                    // Turning these off will fire their events as well.
                    t.SetIsOn(false, true);
                }
            }
        }

        /// <summary>
        /// Returns true if any toggle in this group is ON.
        /// </summary>
        public bool AnyTogglesOn()
        {
            for (int i = 0; i < toggles.Count; i++)
            {
                var t = toggles[i];
                if (t != null && t.IsOn)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Returns all toggles that are currently ON.
        /// </summary>
        public IEnumerable<UiToggle> ActiveToggles()
        {
            for (int i = 0; i < toggles.Count; i++)
            {
                var t = toggles[i];
                if (t != null && t.IsOn)
                    yield return t;
            }
        }

        /// <summary>
        /// Can the given toggle switch OFF according to group rules?
        /// Used by UiToggle when user tries to turn it off.
        /// </summary>
        public bool CanSwitchOff(UiToggle toggle)
        {
            if (allowSwitchOff)
                return true;

            // If any other toggle is ON, we can switch this one off.
            for (int i = 0; i < toggles.Count; i++)
            {
                var t = toggles[i];
                if (t != null && t != toggle && t.IsOn)
                    return true;
            }

            // No others ON and we don't allow all OFF -> reject.
            return false;
        }
    }
}
