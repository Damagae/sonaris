using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityOSC;

public class Test : MonoBehaviour {

	private OSCReciever manager;
	public int port = 9001;

	// Use this for initialization
	void Start () {
		manager = new OSCReciever();
		manager.Open(9001);
	}

	// Update is called once per frame
	void Update () {
		if (manager.hasWaitingMessages()) {
			var msg = manager.getNextMessage();
			foreach (var element in msg.Data) {
				Debug.Log(element);
			}
		}
	}
}
