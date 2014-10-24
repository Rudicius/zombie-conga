using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ZombieController : MonoBehaviour {

	public int congaLineMax = 5;
	public int lives = 3;

	public AudioClip enemyContactSound;
	public AudioClip catContactSound;

	public float moveSpeed;
	public float turnSpeed;
	private Vector3 moveDirection;

	private List<Transform> congaLine = new List<Transform>();

	private bool isInvincible = false;
	private float timeSpentInvincible;

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

		if (isInvincible) {
			timeSpentInvincible += Time.deltaTime;

			if(timeSpentInvincible < 3f) {
				float remainder = timeSpentInvincible % .3f;
				renderer.enabled = remainder > .15f;
			}
			else {
				renderer.enabled = true;
				isInvincible = false;
			}
		}

	
	}

	void OnTriggerEnter2D (Collider2D other){

		if(other.CompareTag("cat")) {
			audio.PlayOneShot(catContactSound);

			Transform followTarget = congaLine.Count == 0 ? transform : congaLine[congaLine.Count-1];

			other.transform.parent.GetComponent<CatController>().JoinConga(followTarget, moveSpeed, turnSpeed);
			congaLine.Add(other.transform);

			if(congaLine.Count >= congaLineMax) {
				Debug.Log("You Won!");
				Application.LoadLevel("WinScene");
			}

		} else if (!isInvincible && other.CompareTag("enemy")) {
			audio.PlayOneShot(enemyContactSound);

			isInvincible = true;
			timeSpentInvincible = 0;

			for(int i=0; i < 2 && congaLine.Count > 0; i++) {
				int lastIdx = congaLine.Count-1;
				Transform cat = congaLine[ lastIdx ];
				congaLine.RemoveAt(lastIdx);
				cat.parent.GetComponent<CatController>().ExitConga();
			}

			if(--lives <= 0) {
				Debug.Log("You lost!");
				Application.LoadLevel("LoseScene");
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
