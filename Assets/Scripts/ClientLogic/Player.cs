using System;
using System.Collections;
using System.Collections.Generic;
using ClientLogic.Singleton;
using UnityEngine;
using UnityEngine.UI;

public enum PacketNames
{
    create,
    move,
    attack,
    damage,
    ohmygod,
    lv999boss,
    None
}

public class Player : MonoBehaviour
{
    private readonly SocketManager socketManager = SocketManager.GetSingleton();

    [SerializeField] private bool isMine;
    [SerializeField] private int healthPoint;
    [SerializeField] private float speed;
    [SerializeField] private string nickname;

    private bool established;
    
    private long recieveTime, sendTime = 0;

    private void Start()
    {
        //socketManager.SocketInit("ws://hojoondev.kro.kr:3001", true);
        
        //socketManager.AddOpenEvent((sender, e) =>
        //{
        //    print("Established!");
        //    established = true;
            //StartCoroutine(SendPositionInfinitely());
        //});
        
        //socketManager.SocketConnect(true);
        //socketManager.AddMessageEvent(
        //    (sender, e) => 
        //    {
        //        recieveTime = DateTime.Now.Ticks;
        //        print($"It took {(recieveTime - sendTime) / 10000} milliseconds\n To recieve \"{e.Data}\"");
        //    }
        //);
        //socketManager.AddCloseEvent((sender, e) => { print("Connection Closed"); });
        StartCoroutine(SendPositionInfinitely());
        StartCoroutine(SendPlayerCreatePacket());
        StartCoroutine(SendPositionInfinitely());
    }
    
    private void Update()
    {
        if (isMine)
        {
            MoveWithInput();
        }
    }

    private void OnApplicationQuit()
    {
        //socketManager.SocketSend($"{PacketNames.ohmygod:f},{nickname}");
        //socketManager.SocketClose();
        ConnectionManager.PutMessage($"{PacketNames.ohmygod:f},{nickname}");
    }

    private void SendPositionPacket()
    {
        var pos = transform.position;
        var packetString = $"{PacketNames.move:f},{nickname},{pos.x},{pos.y}";
        //text.text = packetString;

        ConnectionManager.PutMessage(packetString, true, (error) => { sendTime = DateTime.Now.Ticks; });
        //socketManager.SocketSend(packetString, true );
    }

    private IEnumerator SendPlayerCreatePacket()
    {
        if (!isMine) yield break;

        var pos = transform.position;
        var packetString = $"{PacketNames.create:f},h,{nickname},{pos.x},{pos.y}";
        //text.text = packetString;

        ConnectionManager.PutMessage(packetString, true, (error) => { sendTime = DateTime.Now.Ticks; });
        //socketManager.SocketSend(packetString, true, (error) => { sendTime = DateTime.Now.Ticks; });
    }

    private IEnumerator SendPositionInfinitely()
    {

        Debug.Log("Start sending position coroutine");
        while (true)
        {
            SendPositionPacket();
            yield return new WaitForSeconds(.1f);
        }
    }

    private void MoveWithInput()
    {
        var horizon = Input.GetAxis("Horizontal");
        var vertical = Input.GetAxis("Vertical");

        transform.Translate(new Vector3(horizon, vertical) * (speed * Time.deltaTime));
    }

    private string[] SplitPacket(string packet)
    {
        if (packet.StartsWith("brodcast: "))
        {
            return packet.Substring(10).Split(',');
        }

        return new string[0];
    }

    private PacketNames ConvertPacketToEnum(string packet)
    {
        return packet switch
        {
            "create" => PacketNames.create,
            "move" => PacketNames.move,
            "attack" => PacketNames.attack,
            "damage" => PacketNames.damage,
            "ohmygod" => PacketNames.ohmygod,
            "lv999boss" => PacketNames.lv999boss,
            _ => PacketNames.None
        };
    }

    private void ExecuteActionByPacket(string[] packet)
    {
        if (packet.Length <= 0) return;

        /*
        ConvertPacketToEnum(packet[0]) switch
        {
            PacketNames.create => ,
            PacketNames.move => ,
            PacketNames.attack => ,
            PacketNames.damage => ,
            PacketNames.ohmygod => ,
            PacketNames.lv999boss => ,
            PacketNames.None => ,
            _ => 
        };
        */
    }
}