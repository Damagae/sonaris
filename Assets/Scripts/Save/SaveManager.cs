using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public static class SaveManager {

	public static BrainData Fetch() {
		string path = Application.persistentDataPath + "/brain.dat";

		// Debug.Log(path);

		// If file doesn't exist, create it
    if (!SaveExists()) {
        StreamWriter temp_writer = new StreamWriter(path);
        temp_writer.Close();
    }

		StreamReader reader = new StreamReader(path);

		string existingJSON = reader.ReadToEnd();
		reader.Close();

		var list = JsonHelperList.FromJson<BrainData>(existingJSON);

		return list[0];
	}

	public static void Save(BrainData data) {

    string path = Application.persistentDataPath + "/brain.dat";

		var list = new List<BrainData>();
		list.Add(data);

    Debug.Log(path);

		string newData = JsonHelperList.ToJson(list, true);

		StreamWriter writer = new StreamWriter(path);

		writer.WriteLine(newData); // Rewrite score
		writer.Close();
  }

  public static bool SaveExists() {
		string path = Application.persistentDataPath + "/brain.dat";
        bool IsValid = true;

        if (!File.Exists(path)) {
            IsValid = false;
        }
        else if (Path.GetExtension(path).ToLower() != ".dat") {
            IsValid = false;
        }

        return IsValid;
  }

}
