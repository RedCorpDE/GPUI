using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using LitMotion;


namespace GPUI
{
    [Serializable]

    public class UiAnimationBase
    {

        public float duration;

        public bool isRunning = false;

        public virtual CompositeMotionHandle Play(List<Graphic> graphics, bool loop = false, LoopType loopType = LoopType.Incremental,
            Ease easeType = Ease.Linear)
        {

            CompositeMotionHandle tween = new CompositeMotionHandle();

            return tween;

        }

    }
}