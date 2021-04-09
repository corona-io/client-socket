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

IEnumerator HandleCreateEvent(string[] tokens) 
    {
        if (tokens[1] == "h")
        {
            GameObject go = Instantiate(
                                playerPrefab,
                                new Vector3(int.Parse(tokens[3]), int.Parse(tokens[4]), 0),
                                new Quaternion(0, 0, 0, 0)
                            );
            entityPool.Add(tokens[2], go);
        }
        else 
        { 
            // TODO: do something else
        }

        yield break; 
    }   
    IEnumerator HandleMoveEvent(string[] tokens) 
    {
        entityPool.TryGetValue(tokens[1], out GameObject obj);
        if (!(obj is null)) 
        {
            obj.transform.position = new Vector3(int.Parse(tokens[2]), int.Parse(tokens[3]), 0);
            // TODO: implement dead reckoning
        }

        yield break;
    }
    IEnumerator HandleAttackEvent(string[] tokens) { yield break; }
    IEnumerator HandleDamageEvent(string[] tokens) { yield break; }
    IEnumerator HandleDeathEvent(string[] tokens) 
    {
        entityPool.TryGetValue(tokens[1], out GameObject obj); 
        if (!(obj is null)) Destroy(obj);
        entityPool.Remove(tokens[1]);
        yield break; 
    }
    IEnumerator HandleLVLUpEvent(string[] tokens) { yield break; }
}
