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
    private Animator animator;
    
    private long recieveTime, sendTime = 0;
    private static readonly int IsMoving = Animator.StringToHash("IsMoving");

    public void Attack(Vector2 dir)
    {
        
    }
    
    private void Start()
    {
        animator = GetComponent<Animator>();
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
        while (isMine)
        {
            SendPositionPacket();
            yield return new WaitForSeconds(.1f);
        }
    }

    private void MoveWithInput()
    {
        var horizon = Input.GetAxis("Horizontal");
        var vertical = Input.GetAxis("Vertical");

        if (horizon + vertical != 0)
        {
            var scale = transform.localScale;
            var x = Mathf.Abs(scale.x);

            if (horizon < 0)
            {
                x *= -1;
                scale.x = x;
                transform.localScale = scale;
            }
            else if(horizon > 0)
            {
                scale.x = x;
                transform.localScale = scale;
            }
            
            transform.Translate(new Vector3(horizon, vertical) * (speed * Time.deltaTime));

            animator.SetBool(IsMoving, true);
        }
        else
        {
            animator.SetBool(IsMoving, false);
        }
    }
}