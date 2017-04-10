using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine;
using InControl;

public enum MenuState {
	Home,
	Character,
	Team,
	Confirm
}

public class MenuController : MonoBehaviour {
	private static MenuController		singleton;

	public AutoBackgroundScroller		bg;
	public CharacterSettings[]			characters;

	private EventSystem 				es;
	private GameObject					homeScreen;
	private GameObject					charSelectScreen;
	private GameObject					teamSelectScreen;
	private GameObject					gameConfirmScreen;

	public bool						switching = false;
	public MenuState 					state;
	private Dictionary<MenuState, Action>	stateMapUpdate;
	private CharacterSelectBar[] 		charSelecters;
	private TeamSelectBar[] 			teamSelecters;
	private Dictionary<InputDevice, CharacterSettings>  devices;

	void Awake () {
		singleton = this;
	}

	public static MenuController GetMenuController() {
		return singleton;
	}

	void Start () {
		// Grab child objects
		es = GameObject.Find("EventSystem").GetComponent<EventSystem>();
		homeScreen = transform.Find ("Home").gameObject;
		homeScreen.SetActive (false);
		charSelectScreen = transform.Find ("CharacterSelect").gameObject;
		charSelectScreen.SetActive (false);
		teamSelectScreen = transform.Find ("TeamSelect").gameObject;
		teamSelectScreen.SetActive (false);
		gameConfirmScreen = transform.Find ("Confirm").gameObject;
		gameConfirmScreen.SetActive (false);

		// Set beginning variables
		state = MenuState.Home;

		stateMapUpdate = new Dictionary<MenuState, Action> ();
		stateMapUpdate.Add (MenuState.Home, HomeUpdate);
		stateMapUpdate.Add (MenuState.Character, CharacterUpdate);
		stateMapUpdate.Add (MenuState.Team, TeamUpdate);
		stateMapUpdate.Add (MenuState.Confirm, ConfirmUpdate);

		charSelecters = new CharacterSelectBar[4];
		charSelecters [0] = charSelectScreen.transform.Find ("P1Select").GetComponent<CharacterSelectBar>();
		charSelecters [1] = charSelectScreen.transform.Find ("P2Select").GetComponent<CharacterSelectBar>();
		charSelecters [2] = charSelectScreen.transform.Find ("P3Select").GetComponent<CharacterSelectBar>();
		charSelecters [3] = charSelectScreen.transform.Find ("P4Select").GetComponent<CharacterSelectBar>();

		teamSelecters = new TeamSelectBar[4];
		teamSelecters [0] = teamSelectScreen.transform.Find("Players").transform.Find ("P1Select").GetComponent<TeamSelectBar>();
		teamSelecters [1] = teamSelectScreen.transform.Find("Players").transform.Find ("P2Select").GetComponent<TeamSelectBar>();
		teamSelecters [2] = teamSelectScreen.transform.Find("Players").transform.Find ("P3Select").GetComponent<TeamSelectBar>();
		teamSelecters [3] = teamSelectScreen.transform.Find("Players").transform.Find ("P4Select").GetComponent<TeamSelectBar>();

		devices = new Dictionary<InputDevice, CharacterSettings> ();
		foreach (InputDevice device in InputManager.Devices)
			AddController (device);

		// Update delegates for controllers
		InputManager.OnDeviceAttached += AddController;
		InputManager.OnDeviceDetached += RemoveController;

		InitHomeScreen ();
	}

	void AddController (InputDevice device) {
		Debug.Log ("Device Attached: " + device.Name);
		if (devices.Count < 4) {
			devices.Add (device, null);
			if (state == MenuState.Character) {
				foreach (CharacterSelectBar bar in charSelecters) {
					if (bar.WaitingForPlayer ()) {
						bar.AssignPlayer (device);
						break;
					}
				}
			}
		}
	}

	void RemoveController (InputDevice device) {
		Debug.Log ("Device Removed: " + device.Name);
		if(devices.ContainsKey(device)) {
			if (state == MenuState.Character) {
				foreach (CharacterSelectBar bar in charSelecters) {
					if (bar.GetDevice () == device) {
						if (bar.GetSelectedCharacter ()) {
							bar.GetSelectedCharacter ().selected = false;
							bar.GetSelectedCharacter ().teamNumber = 0;
						}
						bar.ResetPlayerSelect ();
					}
				}
			} else if (state == MenuState.Team) {
				InitCharacterScreen();
			}
			devices.Remove (device);
		}
	}

	void HomeUpdate () {
	}

	void CharacterUpdate () {
		if (InputManager.ActiveDevice.Action2.WasPressed && devices.ContainsKey(InputManager.ActiveDevice)) {
			foreach (CharacterSelectBar bar in charSelecters) {
				if (bar.GetDevice () == InputManager.ActiveDevice
					&& bar.WaitingForPlayer()) {
					InitHomeScreen ();
					return;
				}
			}
		}

		int ready = 0;
		int team1 = 0;
		int team2 = 0;
		foreach (CharacterSelectBar bar in charSelecters) {
			if (!bar.GetSelectedCharacter () && !bar.WaitingForPlayer ())
				return;
			if (bar.GetSelectedCharacter () && bar.GetSelectedTeam () > 0) {
				++ready;
				if (bar.GetSelectedTeam() == 1)
					++team1;
				else if (bar.GetSelectedTeam() == 2)
					++team2;
			}
		}

		if (ready >= 2 && team1 > 0 && team2 > 0) {
			// TODO: Display "Press A to Continue"
			if (InputManager.ActiveDevice.Action1.WasPressed) {
				foreach (CharacterSelectBar bar in charSelecters) {
					if (bar.GetSelectedCharacter ())
						devices [bar.GetDevice ()] = bar.GetSelectedCharacter ();
				}
				InitConfirmScreen ();
				return;
			}
		}
	}

	void TeamUpdate () {
		if (InputManager.ActiveDevice.Action2.WasPressed) {
			InitCharacterScreen ();
			return;
		}

		int team1 = 0, team2 = 0;
		foreach (KeyValuePair<InputDevice, CharacterSettings> entry in devices) {
			if (entry.Value) {
				if (entry.Value.teamNumber == 0)
					return;
				else if (entry.Value.teamNumber == 1)
					++team1;
				else if (entry.Value.teamNumber == 2)
					++team2;
			}
		}

		if(team1 > 0 && team2 > 0) {
			// TODO: Display "Press A to Continue"
			if (InputManager.ActiveDevice.Action1.WasPressed) {
				InitConfirmScreen ();
				return;
			}
		}
	}

	void ConfirmUpdate () {
		if (InputManager.ActiveDevice.Action2.WasPressed) {
			InitCharacterScreen ();
			return;
		}
	}

	void Update () {
		if(!switching) {
			stateMapUpdate[state]();
		}
	}
		
	void ResetCharacterSettings() {
		foreach (CharacterSettings character in characters) {
			character.selected = false;
			character.teamNumber = 0;
		}
	}

	/******************** Switch Screen Functions ********************/

	void InitHomeScreen () {
		state = MenuState.Home;

		DeactivateScreens ();
		homeScreen.SetActive (true);

		ResetCharacterSettings ();
		bg.SetScrollSpeed (0.5f);

		es.SetSelectedGameObject(homeScreen.transform.Find ("Canvas").Find ("StartButton").gameObject);
	}

	void InitCharacterScreen() {
		state = MenuState.Character;

		DeactivateScreens ();
		charSelectScreen.SetActive (true);

		ResetCharacterSettings ();
		bg.SetScrollSpeed (1.5f);

		foreach (CharacterSelectBar bar in charSelecters)
			bar.ResetPlayerSelect ();

		int charSelectCount = 0;
		foreach (InputDevice key in devices.Keys.ToList()) {
			devices [key] = null;
			charSelecters [charSelectCount].AssignPlayer (key);
			++charSelectCount;
		}
	}

	void InitTeamScreen() {
		state = MenuState.Team;

		DeactivateScreens ();
		teamSelectScreen.SetActive (true);

		ResetCharacterSettings ();
		bg.SetScrollSpeed (3f);

		foreach (TeamSelectBar bar in teamSelecters)
			bar.ResetTeamSelect ();

		int teamSelectCount = 0;
		foreach (KeyValuePair<InputDevice, CharacterSettings> entry in devices) {
			if (entry.Value) {
				teamSelecters [teamSelectCount].AssignPlayer (entry.Key, entry.Value);
				++teamSelectCount;
			}
		}
	}

	void InitConfirmScreen () {
		state = MenuState.Confirm;

		DeactivateScreens ();
		gameConfirmScreen.SetActive (true);

		bg.SetScrollSpeed (10f);
		es.SetSelectedGameObject(gameConfirmScreen.transform.Find ("Canvas").Find ("ConfirmButton").gameObject);
	}

	void DeactivateScreens() {
		es.SetSelectedGameObject (null);
		if (homeScreen.activeSelf)
			StartCoroutine (SwitchOffScreen (homeScreen));
		if (charSelectScreen.activeSelf)
			StartCoroutine (SwitchOffScreen (charSelectScreen));
		if (teamSelectScreen.activeSelf)
			StartCoroutine (SwitchOffScreen (teamSelectScreen));
		if (gameConfirmScreen.activeSelf) 
			StartCoroutine (SwitchOffScreen (gameConfirmScreen));
	}

	IEnumerator SwitchOffScreen (GameObject screen) {
		switching = true;
		screen.transform.localScale = Vector3.one;
		Vector3 big = new Vector3 (0f, 1f, 1f);
		print ("Switching off: " + screen.name);
		while (screen.transform.localScale.x > 0.01f) {
			screen.transform.localScale = Vector3.Lerp (screen.transform.localScale, big, 0.25f);
			yield return null;
		}
		screen.SetActive (false);
		screen.transform.localScale = Vector3.one;
		switching = false;
	}

	/******************** Button Functions ********************/

	public void StartButton(){
		if (!switching)
			InitCharacterScreen ();
	}

	public void HowToPlayButton(){
		SceneManager.LoadScene ("tutorial");
	}

	public void ConfirmButton() {
		SceneManager.LoadScene ("main");
	}

	public void ExitButton(){
		Application.Quit ();
	}
}
		