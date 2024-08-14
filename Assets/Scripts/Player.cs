using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
	
	public GameObject HandCapsuleOrigin;
	public GameObject HandVizL;
	public GameObject HandVizR;
	
	public CharacterController characterController;
	
	// networked variables, automatically replicated from server to clients
	// -transform.position is implicitly like this
	public NetworkVariable<Vector2> armPos2D;
	
	// server-only variables
	public float lastInputMoveX = 0f;
	public float lastInputMoveY = 0f;
	public Vector3 velocity;
	public Vector3 lastArmVel;
	
	
	// owning-client-only variables
	public float prevMouseX;
	public float prevMouseY;
	public Vector2 clientTargetArmPos2D;
	public Vector2 clientLazyArmPos2D;
	
	const float armThetaScale = 2f;
	
	void Start()
	{
		characterController = GetComponent<CharacterController>();
	}
	
	[Rpc(SendTo.Server, Delivery = RpcDelivery.Unreliable)]
	void SendPerFrameInputToServerRpc(float moveX, float moveY, float armPosX, float armPosY, Vector3 armVel) {
		lastInputMoveX = moveX;
		lastInputMoveY = moveY;
		lastArmVel = armVel;
		armPos2D.Value = new Vector2(armPosX, armPosY);
	}
	
	[Rpc(SendTo.Server, Delivery = RpcDelivery.Unreliable)]
	void SendJumpInputToServerRpc() {
		if (characterController.isGrounded) {
			velocity = new Vector3(velocity.x, 6f, velocity.z);
		}
	}
	
	Vector3 ArmPos3DFromArmPos2D(Vector2 arm) {
		float theta = arm.magnitude*armThetaScale;
		float z = Mathf.Cos(theta);
		float x = Mathf.Sin(theta);
		return new Vector3(arm.x, 0, arm.y)*x + new Vector3(0, z, 0);
	}
	
	void Update()
	{
		if (IsServer) {
			float moveSpeed = 5f;
			
			velocity = new Vector3(lastInputMoveX*moveSpeed, velocity.y - 15f * Time.deltaTime, lastInputMoveY*moveSpeed);
			
			characterController.Move(velocity * Time.deltaTime);
			
			if (characterController.isGrounded) velocity = new Vector3(velocity.x, 0, velocity.z);
		}
		
		if (IsClient && IsOwner && Application.isFocused) {
			float mouseX = Input.mousePosition.x / (float)Screen.height;
			float mouseY = Input.mousePosition.y / (float)Screen.height;
			float mouseDx = mouseX - prevMouseX;
			float mouseDy = mouseY - prevMouseY;
			
			clientTargetArmPos2D += 3f * new Vector2(mouseDx, mouseDy);
			if (clientTargetArmPos2D.magnitude > 1) {
				clientTargetArmPos2D = clientTargetArmPos2D.normalized;
			}
			
			Vector2 newClientLazyArmPos2D = Vector2.MoveTowards(clientLazyArmPos2D, clientTargetArmPos2D, 7f*Time.deltaTime);
			// Vector2 newClientLazyArmPos2D = Vector2.Lerp(clientLazyArmPos2D, clientTargetArmPos2D, 10f*Time.deltaTime);
			
			Vector3 oldHandPos = ArmPos3DFromArmPos2D(clientLazyArmPos2D);
			Vector3 targetHandPos = ArmPos3DFromArmPos2D(newClientLazyArmPos2D);
			clientLazyArmPos2D = newClientLazyArmPos2D;
			
			Vector3 armVel = (targetHandPos - oldHandPos) / Time.deltaTime;
			
			float moveX = Input.GetAxis("Horizontal");
			float moveY = Input.GetAxis("Vertical");
			SendPerFrameInputToServerRpc(moveX, moveY, clientLazyArmPos2D.x, clientLazyArmPos2D.y, armVel);
			
			prevMouseX = mouseX;
			prevMouseY = mouseY;
			
			if (Input.GetKey(KeyCode.Space)) {
				SendJumpInputToServerRpc();
			}
		}
		
		// draw hand
		if (IsClient) {
			float theta = armPos2D.Value.magnitude*armThetaScale;
			float z = Mathf.Cos(theta);
			float x = Mathf.Sin(theta);
			
			Vector2 armPos2DNorm = armPos2D.Value.normalized;
			// Vector3 armPos3D = new Vector3(armPos2DNorm.x, 0, armPos2DNorm.y)*x + new Vector3(0, z, 0);
			
			Vector3 rot = new Vector3(theta*Mathf.Rad2Deg, -Mathf.Atan2(armPos2DNorm.y, armPos2DNorm.x)*Mathf.Rad2Deg + 90, 0);
			HandCapsuleOrigin.transform.localEulerAngles = rot;
			HandVizL.transform.localEulerAngles = rot;
			HandVizR.transform.localEulerAngles = rot;
		}
	}
}
