using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Data {

	public int length;
	public List<AssociativeArray<double>> data;

	public Data()
	{
		data = new List<AssociativeArray<double>>();
		length = data.Count;
	}

	public void Add(AssociativeArray<double> aa)
	{
		data.Add(aa);
		length = data.Count;
	}

	public void Add(string[] keys, double[] values)
	{
		if (keys.Length != values.Length) {
			Debug.Log("keys length different than values length");
		} else {
			AssociativeArray<double> aa = new AssociativeArray<double>();
			for (int i = 0; i < keys.Length; ++i)
			{
				aa.Add(keys[i], values[i]);
			}
			data.Add(aa);
			length = data.Count;
		}
	}

	public void Add(string key, double value)
	{
		AssociativeArray<double> aa = new AssociativeArray<double>();
		aa.Add(key, value);
		data.Add(aa);
		length = data.Count;
	}


}
