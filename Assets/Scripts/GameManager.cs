using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using TMPro;

public class GameManager : NetworkBehaviour
{
	public Camera camera;
	public Ball ball;
	
	public GameObject maxXBounds;
	public GameObject maxZBounds;
	public GameObject minXBounds;
	public GameObject minZBounds;
	
	public TextMeshProUGUI blueScoreText;
	public TextMeshProUGUI orangeScoreText;
	
	public int blueScore;
	public int orangeScore;
	
	bool ballIsIdle = true;
	
	public NetworkVariable<bool> gameHasStarted;
	
	public void StartGame() {
		gameHasStarted.Value = true;
	}
	
	public void ResetBall(int side) {
		ball.transform.position = new Vector3(0f, 1.5f, (float)side * 4f);
		ball.isIdle = true;
	}
	
	public void OnBallHitGround() {
		int ballSide = ball.transform.position.z > 0 ? 1 : -1;
		
		bool outOfBounds =
			ball.transform.position.x > maxXBounds.transform.position.x ||
			ball.transform.position.x < minXBounds.transform.position.x ||
			ball.transform.position.z > maxZBounds.transform.position.z ||
			ball.transform.position.z < minZBounds.transform.position.z;
		
		int pointToSide = -ballSide * ball.lastTouchedPlayerSide;
		
		if (outOfBounds && ballSide != ball.lastTouchedPlayerSide) {
			pointToSide *= -1;
		}
		
		if (pointToSide > 0) { // +z is blue, -z is orange
			blueScore++;
		}
		else {
			orangeScore++;
		}
		
		// startTurnSide = -startTurnSide;
		ResetBall(-pointToSide);
	}
	
	
	void Start()
	{
		ResetBall(-1);
	}

	void Update()
	{
		blueScoreText.text = $"Blue: {blueScore}";
		orangeScoreText.text = $"Orange: {orangeScore}";
	}
}
