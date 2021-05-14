using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class Bullet : MonoBehaviour
{
    private const int EnemyLayer = 3;
    private const int PlayerLayer = 6;
    private const float Speed = 10f;

    public string shooterName;

    public Vector3 Direction { get; set; }

    void Update()
    {
        transform.Translate(Direction * (Time.deltaTime * Speed));
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            Debug.Log("enemy enter");
            var enemy = other.GetComponent<Enemy>();
            enemy.Hurt(10);
        }
        
        Debug.Log($"trigger : {other.name}, trigger? : {other.isTrigger}");

        if (!CanPassLayer(gameObject.layer, other.gameObject.layer, other.tag) && !other.isTrigger)
        {
            Debug.Log($"trigger disable: {other.name}");
            gameObject.SetActive(false);
        }

        if (!other.CompareTag("Player") && !other.isTrigger)
        {
            Debug.Log($"trigger disable: {other.name}");
            gameObject.SetActive(false);
        }
    }

    private bool CanPassLayer(int origin, int otherLayer, string tag)
    {
        return (tag.Equals("Untagged") && origin > otherLayer) || tag.Equals("Player");
    }
}
