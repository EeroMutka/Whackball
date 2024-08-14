using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Ball : NetworkBehaviour
{
	Vector3 startPosition;
	Rigidbody rigidbody;
	
	float canGetHitTimer = 0f;
	
	void Start()
	{
		startPosition = transform.position;
		rigidbody = GetComponent<Rigidbody>();
	}

	void Update()
	{
		if (IsServer) {
			rigidbody.velocity = rigidbody.velocity + new Vector3(0, -5f*Time.deltaTime, 0);
			
			canGetHitTimer -= Time.deltaTime;
			// if ((transform.position - )
		}
	}
	
	void OnCollisionEnter(Collision collision)
	{
		if (IsServer) {
			Player player = collision.gameObject.GetComponent<Player>();
			if (player == null) {
				Arm arm = collision.gameObject.GetComponent<Arm>();
				if (arm) player = arm.player;
			}
			
			if (player) {
				// if (canGetHitTimer < 0) {
					// canGetHitTimer = 0.5f;
					// rigidbody.velocity = new Vector3(rigidbody.velocity.x, 8f, rigidbody.velocity.z);
					rigidbody.velocity = new Vector3(0.8f*player.lastArmVel.x, 0.5f*player.lastArmVel.y, 0.8f*player.lastArmVel.z) + player.velocity;
					// transform.position += (player.lastArmVel + player.velocity)*0.1f;
				// }
			}
			else if (collision.gameObject.tag == "Ground") {
				transform.position = startPosition;
				rigidbody.velocity = new Vector3(0, 0, 0);
			}
		}
	}
	
	void OnTriggerEnter(Collider collision)
	{
		/*if (IsServer) {
			Arm arm = collision.gameObject.GetComponent<Arm>();
			if (arm) {
				Debug.Log($"Ball collided with arm! {arm.player.lastArmVel}");
				rigidbody.velocity = 0.5f*arm.player.lastArmVel;
			}
			else {
				
			}
		}*/
	}
}
