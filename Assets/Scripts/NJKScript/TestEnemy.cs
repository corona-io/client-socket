using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestEnemy : Enemy
{
    private State<TestEnemy> enemState;
    private float speed;
    private float moveAngle;
    private bool alive;
    
    // Start is called before the first frame update
    void Awake()
    {
        enemState = new CreationState();
        healthPoint = 1;
        speed = 4;
        moveAngle = Random.Range(0f, Mathf.PI * 2);
    }

    void Update()
    {
        enemState.action(this);
        State<TestEnemy> currState = enemState.UpdateState(this);

        if (currState.GetType() != enemState.GetType()) { enemState = currState; }
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
        }
    }

    public class DeathState : State<TestEnemy>
    {
        private float timer;
        public override void EnterState(TestEnemy entity)
        {
            base.EnterState(entity);
            
            Destroy(entity.alertArea);
            Destroy(entity.GetComponent<Collider2D>());
            Destroy(entity.gameObject, 1.5f);

            entity.alive = false; timer = 1f;
        }

        public override void StateBehavior(TestEnemy entity)
        {
            base.StateBehavior(entity);
            timer -= Time.deltaTime;
            entity.GetComponent<SpriteRenderer>().color =
                new Vector4(1, 1, 1, .5f + Mathf.Sin(Mathf.PI * (timer - .5f)) / 2);
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
            entity.alive = true;
        }

        public override void ExitState(TestEnemy entity)
        {
            
        }

        public override State<TestEnemy> UpdateState(TestEnemy entity)
        {
            if (entity.healthPoint <= 0) return new DeathState();

            var colliders = new List<Collider2D>();
            entity.alertArea.OverlapCollider(new ContactFilter2D(), colliders);

            foreach (var x in colliders) {
                
                if (x.gameObject.GetComponent<Player>())
                {
                    entity.target = x.transform;
                    return new AlertedState();
                }
            }

            return this;
        }

        public override void StateBehavior(TestEnemy entity)
        {
            var vect = new Vector3(Mathf.Sin(entity.moveAngle), Mathf.Cos(entity.moveAngle), 0);
            entity.transform.Translate(vect.normalized * entity.speed * Time.deltaTime);
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
            var dir = (entity.target.position - entity.transform.position).normalized;
            entity.transform.Translate(dir * entity.speed * Time.deltaTime);
        }

        public override State<TestEnemy> UpdateState(TestEnemy entity)
        {
            if (entity.healthPoint <= 0) return new DeathState();

            var radius = entity.alertArea.radius;
            var dist = (entity.target.position - entity.transform.position).magnitude;
            if (dist > radius * 1.5f) return new IdleState();
            return this;
        }
    }
}
