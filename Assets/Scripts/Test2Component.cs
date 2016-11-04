using UnityEngine;
using System.Collections;

[Inspectable]
public class Test2Component : MonoBehaviour
{
  [Inspectable]
  float hoge = 123f;

  [Inspectable]
  [System.NonSerialized]
  int[] x = new int[3];

  [Inspectable]
  public float CurrentTime { get { return Time.time; } }
}
