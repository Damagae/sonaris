using System;
﻿using System.Collections;
using System.Collections.Generic;

public class AssociativeArray<T> : List<KeyValuePair<string, T>> {

	public void Add(string key, T value)
	{
		this.Add(new KeyValuePair<string, T>(key, value));
	}

	public void Add(string[] keys, T[] values)
	{
		for (int i = 0; i < keys.Length; ++i) {
			this.Add(new KeyValuePair<string, T>(keys[i], values[i]));
		}
	}

	public bool ContainsKey(string key)
	{
		foreach (var kvp in this)
		{
			if (kvp.Key == key)
				return true;
		}
		return false;
	}

	 public T this[string key]
	 {
		 get
        {
            // If this key is in the dictionary, return its value.
            Int32 index;
            if (TryGetIndexOfKey(key, out index))
            {
                // The key was found; return its value.
                return this[index].Value;
            }
            else
            {
                // The key was not found; return null.
								var tmp = default (T);
                return tmp;
            }
        }

        set
        {
            // If this key is in the dictionary, change its value.
            Int32 index;
            if (TryGetIndexOfKey(key, out index))
            {
                // The key was found; change its value.
                this[index] = new KeyValuePair<string, T>(key, value);
            }
            else
            {
                // This key is not in the dictionary; add this key/value pair.
                Add(key, value);
            }
        }
	 }

	 private Boolean TryGetIndexOfKey(Object key, out Int32 index)
	 {
			 for (index = 0; index < this.Count; index++)
			 {
					 // If the key is found, return true (the index is also returned).
					 if (this[index].Key.Equals(key)) return true;
			 }

			 // Key not found, return false (index should be ignored by the caller).
			 return false;
	 }

	 public string ToString()
	 {
		 string result = "{ ";
		 foreach (var e in this) {
			 result += ("'" + e.Key + "' : " + e.Value + ", ");
		 }
		 result += "}";
		 return result;
	 }



}
