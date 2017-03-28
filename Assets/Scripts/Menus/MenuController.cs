using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine;

public enum MenuStage {
	Home,        		// Start screen
	Objective,       	// 1st page of tutorial
	PlayerControls,		// 2nd page of tutorial
	BuildControls,		// 3rd page of tutorial
	BattleControls		// 4th page of tutorial
}

public class MenuController : MonoBehaviour {

	public AutoBackgroundScroller		bg;
	public MenuStage					stage;

	private GameObject					homeScreen;
	private GameObject 					tutorialScreen;
	private GameObject					objectiveScreen;
	private GameObject 					playerControlScreen;
	private GameObject					buildControlScreen;
	private GameObject					battleControlScreen;

	private SwitchSprites[]				checkBoxes;

	private Dictionary<MenuStage, Action>  tutorialMap;

	void Start() {
		stage = MenuStage.Home;

		homeScreen = transform.Find ("Home").gameObject;
		tutorialScreen = transform.Find ("Tutorial").gameObject;

		objectiveScreen = tutorialScreen.transform.Find ("GameObjective").gameObject;
		playerControlScreen = tutorialScreen.transform.Find ("PlayerControls").gameObject;
		buildControlScreen = tutorialScreen.transform.Find ("BuildControls").gameObject;
		battleControlScreen = tutorialScreen.transform.Find ("BattleControls").gameObject;

		checkBoxes = new SwitchSprites[4];
		checkBoxes [0] = tutorialScreen.transform.Find ("Player1Confirm").GetComponent<SwitchSprites> ();
		checkBoxes [1] = tutorialScreen.transform.Find ("Player2Confirm").GetComponent<SwitchSprites> ();
		checkBoxes [2] = tutorialScreen.transform.Find ("Player3Confirm").GetComponent<SwitchSprites> ();
		checkBoxes [3] = tutorialScreen.transform.Find ("Player4Confirm").GetComponent<SwitchSprites> ();

		tutorialMap = new Dictionary<MenuStage, Action> ();
		tutorialMap.Add (MenuStage.Objective, InitPlayerControls);
		tutorialMap.Add (MenuStage.PlayerControls, InitBuildControls);
		tutorialMap.Add (MenuStage.BuildControls, InitBattleControls);
		tutorialMap.Add (MenuStage.BattleControls, InitHomeScreen);
	}

	void Update() {
		if (stage != MenuStage.Home) {
			if (Input.GetButtonDown ("Start_P1"))
				checkBoxes [0].SwitchOn ();
			if (Input.GetButtonDown ("Start_P2"))
				checkBoxes [1].SwitchOn ();
			if (Input.GetButtonDown ("Start_P3"))
				checkBoxes [2].SwitchOn ();
			if (Input.GetButtonDown ("Start_P4"))
				checkBoxes [3].SwitchOn ();
			if (AllOn ()) {
				SwitchOffConfirms ();
				tutorialMap [stage] ();
			}
		}
	}

	/******************** Switch Screen Functions ********************/

	public void InitTutorial () {
		homeScreen.SetActive (false);

		tutorialScreen.SetActive (true);
		objectiveScreen.SetActive (true);
		stage = MenuStage.Objective;
	}

	public void InitHomeScreen () {
		DeactivateTutorial ();

		homeScreen.SetActive (true);
		stage = MenuStage.Home;
	}

	public void InitPlayerControls() {
		objectiveScreen.SetActive (false);

		playerControlScreen.SetActive (true);
		stage = MenuStage.PlayerControls;
	}

	public void InitBuildControls() {
		playerControlScreen.SetActive (false);

		buildControlScreen.SetActive (true);
		stage = MenuStage.BuildControls;
	}

	public void InitBattleControls() {
		buildControlScreen.SetActive (false);

		battleControlScreen.SetActive (true);
		stage = MenuStage.BattleControls;
	}

	public void DeactivateTutorial () {
		tutorialScreen.SetActive (false);
		objectiveScreen.SetActive (false);
		playerControlScreen.SetActive (false);
		buildControlScreen.SetActive (false);
		battleControlScreen.SetActive (false);
	}

	public bool AllOn() {
		if (checkBoxes [0].CheckSwitchOn () && checkBoxes [1].CheckSwitchOn ()
		    && checkBoxes [2].CheckSwitchOn () && checkBoxes [3].CheckSwitchOn ())
			return true;
		else
			return false;
	}

	public void SwitchOffConfirms() {
		checkBoxes [0].SwitchOff ();
		checkBoxes [1].SwitchOff ();
		checkBoxes [2].SwitchOff ();
		checkBoxes [3].SwitchOff ();
	}

	/******************** Button Functions ********************/

	public void StartButton(){
		SceneManager.LoadScene ("main");
	}

	public void HowToPlayButton(){
		bg.SetScrollSpeed (.1f);
		InitTutorial ();
	}

	public void ExitButton(){
		Application.Quit ();
	}
}
		