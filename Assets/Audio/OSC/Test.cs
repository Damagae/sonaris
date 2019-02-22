using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityOSC;

public class Test : MonoBehaviour {

	private OSCReciever manager;
	public int port = 9001;
	private int previousPort;

	// Use this for initialization
	void Start () {
		previousPort = port;
		manager = new OSCReciever();
		manager.Open(port);
		Debug.Log(port);
	}

	// Update is called once per frame
	void Update () {

		if (port != previousPort) {
			manager.Open(port);
			Debug.Log(port);
			previousPort = port;
		}


		if (manager.hasWaitingMessages()) {
			var msg = manager.getNextMessage();
			string str = "";
			foreach (var element in msg.Data) {
				str += element + " ";
			}
			Debug.Log(str);
		}
	}
}
