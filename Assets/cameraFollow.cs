using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraFollow : MonoBehaviour
{
  public GameObject target;
  public float followDistance = 1f;
  public float cameraHeight = 10f;

  void LateUpdate()
  {
    transform.position = new Vector3(target.transform.position.x + followDistance,
                                          target.transform.position.y + cameraHeight,
                                          target.transform.position.z + followDistance);
  }
}