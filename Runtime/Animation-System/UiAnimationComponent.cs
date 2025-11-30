using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GPUI
{
    public class UiAnimationComponent : SerializedMonoBehaviour
    {

        [OdinSerialize, NonSerialized] public List<UiAnimationData> animationData = new List<UiAnimationData>();

        Queue<UiAnimationData> animationQueue = new Queue<UiAnimationData>();


        private void Start()
        {

            for (int i = 0; i < animationData.Count; i++)
            {

                if (animationData[i] != null)
                {

                    if (animationData[i].playOnAwake)
                        animationData[i].Play();

                }
                else
                    continue;

            }

        }

        [Button]
        public void Play(string data = "")
        {

            for (int i = 0; i < animationData.Count; i++)
            {

                if (animationData[i] != null)
                {

                    if (animationData[i].trigger == data)
                    {

                        animationQueue.Enqueue(animationData[i]);

                        //StartCoroutine(Process());

                        animationData[i].Play();

                    }

                }
                else
                    continue;

            }

        }

        /*
    private IEnumerator Process()
    {
        if (animationQueue.Count > 0)
        {
            Debug.Log("Animation Queue not Empty");

            if (!animationQueue.Peek().animation.isRunning)
            {
                
                UiAnimationData animationData = animationQueue.Dequeue();

                this.StartCoroutine(animationData.Play());

                yield return null;
                
            }
            else
                yield return null;
        }
        else
            yield return null;
    }
    */

    }
}