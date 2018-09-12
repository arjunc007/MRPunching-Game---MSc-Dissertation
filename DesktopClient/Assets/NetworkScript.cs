using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkScript : MonoBehaviour {

    [System.Serializable]
    class HandTransforms
    {
        public Vector3 lPos, rPos;
        public Quaternion lRot, rRot;
    }

    int channelId;
    int socketId;
    int socketPort = 8888;

    int connectionId;

    int hostId;

    public GameObject[] hands;

	// Use this for initialization
	void Start () {
        NetworkTransport.Init();

        ConnectionConfig config = new ConnectionConfig();

        channelId = config.AddChannel(QosType.Reliable);

        int maxConnections = 1;
        HostTopology topology = new HostTopology(config, maxConnections);

        socketId = NetworkTransport.AddHost(topology, 0);

        Debug.Log("Socket Open. SocketId is: " + socketId);
    }
	
	// Update is called once per frame
	void Update () {
        int recHostId;
        int recConnectionId;
        int recChannelId;
        byte[] recbufffer = new byte[1024];
        int bufferSize = 1024;
        int dataSize;
        byte error;

        NetworkEventType recNetworkEvent = NetworkTransport.Receive(out recHostId, out recConnectionId, out recChannelId, recbufffer, bufferSize, out dataSize, out error);

        switch(recNetworkEvent)
        {
            case NetworkEventType.DataEvent:
                Stream stream = new MemoryStream(recbufffer);
                BinaryFormatter formatter = new BinaryFormatter();
                HandTransforms message = JsonUtility.FromJson<HandTransforms>(formatter.Deserialize(stream) as string);
                hands[0].transform.SetPositionAndRotation(message.lPos, message.lRot);
                hands[1].transform.SetPositionAndRotation(message.rPos, message.rRot);

                Debug.Log("incoming message event receive: " + message);
                break;
        }
	}

    public void Connect()
    {
        byte error;
        connectionId = NetworkTransport.Connect(socketId, "127.0.0.1", socketPort, 0, out error);
        Debug.Log("Connected to server. ConnectionId: " + connectionId);
    }
}
