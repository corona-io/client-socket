using System;
using System.Collections;
using UnityEngine;

namespace MyExtensions
{
    public static class MonoBehaviourExtensions
    {
        public static IEnumerator ChangeWithDelay<T>(this T origin, T changeValue, 
            float delay, Action<T> makeResult)
        {
            yield return new WaitForSeconds(delay);
            makeResult(changeValue);
        }
    }
}