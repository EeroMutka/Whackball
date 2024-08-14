using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
	
	public GameObject handViz;
	
	CharacterController characterController;
	
	float prevMouseX;
	float prevMouseY;
	
	
	int localFrameIndex = 0;
	
	// networked variables, automatically replicated from server to clients
	// -transform.position is implicitly like this
	public NetworkVariable<Vector2> armPos2D;
	
	// server-only variables
	float lastInputMoveX = 0f;
	float lastInputMoveY = 0f;
	
	// owning-client-only variables
	Vector2 clientArmPos2D;
	
	void Start()
	{
		characterController = GetComponent<CharacterController>();
	}
	
	[Rpc(SendTo.Server, Delivery = RpcDelivery.Unreliable)]
	void SendInputToServerRpc(float moveX, float moveY, float armPosX, float armPosY) {
		lastInputMoveX = moveX;
		lastInputMoveY = moveY;
		armPos2D.Value = new Vector2(armPosX, armPosY);
	}
	
	void Update()
	{
		if (IsServer) {
			Vector3 move = new Vector3(lastInputMoveX, 0, lastInputMoveY);
			Debug.Log($"Server move: {move}");
			characterController.Move(move * Time.deltaTime * 5f);
		}
		
		if (IsClient && IsOwner && Application.isFocused) {
			float mouseX = Input.mousePosition.x / (float)Screen.height;
			float mouseY = Input.mousePosition.y / (float)Screen.height;
			float mouseDx = mouseX - prevMouseX;
			float mouseDy = mouseY - prevMouseY;
			
			clientArmPos2D += 3f * new Vector2(mouseDx, mouseDy);
			if (clientArmPos2D.magnitude > 1) {
				clientArmPos2D = clientArmPos2D.normalized;
			}
			
			float moveX = Input.GetAxis("Horizontal");
			float moveY = Input.GetAxis("Vertical");
			SendInputToServerRpc(moveX, moveY, clientArmPos2D.x, clientArmPos2D.y);
			
			prevMouseX = mouseX;
			prevMouseY = mouseY;
		}
		
		// draw hand
		if (IsClient) {
			float theta = armPos2D.Value.magnitude*2f;
			float z = Mathf.Cos(theta);
			float x = Mathf.Sin(theta);
			
			Vector2 armPos2DNorm = armPos2D.Value.normalized;
			Vector3 armPos3D = new Vector3(armPos2DNorm.x, 0, armPos2DNorm.y)*x + new Vector3(0, z, 0);
			
			handViz.transform.localPosition = armPos3D;
		}
	}
}
