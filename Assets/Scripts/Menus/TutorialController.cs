using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;
using InControl;

public enum TutorialStage {
	Objective,       	// 1st page of tutorial
	BuildControls,		// 2nd page of tutorial
    Countdown,          // filler page
	BattleControls		// 3rd page of tutorial
}

public class TutorialController : MonoBehaviour {

	public TutorialStage				stage;
    public bool                         in_tutorial = true;
    public GameObject                   battleText;
    public AudioClip                    countdownSFX;

    private static TutorialController   singleton = null;


	private GameObject					objectiveScreen;
	private GameObject					buildControlScreen;
	private GameObject					battleControlScreen;

    private GameObject                  team1Rocket;
    private GameObject                  team2Rocket;

	private GameObject 					buildSpotUI;
	private GameObject					battleSpotUI;

    private GameObject                  scenery;

    private Text                        team1BlocksToGo;
    private Text                        team2BlocksToGo;

    private AudioSource                 nasaCountdownAudioSource;

    private float                       timeLeft = 3f;
    private float                       startTime;
    private float                       DELAY_TIME = 10f;

	private SwitchSprites[]				checkBoxes;
	private GameObject[]				players;
	private InputDevice[]               inputDevices;

    private Dictionary<TutorialStage, Action>  tutorialMap;
    private Dictionary<TutorialStage, Action>  advanceToNextStageMap;
    private bool                        shouldAdvanceToNextStage = false;
	private bool						spotlight = false;
	private	GameObject 					spotlightUI = null;


    void Awake() {
        singleton = this;
        PhaseManager.S.SetInTutorial();
    }

	void Start() {
        stage = TutorialStage.Objective;


		objectiveScreen = transform.Find ("GameObjective").gameObject;
		buildControlScreen = transform.Find ("BuildControls").gameObject;
		battleControlScreen = transform.Find ("BattleControls").gameObject;

        team1Rocket = GameObject.Find ("Team1Base").gameObject;
        team2Rocket = GameObject.Find ("Team2Base").gameObject;

        scenery = GameObject.Find ("Scenery").gameObject;

		buildSpotUI = GameObject.Find ("BuildSpotUI");
		buildSpotUI.SetActive (false);
		battleSpotUI = GameObject.Find ("BattleSpotUI");
		battleSpotUI.SetActive (false);

        team1BlocksToGo = GameObject.Find ("Team1BlocksToGo").gameObject.GetComponent<Text>();
        team2BlocksToGo = GameObject.Find ("Team2BlocksToGo").gameObject.GetComponent<Text>();
        nasaCountdownAudioSource = gameObject.AddComponent<AudioSource>();
        nasaCountdownAudioSource.clip = countdownSFX;
        nasaCountdownAudioSource.time = 7.5f; // play the countdown at exactly the "3, 2, 1...liftoff" part

		players = new GameObject[4];
        players [0] = GameObject.Find ("Players").transform.Find("Player1").gameObject;
        players [1] = GameObject.Find ("Players").transform.Find ("Player2").gameObject;
        players [2] = GameObject.Find ("Players").transform.Find ("Player3").gameObject;
        players [3] = GameObject.Find ("Players").transform.Find ("Player4").gameObject;

		tutorialMap = new Dictionary<TutorialStage, Action> ();
        tutorialMap.Add (TutorialStage.Objective, InitBuildControls);
        tutorialMap.Add (TutorialStage.BuildControls, InitCountdown);
        tutorialMap.Add (TutorialStage.Countdown, InitBattleControls);
		tutorialMap.Add (TutorialStage.BattleControls, FinishTutorial);

        advanceToNextStageMap = new Dictionary<TutorialStage, Action> ();
        advanceToNextStageMap.Add (TutorialStage.Objective, GenericAdvanceCondition);
        advanceToNextStageMap.Add (TutorialStage.BuildControls, BuildAdvanceCondition);
        advanceToNextStageMap.Add (TutorialStage.Countdown, CountdownAdvanceCondition);
        advanceToNextStageMap.Add (TutorialStage.BattleControls, ShrinkControls);


		inputDevices = new InputDevice[4];
		for (int i = 0; i < 4; ++i) {
			try {
				inputDevices[i] = InputManager.Devices [i];
			} catch {
				Debug.Log ("4 controllers are not connected. Assigning extra players to first player");
				inputDevices[i] = InputManager.Devices [0];
			}
		}

		InitTutorial ();
	}

	void Update() {
        PhaseManager.S.SetInTutorial();

		if (!spotlight) {
			for (int i = 0; i < 4; ++i) {
				if (inputDevices [i].MenuWasPressed) {
					SFXManager.GetSFXManager ().PlaySFX (SFX.MenuConfirm);
				}
			}
			advanceToNextStageMap [stage] ();

			if (shouldAdvanceToNextStage) {
				tutorialMap [stage] ();
			}
		} else {
			if (InputManager.ActiveDevice.Action1) {
				SpotlightPause.S.DestroySpotlight ();
				spotlight = false;
				spotlightUI.SetActive (false);
			}
		}
	}

    public static TutorialController GetTutorialController() {
        return singleton;
    }
	/******************** Switch Screen Functions ********************/

	public void InitTutorial () {
		objectiveScreen.SetActive (true);
		buildControlScreen.SetActive (false);
		battleControlScreen.SetActive (false);
        SetRocketsActive (false);
        scenery.SetActive (false);
		TurnOffPlayers ();

		stage = TutorialStage.Objective;
	}

    void GenericAdvanceCondition(){
        shouldAdvanceToNextStage = StartButtonWasPressed ();
    }

	public void InitBuildControls() {
        scenery.SetActive (true);
        shouldAdvanceToNextStage = false;
        objectiveScreen.SetActive (false);
    	buildControlScreen.SetActive (true);
		SetRocketsActive (true);
        TurnOnPlayers ();

		stage = TutorialStage.BuildControls;

		Invoke ("HighlightBlocks", 1.5f);
	}

    void BuildAdvanceCondition(){
        shouldAdvanceToNextStage = ((int.Parse (team1BlocksToGo.text) == 0
                                     && int.Parse (team2BlocksToGo.text) == 0)
                                     || StartButtonWasPressed());
		
    }

    public void InitCountdown(){
		CancelInvoke ();
        GameObject.Find ("Team1BlocksToBuild").gameObject.GetComponent<Text>().text = "";
        GameObject.Find ("Team2BlocksToBuild").gameObject.GetComponent<Text>().text = "";
        team1BlocksToGo.gameObject.SetActive (true);
        team2BlocksToGo.gameObject.SetActive (true);
        stage = TutorialStage.Countdown;
        nasaCountdownAudioSource.Play();
    }

    void CountdownAdvanceCondition(){
        timeLeft -= Time.deltaTime;
        string seconds = (timeLeft % 60f).ToString("00");
        SetTeamText (seconds);
        shouldAdvanceToNextStage = StartButtonWasPressed ();
        if (timeLeft % 60f <= 0f) {
            shouldAdvanceToNextStage = true;
        }
    }

	public void InitBattleControls() {
        buildControlScreen.SetActive (false);
        battleControlScreen.SetActive (true);
        TurnOnPlayers ();

		Player[] players = GameManager.GetGameManager ().GetPlayers ();
		foreach (Player player in players)
			player.SwitchToBattle ();

        PhaseManager.S.SwitchToBattlePhase ();
		stage = TutorialStage.BattleControls;
        startTime = Time.time;
        Invoke ("ClearBattleText", DELAY_TIME);
		Invoke ("HighlightCockpit", 3f);
	}

    void ShrinkControls(){
        // We enter the battle phase via the PhaseManager so
        // once a team destroys the core there we will go back to the main menu
        // or when someone skips it and presses the start button
        shouldAdvanceToNextStage = StartButtonWasPressed ();
    }

	public void TurnOnPlayers() {
		players [0].SetActive (true);
		players [1].SetActive (true);
		players [2].SetActive (true);
		players [3].SetActive (true);
	}

	public void TurnOffPlayers() {
		players [0].SetActive (false);
		players [1].SetActive (false);
		players [2].SetActive (false);
		players [3].SetActive (false);
	}

    void SetRocketsActive(bool active){
        team1Rocket.SetActive (active);
        team2Rocket.SetActive (active);
    }

    bool StartButtonWasPressed(){
        for (int i = 0; i < 4; ++i) {
            if (inputDevices [i].MenuWasPressed) {
                SFXManager.GetSFXManager ().PlaySFX (SFX.MenuConfirm);
                return true;
            }
        }
        return false;
    }

    public void FinishTutorial() {
        SceneManager.LoadScene ("menu");
    }

    public void DecreaseBlocksToGo(GameObject settableBlock){
        int teamNum = settableBlock.transform.parent.parent.GetComponent<Player> ().teamNum;
        Text blocksToGoText = GetTeamText (teamNum);
        blocksToGoText.text = Mathf.Max (int.Parse (blocksToGoText.text) - 1, 0f).ToString();
        if (blocksToGoText.text == "0") {
            Text blockInfo = GameObject.Find ("Team" + teamNum + "BlocksToBuild").gameObject.GetComponent<Text> ();
            blockInfo.text = "Board your rocket!";
            blockInfo.alignment = TextAnchor.UpperCenter;
            blocksToGoText.gameObject.SetActive (false);
        }
    }

    Text GetTeamText(int teamNum){
        if (teamNum == 1) {
            return team1BlocksToGo;
        } else {
            return team2BlocksToGo;
        }
    }

    void SetTeamText(string to_set){
        team1BlocksToGo.text = to_set;
        team2BlocksToGo.text = to_set;
    }

    void ClearBattleText(){
        Destroy (battleText);
    }
		
	void HighlightBlocks () {
		spotlight = true;
		SpotlightPause.S.CreateSpotlight (5f);
		spotlightUI = buildSpotUI;
		spotlightUI.SetActive (true);
	}

	void HighlightCockpit() {
		spotlight = true;
		SpotlightPause.S.CreateSpotlight (2.5f, -7f);
		spotlightUI = battleSpotUI;
		spotlightUI.SetActive (true);
	}
}
