using System;
using UnityEngine;

public class PoolingObject : MonoBehaviour
{
    [SerializeField] private ObjectPoolManager.PoolingObjects type;
    
    private void OnDisable()
    {
        ObjectPoolManager.Instance.Enqueue(type, transform);
    }
}