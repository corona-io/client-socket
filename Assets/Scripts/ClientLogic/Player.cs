using System;
using System.Collections;
using System.Collections.Generic;
using ClientLogic.Singleton;
using MyExtensions;
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
    shot,
    roll,
    None
}

public partial class Player : MonoBehaviour
{
    [SerializeField] public bool isMine;
    [SerializeField] private int healthPoint;
    [SerializeField] private float speed;
    [SerializeField] public string nickname;

    [HideInInspector] public bool isRolling;
    [HideInInspector] public float horizontal;
    [HideInInspector] public float vertical;
    
    private Animator animator;
    private Rigidbody2D rigidbody;
    private SpriteRenderer renderer;
    private State<Player> nowState;
    private bool isInvincibility;
    
    private static readonly int IsMoving = Animator.StringToHash("IsMoving");

    public int HealthPoint
    {
        get => healthPoint;
        set => healthPoint = value;
    }

    public void Attack(Vector3 dir)
    {
        var bullet = ObjectPoolManager.Instance.Dequeue(ObjectPoolManager.PoolingObjects.Bullet);
        bullet.GetComponent<Bullet>().Direction = dir;
        bullet.position = transform.position;
        bullet.gameObject.layer = gameObject.layer;
        bullet.GetComponent<Bullet>().shooterName = nickname;
        
        var bulletSprite = bullet.GetComponent<SpriteRenderer>();
        bulletSprite.sortingLayerName = renderer.sortingLayerName;
        bulletSprite.sortingOrder = renderer.sortingOrder;
    }

    public void Hurt()
    {
        if (!isRolling && !isInvincibility)
        {
            isInvincibility = true;
            StartCoroutine(isInvincibility.ChangeWithDelay(false, .2f, (value) => isInvincibility = value));
            healthPoint -= 10;

            healthPoint.Log("hp : ");
        }
    }
    
    private void Start()
    {
        animator = GetComponent<Animator>();
        rigidbody = GetComponent<Rigidbody2D>();
        renderer = GetComponent<SpriteRenderer>();
        StartCoroutine(SendPlayerCreatePacket());
        StartCoroutine(SendPositionInfinitely());
        
        nowState = new IdleState();
    }
    
    private void Update()
    {
        nowState.action(this);
        var currState = nowState.UpdateState(this);
        if (currState.GetType() != nowState.GetType())
        {
            nowState = currState;
            Debug.Log(nowState.GetType());
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
        if (!isMine) return;
        
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
        }
        else
        {
            animator.SetBool(IsMoving, false);
        }
        
        rigidbody.velocity = new Vector2(horizon, vertical).normalized * (speed);
        
        if (Input.GetKeyDown(KeyCode.Space) && (horizon != 0 || vertical != 0))
        {
            isRolling = true;
            horizontal = horizon;
            this.vertical = vertical;
        }
    }

    private void AttackWithInput()
    {
        if (!isMine) return;
        
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            var target = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var position = transform.position;
            var dir = target - position;
            float angle = Mathf.Atan2(dir.y, dir.x) - Mathf.PI / 2;
            var direction = new Vector3(-Mathf.Sin(angle), Mathf.Cos(angle), 0);

            var packetString = $"{PacketNames.shot:f},{nickname},{position.x},{position.y},{direction.x},{direction.y}";
            ConnectionManager.PutMessage(packetString, true, (error) => { });
            
            Attack(direction);
        }
    }
}

public partial class Player : MonoBehaviour
{
    private static readonly int StartRolling = Animator.StringToHash("StartRolling");

    public class IdleState : State<Player>
    {
        public override State<Player> UpdateState(Player entity)
        {
            if (entity.isRolling)
            {
                if (entity.isMine)
                {
                    var packetString = $"{PacketNames.roll:f},{entity.nickname},{entity.horizontal},{entity.vertical}";
                    ConnectionManager.PutMessage(packetString, true, (error) => { });
                }
                
                return new RollingState(entity.horizontal, entity.vertical);
            }

            return this;
        }

        public override void StateBehavior(Player entity)
        {
            base.StateBehavior(entity);
            entity.MoveWithInput();
            entity.AttackWithInput();
        }
    }

    public class RollingState : State<Player>
    {
        private float leftRollingTime;
        private float horizontal, vertical;

        private const float MaxRollingTime = 0.5f;

        public RollingState(float horizontal, float vertical)
        {
            this.horizontal = horizontal;
            this.vertical = vertical;
        }

        public override void EnterState(Player entity)
        {
            base.EnterState(entity);
            leftRollingTime = MaxRollingTime;
            entity.rigidbody.velocity = new Vector2(horizontal, vertical) * entity.speed * 1.5f;
            entity.animator.SetTrigger(StartRolling);
        }

        public override State<Player> UpdateState(Player entity)
        {
            if (leftRollingTime > 0f)
            {
                return this;
            }

            ExitState(entity);
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
            entity.isRolling = false;
            entity.rigidbody.velocity = Vector2.zero;
        }
    }
}