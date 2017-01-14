using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

  public float walkingSpeed = 5f;
  public float runningSpeed = 10f;

	
	void Start () {
    transform.position = new Vector3(0, 5, 0);
	}
	
	
	void Update () {
		
	}
}
