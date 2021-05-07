using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    protected int healthPoint;
    protected float invTime = 99f;
    public Transform target;

    public string enemyName;
    public float alertRadius;

    public void Hurt(int dmg) 
    { 
        if (invTime <= 0f) { healthPoint -= dmg; invTime = .5f; } 
    }
}
