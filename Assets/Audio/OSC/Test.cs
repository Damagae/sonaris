using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityOSC;

public class Test : MonoBehaviour {

	private OSCReciever manager;
	public int port = 9001;

	public bool close = false;

	// Use this for initialization
	void Start () {
		manager = new OSCReciever();
		manager.Open(port);
	}

	// Update is called once per frame
	void Update () {
		if (manager.hasWaitingMessages()) {
			var msg = manager.getNextMessage();
			Debug.Log(msg);
		}

	}

	void OnDisable() {
				Debug.Log("Closing");
				manager.Close();
	}
}
