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
	public TextMeshProUGUI winnerText;
	
	public NetworkVariable<int> blueScore;
	public NetworkVariable<int> orangeScore;
	
	public NetworkVariable<bool> gameHasStarted;
	public NetworkVariable<int> winnerSide; // 0 if neither
	
	public float clientCameraDirection = 1f;
	
	public void StartGame() {
		gameHasStarted.Value = true;
	}
	
	public void ResetBall(int side) {
		ball.transform.position = new Vector3(0f, 1.5f, (float)side * 4f);
		ball.isIdle.Value = true;
		
		if (IsServer) { // we want the ball to snap instead of lerping on clients when the ball is teleported
			ball.SendSnapBallPosRpc(ball.transform.position);
		}
	}
	
	// must be called by the server
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
		
		const int pointsToWin = 10;
		
		if (pointToSide > 0) { // +z is blue, -z is orange
			blueScore.Value++;
			if (blueScore.Value == pointsToWin && winnerSide.Value == 0) {
				winnerSide.Value = 1;
			}
		}
		else {
			orangeScore.Value++;
			if (orangeScore.Value == pointsToWin && winnerSide.Value == 0) {
				winnerSide.Value = -1;
			}
		}
		
		// startTurnSide = -startTurnSide;
		ResetBall(-pointToSide);
	}
	
	public void SpecifyLocalClientSide(int side) {
		clientCameraDirection = -(float)side;
		if (side > 0) {
			Vector3 cam_pos = camera.transform.position;
			Vector3 cam_rot = camera.transform.localEulerAngles;
			camera.transform.position = new Vector3(cam_pos.x, cam_pos.y, -cam_pos.z);
			camera.transform.localEulerAngles = new Vector3(cam_rot.x, cam_rot.y + 180, cam_rot.z);
		}
	}
	
	void Start()
	{
		ResetBall(-1);
	}

	void Update()
	{
		blueScoreText.text = $"Blue: {blueScore.Value}";
		orangeScoreText.text = $"Orange: {orangeScore.Value}";
		
		if (winnerSide.Value != 0) {
			winnerText.text = winnerSide.Value > 0 ? "Blue wins!" : "Orange wins!";
		}
	}
}
