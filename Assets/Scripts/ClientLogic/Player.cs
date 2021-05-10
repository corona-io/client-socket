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
    [SerializeField] public bool isMine;
    [SerializeField] private int healthPoint;
    [SerializeField] private float speed;
    [SerializeField] public string nickname;
    
    private Animator animator;
    private Rigidbody2D rigidbody;
    private State<Player> nowState;
    private static readonly int IsMoving = Animator.StringToHash("IsMoving");

    public void Attack(Vector3 dir)
    {
        var bullet = ObjectPoolManager.Instance.Dequeue(ObjectPoolManager.PoolingObjects.Bullet);
        bullet.GetComponent<Bullet>().Direction = dir;
        bullet.position = transform.position;
    }

    public void Hurt()
    {
        
    }
    
    private void Start()
    {
        animator = GetComponent<Animator>();
        rigidbody = GetComponent<Rigidbody2D>();
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

        ConnectionManager.PutMessage(packetString, true, (error) => { });
    }

    private IEnumerator SendPlayerCreatePacket()
    {
        if (!isMine) yield break;

        var pos = transform.position;
        var packetString = $"{PacketNames.create:f},h,{nickname},{pos.x},{pos.y}";
        UIManager.Instance.SetPacketMessage(packetString);
        //text.text = packetString;

        ConnectionManager.PutMessage(packetString, true, (error) => { });
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
                horizon = -1;
            }
            else if(horizon > 0)
            {
                scale.x = x;
                horizon = 1;
                transform.localScale = scale;
            }

            vertical = vertical != 0 ? vertical < 0 ? -1 : 1 : 0;

            animator.SetBool(IsMoving, true);
            Debug.Log($"horizon : {horizon}, vertical : {vertical}");
        }
        else
        {
            animator.SetBool(IsMoving, false);
        }
        
        rigidbody.velocity = new Vector2(horizon, vertical).normalized * (speed);
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
            if (Input.GetKeyDown(KeyCode.Space))
            {
                return new RollingState();
            }

            return this;
        }
    }

    public class RollingState : State<Player>
    {
        private float leftRollingTime;

        public override void EnterState(Player entity)
        {
            base.EnterState(entity);
            leftRollingTime = 0.5f;
        }

        public override State<Player> UpdateState(Player entity)
        {
            if (leftRollingTime > 0f)
            {
                return this;
            }
            return new IdleState();
        }

        public override void StateBehavior(Player entity)
        {
            base.StateBehavior(entity);
            leftRollingTime -= Time.deltaTime;
        }

        public override void ExitState(Player entity)
        {
            base.ExitState(entity);
        }
    }
}