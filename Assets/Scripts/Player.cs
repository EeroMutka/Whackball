using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
	
	public GameObject handViz;
	
	CharacterController characterController;
	
	float prevMouseX;
	float prevMouseY;
	
	Vector2 armCircle;
	
	void Start()
	{
		characterController = GetComponent<CharacterController>();
	}
	
	void Update()
	{
		float mouseX = Input.mousePosition.x / (float)Screen.height;
		float mouseY = Input.mousePosition.y / (float)Screen.height;
		float mouseDx = mouseX - prevMouseX;
		float mouseDy = mouseY - prevMouseY;
		
		armCircle += 3f * new Vector2(mouseDx, mouseDy);
		if (armCircle.magnitude > 1) {
			armCircle = armCircle.normalized;
		}
		
		float theta = armCircle.magnitude*2f;
		float z = Mathf.Cos(theta);
		float x = Mathf.Sin(theta);
		
		Vector2 armCircleNorm = armCircle.normalized;
		Vector3 armSphere = new Vector3(armCircleNorm.x, 0, armCircleNorm.y)*x + new Vector3(0, z, 0);
		
		float yaw = Mathf.Atan2(armCircle.y, armCircle.x);
		
		// 3d position
		
		// arm local position in 3D... we need to project from
		handViz.transform.localPosition = armSphere;//new Vector3(armCircle.x, 0, armCircle.y);
		
		prevMouseX = mouseX;
		prevMouseY = mouseY;
		
		Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
		characterController.Move(move * Time.deltaTime * 5f);
	}
}
