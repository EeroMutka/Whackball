using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class WhackballManager : MonoBehaviour
{
	NetworkManager networkManager;
	
	void Awake()
	{
		networkManager = GetComponent<NetworkManager>();
		// Debug.Log("Starting host");
		// networkManager.StartHost();
	}
	
	void OnGUI()
	{
		GUILayout.BeginArea(new Rect(10, 10, 300, 300));
		// Debug.Log($"networkManager.IsClient {networkManager.IsClient} networkManager.IsServer {networkManager.IsServer}");
		if (!networkManager.IsClient && !networkManager.IsServer)
		{
			StartButtons();
		}
		else
		{
			StatusLabels();
			SubmitNewPosition();
		}

		GUILayout.EndArea();
	}

	void StartButtons()
	{
		if (GUILayout.Button("Host")) networkManager.StartHost();
		if (GUILayout.Button("Client")) networkManager.StartClient();
		if (GUILayout.Button("Server")) networkManager.StartServer();
	}
	
	void StatusLabels()
	{
		var mode = networkManager.IsHost ?
			"Host" : networkManager.IsServer ? "Server" : "Client";

		GUILayout.Label("Transport: " +
			networkManager.NetworkConfig.NetworkTransport.GetType().Name);
		GUILayout.Label("Mode: " + mode);
	}
	
	void SubmitNewPosition()
	{
		/*if (GUILayout.Button(networkManager.IsServer ? "Move" : "Request Position Change"))
		{
			if (networkManager.IsServer && !networkManager.IsClient)
			{
				foreach (ulong uid in networkManager.ConnectedClientsIds)
					networkManager.SpawnManager.GetPlayerNetworkObject(uid).GetComponent<Player>().Move();
			}
			else
			{
				var playerObject = networkManager.SpawnManager.GetLocalPlayerObject();
				var player = playerObject.GetComponent<Player>();
				player.Move();
			}
		}*/
	}
}