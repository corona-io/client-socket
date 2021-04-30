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
    public string localPlayerName;
    public Player playerPrefab;
    
    Dictionary<string, GameObject> entityPool;
    Dictionary<string, int> mutexPool;
    Dictionary<string, float> lastPacket;
    Dictionary<string, Vector3> lastPos;
    Dictionary<string, Vector3> lastVelo;

    // Start is called before the first frame update
    void Start()
    {
        entityPool = new Dictionary<string, GameObject>();
        mutexPool = new Dictionary<string, int>();
        lastPacket = new Dictionary<string, float>();
        lastPos = new Dictionary<string, Vector3>();
        lastVelo = new Dictionary<string, Vector3>();
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
        var go = Instantiate(
                    playerPrefab,
                    new Vector3(x, y, 0),
                    new Quaternion(0, 0, 0, 0)
                );
        go.nickname = name;
        entityPool[name] = go.gameObject;
        mutexPool.Add(name, 0);
        lastPacket.Add(name, Time.time);
        lastPos.Add(name, new Vector3(x, y, 0));
        lastVelo.Add(name, new Vector3(0, 0, 0));
    }

    IEnumerator HandleCreateEvent(string[] tokens) 
    {
        var name = tokens[2];
        if (name == localPlayerName) yield break;
        entityPool.TryGetValue(tokens[1], out GameObject obj);
        if (obj is null)
        {
            if (tokens[1] == "h")
            {
                if (!name.Equals(localPlayerName))
                    CreateEntity(name, float.Parse(tokens[3]), float.Parse(tokens[4]));
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
        if (name == localPlayerName) yield break;
        entityPool.TryGetValue(name, out GameObject obj);
        if (obj is null)
        {
            // moving an object that doesnt exist???
            CreateEntity(name, float.Parse(tokens[2]), float.Parse(tokens[3]));
        }
        else 
        {
            print($"MOVE CMD: {name} {tokens[2]} {tokens[3]}");

            Vector3 newPos, velocity;
            float lastMoveTime;

            newPos = new Vector3(float.Parse(tokens[2]), float.Parse(tokens[3]), 0);
            velocity = (newPos - lastPos[name]) / (Time.time - lastPacket[name]);
            
            // Sanitizing Inputs
            if (velocity.sqrMagnitude > 10000f) velocity = lastVelo[name];
            
            lastMoveTime = lastPacket[name] = Time.time;        
            lastPos[name] = newPos;

            mutexPool[name]++;
            while (mutexPool[name] > 1) yield return new WaitForEndOfFrame();



            while (mutexPool[name] < 2)
            {
                obj.transform.position += 
                    velocity * (Time.time - lastMoveTime)
                +   0.5f * (velocity - lastVelo[name]) * (Time.time - lastMoveTime) * (Time.time - lastMoveTime);

                lastMoveTime = Time.time;
                yield return new WaitForEndOfFrame();
            }
            obj.transform.position = newPos;
            lastVelo[name] = velocity;
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
            lastVelo.Remove(name);
        }
        yield break; 
    }
    IEnumerator HandleLVLUpEvent(string[] tokens) { yield break; }
}
