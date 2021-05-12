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
            timer = 0;
            var pos = transform.position;
            pos += new Vector3(Random.Range(-spawnRadius, spawnRadius), Random.Range(-spawnRadius, spawnRadius), -pos.z);
            Enemy enem = Instantiate(spawnObjects[Random.Range(0, spawnObjects.Length)], pos, Quaternion.identity).GetComponent<Enemy>();
            enem.enemyName += Time.realtimeSinceStartup.ToString();
            enem.isMine = true;
            inst.SetLocal(enem.enemyName);
        }
    }
}
