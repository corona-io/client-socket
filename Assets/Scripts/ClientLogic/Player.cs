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

    [SerializeField] public bool isMine;
    [SerializeField] private int healthPoint;
    [SerializeField] private float speed;
    [SerializeField] public string nickname;

    private bool established;
    
    private long recieveTime, sendTime = 0;

    public void Attack(Vector2 dir)
    {
        
    }
    
    private void Start()
    {
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
        UIManager.Instance.SetPacketMessage(packetString);
        //text.text = packetString;

        ConnectionManager.PutMessage(packetString, true, (error) => { sendTime = DateTime.Now.Ticks; });
        //socketManager.SocketSend(packetString, true );
    }

    private IEnumerator SendPlayerCreatePacket()
    {
        if (!isMine) yield break;

        var pos = transform.position;
        var packetString = $"{PacketNames.create:f},h,{nickname},{pos.x},{pos.y}";
        UIManager.Instance.SetPacketMessage(packetString);
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
}