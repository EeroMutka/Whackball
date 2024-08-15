using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Ball : NetworkBehaviour
{
	public GameManager game;
	public Rigidbody rigidbody;
	
	public NetworkVariable<bool> isIdle;
	
	// server-only
	public int lastTouchedPlayerSide;
	
	void Start()
	{
		rigidbody = GetComponent<Rigidbody>();
		game = GameObject.Find("GameManager").GetComponent<GameManager>();
	}

	[Rpc(SendTo.ClientsAndHost, Delivery = RpcDelivery.Unreliable)]
	void SendPerFramePosAndVelToClientsRpc(Vector3 position, Vector3 velocity) {
		transform.position = Vector3.Lerp(transform.position, position, 0.3f); // adjust the position over time towards the truth
		rigidbody.velocity = velocity;
	}
	
	[Rpc(SendTo.ClientsAndHost)]
	public void SendSnapBallPosRpc(Vector3 position) {
		transform.position = position;
	}
	
	void Update()
	{
		// do physics on both client and server
		if (isIdle.Value) {
			rigidbody.velocity = new Vector3(0, 0, 0);
		}
		else {
			rigidbody.velocity = rigidbody.velocity + new Vector3(0, -5f*Time.deltaTime, 0);
		}
		
		if (IsServer) {
			SendPerFramePosAndVelToClientsRpc(transform.position, rigidbody.velocity);
		}
	}
	
	void OnCollisionEnter(Collision collision)
	{
		if (IsServer) {
			isIdle.Value = false;
			
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
