using System.Collections;
using System.Collections.Generic;

public class List3D<T>: List<List<List<T>>> {

  public string ToString()
  {
    string result = " {\n ";
    foreach (var e in this) {
      result += " { ";
      foreach (var l in e) {
        result += " { ";
        foreach (var m in l) {
          result += m + ", ";
        }
        result += "}, ";
      }
      result += "},\n ";
    }
    result += "}";
    return result;
  }

}
