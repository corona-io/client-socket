using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.UI;

class InvalidTokenException : Exception
{

    public InvalidTokenException(string message)
    { 
    
    }
}

public class SyncManager : MonoBehaviour
{
    string? message;
    public GameObject playerPrefab;
    
    Dictionary<string, GameObject> entityPool;
    Dictionary<string, int> mutexPool;
    Dictionary<string, float> lastPacket;
    Dictionary<string, Vector3> lastPos;

    // Start is called before the first frame update
    void Start()
    {
        entityPool = new Dictionary<string, GameObject>();
        mutexPool = new Dictionary<string, int>();
        lastPacket = new Dictionary<string, float>();
        lastPos = new Dictionary<string, Vector3>();
    }

    // Update is called once per frame
    void Update()
    {
        message = ConnectionManager.GetRecentMessage();
        if (!(message is null))
        {
            string[] splitMessage = message.Split(',');
            IEnumerator routine = splitMessage[0] switch
            {
                "create" => HandleCreateEvent(splitMessage),
                "move" => HandleMoveEvent(splitMessage),
                "attack" => HandleAttackEvent(splitMessage),
                "damage" => HandleDamageEvent(splitMessage),
                "ohmygod" => HandleDeathEvent(splitMessage),
                "lv999boss" => HandleLVLUpEvent(splitMessage),
                _ => throw new NotSupportedException()
            };
            StartCoroutine(routine);
        }
    }

    void CreateEntity(string name, float x, float y) {
        if (entityPool.ContainsKey(name)) return;
        entityPool.Add(name, null);
        GameObject go = Instantiate(
                    playerPrefab,
                    new Vector3(x, y, 0),
                    new Quaternion(0, 0, 0, 0)
                );
        entityPool[name] = go;
        mutexPool.Add(name, 0);
        lastPacket.Add(name, Time.time);
        lastPos.Add(name, new Vector3(x,y,0));
    }

    IEnumerator HandleCreateEvent(string[] tokens) 
    {
        entityPool.TryGetValue(tokens[1], out GameObject obj);
        if (obj is null)
        {
            if (tokens[1] == "h")
            {
                CreateEntity(tokens[2], float.Parse(tokens[3]), float.Parse(tokens[4]));
            }
            else
            {
                // TODO: do something else
            }
        }

        yield break; 
    }   
    IEnumerator HandleMoveEvent(string[] tokens) 
    {
        var name = tokens[1];

        entityPool.TryGetValue(name, out GameObject obj);
        if (obj is null)
        {
            // moving an object that doesnt exist???
            CreateEntity(name, float.Parse(tokens[2]), float.Parse(tokens[3]));
        }
        else 
        {
            mutexPool[name]++;
            while (mutexPool[name] > 1) yield return new WaitForEndOfFrame();
            
            Vector3 newPos = new Vector3(float.Parse(tokens[2]), float.Parse(tokens[3]), 0);
            Vector3 velocity = (newPos - lastPos[name]) / (Time.time - lastPacket[name]);
            float lastMovetime;
            
            print($"{velocity}");
            obj.transform.position = lastPos[name] = newPos;
            lastPacket[name] = lastMovetime = Time.time;

            
            while (mutexPool[name] < 2 && obj)
            {
                obj.transform.position += velocity * (Time.time - lastMovetime);
                lastMovetime = Time.time;
                yield return new WaitForEndOfFrame();
                // TODO: implement dead reckoning
            }
            mutexPool[name]--;
        }

        yield break;
    }
    IEnumerator HandleAttackEvent(string[] tokens) { yield break; }
    IEnumerator HandleDamageEvent(string[] tokens) { yield break; }
    IEnumerator HandleDeathEvent(string[] tokens) 
    {
        var name = tokens[1];

        entityPool.TryGetValue(name, out GameObject obj);
        if (!(obj is null))
        {
            Destroy(obj);
            entityPool.Remove(name);
            mutexPool.Remove(name);
            lastPacket.Remove(name);
            lastPos.Remove(name);
        }
        yield break; 
    }
    IEnumerator HandleLVLUpEvent(string[] tokens) { yield break; }
}
