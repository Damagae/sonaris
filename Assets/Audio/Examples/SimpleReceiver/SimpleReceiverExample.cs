using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityOSC;

public class SimpleReceiverExample : MonoBehaviour {

	private OSCReciever reciever;

	public int port = 57120;
	private int openedPort;

	// Use this for initialization
	void Start () {
		openedPort = port;
		reciever = new OSCReciever();
		Debug.Log("Open port " + openedPort);
		reciever.Open(openedPort);
	}

	// Update is called once per frame
	void Update () {

		if(reciever.hasWaitingMessages()){
			OSCMessage msg = reciever.getNextMessage();
			Debug.Log(string.Format("message received: {0} {1}", msg.Address, DataToString(msg.Data)));
		}
	}

	void OnDisable() {
		Debug.Log("Close port " + openedPort);
		reciever.Close();
	}

	private string DataToString(List<object> data)
	{
		string buffer = "";

		for(int i = 0; i < data.Count; i++)
		{
			buffer += data[i].ToString() + " ";
		}

		buffer += "\n";

		return buffer;
	}
}
