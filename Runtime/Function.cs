using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;


[System.Serializable]
public class Function
{

    [SerializeField]
    [FoldoutGroup("Function Block")] public UnityEvent functionName;
    [FoldoutGroup("Function Block")] public float functionDelay;


    public static void InvokeEvent(Function eventFunctions, MonoBehaviour eventSender)
    {

        eventSender.StartCoroutine(InvokeEventWithDelay(eventFunctions));

    }

    public static void RevokeEvent(MonoBehaviour eventSender)
    {

        eventSender.CancelInvoke("InvokeEventList");

    }

    static IEnumerator InvokeEventWithDelay(Function eventFunctions)
    {

        yield return new WaitForSecondsRealtime(eventFunctions.functionDelay);

        eventFunctions.functionName?.Invoke();

    }

}