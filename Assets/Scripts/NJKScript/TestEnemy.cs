using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestEnemy : Enemy
{
    private State<TestEnemy> enemState;
    private float speed;
    private Rigidbody2D rigidbody;
    private float moveAngle;
    private float attackDelay = 0;

    // Start is called before the first frame update
    void Awake()
    {
        enemState = new CreationState();
        healthPoint = 20;
        speed = 2;
        strength = 1;
        moveAngle = Random.Range(0f, Mathf.PI * 2);
        rigidbody = gameObject.GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        enemState.action(this);
        State<TestEnemy> currState = enemState.UpdateState(this);
        if (currState.GetType() != enemState.GetType()) { enemState = currState; }

        attackDelay -= Time.deltaTime;
    }

    private void OnCollisionEnter(Collision collision)
    {
        Player plr = collision.collider.GetComponent<Player>();
        if (plr && attackDelay <= 0f) 
        {
            plr.Hurt();
            attackDelay = 1f;
        }
    }

    public class CreationState : State<TestEnemy>
    {
        private float timer;
        public override void EnterState(TestEnemy entity)
        {
            base.EnterState(entity);
            entity.GetComponent<SpriteRenderer>().color = new Vector4(1, 1, 1, 0);
            timer = 1f;
        }

        public override State<TestEnemy> UpdateState(TestEnemy entity)
        {
            if (timer <= 0f)
            {
                entity.GetComponent<SpriteRenderer>().color = Color.white;
                ExitState(entity);
                return new IdleState();
            }
            return this;
        }

        public override void StateBehavior(TestEnemy entity)
        {
            timer -= Time.deltaTime;
            entity.GetComponent<SpriteRenderer>().color =
                new Vector4(1, 1, 1, .5f - Mathf.Sin(Mathf.PI * (timer - .5f)) / 2);
        }

        public override void ExitState(TestEnemy entity)
        {
            base.ExitState(entity);
            entity.invTime = 0f;
            entity.attackDelay = 0f;
        }
    }

    public class DeathState : State<TestEnemy>
    {
        private float timer;
        public override void EnterState(TestEnemy entity)
        {
            base.EnterState(entity);
            
            Destroy(entity.GetComponent<Collider2D>());
            Destroy(entity.gameObject, 1.5f);

            timer = 1f;
        }

        public override void ExitState(TestEnemy entity)
        {
            entity.SendDestroyPacket();
            Destroy(entity.gameObject);
        }

        public override void StateBehavior(TestEnemy entity)
        {
            base.StateBehavior(entity);

            if (timer > 0f)
            {
                timer -= Time.deltaTime;
                entity.GetComponent<SpriteRenderer>().color =
                    new Vector4(1, 1, 1, .5f + Mathf.Sin(Mathf.PI * (timer - .5f)) / 2);
            }
            else ExitState(entity);
        }

        public override State<TestEnemy> UpdateState(TestEnemy entity)
        {
            return this;
        }
    }

    public class IdleState : State<TestEnemy>
    {
        public override void EnterState(TestEnemy entity)
        {
            base.EnterState(entity);
        }

        public override void ExitState(TestEnemy entity)
        {

        }

        public override State<TestEnemy> UpdateState(TestEnemy entity)
        {
            if (entity.healthPoint <= 0) return new DeathState();
            if (!entity.isMine) return this;

            var colliders = Physics2D.OverlapCircleAll(entity.transform.position, entity.alertRadius);
                foreach (var x in colliders)
                {

                    if (x.gameObject.GetComponent<Player>())
                    {
                        entity.target = x.transform;
                        ExitState(entity);
                        return new AlertedState();
                    }
                }
            return this;
        }

        public override void StateBehavior(TestEnemy entity)
        {
            entity.invTime -= Time.deltaTime;
            if (!entity.isMine) return;

            var vect = new Vector3(Mathf.Sin(entity.moveAngle), Mathf.Cos(entity.moveAngle), 0);
            entity.rigidbody.velocity = vect.normalized * entity.speed;
            entity.moveAngle += Random.Range(-.05f, .05f);
            
        }
    }

    public class AlertedState : State<TestEnemy>
    {
        public override void EnterState(TestEnemy entity)
        {
            base.EnterState(entity);
        }

        public override void StateBehavior(TestEnemy entity)
        {
            entity.invTime -= Time.deltaTime;
            if (!entity.isMine) return;

            var dir = (entity.target.position - entity.transform.position);
            entity.rigidbody.velocity = dir.normalized * entity.speed;
            
        }

        public override State<TestEnemy> UpdateState(TestEnemy entity)
        {
            if (entity.healthPoint <= 0) return new DeathState();
            if (!entity.isMine) return this;

            if (entity is null) { ExitState(entity); return new IdleState(); }
            var dist = (entity.target.position - entity.transform.position).magnitude;
            if (dist > entity.alertRadius * 1.5f) { ExitState(entity); return new IdleState(); }
            return this;
        }
    }
}
