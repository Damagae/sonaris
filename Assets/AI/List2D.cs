using System.Collections;
using System.Collections.Generic;

public class List2D<T>: List<List<T>> {

  public string ToString()
  {
    string result = " {\n ";
    foreach (var e in this) {
      result += " { ";
      foreach (var l in e) {
          result += l + ", ";
      }
      result += "},\n ";
    }
    result += "}";
    return result;
  }

}
