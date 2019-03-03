using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioData : MonoBehaviour {

	// Niveaux
	public double level1;
	public double level2;
	public double level3;

	// Vitesse
	public double speed1;
	public double speed2;
	public double speed3;

	// Fréquence d'échantillonnage
	public double frqce1;
	public double frqce2;
	public double frqce3;

	public void Fill(List2D<object> array) {
		
	}

	public Data GetArray() {
		Data data = new Data();

		data.Add("level1", level1);
		data.Add("level2", level2);
		data.Add("level3", level3);

		data.Add("speed1", speed1);
		data.Add("speed2", speed2);
		data.Add("speed3", speed3);

		data.Add("frqce1", frqce1);
		data.Add("frqce2", frqce2);
		data.Add("frqce3", frqce3);


		return data;
	}


	// Use this for initialization
	void Start () {

	}

	// Update is called once per frame
	void Update () {

	}
}
