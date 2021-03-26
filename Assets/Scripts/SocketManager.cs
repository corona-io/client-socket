using WebSocketSharp;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class SocketManager
{
    // Singleton Implementation
    private SocketManager() {
        openEventHandlers = new List<EventHandler>();
        messageEventHandlers = new List<EventHandler<MessageEventArgs>>();
        errorEventHandlers = new List<EventHandler<ErrorEventArgs>>();
        closeEventHandlers = new List<EventHandler<CloseEventArgs>>();
    }
    private static readonly Lazy<SocketManager> lazyInstance
        = new Lazy<SocketManager>(() => new SocketManager());
    public static SocketManager GetSingleton() { return lazyInstance.Value; }

// Websocket Implementation
    
    private WebSocket userSocket;

    private List<EventHandler> openEventHandlers;
    private List<EventHandler<MessageEventArgs>> messageEventHandlers;
    private List<EventHandler<ErrorEventArgs>> errorEventHandlers;
    private List<EventHandler<CloseEventArgs>> closeEventHandlers;

    /// <summary>
    /// Adds an event to the socket's connection establishment handling events.
    /// </summary>
    /// <param name="openEvent">
    /// An EventHandler delegate that will be subscribed to the socket's handler.
    /// </param>
    public void AddOpenEvent(EventHandler openEvent)
    {
        openEventHandlers.Add(openEvent);
        userSocket.OnOpen += openEvent;
    }

    /// <summary>
    /// Deletes an event from the socket's connection establishment handling events.
    /// </summary>
    /// <param name="openEvent">
    /// An EventHandler delegate that will be unsubscribed from the socket's handler.
    /// </param>
    public void DeleteOpenEvent(EventHandler openEvent)
    {
        openEventHandlers.Remove(openEvent);
        userSocket.OnOpen -= openEvent;
    }

    /// <summary>
    /// Deletes the most recent event that has been subscribed to 
    /// the socket's connection establishment handling events.
    /// </summary>
    public void DeleteRecentOpenEvent()
    {
        var handler = openEventHandlers[openEventHandlers.Count - 1];
        userSocket.OnOpen -= handler;
        openEventHandlers.RemoveAt(openEventHandlers.Count - 1);
    }



    /// <summary>
    /// Adds an event to the socket's message handling events.
    /// </summary>
    /// <param name="messageEvent">
    /// An EventHandler delegate that will be subscribed to the socket's handler.
    /// </param>
    public void AddMessageEvent(EventHandler<MessageEventArgs> messageEvent)
    {
        messageEventHandlers.Add(messageEvent);
        userSocket.OnMessage += messageEvent;
    }

    /// <summary>
    /// Deletes an event from the socket's message handling events.
    /// </summary>
    /// <param name="messageEvent">
    /// An EventHandler delegate that will be unsubscribed from the socket's handler.
    /// </param>
    public void DeleteMessageEvent(EventHandler<MessageEventArgs> messageEvent)
    {
        messageEventHandlers.Remove(messageEvent);
        userSocket.OnMessage -= messageEvent;
    }

    /// <summary>
    /// Deletes the most recent event that has been subscribed to 
    /// the socket's message handling events.
    /// </summary>
    public void DeleteRecentMessageEvent()
    {
        var handler = messageEventHandlers[messageEventHandlers.Count - 1];
        userSocket.OnMessage -= handler;
        messageEventHandlers.RemoveAt(messageEventHandlers.Count - 1);
    }



    /// <summary>
    /// Adds an event to the socket's error handling events.
    /// </summary>
    /// <param name="errorEvent">
    /// An EventHandler delegate that will be subscribed to the socket's handler.
    /// </param>
    public void AddErrorEvent(EventHandler<ErrorEventArgs> errorEvent)
    {
        errorEventHandlers.Add(errorEvent);
        userSocket.OnError += errorEvent;
    }

    /// <summary>
    /// Deletes an event from the socket's error handling events.
    /// </summary>
    /// <param name="errorEvent">
    /// An EventHandler delegate that will be unsubscribed from the socket's handler.
    /// </param>
    public void DeleteErrorEvent(EventHandler<ErrorEventArgs> errorEvent)
    {
        errorEventHandlers.Remove(errorEvent);
        userSocket.OnError -= errorEvent;
    }

    /// <summary>
    /// Deletes the most recent event that has been subscribed to 
    /// the socket's error handling events.
    /// </summary>
    public void DeleteRecentErrorEvent()
    {
        var handler = errorEventHandlers[errorEventHandlers.Count - 1];
        userSocket.OnError -= handler;
        errorEventHandlers.RemoveAt(errorEventHandlers.Count - 1);
    }



    /// <summary>
    /// Adds an event to the socket connection termination handling events.
    /// </summary>
    /// <param name="closeEvent">
    /// An EventHandler delegate that will be subscribed to the socket's handler.
    /// </param>
    public void AddCloseEvent(EventHandler<CloseEventArgs> closeEvent)
    {
        closeEventHandlers.Add(closeEvent);
        userSocket.OnClose += closeEvent;
    }

    /// <summary>
    /// Deletes an event from the socket's connection termination handling events.
    /// </summary>
    /// <param name="closeEvent">
    /// An EventHandler delegate that will be subscribed to the socket's handler.
    /// </param>
    public void DeleteCloseEvent(EventHandler<CloseEventArgs> closeEvent)
    {
        closeEventHandlers.Remove(closeEvent);
        userSocket.OnClose -= closeEvent;
    }

    /// <summary>
    /// Deletes the most recent event that has been subscribed to 
    /// the socket's connection termination handling events.
    /// </summary>
    public void DeleteRecentCloseEvent()
    {
        var handler = openEventHandlers[openEventHandlers.Count - 1];
        userSocket.OnOpen -= handler;
        openEventHandlers.RemoveAt(openEventHandlers.Count - 1);
    }



    /// <summary>
    /// Initializes the socket with the specified URI.
    /// </summary>
    /// <param name="uri">
    /// The desired location of the server. The scheme must be ws or wss.
    /// </param>
    /// <param name="redirection">
    /// A flag to enable/disable the redirection for the socket.<br></br>
    /// True by default.
    /// </param>
    /// <param name="socketParams">
    /// Parameters to send upon the start of the connection.<br></br>
    /// All parameters must follow the format of "[Name]=[Value]".
    /// </param>
    public void SocketInit(string uri, bool redirection = true, params string[] socketParams) 
    {

        string tmp = uri;
        Regex paramRegex = new Regex("[a-zA-z0-9_-]+=[a-zA-z0-9_-]+");
        if (socketParams.Length != 0) 
        {
            tmp += "?";

            for (int i = 0; i < socketParams.Length; i++) 
            {
                if (!paramRegex.IsMatch(socketParams[i])) 
                {
                    throw new ArgumentException($"Parameter #{i + 1} is invalid");
                }

                tmp += socketParams[i];
                if (i != socketParams.Length - 1) tmp += "&";
            }

        }
        userSocket = new WebSocket(uri);
        userSocket.EnableRedirection = redirection;
    }

    /// <summary>
    /// Sends a message via a socket.
    /// </summary>
    /// <param name="message">
    /// The message to send.
    /// </param>
    /// <param name="asyncSend">
    /// Set this to true to send the message asynchronously.<br></br>
    /// This flag is set to false by default.
    /// </param>
    /// <param name="callback">
    /// A callback function that will be executed after the transmission.<br></br>
    /// The argument for this function will be true if there were no errors; false otherwise.
    /// </param>
    public void SocketSend(string message, bool asyncSend = false, Action<bool> callback = null) 
    {
        if (!asyncSend) userSocket.Send(message);
        else userSocket.SendAsync(message, callback);
    }

    /// <summary>
    /// Connects the websocket to the pre-specified server.
    /// </summary>
    /// <param name="asyncConnect">
    /// Set this to true to connect asynchronously.<br></br>
    /// This flag is set to false by default.
    /// </param>
    public void SocketConnect(bool asyncConnect = false) 
    {
        if (!asyncConnect) userSocket.Connect();
        else userSocket.ConnectAsync();
    }

    /// <summary>
    /// Force the websocket to disconnect from the server.
    /// </summary>
    /// <param name="asyncClose">
    /// Set this to true to disconnect asynchronously.<br></br>
    /// This flag is set to false by default.
    /// </param>
    /// <param name="statusCode">
    /// The status code for closing the connection, per RFC 6455.
    /// </param>
    /// <param name="reason">
    /// The human readable form of reason for the disconnection.<br></br>
    /// This parameter will be ignored if <paramref name="statusCode"/> is null.
    /// </param>
    public void SocketClose(bool asyncClose = false, ushort? statusCode = null, string reason = null) 
    {
        if (!asyncClose) 
        {
            if (statusCode is null) userSocket.Close();
            else {
                if (reason is null) userSocket.Close((ushort)statusCode);
                else userSocket.Close((ushort)statusCode, reason);
            }
        }
        else
        {
            if (statusCode is null) userSocket.CloseAsync();
            else { 
                if (reason is null) userSocket.CloseAsync((ushort)statusCode);
                else userSocket.CloseAsync((ushort)statusCode, reason);
            }
        }
    }
}
