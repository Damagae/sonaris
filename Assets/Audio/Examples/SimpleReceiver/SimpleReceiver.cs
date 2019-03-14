using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityOSC;

// ---------------------------------------------------------------------------
// Simple Receiver
// Fetch the audio data (OSC Messages) sent by Max
// Transform it into an associative array
// ---------------------------------------------------------------------------
public class SimpleReceiver : MonoBehaviour {

	private OSCReciever reciever;
	private List2D<object> currentData;

	public int port = 57120;
	private int openedPort;

	// Use this for initialization
	void Start () {
		openedPort = port;
		currentData = new List2D<object>();
		reciever = new OSCReciever();
		Debug.Log("Open port " + openedPort);
		reciever.Open(openedPort);
	}

	// Update is called once per frame
	void Update () {
		OSCMessage data = GetOSCMessage();
		UpdateData(data);
	}

	public List2D<object> GetData() {
		return currentData;
	}

	public OSCMessage GetOSCMessage() {
		if(reciever.hasWaitingMessages()){
			OSCMessage msg = reciever.getNextMessage();
			// if (msg.Data[0].ToString() != "0") {
			// 	Debug.Log(string.Format("message received: {0} {1}", msg.Address, DataToString(msg.Data)));
			// }
			return msg;
		}
		return null;
	}

	void OnDisable() {
		Debug.Log("Close port " + openedPort);
		reciever.Close();
	}

	private void UpdateData(OSCMessage newEntry) {
		if (currentData.Count > 0) {
			for (int i = 0; i < currentData.Count; ++i) {
				if (newEntry != null && (string) currentData[i][0] == (string) newEntry.Address) {
					currentData.Remove(currentData[i]);
				}
			}
		}
		if (newEntry != null) {
			var newElement = new List<object>();
			if (newEntry.Address != null && newEntry.Data[0] != null) {
				newElement.Add(newEntry.Address);
				newElement.Add(newEntry.Data[0]);
				currentData.Add(newElement);
			}
		}
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
