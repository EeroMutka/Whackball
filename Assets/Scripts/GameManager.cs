using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
	public Camera camera;
	public Ball ball;
	
	public GameObject maxXBounds;
	public GameObject maxZBounds;
	public GameObject minXBounds;
	public GameObject minZBounds;
	
	// int startTurnSide = -1;
	bool ballIsIdle = true;
	
	public NetworkVariable<bool> gameHasStarted;
	
	public void StartGame() {
		gameHasStarted.Value = true;
		// ResetBall();
	}
	
	public void ResetBall() {
		int pointToSide = ball.transform.position.z > 0 ? -1 : +1;
		
		bool outOfBounds =
			ball.transform.position.x > maxXBounds.transform.position.x ||
			ball.transform.position.x < minXBounds.transform.position.x ||
			ball.transform.position.z > maxZBounds.transform.position.z ||
			ball.transform.position.z < minZBounds.transform.position.z;
		if (outOfBounds) pointToSide = -pointToSide;
		
		float side = -(float)pointToSide;
		ball.transform.position = new Vector3(0f, 1.5f, side * 4f);
		ball.isIdle = true;
	}
	
	public void OnBallHitGround() {
		// startTurnSide = -startTurnSide;
		ResetBall();
	}
	
	
	void Start()
	{
		ResetBall();
	}

	void Update()
	{
		
	}
}
