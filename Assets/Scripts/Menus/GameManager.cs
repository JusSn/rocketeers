using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InControl;

// This class persists through all scenes and stores:
// - Player Settings (future possibility)
// - Game objects relevant over multipler scenes (players)

public class GameManager : MonoBehaviour {
    public bool                         jellyMode;
	private static GameManager 			singleton;


	public static GameManager GetGameManager() {
		return singleton;
	}

	void Awake() {
        DontDestroyOnLoad(this);

		singleton = this;

		Cursor.visible = false;
	}


	/**************** Utility ****************/
	// TODO: Update GameManager to handle player spawning
	public Player[] GetPlayers() {
		Player[] players = new Player[4];
		players[0] = GameObject.Find ("Player1").GetComponent<Player>();
		players[1] = GameObject.Find ("Player2").GetComponent<Player>();
		players[2] = GameObject.Find ("Player3").GetComponent<Player>();
		players[3] = GameObject.Find ("Player4").GetComponent<Player>();
		return players;
	}

    public bool IsJellyMode(){
        return jellyMode;
    }

    public void ToggleJellyMode(){
        jellyMode = !jellyMode;
    }
}
