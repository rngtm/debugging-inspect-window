using UnityEngine;
using System.Collections;

[Inspectable]
public class TestComponent : MonoBehaviour
{
  [Inspectable]
  Vector2 vector2;

  [Inspectable]
  Vector3 Position { get { return this.transform.position; } }


  [Inspectable]
  [SerializeField]
  int[,] y = new int[2, 3];

}
