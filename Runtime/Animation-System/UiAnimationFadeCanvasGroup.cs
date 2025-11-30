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
    [System.Serializable]

    public class UiAnimationFadeCanvasGroup : UiAnimationBase
    {

        public float fadeValue;


        public override CompositeMotionHandle Play(List<Graphic> graphics, bool loop = false, LoopType loopType = LoopType.Incremental,
            Ease easeType = Ease.Linear)
        {

            CompositeMotionHandle tween = new CompositeMotionHandle();

            List<CanvasGroup> canvasGroups = new List<CanvasGroup>();

            for (int i = 0; i < graphics.Count; i++)
            {

                if (graphics[i].TryGetComponent<CanvasGroup>(out CanvasGroup group))
                    canvasGroups.Add(group);
                else
                    continue;

            }

            for (int i = 0; i < canvasGroups.Count; i++)
            {

                if (loop)
                {

                    //tween = DOTween.Sequence().Insert(0f,
                    //    canvasGroups[i].DOFade(fadeValue, duration).SetLoops(-1, loopType).SetEase(easeType));
                    
                    var value = 0f;
                    LMotion.Create(canvasGroups[i].alpha, fadeValue, duration)
                        .WithEase(easeType)
                        .WithLoops(-1, loopType)
                        .BindToAlpha(canvasGroups[i])
                        .AddTo(tween);

                }
                else
                {

                    var value = 0f;
                    LMotion.Create(canvasGroups[i].alpha, fadeValue, duration)
                        .WithEase(easeType)
                        .BindToAlpha(canvasGroups[i])
                        .AddTo(tween);

                }

            }

            return tween;

        }

    }
}