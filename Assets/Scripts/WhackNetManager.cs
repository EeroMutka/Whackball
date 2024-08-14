using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class WhackNetManager : MonoBehaviour
{
	NetworkManager networkManager;
	GameManager game;
	
	void Awake()
	{
		networkManager = GetComponent<NetworkManager>();
		game = GameObject.Find("GameManager").GetComponent<GameManager>();
		// Debug.Log("Starting host");
		// networkManager.StartHost();
	}
	
	void OnGUI()
	{
		GUILayout.BeginArea(new Rect(10, 10, 300, 300));
		
		if (!networkManager.IsClient && !networkManager.IsServer)
		{
			StartButtons();
		}
		else
		{
			StatusLabels();
			
			if (!game.gameHasStarted.Value) {
				if (networkManager.IsServer) {
					if (GUILayout.Button("Start Game")) {
						game.StartGame();
					}
				}
				else {
					GUILayout.Label("Waiting for the host to start the game...");
				}
			}
			
		}

		GUILayout.EndArea();
	}

	[Rpc(SendTo.Server)]
	void OnStartGame() {
	}
	
	void StartButtons()
	{
		if (GUILayout.Button("Host")) networkManager.StartHost();
		if (GUILayout.Button("Client")) networkManager.StartClient();
		if (GUILayout.Button("Server")) networkManager.StartServer();
	}
	
	// void LaunchBall
	
	void StatusLabels()
	{
		var mode = networkManager.IsHost ?
			"Host" : networkManager.IsServer ? "Server" : "Client";

		GUILayout.Label("Transport: " +
			networkManager.NetworkConfig.NetworkTransport.GetType().Name);
		GUILayout.Label("Mode: " + mode);
	}
	
}