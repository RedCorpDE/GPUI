using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LitMotion;
using LitMotion.Adapters;
using LitMotion.Collections;
using LitMotion.Extensions;



namespace GPUI
{
    public class UiAnimationCanvasOrder : UiAnimationBase
    {

        public int layer;


        public override CompositeMotionHandle Play(List<Graphic> graphics, bool loop = false, LoopType loopType = LoopType.Incremental,
            Ease easeType = Ease.Linear)
        {

            CompositeMotionHandle tween = new CompositeMotionHandle();

            for (int i = 0; i < graphics.Count; i++)
            {

                if (graphics[i].TryGetComponent(out Canvas canvas))
                    canvas.sortingOrder = layer;

            }

            return tween;

        }

    }
}