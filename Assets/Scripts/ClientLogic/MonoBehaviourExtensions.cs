using System;
using System.Collections;
using System.Text;
using UnityEngine;

namespace MyExtensions
{
    public static class MonoBehaviourExtensions
    {
        private static StringBuilder stringBuilder = new StringBuilder();
        
        public static IEnumerator ChangeWithDelay<T>(this T origin, T changeValue, 
            float delay, Action<T> makeResult)
        {
            yield return new WaitForSeconds(delay);
            makeResult(changeValue);
        }

        public static void Log(this object origin, string header = "")
        {
#if UNITY_EDITOR
            stringBuilder.Capacity = 0;
            Debug.Log(stringBuilder.Append(header).Append(origin).ToString());
#endif
        }
    }
}