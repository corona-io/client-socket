using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

class PacketInfo {
    public string packetString { get; private set; }
    public bool asyncSend { get; private set; }
    public Action<bool> callback { get; private set; }

    public PacketInfo(string _p, bool _a, Action<bool> _c) { packetString = _p; asyncSend = _a; callback = _c; }
};

public class ConnectionManager : MonoBehaviour
{
    public static SocketManager globalSocket=SocketManager.GetSingleton();
    private static bool established=false, terminated=false;
    private static Queue<string> inputMessageQueue;
    private static Queue<PacketInfo> outputMessageQueue;

    // Start is called before the first frame update
    void Start()
    {
        if (inputMessageQueue is null) inputMessageQueue = new Queue<string>();
        if (outputMessageQueue is null) outputMessageQueue = new Queue<PacketInfo>();

        if (!established)
        {
            terminated = false;
            globalSocket.SocketInit("ws://hojoondev.kro.kr:3001", true);
            globalSocket.AddOpenEvent((sender, e) => { print("Established!"); established = true; });
            globalSocket.AddMessageEvent(
                (sender, e) =>
                {
                    string[] splitTokens = { "brodcast: " };
                    string message;
                    if (e.Data.Contains("brodcast:") == false)
                        message = e.Data;
                    else
                        message = e.Data.Split(splitTokens, StringSplitOptions.RemoveEmptyEntries)[0];
                    inputMessageQueue.Enqueue(message);
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
        if (inputMessageQueue is null) return null;
        if (inputMessageQueue.Count < 1) return null;

        return inputMessageQueue.Dequeue();
    }
#nullable disable

    public static void PutMessage(string arg, bool asyncSend = false, Action<bool> callback = null)
    {
        if (asyncSend) outputMessageQueue.Enqueue(new PacketInfo(arg, asyncSend, callback));
        else globalSocket.SocketSend(arg, asyncSend, callback);
    }

    IEnumerator GlobalSocketLoop()
    {

        if (!established) globalSocket.SocketConnect(true);
        while (!established) yield return new WaitForEndOfFrame();


        while (!terminated)
        {
            if (outputMessageQueue.Count > 0) {
                var item = outputMessageQueue.Dequeue();
                globalSocket.SocketSend(item.packetString, item.asyncSend, item.callback);
            }
            
            yield return null;
        }

        // flush output queue
        while (outputMessageQueue.Count > 0)
        {
            var item = outputMessageQueue.Dequeue();
            globalSocket.SocketSend(item.packetString, false, item.callback);
        }

        globalSocket.SocketClose(false);
        yield break;
    }
}
