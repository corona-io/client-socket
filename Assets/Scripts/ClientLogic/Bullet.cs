using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class Bullet : MonoBehaviour
{
    private const int EnemyLayer = 3;
    private const float Speed = 50f;
    
    public Vector3 Direction { get; set; }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Direction * (Time.deltaTime * Speed));
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == EnemyLayer)
        {
            var enemy = other.GetComponent<Enemy>();
            enemy.Hurt(10);
        }
    }
}
