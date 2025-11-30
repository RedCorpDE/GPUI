using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using LitMotion;


namespace GPUI
{
    [System.Serializable]

    public class UiAnimationData
    {

        public string trigger;
        public bool playOnAwake = false;
        public bool loop = false;

        [ShowIf("loop")] public LoopType loopType;
        public Ease easeType;

        public List<Graphic> targetGraphics = new List<Graphic>();

        [OdinSerialize,
         PolymorphicDrawerSettings(ShowBaseType = false,
             NonDefaultConstructorPreference = NonDefaultConstructorPreference.ConstructIdeal), NonSerialized]
        public UiAnimationBase animation = new UiAnimationBase();

        public event Action OnAnimationFinished;

        public CompositeMotionHandle currentTween;

        public void Play()
        {

            currentTween?.Cancel();
            
            currentTween = animation.Play(targetGraphics, loop, loopType, easeType);

        }

    }
}