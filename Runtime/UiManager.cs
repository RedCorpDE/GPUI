using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace GPUI
{
    public class UiManager : MonoBehaviour
    {

        #region Singleton

        public static UiManager Instance;

        #endregion

        public UiSkinPalette currentPalette;
        
        public List<UiSkinPalette> palettes = new List<UiSkinPalette>();

        public bool scaleToScreenSize = true;

        public int selectedWindow = 0;
        public int lastWindow = -1;

        [ShowInInspector, ReadOnly] public Stack<int> lastWindows = new Stack<int>(128);

        public List<UiElement> mainWindows = new List<UiElement>();

        public UnityEvent<UiSkinPalette> OnSkinChanged;

        private void Awake()
        {

            if (Instance == null)
                Instance = this;
            else
                Destroy(this.gameObject);

            lastWindows = new Stack<int>(128);

            if (scaleToScreenSize)
                this.GetComponent<CanvasScaler>().referenceResolution = new Vector2(Screen.width, Screen.height);

        }

        public virtual void Start()
        {

            LayoutRebuilder.ForceRebuildLayoutImmediate(this.GetComponentInChildren<RectTransform>());
            Canvas.ForceUpdateCanvases();

        }

        [Button]
        public void ApplyClientSkin(int clientSkinIndex)
        {

            if (currentPalette == null)
                currentPalette = UiSettings.instance.DefaultPalette;

            currentPalette = palettes[clientSkinIndex];
            
            OnSkinChanged?.Invoke(currentPalette);

            Canvas.ForceUpdateCanvases();

        }

        [Button]
        public void SwitchWindow(int index = 0)
        {

            Canvas.ForceUpdateCanvases();



            lastWindows.Push(selectedWindow);

            if (lastWindows.Count > 0 && lastWindows.Peek() == lastWindow)
                lastWindows.Pop();

            lastWindow = selectedWindow;
            selectedWindow = index;

            if (lastWindows.Count > 0 && lastWindows.Peek() == -1)
                lastWindows.Push(selectedWindow);

            if (lastWindows.Count > 0)
                Debug.Log($"Switching Window: {selectedWindow} ({lastWindows.Peek()})");
            else
                Debug.Log($"Switching Window: {selectedWindow} ({index})");

            for (int i = 0; i < mainWindows.Count; i++)
            {

                int currentIndex = i;

                if (currentIndex == index)
                {

                    mainWindows[currentIndex].FadeElement(true);

                }
                else
                {

                    mainWindows[currentIndex].FadeElement(false);

                    continue;

                }

            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(this.GetComponentInChildren<RectTransform>());
            Canvas.ForceUpdateCanvases();

        }

        public void GoToLastWindow()
        {

            if (lastWindows.Count == 0)
                return;

            int oldIndex = lastWindows.Pop();
            lastWindow = selectedWindow;

            SwitchWindow(oldIndex);

        }

    }
}