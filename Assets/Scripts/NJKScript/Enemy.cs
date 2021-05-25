using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    protected int healthPoint;
    protected float invTime = 99f;
    public Transform target;

    public string enemyName;
    public bool isMine;
    public float alertRadius;

    public void Hurt(int dmg) 
    { 
        if (invTime <= 0f) { healthPoint -= dmg; invTime = .5f; } 
    }

    void OnDestroy()
    {
        var packetString = $"{PacketNames.create:f},{enemyName}";
        ConnectionManager.PutMessage(packetString, false, null);
    }

    void Start()
    {
        StartCoroutine(SendCreatePacket());
        StartCoroutine(SendPositionInfinitely());
    }
    
    public void SendDestroyPacket()
    {
        var packetString = $"{PacketNames.ohmygod:f},{enemyName}";
        ConnectionManager.PutMessage(packetString, false, null);
    }

    private IEnumerator SendCreatePacket()
    {
        if (!isMine) yield break;

        var pos = transform.position;
        var packetString = $"{PacketNames.create:f},enemy,{enemyName},{pos.x},{pos.y}";
        ConnectionManager.PutMessage(packetString, true, null);
    }

    private IEnumerator SendPositionInfinitely()
    {
        while (isMine)
        {
            var pos = transform.position;
            var packetString = $"{PacketNames.move:f},{enemyName},{pos.x},{pos.y}";
            ConnectionManager.PutMessage(packetString, true, null);
            yield return new WaitForSeconds(.1f);
        }
    }
}
