using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InControl;

// This class persists through all scenes and stores:
// - Player Settings (future possibility)
// - Game objects relevant over multipler scenes (players)

public class PlayerInfo {
	public PlayerInfo (InputDevice dev, CharacterSettings cs, TeamSettings ts) {
		input = dev;
		charSettings = cs;
		teamSettings = ts;
	}
	public InputDevice 			input;
	public CharacterSettings 	charSettings;
	public TeamSettings 		teamSettings;
}

public class GameManager : MonoBehaviour {

	private static GameManager 			singleton = null;
	private	static List<PlayerInfo> 	players = null;

	public static GameManager GetGameManager() {
		return singleton;
	}

	void Awake() {
		if(singleton == null)
			singleton = this;
		if (players == null)
			players = new List<PlayerInfo> ();

		Cursor.visible = false;
	}


	/**************** Utility ****************/
	// TODO: Update GameManager to handle player spawning
	public static List<PlayerInfo> GetPlayerList () {
		return players;
	}

	public Player[] GetPlayers() {
		Player[] players = new Player[4];
		players[0] = GameObject.Find ("Player1").GetComponent<Player>();
		players[1] = GameObject.Find ("Player2").GetComponent<Player>();
		players[2] = GameObject.Find ("Player3").GetComponent<Player>();
		players[3] = GameObject.Find ("Player4").GetComponent<Player>();
		return players;
	}

	/**************** Configuration Funcs ****************/
	public void AddPlayer (InputDevice input, CharacterSettings charSet, TeamSettings teamSet) {
		PlayerInfo new_player = new PlayerInfo (input, charSet, teamSet);
		players.Add (new_player);
	}
}
