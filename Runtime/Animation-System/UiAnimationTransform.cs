using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LitMotion;
using LitMotion.Adapters;
using LitMotion.Collections;
using LitMotion.Extensions;
using Sirenix.Utilities;


namespace GPUI
{
    public class UiAnimationTransform : UiAnimationBase
    {

        public bool isAbsolute = false;

        public List<Vector3> transforms = new List<Vector3>();
        public List<Vector3> rotations = new List<Vector3>();
        public List<Vector3> scales = new List<Vector3>();


        public override CompositeMotionHandle Play(List<Graphic> graphics, bool loop = false, LoopType loopType = LoopType.Incremental,
            Ease easeType = Ease.Linear)
        {

            CompositeMotionHandle tween = new CompositeMotionHandle();

            transforms.SetLength(graphics.Count);
            rotations.SetLength(graphics.Count);
            scales.SetLength(graphics.Count);

            for (int i = 0; i < graphics.Count; i++)
            {

                if (loop)
                {

                    if (isAbsolute)
                    {
                        
                        LMotion.Create(graphics[i].rectTransform.anchoredPosition3D, transforms[i], duration)
                            .WithEase(easeType)
                            .WithLoops(-1, loopType)
                            .BindToAnchoredPosition3D(graphics[i].rectTransform)
                            .AddTo(tween);
                        
                    }
                    else
                    {
                        
                        LMotion.Create(graphics[i].rectTransform.anchoredPosition3D, graphics[i].rectTransform.anchoredPosition3D + transforms[i], duration)
                            .WithEase(easeType)
                            .WithLoops(-1, loopType)
                            .BindToAnchoredPosition3D(graphics[i].rectTransform)
                            .AddTo(tween);
                        
                    }
                    
                    LMotion.Create(graphics[i].rectTransform.localRotation.eulerAngles, rotations[i], duration)
                        .WithEase(easeType)
                        .WithLoops(-1, loopType)
                        .BindToLocalEulerAngles(graphics[i].rectTransform)
                        .AddTo(tween);
                    
                    LMotion.Create(graphics[i].rectTransform.localScale, scales[i], duration)
                        .WithEase(easeType)
                        .WithLoops(-1, loopType)
                        .BindToLocalScale(graphics[i].rectTransform)
                        .AddTo(tween);

                }
                else
                {

                    if (isAbsolute)
                    {
                        
                        LMotion.Create(graphics[i].rectTransform.anchoredPosition3D, transforms[i], duration)
                            .WithEase(easeType)
                            .BindToAnchoredPosition3D(graphics[i].rectTransform)
                            .AddTo(tween);
                        
                    }
                    else
                    {
                        
                        LMotion.Create(graphics[i].rectTransform.anchoredPosition3D, graphics[i].rectTransform.anchoredPosition3D + transforms[i], duration)
                            .WithEase(easeType)
                            .BindToAnchoredPosition3D(graphics[i].rectTransform)
                            .AddTo(tween);
                        
                    }

                    LMotion.Create(graphics[i].rectTransform.localRotation.eulerAngles, rotations[i], duration)
                        .WithEase(easeType)
                        .BindToLocalEulerAngles(graphics[i].rectTransform)
                        .AddTo(tween);
                    
                    LMotion.Create(graphics[i].rectTransform.localScale, scales[i], duration)
                        .WithEase(easeType)
                        .BindToLocalScale(graphics[i].rectTransform)
                        .AddTo(tween);

                }

            }

            return tween;

        }

    }
}