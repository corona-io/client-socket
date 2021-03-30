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
    
    void Start()
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
    }

    // Update is called once per frame
    void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Space)) return;
        
        var pos = transform.position;
        socketManager.SocketSend($"{gameObject.name},{pos.x},{pos.y}", true, (error) => {sendTime = DateTime.Now.Ticks; });
    }
}
