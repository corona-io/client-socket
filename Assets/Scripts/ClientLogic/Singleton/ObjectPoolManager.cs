using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ObjectPoolManager : Singleton<ObjectPoolManager>
{
    public enum PoolingObjects : int
    {
        Bullet = 1
    }
    
    private Dictionary<PoolingObjects, Queue<Transform>> objectQueues = Enum.GetValues(typeof(PoolingObjects))
        .OfType<PoolingObjects>()
        .ToDictionary(x => x, x => new Queue<Transform>());

    public void Enqueue(PoolingObjects type, Transform obj)
    {
        objectQueues[type].Enqueue(obj);
    }

    public Transform Dequeue(PoolingObjects type)
    {
        Transform obj;
        
        if (objectQueues[type].Count > 0)
        {
            obj = objectQueues[type].Dequeue();
            obj.gameObject.SetActive(true);
        }
        else
        {
            obj = Instantiate<Transform>(Resources.Load<Transform>(TypeToPath(type)));
        }

        return obj;
    }

    private static string TypeToPath(PoolingObjects type)
    {
        return type switch
        {
            PoolingObjects.Bullet => @"Prefabs/Bullet",
            _ => @""
        };
    }
}
