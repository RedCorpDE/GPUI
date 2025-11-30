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

    public class UiAnimationFade : UiAnimationBase
    {

        public float fadeValue;


        public override CompositeMotionHandle Play(List<Graphic> graphics, bool loop = false, LoopType loopType = LoopType.Incremental,
            Ease easeType = Ease.Linear)
        {

            CompositeMotionHandle tween = new CompositeMotionHandle();

            for (int i = 0; i < graphics.Count; i++)
            {

                if (loop)
                {

                    //tween = DOTween.Sequence().Insert(0f,
                    //    graphics[i].DOFade(fadeValue, duration).SetLoops(-1, loopType).SetEase(easeType));
                    
                    var value = 0f;
                    LMotion.Create(graphics[i].color.a, fadeValue, duration)
                        .WithEase(easeType)
                        .WithLoops(-1, loopType)
                        .BindToColorA(graphics[i])
                        .AddTo(tween);

                }
                else
                {

                    //tween = DOTween.Sequence().Insert(0f, graphics[i].DOFade(fadeValue, duration).SetEase(easeType));
                    
                    var value = 0f;
                    LMotion.Create(graphics[i].color.a, fadeValue, duration)
                        .WithEase(easeType)
                        .BindToColorA(graphics[i])
                        .AddTo(tween);

                }

            }

            return tween;

        }

    }
}