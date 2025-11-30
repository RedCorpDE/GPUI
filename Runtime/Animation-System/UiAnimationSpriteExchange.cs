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

    public class UiAnimationSpriteExchange : UiAnimationBase
    {

        public List<Sprite> sprites = new List<Sprite>();

        public override CompositeMotionHandle Play(List<Graphic> graphics, bool loop = false, LoopType loopType = LoopType.Incremental,
            Ease easeType = Ease.Linear)
        {

            CompositeMotionHandle tween = new CompositeMotionHandle();

            for (int i = 0; i < graphics.Count; i++)
            {

                //if (loop)
                //    graphics[i].rectTransform.DOAnchorPos(transforms[i], duration).SetLoops(-1, loopType);
                //else
                (graphics[i] as Image).sprite = sprites[i];
            }

            return null;

        }

    }
}