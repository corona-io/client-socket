using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

class InvalidTokenException : Exception
{

    public InvalidTokenException(string message)
    { 
    
    }
}

public class SyncManager : MonoBehaviour
{
    string message;
    public GameObject playerPrefab;
    Dictionary<string, GameObject> entityPool;
    // Start is called before the first frame update
    void Start()
    {
        entityPool = new Dictionary<string, GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        message = ConnectionManager.GetRecentMessage();
        if (message is null) return;

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

    void CreateEntity(string name, float x, float y) {
        if (entityPool.ContainsKey(name)) return;
        entityPool.Add(name, null);
        GameObject go = Instantiate(
                    playerPrefab,
                    new Vector3(x, y, 0),
                    new Quaternion(0, 0, 0, 0)
                );
        entityPool[name] = go;
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
        entityPool.TryGetValue(tokens[1], out GameObject obj);
        if (obj is null)
        {
            // moving an object that doesnt exist???
            CreateEntity(tokens[1], float.Parse(tokens[2]), float.Parse(tokens[3]));
        }
        else 
        {
            obj.transform.position = new Vector3(float.Parse(tokens[2]), float.Parse(tokens[3]), 0);
            // TODO: implement dead reckoning
        }

        yield break;
    }
    IEnumerator HandleAttackEvent(string[] tokens) { yield break; }
    IEnumerator HandleDamageEvent(string[] tokens) { yield break; }
    IEnumerator HandleDeathEvent(string[] tokens) 
    {
        entityPool.TryGetValue(tokens[1], out GameObject obj);
        if (!(obj is null))
        {
            Destroy(obj);
            entityPool.Remove(tokens[1]);
        }
        yield break; 
    }
    IEnumerator HandleLVLUpEvent(string[] tokens) { yield break; }
}
