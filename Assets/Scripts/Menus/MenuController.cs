using System;
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

	private GameObject					homeScreen;
	private GameObject					charSelectScreen;
	private GameObject					teamSelectScreen;
	private GameObject					gameConfirmScreen;

	private MenuState 					state;
	private CharacterSelectBar[] 		charSelecters;
	private Dictionary<InputDevice, CharacterSettings>  deviceSettings;

	void Awake () {
		singleton = this;
	}

	public static MenuController GetMenuController() {
		return singleton;
	}

	void Start () {
		// Grab child objects
		homeScreen = transform.Find ("Home").gameObject;
		charSelectScreen = transform.Find ("CharacterSelect").gameObject;
		teamSelectScreen = transform.Find ("TeamSelect").gameObject;
		gameConfirmScreen = transform.Find ("Confirm").gameObject;

		// Set beginning variables
		state = MenuState.Home;
		charSelecters = new CharacterSelectBar[4];
		charSelecters [0] = charSelectScreen.transform.Find ("P1Select").GetComponent<CharacterSelectBar>();
		charSelecters [1] = charSelectScreen.transform.Find ("P2Select").GetComponent<CharacterSelectBar>();
		charSelecters [2] = charSelectScreen.transform.Find ("P3Select").GetComponent<CharacterSelectBar>();
		charSelecters [3] = charSelectScreen.transform.Find ("P4Select").GetComponent<CharacterSelectBar>();

		deviceSettings = new Dictionary<InputDevice, CharacterSettings> ();
		foreach (InputDevice device in InputManager.Devices)
			AddController (device);

		// Update delegates for controllers
		InputManager.OnDeviceAttached += AddController;
		InputManager.OnDeviceDetached += RemoveController;

		InitHomeScreen ();
	}

	void AddController (InputDevice device) {
		Debug.Log ("Device Attached: " + device.Name);
		if (deviceSettings.Count < 4) {
			deviceSettings.Add (device, null);
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
		if(deviceSettings.ContainsKey(device))
			deviceSettings.Remove (device);
		if (state == MenuState.Character) {
			foreach (CharacterSelectBar bar in charSelecters) {
				if (bar.GetDevice () == device) {
					bar.RemovePlayer ();
				}
			}
		}
	}

	void Update () {
		if (state == MenuState.Character) {
			if (InputManager.ActiveDevice.MenuWasPressed) {
				foreach (KeyValuePair<InputDevice, CharacterSettings> entry in deviceSettings) {
					if (!entry.Value)
						return;
				}
				InitTeamScreen ();
			}
		}
	}

	/******************** Public Interface ********************/

	public void SetDeviceCharacter (InputDevice device, CharacterSettings character) {
		deviceSettings [device] = character;
	}

	public void ResetDeviceCharacter (InputDevice device) {
		deviceSettings [device] = null;
	}


	/******************** Switch Screen Functions ********************/

	public void InitHomeScreen () {
		DeactivateScreens ();
		homeScreen.SetActive (true);
	}

	public void InitCharacterScreen() {
		DeactivateScreens ();
		charSelectScreen.SetActive (true);

		int charSelectCount = 0;
		foreach (KeyValuePair<InputDevice, CharacterSettings> entry in deviceSettings) {
			charSelecters [charSelectCount].AssignPlayer (entry.Key);
			++charSelectCount;
		}
	}

	public void InitTeamScreen() {
		//DeactivateScreens ();
		//teamSelectScreen.SetActive (true);
		SceneManager.LoadScene ("main");
	}

	public void DeactivateScreens() {
		homeScreen.SetActive (false);
		charSelectScreen.SetActive (false);
		teamSelectScreen.SetActive (false);
		gameConfirmScreen.SetActive (false);
	}

	/******************** Button Functions ********************/

	public void StartButton(){
		state = MenuState.Character;
		InitCharacterScreen ();
		//SceneManager.LoadScene ("main");
	}

	public void HowToPlayButton(){
		SceneManager.LoadScene ("tutorial");
	}

	public void ExitButton(){
		Application.Quit ();
	}
}
		