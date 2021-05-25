using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject[] spawnObjects;
    public float spawnPeriod, spawnRadius;
    private float timer;

    // Start is called before the first frame update
    void Start()
    {
        timer = 0;
    }

    // Update is called once per frame
    void Update()
    {
        SyncManager inst = FindObjectOfType<SyncManager>();
        if (!inst) return;

        timer += Time.deltaTime;
        if (timer > spawnPeriod) {
            var pos = transform.position;
            pos += new Vector3(Random.Range(-spawnRadius, spawnRadius), Random.Range(-spawnRadius, spawnRadius), -pos.z);
            if (Physics2D.OverlapCircle(pos, .5f) is null) {
                timer = 0;
                Enemy spawnee = inst.CreateEntity(true, false, "lmfao"+Time.realtimeSinceStartup.ToString(), pos.x, pos.y).GetComponent<Enemy>();
            }
            

        }
    }
}
