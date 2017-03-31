﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This class persists through all scenes and stores:
// - Player Settings (future possibility)
// - Game objects relevant over multipler scenes (players)

public class GameManager : MonoBehaviour {
	private static GameManager singleton;

	public static GameManager GetGameManager() {
		return singleton;
	}

	void Awake() {
		singleton = this;
	}

	// TODO: Update GameManager to handle player spawning
	public Player[] GetPlayers() {
		Player[] players = new Player[4];
		players[0] = GameObject.Find ("Player1").GetComponent<Player>();
		players[1] = GameObject.Find ("Player2").GetComponent<Player>();
		players[2] = GameObject.Find ("Player3").GetComponent<Player>();
		players[3] = GameObject.Find ("Player4").GetComponent<Player>();
		return players;
	}
}