﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ZombieController : MonoBehaviour {

	public float moveSpeed;
	public float turnSpeed;
	private Vector3 moveDirection;

	private List<Transform> congaLine = new List<Transform>();

	[SerializeField]
	private PolygonCollider2D[] colliders;
	private int currentColliderInder = 0;




	// Use this for initialization
	void Start () {
		moveDirection = Vector3.right;
	
	}
	
	// Update is called once per frame
	void Update () {
		//1
		Vector3 currentPosition = transform.position;

		//2
		if( Input.GetButton("Fire1") ) {
			// 3
			Vector3 moveToward = Camera.main.ScreenToWorldPoint( Input.mousePosition);
			// 4
			moveDirection = moveToward - currentPosition;
			moveDirection.z = 0;
			moveDirection.Normalize();

		}

		Vector3 target = moveDirection * moveSpeed + currentPosition;
		transform.position = Vector3.Lerp(currentPosition, target, Time.deltaTime);

		float targetAngle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
		transform.rotation = Quaternion.Slerp (transform.rotation, Quaternion.Euler(0,0, targetAngle), turnSpeed * Time.deltaTime);

		EnforceBounds ();

	
	}

	void OnTriggerEnter2D (Collider2D other){

		if(other.CompareTag("cat")) {
			//Debug.Log("Oops. Stepped on a cat.");
			Transform followTarget = congaLine.Count == 0 ? transform : congaLine[congaLine.Count-1];
			//other.GetComponent<CatController>().JoinConga(followTarget, moveSpeed, turnSpeed);
			other.transform.parent.GetComponent<CatController>().JoinConga(followTarget, moveSpeed, turnSpeed);
			congaLine.Add(other.transform);

		} else if (other.CompareTag("enemy")) {		
			//Debug.Log("PArdon me, ma'am");
			for(int i=0; i < 2 && congaLine.Count > 0; i++) {
				int lastIdx = congaLine.Count-1;
				Transform cat = congaLine[ lastIdx ];
				congaLine.RemoveAt(lastIdx);
				cat.parent.GetComponent<CatController>().ExitConga();
			}
		}
	}

	public void SerColliderForSprite(int spriteNum){
		colliders[currentColliderInder].enabled = false;
		currentColliderInder = spriteNum;
		colliders[currentColliderInder].enabled = true;
	}

	private void EnforceBounds() {
		//1
		Vector3 newPosition = transform.position;
		Camera mainCamera = Camera.main;
		Vector3 cameraPosition = mainCamera.transform.position;

		//2
		float xDist = mainCamera.aspect * mainCamera.orthographicSize;
		float yDist = mainCamera.orthographicSize;
		float xMax = cameraPosition.x + xDist;
		float xMin = cameraPosition.x - xDist;
		float yMax = cameraPosition.y + yDist;
		float yMin = cameraPosition.y - yDist;

		//3
		if (newPosition.x < xMin || newPosition.x > xMax) {
			newPosition.x = Mathf.Clamp(newPosition.x, xMin, xMax);
			moveDirection.x = -moveDirection.x;
		}

		//TODO vertical bounds
		if (newPosition.y < yMin || newPosition.y > yMax) {
			newPosition.y = Mathf.Clamp(newPosition.y, yMin, yMax);
			moveDirection.y = -moveDirection.y;
		}

		//4 
		transform.position = newPosition;
	}




}
