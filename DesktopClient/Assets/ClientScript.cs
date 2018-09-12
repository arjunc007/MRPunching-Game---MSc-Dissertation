using UnityEngine;
using UnityEngine.Networking;

public class ClientScript : MonoBehaviour {

    [System.Serializable]
    class HandTransforms
    {
        public Vector3 lPos, rPos;
        public Quaternion lRot, rRot;
    }

    public Transform[] palm;

    public int serverPort = 8888;

    float t = 0;

    NetworkServerSimple server = null;

    // Use this for initialization
    void Start () {
        server = new NetworkServerSimple();
        server.RegisterHandler(MsgType.Connect, OnConnect);
        server.RegisterHandler(MsgType.Disconnect, OnDisconnect);
        server.RegisterHandler(NetMsg.StartCalibration, OnCalibration);
        server.RegisterHandler(NetMsg.StartLeap, OnLeap);
       
        server.Listen(serverPort);
    }
	
	// Update is called once per frame
	void Update () {
        if (server != null)
        {
            server.Update();
        }
	}

    private void OnConnect(NetworkMessage netMsg)
    {
        Debug.Log("Client connect");
    }

    private void OnDisconnect(NetworkMessage netMsg)
    {
        Debug.Log("Client Disconnect");
    }

    private void OnCalibration(NetworkMessage netMsg)
    {
        CalibrationMessage msg = netMsg.ReadMessage<CalibrationMessage>();
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(msg.imageData);

        // Create a GameObject to which the texture can be applied
        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        Renderer quadRenderer = quad.GetComponent<Renderer>() as Renderer;
        quadRenderer.material = new Material(Shader.Find("Unlit/Texture"));
        quad.transform.parent = this.transform;
        quad.transform.localPosition = new Vector3(0.0f, 0.0f, 3.0f);
        quadRenderer.material.SetTexture("_MainTex", tex);

        var response = new CalibrationResponseMessage();
        response.response = "Image received";
        netMsg.conn.Send(NetMsg.StartCalibration, response);
    }

    private void OnLeap(NetworkMessage netMsg)
    {

    }
}
