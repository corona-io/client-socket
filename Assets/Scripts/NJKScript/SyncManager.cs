using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

class InvalidTokenException : Exception
{

    public InvalidTokenException(string message)
    { 
    
    }
}

public class SyncManager : MonoBehaviour
{
    int recievedPlayerInitPackets = 0;
    List<string[]> playerInitPackets;
    string? message;
    public string localPlayerName;
    public Player playerPrefab;
    public Enemy enemyPrefab;

    Dictionary<string, bool> isLocalEntity;
    Dictionary<string, GameObject> entityPool;
    Dictionary<string, int> mutexPool;
    Dictionary<string, float> lastPacket;
    Dictionary<string, Vector3> lastPos;
    Dictionary<string, Vector3> lastVelo;

    public void SetLocal(string entityName) 
    {
        isLocalEntity.Add(entityName, true);
    }

    // Start is called before the first frame update
    void Start()
    {
        playerInitPackets = new List<string[]>();
        entityPool = new Dictionary<string, GameObject>();
        mutexPool = new Dictionary<string, int>();
        lastPacket = new Dictionary<string, float>();
        lastPos = new Dictionary<string, Vector3>();
        lastVelo = new Dictionary<string, Vector3>();
        isLocalEntity = new Dictionary<string, bool>();
    }

    void InitializePlayers() 
    {
        string[] names = playerInitPackets[0];
        string[] hp = playerInitPackets[1];
        string[] xPos = playerInitPackets[2];
        string[] yPos = playerInitPackets[3];
        for (int i = 0; i < names.Length; i++)
        {
            if (float.TryParse(hp[i], out float curr_hp) && float.TryParse(xPos[i], out float curr_xPos) && float.TryParse(yPos[i], out float curr_yPos)) {
                Player plrObj = CreateEntity(false, true, names[i], curr_xPos, curr_yPos).GetComponent<Player>();
                plrObj.HealthPoint = (int)curr_hp;
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        message = ConnectionManager.GetRecentMessage();
        if (!(message is null))
        {
            string[] splitMessage = message.Split(',');
            switch (splitMessage[0]) 
            {
                case "create": StartCoroutine(HandleCreateEvent(splitMessage)); break;
                case "move": StartCoroutine(HandleMoveEvent(splitMessage)); break;
                case "shot": StartCoroutine(HandleProjectileEvent(splitMessage)); break;
                case "attack": StartCoroutine(HandleAttackEvent(splitMessage)); break;
                case "damage": StartCoroutine(HandleDamageEvent(splitMessage)); break;
                case "ohmygod": StartCoroutine(HandleDeathEvent(splitMessage)); break;
                case "lv999boss": StartCoroutine(HandleLVLUpEvent(splitMessage)); break;
                case "roll": StartCoroutine(HandleRollEvent(splitMessage)); break;
                case "undefined":
                    playerInitPackets.Add(new string[] { }); recievedPlayerInitPackets++;
                    if (recievedPlayerInitPackets == 4) InitializePlayers();
                    break;
                default:
                    print(message);
                    playerInitPackets.Add(splitMessage); recievedPlayerInitPackets++;
                    if (recievedPlayerInitPackets == 4) InitializePlayers();
                    break;
            };
        }
    }

    public GameObject CreateEntity(bool isMine, bool isPlayer, string name, float x, float y) {
        if (entityPool.ContainsKey(name)) return null;
        entityPool.Add(name, null);

        if (isMine) isLocalEntity.Add(name, true);

        if (isPlayer)
        {
            var go = Instantiate(
                    playerPrefab,
                    new Vector3(x, y, 0),
                    new Quaternion(0, 0, 0, 0)
                );
            go.nickname = name;
            go.isMine = isMine;
            entityPool[name] = go.gameObject;
            mutexPool.Add(name, 0);
            lastPacket.Add(name, Time.time);
            lastPos.Add(name, new Vector3(x, y, 0));
            lastVelo.Add(name, new Vector3(0, 0, 0));

            return go.gameObject;
        }

        else 
        {
            var go = Instantiate(
                    enemyPrefab,
                    new Vector3(x, y, 0),
                    new Quaternion(0, 0, 0, 0)
            );
            go.enemyName = name;
            go.isMine = isMine;
            entityPool[name] = go.gameObject;
            mutexPool.Add(name, 0);
            lastPacket.Add(name, Time.time);
            lastPos.Add(name, new Vector3(x, y, 0));
            lastVelo.Add(name, new Vector3(0, 0, 0));

            return go.gameObject;
        }
    }

    IEnumerator HandleCreateEvent(string[] tokens) 
    {
        var name = tokens[2];
        if (isLocalEntity.ContainsKey(name)) yield break;
        entityPool.TryGetValue(tokens[1], out GameObject obj);
        if (obj is null)
        {
            if (tokens[1] == "h")
            {
                if (!name.Equals(localPlayerName))
                    CreateEntity(false, true, name, float.Parse(tokens[3]), float.Parse(tokens[4]));
            }
            else
            {
                CreateEntity(false, false, name, float.Parse(tokens[3]), float.Parse(tokens[4]));
            }
        }

        yield break; 
    }   
    IEnumerator HandleMoveEvent(string[] tokens)
    {
        var name = tokens[1];
        if (isLocalEntity.ContainsKey(name)) yield break;
        entityPool.TryGetValue(name, out GameObject obj);

        if (obj)
        {

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
                + 0.5f * (velocity - lastVelo[name]) * (Time.time - lastMoveTime) * (Time.time - lastMoveTime);

                lastMoveTime = Time.time;
                yield return new WaitForEndOfFrame();
            }
            obj.transform.position = newPos;
            lastVelo[name] = velocity;
            mutexPool[name]--;

        }
        else 
        { 
            // work out how i should distinguish an enemy from a player
        }

        yield break;
    }
    IEnumerator HandleAttackEvent(string[] tokens) { yield break; }
    IEnumerator HandleDamageEvent(string[] tokens) { yield break; }
    IEnumerator HandleDeathEvent(string[] tokens) 
    {
        var name = tokens[1];
        print($"DESTROY {name}");
        entityPool.TryGetValue(name, out GameObject obj);
        if (!(obj is null))
        {
            Destroy(obj);
            entityPool.Remove(name);
            mutexPool.Remove(name);
            lastPacket.Remove(name);
            lastPos.Remove(name);
            lastVelo.Remove(name);

            isLocalEntity.TryGetValue(name, out bool result);
            if (result) isLocalEntity.Remove(name);
        }
        yield break; 
    }
    IEnumerator HandleLVLUpEvent(string[] tokens) { yield break; }

    IEnumerator HandleProjectileEvent(string[] tokens)
    {
        var name = tokens[1];
        if (isLocalEntity.ContainsKey(name)) yield break;

        entityPool.TryGetValue(name, out GameObject shooterObj);
        if (shooterObj is null) yield break;

        shooterObj.GetComponent<Player>().Attack(new Vector3(float.Parse(tokens[4]), float.Parse(tokens[5]))); 
    }

    IEnumerator HandleRollEvent(string[] tokens)
    {
        var name = tokens[1];
        if (isLocalEntity.ContainsKey(name)) yield break;

        entityPool.TryGetValue(name, out GameObject rollerObj);
        if (rollerObj is null) yield break;

        Player plr = rollerObj.GetComponent<Player>();
        plr.horizontal = float.Parse(tokens[2]);
        plr.vertical = float.Parse(tokens[3]);
        plr.isRolling = true;
    }
}
