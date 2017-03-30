using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine;

public enum TutorialStage {
	Objective,       	// 1st page of tutorial
	PlayerControls,		// 2nd page of tutorial
	BuildControls,		// 3rd page of tutorial
	BattleControls		// 4th page of tutorial
}

public class TutorialController : MonoBehaviour {

	public AutoBackgroundScroller		bg;
	public TutorialStage				stage;

	private GameObject					objectiveScreen;
	private GameObject 					playerControlScreen;
	private GameObject					buildControlScreen;
	private GameObject					battleControlScreen;

	private GameObject					rocket;

	private SwitchSprites[]				checkBoxes;
	private GameObject[]				players;

	private Dictionary<TutorialStage, Action>  tutorialMap;

	void Start() {
		stage = TutorialStage.Objective;

		objectiveScreen = transform.Find ("GameObjective").gameObject;
		playerControlScreen = transform.Find ("PlayerControls").gameObject;
		buildControlScreen = transform.Find ("BuildControls").gameObject;
		battleControlScreen = transform.Find ("BattleControls").gameObject;

		rocket = transform.Find ("TutorialRocket").gameObject;

		checkBoxes = new SwitchSprites[4];
		checkBoxes [0] = transform.Find ("Player1Confirm").GetComponent<SwitchSprites> ();
		checkBoxes [1] = transform.Find ("Player2Confirm").GetComponent<SwitchSprites> ();
		checkBoxes [2] = transform.Find ("Player3Confirm").GetComponent<SwitchSprites> ();
		checkBoxes [3] = transform.Find ("Player4Confirm").GetComponent<SwitchSprites> ();

		players = new GameObject[4];
		players [0] = transform.Find ("Player1").gameObject;
		players [1] = transform.Find ("Player2").gameObject;
		players [2] = transform.Find ("Player3").gameObject;
		players [3] = transform.Find ("Player4").gameObject;

		tutorialMap = new Dictionary<TutorialStage, Action> ();
		tutorialMap.Add (TutorialStage.Objective, InitPlayerControls);
		tutorialMap.Add (TutorialStage.PlayerControls, InitBuildControls);
		tutorialMap.Add (TutorialStage.BuildControls, InitBattleControls);
		tutorialMap.Add (TutorialStage.BattleControls, FinishTutorial);

		InitTutorial ();
	}

	void Update() {
		if (Input.GetButtonDown ("Start_P1"))
			checkBoxes [0].SwitchOn ();
		if (Input.GetButtonDown ("Start_P2"))
			checkBoxes [1].SwitchOn ();
		if (Input.GetButtonDown ("Start_P3"))
			checkBoxes [2].SwitchOn ();
		if (Input.GetButtonDown ("Start_P4"))
			checkBoxes [3].SwitchOn ();
		if (AllConfirmsOn ()) {
			SwitchOffConfirms ();
			tutorialMap [stage] ();
		}
	}

	/******************** Switch Screen Functions ********************/

	public void InitTutorial () {
		objectiveScreen.SetActive (true);
		playerControlScreen.SetActive (false);
		buildControlScreen.SetActive (false);
		battleControlScreen.SetActive (false);
		rocket.SetActive (false);
		TurnOffPlayers ();
		SwitchOffConfirms ();

		stage = TutorialStage.Objective;
	}

	public void InitPlayerControls() {
		objectiveScreen.SetActive (false);
		playerControlScreen.SetActive (true);

		TurnOnPlayers ();
		stage = TutorialStage.PlayerControls;
	}

	public void InitBuildControls() {
		playerControlScreen.SetActive (false);
		buildControlScreen.SetActive (true);
		rocket.SetActive (true);
		 	 	
		stage = TutorialStage.BuildControls;
	}

	public void InitBattleControls() {
		buildControlScreen.SetActive (false);
		battleControlScreen.SetActive (true);

		Player[] players = GameManager.GetGameManager ().GetPlayers ();
		foreach (Player player in players)
			player.SwitchToBattle ();

		stage = TutorialStage.BattleControls;
	}

	public void FinishTutorial() {
		SceneManager.LoadScene ("menu");
	}

	public bool AllConfirmsOn() {
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

	public void TurnOnPlayers() {
		Vector3 pos;
		players [0].SetActive (true);
		pos = players [0].transform.localPosition;
		pos.x = -600f; pos.y = -350f;
		players [0].transform.localPosition = pos;

		players [1].SetActive (true);
		pos = players [1].transform.localPosition;
		pos.x = -360f; pos.y = -350f;
		players [1].transform.localPosition = pos;

		players [2].SetActive (true);
		pos = players [2].transform.localPosition;
		pos.x = 360f; pos.y = -350f;
		players [2].transform.localPosition = pos;

		players [3].SetActive (true);
		pos = players [3].transform.localPosition;
		pos.x = 600f; pos.y = -350f;
		players [3].transform.localPosition = pos;
	}

	public void TurnOffPlayers() {
		players [0].SetActive (false);
		players [1].SetActive (false);
		players [2].SetActive (false);
		players [3].SetActive (false);
	}
}
