using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rawInput : MonoBehaviour {

  public float moveSpeed = 10f;
  public float rotateSpeed = 50f;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
    checkForInput();
	}

  private void checkForInput()
  {
    if (Input.GetKey(KeyCode.W))
    {
      moveVertical(true);
    }
    if (Input.GetKey(KeyCode.S))
    {
      moveVertical(false);
    }
    if (Input.GetKey(KeyCode.A))
    {
      moveHorizontal(true);
    }
    if (Input.GetKey(KeyCode.D))
    {
      moveHorizontal(false);
    }
    if (Input.GetKey(KeyCode.LeftArrow))
    {
      rotate(true);
    }
    if (Input.GetKey(KeyCode.RightArrow))
    {
      rotate(false);
    }
  }

  private void moveHorizontal(bool moveLeft)
  {
    if (moveLeft)
    {
      transform.Translate(Vector3.left * moveSpeed * Time.deltaTime);
    } else
    {
      transform.Translate(Vector3.right * moveSpeed * Time.deltaTime);
    }
  }

  private void moveVertical(bool moveForward)
  {
    if (moveForward)
    {
      transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
    } else
    {
      transform.Translate(Vector3.back * moveSpeed * Time.deltaTime);
    }
  }

  private void rotate(bool rotateLeft)
  {
    if (rotateLeft)
    {
      transform.Rotate(Vector3.down * rotateSpeed * Time.deltaTime);
    } else
    {
      transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);
    }
  }
}
