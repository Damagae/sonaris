using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityOSC;

public class SimpleReceiverExample : MonoBehaviour {

	private OSCReciever reciever;

	public int port = 57120;

	// Use this for initialization
	void Start () {
		reciever = new OSCReciever();
		Debug.Log("Open port " + port);
		reciever.Open(port);
	}

	// Update is called once per frame
	void Update () {

		if(reciever.hasWaitingMessages()){
			OSCMessage msg = reciever.getNextMessage();
			Debug.Log(string.Format("message received: {0} {1}", msg.Address, DataToString(msg.Data)));
		}
	}

	void OnDisable() {
		Debug.Log("Close port " + port);
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
