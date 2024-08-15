using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Ball : NetworkBehaviour
{
	public GameManager game;
	public Rigidbody rigidbody;
	
	// server-only
	public bool isIdle = true;
	public int lastTouchedPlayerSide;
	
	void Start()
	{
		rigidbody = GetComponent<Rigidbody>();
		game = GameObject.Find("GameManager").GetComponent<GameManager>();
	}

	void Update()
	{
		if (IsServer) {
			
			if (!isIdle) {
				rigidbody.velocity = rigidbody.velocity + new Vector3(0, -5f*Time.deltaTime, 0);
			}
			else {
				rigidbody.velocity = new Vector3(0, 0, 0);
			}
		}
	}
	
	void OnCollisionEnter(Collision collision)
	{
		if (IsServer) {
			isIdle = false;
			
			Player player = collision.gameObject.GetComponent<Player>();
			if (player == null) {
				Arm arm = collision.gameObject.GetComponent<Arm>();
				if (arm) player = arm.player;
			}
			
			if (player) {
				rigidbody.velocity = new Vector3(0.8f*player.lastArmVel.x, 0.5f*player.lastArmVel.y, 0.8f*player.lastArmVel.z) + player.velocity;
				lastTouchedPlayerSide = player.fieldSide;
			}
			else if (collision.gameObject.tag == "Ground") {
				game.OnBallHitGround();
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
