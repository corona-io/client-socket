using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ConnectionManager : MonoBehaviour
{
    public static SocketManager globalSocket=SocketManager.GetSingleton();
    private static bool established=false, terminated=false;
    private static Queue<string> messageQueue;
    // Start is called before the first frame update
    void Start()
    {
        if (messageQueue is null) messageQueue = new Queue<string>();

        if (!established)
        {
            terminated = false;
            globalSocket.SocketInit("ws://hojoondev.kro.kr:3001", true);
            globalSocket.AddOpenEvent((sender, e) => { print("Established!"); });
            globalSocket.AddMessageEvent(
                (sender, e) =>
                {
                    print($" RECIEVED \"{e.Data}\", communicating with socket");
                    established = true;
                }
            );
            globalSocket.AddCloseEvent((sender, e) => { print("Connection Closed"); });
        }
        StartCoroutine(GlobalSocketLoop());
    }
    
    void OnApplicationQuit()
    {
        terminated = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

#nullable enable
    public static string? GetRecentMessage() {
        if (messageQueue is null) return null;
        if (messageQueue.Count < 1) return null;

        return messageQueue.Dequeue();
    }
#nullable disable

    IEnumerator GlobalSocketLoop()
    {

        if (!established) globalSocket.SocketConnect(true);
        while (!established) yield return new WaitForEndOfFrame();


        while (!terminated)
        {
            globalSocket.DeleteRecentMessageEvent();
            globalSocket.AddMessageEvent(
                (sender, e) =>
                {
                    string[] splitTokens = {"brodcast: "};
                    var message = e.Data.Split(splitTokens, StringSplitOptions.RemoveEmptyEntries)[0];
                    messageQueue.Enqueue(message);
                }
            );
            yield return null;
        }

        globalSocket.SocketClose(false);
        yield break;
    }
}
