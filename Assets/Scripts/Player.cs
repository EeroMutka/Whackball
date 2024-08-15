using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using TMPro;

public class Player : NetworkBehaviour
{
	static readonly string[] emoteFromId = new string[]{"", "Hello", "Oh no...", "Whoops", "Yo", "What a shot!", "Unlucky...", "Great whack", "lol", "wtf"};
	
	public GameObject HandCapsuleOrigin;
	public GameObject HandVizL;
	public GameObject HandVizR;
	public MeshRenderer[] Meshes;
	public Material OrangeMaterial;
	public Material BlueMaterial;
	
	public TextMeshPro emoteText;
	public CharacterController characterController;
	public GameManager game;
	
	// networked variables, automatically replicated from server to clients
	// -transform.position is implicitly like this
	public NetworkVariable<Vector2> armPos2D;
	public NetworkVariable<int> displayEmoteId; // 0 represents none
	
	// valid on server + client
	public int fieldSide; // -1 for z < 0, +1 for z > 0 (+z is blue, -z is orange)
	
	// server-only variables
	public bool serverHasInitialized = false;
	public float lastInputMoveX = 0f;
	public float lastInputMoveY = 0f;
	public Vector3 velocity;
	public Vector3 lastArmVel;
	public float displayEmoteTimer;
	
	// owning-client-only variables
	public float prevMouseX;
	public float prevMouseY;
	public Vector2 clientTargetArmPos2D;
	public Vector2 clientLazyArmPos2D;
	
	const float armThetaScale = 2f;
	
	void Start()
	{
		characterController = GetComponent<CharacterController>();
		
		game = GameObject.Find("GameManager").GetComponent<GameManager>();
		
		fieldSide = OwnerClientId % 2 == 0 ? 1 : -1;
		
		// float forwardDir =  ? 1f : -1f;
		if (IsClient && IsOwner) {
			game.SpecifyLocalClientSide(fieldSide);
		}
		
		if (IsClient) {
			List<Material> mats = new List<Material>();
			mats.Add(fieldSide > 0 ? BlueMaterial : OrangeMaterial); // +z is blue, -z is orange
			
			for (int i = 0; i < Meshes.Length; i++) {
				Meshes[i].SetMaterials(mats);
			}
		}
	}
	
	[Rpc(SendTo.Server, Delivery = RpcDelivery.Unreliable)]
	void SendPerFrameInputToServerRpc(float moveX, float moveY, Vector2 armPos, Vector3 armVel, bool jumping) {
		lastInputMoveX = moveX;
		lastInputMoveY = moveY;
		lastArmVel = armVel;
		armPos2D.Value = armPos;
		
		if (jumping && characterController.isGrounded) {
			velocity = new Vector3(velocity.x, 7f, velocity.z);
		}
	}
	
	[Rpc(SendTo.Server)]
	void SendShowEmoteRpc(int newEmoteId) {
		displayEmoteId.Value = newEmoteId;
		displayEmoteTimer = 2f;
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
			if (game.gameHasStarted.Value) {
				const float moveSpeed = 5f;
				
				velocity = new Vector3(lastInputMoveX*moveSpeed, velocity.y - 15f * Time.deltaTime, lastInputMoveY*moveSpeed);
				
				characterController.Move(velocity * Time.deltaTime);
				
				if (characterController.isGrounded) velocity = new Vector3(velocity.x, 0, velocity.z);
			}

			// for some reason this only works after calling characterController.Move(), not before.
			if (!serverHasInitialized) {
				transform.position = new Vector3(0, 0.5f, (float)fieldSide * 6f);
				serverHasInitialized = true;
			}
			
			if (displayEmoteId.Value != 0) {
				displayEmoteTimer -= Time.deltaTime;
				if (displayEmoteTimer < 0f) {
					displayEmoteId.Value = 0;
				}
			}
		}
		
		if (IsClient && IsOwner && Application.isFocused) {
			float mouseX = Input.mousePosition.x / (float)Screen.height;
			float mouseY = Input.mousePosition.y / (float)Screen.height;
			float mouseDx = game.clientCameraDirection * (mouseX - prevMouseX);
			float mouseDy = game.clientCameraDirection * (mouseY - prevMouseY);
			
			clientTargetArmPos2D += 3f * new Vector2(mouseDx, mouseDy);
			if (clientTargetArmPos2D.magnitude > 1) {
				clientTargetArmPos2D = clientTargetArmPos2D.normalized;
			}
			
			Vector2 newClientLazyArmPos2D = Vector2.MoveTowards(clientLazyArmPos2D, clientTargetArmPos2D, 7f*Time.deltaTime);
			Vector3 oldHandPos = ArmPos3DFromArmPos2D(clientLazyArmPos2D);
			Vector3 targetHandPos = ArmPos3DFromArmPos2D(newClientLazyArmPos2D);
			clientLazyArmPos2D = newClientLazyArmPos2D;
			
			Vector3 armVel = (targetHandPos - oldHandPos) / Time.deltaTime;
			
			float moveX = Input.GetAxis("Horizontal") * game.clientCameraDirection;
			float moveY = Input.GetAxis("Vertical") * game.clientCameraDirection;
			
			bool jumping = Input.GetKey(KeyCode.Space);
			
			SendPerFrameInputToServerRpc(moveX, moveY, clientLazyArmPos2D, armVel, jumping);
			
			prevMouseX = mouseX;
			prevMouseY = mouseY;
			
			// emotes
			int openEmoteId = 0;
			if (Input.GetKeyDown(KeyCode.Alpha1)) openEmoteId = 1;
			if (Input.GetKeyDown(KeyCode.Alpha2)) openEmoteId = 2;
			if (Input.GetKeyDown(KeyCode.Alpha3)) openEmoteId = 3;
			if (Input.GetKeyDown(KeyCode.Alpha4)) openEmoteId = 4;
			if (Input.GetKeyDown(KeyCode.Alpha5)) openEmoteId = 5;
			if (Input.GetKeyDown(KeyCode.Alpha6)) openEmoteId = 6;
			if (Input.GetKeyDown(KeyCode.Alpha7)) openEmoteId = 7;
			if (Input.GetKeyDown(KeyCode.Alpha8)) openEmoteId = 8;
			if (Input.GetKeyDown(KeyCode.Alpha9)) openEmoteId = 9;
			if (openEmoteId != 0) {
				SendShowEmoteRpc(openEmoteId);
			}
		}
		
		// drawing
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
			
			emoteText.text = emoteFromId[displayEmoteId.Value];
			emoteText.transform.eulerAngles = new Vector3(45, game.clientCameraDirection < 0 ? 180 : 0, 0); // rotate text to view
		}
	}
}
