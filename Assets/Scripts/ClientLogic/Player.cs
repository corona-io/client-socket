using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private SocketManager socketManager = SocketManager.GetSingleton();
    
    private bool isMine;
    private int healthPoint;
    private float speed;
    private bool established;
    
    private long recieveTime, sendTime = 0;
    
    private void Start()
    {
        socketManager.SocketInit("ws://hojoondev.kro.kr:3001", true);
        
        socketManager.AddOpenEvent((sender, e) => { print("Established!"); });
        
        socketManager.SocketConnect(true);
        socketManager.AddMessageEvent(
            (sender, e) => 
            {
                recieveTime = DateTime.Now.Ticks;
                print($"It took {(recieveTime - sendTime) / 10000} milliseconds\n To recieve \"{e.Data}\"");
            }
        );
        socketManager.AddCloseEvent((sender, e) => { print("Connection Closed"); });

        StartCoroutine(SendPositionInfinitely());
    }

    // Update is called once per frame
    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Space)) return;
    }

    private void SendPositionPacket()
    {
        var pos = transform.position;
        socketManager.SocketSend($"{gameObject.name},{pos.x},{pos.y}", true, (error) => {sendTime = DateTime.Now.Ticks; });
    }

    private IEnumerator SendPositionInfinitely()
    {
        var time = 0f;
        while (true)
        {
            if (time >= .1f)
            {
                SendPositionPacket();
                time = 0f;
            }
            
            time += Time.deltaTime;
            yield return null;
        }
    }
}
