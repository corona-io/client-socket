using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UIView : MonoBehaviour
{
    public UnityEvent StartAction;
    public UnityEvent UpdateAction;

    private void Start()
    {
        StartAction?.Invoke();
    }

    private void Update()
    {
        UpdateAction?.Invoke();
    }
}
