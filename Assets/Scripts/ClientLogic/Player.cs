using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PacketNames
{
    create,
    move,
    attack,
    damage,
    ohmygod,
    lv999boss
}

public class Player : MonoBehaviour
{
    private SocketManager socketManager = SocketManager.GetSingleton();
    
    [SerializeField] private bool isMine;
    [SerializeField] private int healthPoint;
    [SerializeField] private float speed;
    private bool established;
    
    private long recieveTime, sendTime = 0;
    
    private void Start()
    {
        socketManager.SocketInit("ws://hojoondev.kro.kr:3001", true);
        
        socketManager.AddOpenEvent((sender, e) =>
        {
            print("Established!");
            established = true;
            //StartCoroutine(SendPositionInfinitely());
        });
        
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

    private void Update()
    {
        if (isMine)
        {
            MoveWithInput();
        }
    }

    private void SendPositionPacket()
    {
        var pos = transform.position;
        socketManager.SocketSend($"{PacketNames.move:f},{gameObject.name},{pos.x},{pos.y}", true, (error) => {sendTime = DateTime.Now.Ticks; });
    }

    private IEnumerator SendPositionInfinitely()
    {
        Debug.Log("Start coroutine");
        while (true)
        {
            if(established) SendPositionPacket();
            yield return new WaitForSeconds(.1f);
        }

        yield return null;
    }

    private void MoveWithInput()
    {
        var horizon = Input.GetAxis("Horizontal");
        var vertical = Input.GetAxis("Vertical");

        transform.Translate(new Vector3(horizon, vertical) * (speed * Time.deltaTime));
    }
}
