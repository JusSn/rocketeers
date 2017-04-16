using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PhaseManager : MonoBehaviour {
    // Inspector manipulated attributes
    //public GameObject                   divider;
    public Text                         ui_phase;
    public Text                         ui_timeLeft;
    public LayerMask                    layerMask;
    public GameObject                   ground;
    public GameObject[]                 backgroundObjects;

    public GameObject                   LaunchCautionUI;
	public AudioClip					buildBgm;
	public AudioClip 					battleBgm;

    public GameObject[]                 rockets;
    public GameObject                   explosion;
    public GameObject                   spawn_flash;
    public GameObject                   lightning_fizzle;
    public GameObject                   smoke_plume;

    // JF: References to core and player objects
	public GameObject					playerPrefab;
	public List<GameObject>             players;
    public GameObject[]                 cores;

    public GameObject                   TopWall;
    public GameObject                   LeftWall;
    public GameObject                   RightWall;
    public GameObject                   BottomWall;
    public GameObject                   TopBattleWall;
    public GameObject                   TimerDisplay;

    // Encapsulated attributes
    public static PhaseManager          S;
    public bool                         inBuildPhase = true;
    public bool                         countdownNotStarted = true;
    // public int                          currentRound = 0;
    public bool                         gameOver = false;
    public List<GameObject>             placedBlocks;
    private Vector3                     groundDestination;
    public float                        flyingSpeed = 2f;
    public float                        gravityScale = 0.9f;

    // used for switching to battle phase early
    private int                         numPlayersOutOfPoints = 0;

    // Timer
    private float                       timeLeft;
    private string                      seconds;

    // Gameplay Variables
    public float                        build_time = 5;
    public float                        battle_time = 5;
    public bool                         in_tutorial = false;
    private bool                        begin_game = false;
    // public int                         rounds_to_play = 2;

	// Components
	private AudioSource                 audioSource;

    private void Awake() {
        S = this;
    }
    // Use this for initialization
    void Start () {
        placedBlocks = new List<GameObject> ();
        timeLeft = build_time;
        groundDestination = new Vector2 (0, -50f);

		// Instantiating the players
		List<PlayerInfo> playerSettings = GameManager.GetPlayerList();
		players = new List<GameObject>();
		foreach (PlayerInfo pi in GameManager.GetPlayerList()) {
			GameObject go = Instantiate<GameObject> (playerPrefab);
			go.GetComponent<Player> ().SetPlayerSettings (pi.input, pi.charSettings, pi.teamSettings);
			go.transform.position = new Vector3 (UnityEngine.Random.Range (-25f, 25f), 10f, 0f);
			players.Add (go);
		}

        // JF: Set up audiosources:
        // [0] background musics that loop
        // [1] sound effects that do not loop
        audioSource = GetComponent<AudioSource> ();
        Invoke ("PlayMusic", 3f);
    }

	// Update is called once per frame
	void Update () {
        if (gameOver || in_tutorial || !begin_game) {
            return;
        }

        timeLeft -= Time.deltaTime;
        if (timeLeft % 60 < 11 && countdownNotStarted) {
            SwitchToCountdownPhase();
        }
        else {
            if (inBuildPhase) {
                if (!countdownNotStarted) {
                    // shake the camera with an increasing magnitude per the countdown timer
                    CameraShake.Shake (0.1f, 0.5f / (timeLeft + 1f)); // timeLeft + 1f so we don't divide by zero
                }

                seconds = (timeLeft % 60).ToString("00");
                ui_timeLeft.text = seconds;
            }
        }
	}

    public void SwitchToCountdownPhase() {
        countdownNotStarted = false;
		SFXManager.GetSFXManager ().PlaySFX (SFX.NasaCountdown);
        Invoke ("SwitchToBattlePhase", 10);
        Invoke ("EraseTimer", 13);
        InvokeRepeating ("FlashCautionUI", 0, 1);
        ui_timeLeft.fontSize = 60;
        MainCamera.S.SwitchToBattlePhase ();
    }

    // Switches to battle phase:
    // Resets timer, makes divider more transparent and allows projectiles through
    public void SwitchToBattlePhase() {
        inBuildPhase = false;

        if (!in_tutorial) {
            audioSource.clip = battleBgm;
            audioSource.loop = true;
            audioSource.Play ();
        }

        // JF: Stop caution UI from flashing
        LaunchCautionUI.SetActive(false);
        CancelInvoke ("FlashCautionUI");

        StartCoroutine(moveGround(Vector3.down, groundDestination));

        // CG: Destroy the inner walls of the build phase so players can drive their
        // ships around more
        DestroyInnerWalls ();

        timeLeft = battle_time;
        ui_phase.text = "BATTLE";
        ui_timeLeft.text = "GO!";

        foreach (GameObject obj in backgroundObjects) {

            obj.GetComponent<Rigidbody2D>().gravityScale = gravityScale;

            if(obj.GetComponent<SpaceBackground>() != null) {
                obj.GetComponent<SpaceBackground>().StartFlying();
            }
        }

        foreach (GameObject obj in rockets) {
            obj.GetComponent<LoopingAnimation>().StartAnimation();
        }

        foreach (GameObject go in placedBlocks) {
            go.GetComponent<Rigidbody2D>().gravityScale = 1;
            go.GetComponent<Block> ().RemoveHighlights ();
        }

        // JF: Enable tooltips on cores
        foreach (GameObject obj in cores) {
            obj.GetComponent<Block> ().image.enabled = true;
            obj.GetComponent<Block> ().RemoveHighlights ();
        }

        ExecuteOverPlayers (new player_delegate (SwitchToBattle));
    }

    public delegate void player_delegate(Player player);

    void ExecuteOverPlayers(player_delegate fn_ptr){
        foreach (GameObject player in players) {
            Player player_script = player.GetComponent<Player> ();
            fn_ptr (player_script);
        }
    }

    void SwitchToBattle(Player player){
        player.SwitchToBattle ();
    }

    void DisableIndicators(Player player){
        player.DisableIndicator ();
    }

    IEnumerator moveGround(Vector3 direction, Vector3 destination) {
        float starttime = Time.time;
        while (ground.transform.position.y > destination.y) {
            // CG: 0.5f is the screen shake magnitude at the time of switching to battle phase
            // so the magnitude function here goes from 0.5f to 0 and subtracted by the amount of time
            // we've been in the battle phase divided by 5f. 5f acts as a scaling value since Time.time - starttime
            // will be much larger than 0.5f after 0.5 seconds has passed. These values seemed to give a decent transition
            CameraShake.Shake (0.1f, 0.5f - ((Time.time - starttime) / 5f));
            ground.transform.position = Vector3.MoveTowards(ground.transform.position, destination, (Time.time - starttime) * flyingSpeed);
            yield return null;
        }
        Destroy(ground);
    }

    // Sets gameOver to true and creates an explosion at the losing team's core
    public void EndGame(Block destroyedCoreBlock) {
        Destroy (TopBattleWall);
        ExecuteOverPlayers (new player_delegate(DisableIndicators));

        int winner = (destroyedCoreBlock.teamNum == 1) ? 2 : 1;
        float slow_mo_speed = 0.2f;
        SlowMo.Begin (slow_mo_speed, destroyedCoreBlock.transform.position);
        StartCoroutine (MichaelBay (destroyedCoreBlock.transform.position));

        gameOver = true;
        if (!in_tutorial) {
            TimerDisplay.SetActive (true);
        }
        ui_phase.text = "Team " + winner;
        ui_timeLeft.text = "Wins";

        // SK: Winning ship flies up off the screen
        cores [winner - 1].GetComponent<Controllable> ().SetGameOver ();

        Invoke("BackToMenu", 5);
    }

    // Explodes losing team's Rocket base and shakes the screen
    private IEnumerator MichaelBay (Vector3 corePos) {

        Vector3 rocketPosL = new Vector3 (corePos.x -1, corePos.y - 1, corePos.z);
        Vector3 rocketPosR = new Vector3 (corePos.x + 1, corePos.y - 1, corePos.z);

        GameObject boom0 = Instantiate(explosion, corePos, Quaternion.identity);
        boom0.GetComponent<LoopingAnimation>().StartAnimation();

        CameraShake.Shake(0.5f, 0.4f, true);

        yield return new WaitForSeconds(0.2f);

        GameObject boom1 = Instantiate(explosion, rocketPosL, Quaternion.identity);
        boom1.GetComponent<LoopingAnimation>().StartAnimation();

        CameraShake.Shake(0.5f, 0.4f, true);

        yield return new WaitForSeconds(0.2f);

        GameObject boom2 = Instantiate(explosion, rocketPosR, Quaternion.identity);
        boom2.GetComponent<LoopingAnimation>().StartAnimation();

        CameraShake.Shake(0.5f, 0.4f, true);
        SlowMo.End ();
    }

    private void BackToMenu() {
        SceneManager.LoadScene("Menu");
    }

    private void FlashCautionUI () {
        LaunchCautionUI.SetActive(!LaunchCautionUI.activeInHierarchy);
    }

    private void DestroyInnerWalls () {
        Destroy (TopWall);
        Destroy (LeftWall);
        Destroy (RightWall);
        Destroy (BottomWall);
    }

    void EraseTimer(){
        TimerDisplay.SetActive (false);
    }


    public void SetInTutorial(){
        in_tutorial = true;
    }

    void PlayMusic(){
        if (!in_tutorial) {
            begin_game = true;
            audioSource.clip = buildBgm;
            audioSource.loop = true;
            audioSource.Play ();
        }
    }

    public void AddBlock(GameObject go, GameObject settableBlock){
        PhaseManager.S.placedBlocks.Add(go);
        if (in_tutorial) {
            TutorialController.GetTutorialController ().DecreaseBlocksToGo (settableBlock);
        }
    }

    public void ReportTeamOutOfPoints(int teamNum){
        numPlayersOutOfPoints |= teamNum;
        if (numPlayersOutOfPoints == 3) {
            SwitchToCountdownPhase ();
            timeLeft = 11f;
        }
    }
}
