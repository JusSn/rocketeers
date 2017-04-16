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
	Confirm
}

public class MenuController : MonoBehaviour {
	
	private static MenuController		singleton;

	public AutoBackgroundScroller		bg;
	public CharacterSettings[]			characters;

	private EventSystem 				es;
	private GameObject					homeScreen;
	private GameObject					charSelectScreen;
	private GameObject					gameConfirmScreen;

	public bool								switching = false;
	public MenuState 						state;
	private Dictionary<MenuState, Action>	stateMapUpdate;
	private CharacterSelectBar[] 			charSelecters;
	private Dictionary<Character, bool>		charStatus;
	private Dictionary<InputDevice, CharacterSelectBar>  devices;

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
		gameConfirmScreen = transform.Find ("Confirm").gameObject;
		gameConfirmScreen.SetActive (false);

		// Set beginning variables
		state = MenuState.Home;

		stateMapUpdate = new Dictionary<MenuState, Action> ();
		stateMapUpdate.Add (MenuState.Home, HomeUpdate);
		stateMapUpdate.Add (MenuState.Character, CharacterUpdate);
		stateMapUpdate.Add (MenuState.Confirm, ConfirmUpdate);

		charSelecters = new CharacterSelectBar[4];
		charSelecters [0] = charSelectScreen.transform.Find ("P1Select").GetComponent<CharacterSelectBar>();
		charSelecters [1] = charSelectScreen.transform.Find ("P2Select").GetComponent<CharacterSelectBar>();
		charSelecters [2] = charSelectScreen.transform.Find ("P3Select").GetComponent<CharacterSelectBar>();
		charSelecters [3] = charSelectScreen.transform.Find ("P4Select").GetComponent<CharacterSelectBar>();

		charStatus = new Dictionary<Character, bool> ();
		charStatus [Character.Yellow] = false;
		charStatus [Character.Blue] = false;
		charStatus [Character.Green] = false;
		charStatus [Character.Pink] = false;

		devices = new Dictionary<InputDevice, CharacterSelectBar> ();
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
						devices [device] = bar;
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
				if (devices [device].GetSelectedCharacter ()) {
					charStatus [devices [device].GetSelectedCharacter ().GetCharacterType ()] = false;
				}
				devices [device].ResetPlayerSelect ();
			}
			devices.Remove (device);
		}
	}

	void HomeUpdate () {
	}

	void CharacterUpdate () {
		// Return to the main menu
		if (InputManager.ActiveDevice.Action2.WasPressed && devices.ContainsKey(InputManager.ActiveDevice)) {
			foreach (CharacterSelectBar bar in charSelecters) {
				if (bar.GetDevice () == InputManager.ActiveDevice
					&& bar.WaitingForPlayer()) {
					InitHomeScreen ();
					return;
				}
			}
		}
			
		bool ready = true;

		int team1 = 0;
		int team2 = 0;
		foreach (CharacterSelectBar bar in charSelecters) {
			if (!bar.IsReady () && !bar.WaitingForPlayer ()) {
				ready = false;
			} else if (bar.IsReady ()) {
				if (bar.GetSelectedTeam () == 1)
					++team1;
				else if (bar.GetSelectedTeam () == 2)
					++team2;
			}
		}

		if (ready)
			ready = team1 > 0 && team2 > 0 && team1 + team2 > 1;

		//if (ready >= 2 && team1 > 0 && team2 > 0) 
		if (ready && InputManager.ActiveDevice.Action1.WasPressed && devices.ContainsKey(InputManager.ActiveDevice)) {
			// TODO: Display "Press A to Continue"
				InitConfirmScreen ();
				return;
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

	/******************** Switch Screen Functions ********************/

	void InitHomeScreen () {
		state = MenuState.Home;

		DeactivateScreens ();
		homeScreen.SetActive (true);

		bg.SetScrollSpeed (0.5f);

		es.SetSelectedGameObject(homeScreen.transform.Find ("Canvas").Find ("StartButton").gameObject);
	}

	void InitCharacterScreen() {
		state = MenuState.Character;

		DeactivateScreens ();
		charSelectScreen.SetActive (true);

		bg.SetScrollSpeed (1.5f);

		foreach (CharacterSelectBar bar in charSelecters)
			bar.ResetPlayerSelect ();
		foreach (Character setting in charStatus.Keys.ToList())
			charStatus [setting] = false;
		
		int charSelectCount = 0;
		foreach (InputDevice key in devices.Keys.ToList()) {
			devices [key] = charSelecters [charSelectCount];
			charSelecters [charSelectCount].AssignPlayer (key);
			++charSelectCount;
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

	/******************** Public Interface Functions ********************/

	public bool CharacterIsSelected (Character charType) {
		return charStatus [charType];
	}

	public void SetCharacterSelect (Character charType, bool selected) {
		charStatus [charType] = selected;
	}

	/******************** Button Functions ********************/

	public void StartButton(){
		if (!switching)
			InitCharacterScreen ();
	}

	public void HowToPlayButton(){
		SceneManager.LoadScene ("new_tutorial");
	}

	public void ConfirmButton() {
		SceneManager.LoadScene ("main");
	}

	public void ExitButton(){
		Application.Quit ();
	}
}
		