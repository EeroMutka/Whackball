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
	public WhackballManager whackballManager;
	
	// networked variables, automatically replicated from server to clients
	// -transform.position is implicitly like this
	public NetworkVariable<Vector2> armPos2D;
	
	
	// valid on server + client
	public int fieldSide; // -1 for z < 0, +1 for z > 0
	
	// server-only variables
	public bool serverHasInitialized = false;
	public float lastInputMoveX = 0f;
	public float lastInputMoveY = 0f;
	public Vector3 velocity;
	public Vector3 lastArmVel;
	
	// owning-client-only variables
	float cameraDirection = 1f;
	public float prevMouseX;
	public float prevMouseY;
	public Vector2 clientTargetArmPos2D;
	public Vector2 clientLazyArmPos2D;
	
	const float armThetaScale = 2f;
	
	void Start()
	{
		characterController = GetComponent<CharacterController>();
		
		GameObject networkManagerObject = GameObject.Find("NetworkManager");
		whackballManager = networkManagerObject.GetComponent<WhackballManager>();
		
		fieldSide = OwnerClientId % 2 == 0 ? 1 : -1;
		
		// float forwardDir =  ? 1f : -1f;
		if (IsClient && IsOwner) {
			cameraDirection = -(float)fieldSide;
			if (fieldSide > 0) {
				Vector3 cam_pos = whackballManager.camera.transform.position;
				Vector3 cam_rot = whackballManager.camera.transform.localEulerAngles;
				whackballManager.camera.transform.position = new Vector3(cam_pos.x, cam_pos.y, -cam_pos.z);
				whackballManager.camera.transform.localEulerAngles = new Vector3(cam_rot.x, cam_rot.y + 180, cam_rot.z);
			}
		}
		
	}
	
	/*public override void OnNetworkSpawn() {
		base.OnNetworkSpawn();
		
		if (IsServer) {
			// transform.position = new Vector3(0, 10f, 10f);
			
		}
	}*/
	
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
			velocity = new Vector3(velocity.x, 7f, velocity.z);
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
			const float moveSpeed = 5f;
			
			velocity = new Vector3(lastInputMoveX*moveSpeed, velocity.y - 15f * Time.deltaTime, lastInputMoveY*moveSpeed);
			
			characterController.Move(velocity * Time.deltaTime);
			
			if (characterController.isGrounded) velocity = new Vector3(velocity.x, 0, velocity.z);

			// for some reason this only works after calling characterController.Move(), not before.
			if (!serverHasInitialized) {
				transform.position = new Vector3(0, 4f, (float)fieldSide * 4f);
				serverHasInitialized = true;
			}
		}
		
		if (IsClient && IsOwner && Application.isFocused) {
			float mouseX = Input.mousePosition.x / (float)Screen.height;
			float mouseY = Input.mousePosition.y / (float)Screen.height;
			float mouseDx = cameraDirection * (mouseX - prevMouseX);
			float mouseDy = cameraDirection * (mouseY - prevMouseY);
			
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
			
			float moveX = Input.GetAxis("Horizontal") * cameraDirection;
			float moveY = Input.GetAxis("Vertical") * cameraDirection;
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
