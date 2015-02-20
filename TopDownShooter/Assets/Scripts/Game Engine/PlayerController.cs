﻿using UnityEngine;
using System.Collections;

//[RequireComponent (typeof (CharacterController))]
public class PlayerController : MonoBehaviour {
	//system
	private Quaternion targetRotation;
	//components
	//private CharacterController controller;
	//Handling
	public float playerHP = 10;


	public float rotationSpeed = 450;
	public float walkSpeed = 5;
	public float runSpeed = 3;

	public Rigidbody Bullet;
	public float speed = 100;
	
	void FireBullet () {
		var inFront = new Vector3 (0, 1, 0);

		Rigidbody bulletClone = (Rigidbody) Instantiate(Bullet, transform.position, transform.rotation);
		bulletClone.velocity = transform.forward * speed;

		//bulletClone.GetComponent<MyRocketScript>().DoSomething();
	}
	
	// Calls the fire method when holding down ctrl or mouse


	// Use this for initialization
	void Start () {


	}

	// Update is called once per frame
	void Update () {
		Vector3 movement = new Vector3 (0, 0, 0);

		var mouse = Input.mousePosition;
		var screenPoint = Camera.main.WorldToScreenPoint(transform.localPosition);
		var offset = new Vector2(mouse.x - screenPoint.x, mouse.y - screenPoint.y);
		var angle = Mathf.Atan2(offset.x, offset.y) * Mathf.Rad2Deg;
		transform.rotation = Quaternion.Euler(0, angle, 0);

		//Vector3 input = new Vector3 (Input.GetAxisRaw ("Horizontal"), 0, Input.GetAxisRaw ("Vertical"));
		float moveHorizontal = Input.GetAxis ("Horizontal");
		float moveVertical = Input.GetAxis ("Vertical");
		
		movement = new Vector3 (moveHorizontal, 0.0f, moveVertical);
		rigidbody.velocity = movement * walkSpeed;

		if (Input.GetMouseButtonDown(0)) {

			FireBullet();
		}
		//controller.Move (movement * Time.deltaTime);


	
	}

}