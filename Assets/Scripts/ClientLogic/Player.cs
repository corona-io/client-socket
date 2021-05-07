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

public partial class Player : MonoBehaviour
{
    private readonly SocketManager socketManager = SocketManager.GetSingleton();

    [SerializeField] public bool isMine;
    [SerializeField] private int healthPoint;
    [SerializeField] private float speed;
    [SerializeField] public string nickname;
    
    private Animator animator;
    private long recieveTime, sendTime = 0;
    private State<Player> nowState;
    private static readonly int IsMoving = Animator.StringToHash("IsMoving");

    public void Attack(Vector3 dir)
    {
        var bullet = ObjectPoolManager.Instance.Dequeue(ObjectPoolManager.PoolingObjects.Bullet);
        bullet.GetComponent<Bullet>().Direction = dir;
        bullet.position = transform.position;
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
            AttackWithInput();
        }
    }

    private void OnApplicationQuit()
    {
        ConnectionManager.PutMessage($"{PacketNames.ohmygod:f},{nickname}");
    }

    private void SendPositionPacket()
    {
        var pos = transform.position;
        var packetString = $"{PacketNames.move:f},{nickname},{pos.x},{pos.y}";
        UIManager.Instance.SetPacketMessage(packetString);

        ConnectionManager.PutMessage(packetString, true, (error) => { sendTime = DateTime.Now.Ticks; });
    }

    private IEnumerator SendPlayerCreatePacket()
    {
        if (!isMine) yield break;

        var pos = transform.position;
        var packetString = $"{PacketNames.create:f},h,{nickname},{pos.x},{pos.y}";
        UIManager.Instance.SetPacketMessage(packetString);
        //text.text = packetString;

        ConnectionManager.PutMessage(packetString, true, (error) => { sendTime = DateTime.Now.Ticks; });
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

    private void AttackWithInput()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            var target = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var dir = target - transform.position;
            float angle = Mathf.Atan2(dir.y, dir.x) - Mathf.PI / 2;
            Attack(new Vector3(-Mathf.Sin(angle), Mathf.Cos(angle), 0));
        }
    }
}

public partial class Player : MonoBehaviour
{
    public class IdleState : State<Player>
    {
        public override State<Player> UpdateState(Player entity)
        {
            return this;
        }
    }

    public class RollingState : State<Player>
    {
        public override State<Player> UpdateState(Player entity)
        {
            throw new NotImplementedException();
        }
    }
}