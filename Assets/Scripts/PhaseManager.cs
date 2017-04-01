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
    public GameObject[]                 players;
    // public int[]                        TeamLives;
    public GameObject[]                 cores;

    public GameObject                   TopWall;
    public GameObject                   LeftWall;
    public GameObject                   RightWall;

    // Encapsulated attributes
    public static PhaseManager          S;
    public bool                         inBuildPhase = true;
    public bool                         countdownNotStarted = true;
    // public int                          currentRound = 0;
    public bool                         gameOver = false;
    public List<GameObject>             placedBlocks;
    private Vector3                     groundDestination;
    public float                        flyingSpeed = 2f;
    private Vector3                     groundStartPosition;
    public float                        gravityScale = 0.9f;

    // Timer
    private float                       timeLeft;
    private string                      seconds;

    // Gameplay Variables
    public float                       build_time = 5;
    public float                       battle_time = 5;
    // public int                         rounds_to_play = 2;

	// Components
	private AudioSource               audioSource;

    private void Awake() {
        S = this;
    }
    // Use this for initialization
    void Start () {
        placedBlocks = new List<GameObject>();
        timeLeft = build_time;
        groundDestination = new Vector2(0, -25f);
        groundStartPosition = new Vector2(0, -7f);

        // JF: Set up audiosources: 
        // [0] background musics that loop
        // [1] sound effects that do not loop
		audioSource = GetComponent<AudioSource>();
		audioSource.clip = buildBgm;
		audioSource.loop = true;
		audioSource.Play ();
    }
	
	// Update is called once per frame
	void Update () {
        if (gameOver) {
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
            // JF: If in battle phase, kill players that fall out of camera
            else {
                foreach (GameObject player in players) {
                    if (player != null) {
                        RespawnPlayerIfBelowScreen (player);
                    }
                }
            }
        }
	}

    public void SwitchToCountdownPhase() {
        countdownNotStarted = false;
		SFXManager.GetSFXManager ().PlaySFX (SFX.NasaCountdown);
        Invoke ("SwitchToBattlePhase", 10);
        InvokeRepeating ("FlashCautionUI", 0, 1);
        ui_timeLeft.fontSize = 50;
        MainCamera.S.SwitchToBattlePhase ();
    }

    // Switches to battle phase:
    // Resets timer, makes divider more transparent and allows projectiles through
    public void SwitchToBattlePhase() {
        inBuildPhase = false;
        
		audioSource.clip = battleBgm;
		audioSource.loop = true;
		audioSource.Play ();

        // JF: Stop caution UI from flashing
        LaunchCautionUI.SetActive(false);
        CancelInvoke ("FlashCautionUI");

        StartCoroutine(moveGround(Vector3.down, groundDestination));

        // JF: Expand outer walls to match zoomed out camera
        ExpandWalls ();

        timeLeft = battle_time;
        ui_phase.text = "BATTLE";
        ui_timeLeft.text = "";

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

        foreach (GameObject player in players) {
            Player player_script = player.GetComponent<Player> ();
			player_script.SwitchToBattle ();
        }
    }

    // public void SwitchToBuildPhase() {
    //     inBuildPhase = true;
    //     timeLeft = build_time;
    //     StartCoroutine(moveGround(Vector3.up, groundStartPosition));
    //     ui_phase.text = "BUILD";
	// 	audioSource.clip = buildBgm;
	// 	audioSource.Play ();

    //     foreach (GameObject obj in backgroundObjects) {
    //         obj.GetComponent<Rigidbody2D>().gravityScale = 0;
    //     }

    // }

    IEnumerator moveGround(Vector3 direction, Vector3 destination) {
        float starttime = Time.time;
        while (ground.transform.position != destination) {
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
        int winner = (destroyedCoreBlock.teamNum == 1) ? 2 : 1;

        StartCoroutine (MichaelBay (destroyedCoreBlock.transform.position));

        gameOver = true;
        ui_phase.text = "Team " + winner + " wins!";

        // SK: Winning ship flies up off the screen
        cores[winner - 1].GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None;
        cores[winner - 1].GetComponent<Rigidbody2D>().gravityScale = -10;
        Destroy(GameObject.Find("TopWall"));

        Invoke("BackToMenu", 5);
    }

    // Explodes losing team's Rocket base and shakes the screen
    private IEnumerator MichaelBay (Vector3 corePos) {
        Vector3 rocketPosL = new Vector3 (corePos.x -1, corePos.y - 1, corePos.z);
        Vector3 rocketPosR = new Vector3 (corePos.x + 1, corePos.y - 1, corePos.z);

        GameObject boom0 = Instantiate(explosion, corePos, Quaternion.identity);
        boom0.GetComponent<LoopingAnimation>().StartAnimation();

        CameraShake.Shake(0.5f, 0.4f);

        yield return new WaitForSecondsRealtime(0.2f);

        GameObject boom1 = Instantiate(explosion, rocketPosL, Quaternion.identity);
        boom1.GetComponent<LoopingAnimation>().StartAnimation();

        CameraShake.Shake(0.5f, 0.4f);

        yield return new WaitForSecondsRealtime(0.2f);

        GameObject boom2 = Instantiate(explosion, rocketPosR, Quaternion.identity);
        boom2.GetComponent<LoopingAnimation>().StartAnimation();

        CameraShake.Shake(0.5f, 0.4f);
    }

    private void BackToMenu() {
        SceneManager.LoadScene("Menu");
    }

    private void FlashCautionUI () {
        LaunchCautionUI.SetActive(!LaunchCautionUI.activeInHierarchy);
    }

    private void ExpandWalls () {
        LeftWall.transform.Translate(-Vector3.right);
        RightWall.transform.Translate(Vector3.right);
        TopWall.transform.Translate(Vector3.up);
    }

    // JF: Calling condition: check and respawn this player if it's fallen too far
    // SK: Changed to respawning and added effects for JUICE
    // Called by: this.Update()
    private void RespawnPlayerIfBelowScreen(GameObject obj){
        if (MainCamera.S.IsBelowScreen (obj.transform.position) && !gameOver) {
			SFXManager.GetSFXManager ().PlaySFX (SFX.PlayerDied);
            StartCoroutine (RespawnPlayer(obj));

            //TeamLives[teamNum - 1]--;
            //Destroy (obj);

            // If out of lives, destroy team's core
            //if (TeamLives[teamNum - 1] <= 0) {
            //    float damage = cores[teamNum - 1].gameObject.GetComponent<Health> ().MAX_HEALTH;
            //    cores[teamNum - 1].gameObject.GetComponent<Block> ().TakeDamage (damage);
            //}
        }
    }

    private IEnumerator RespawnPlayer (GameObject obj) {
        int teamNum = obj.GetComponent<Player> ().teamNum;
        GameObject core = cores[teamNum - 1];

        // JF: Move player way above screen to prevent this method from being called again
        obj.transform.position = new Vector3 (0, 1000, 0);

        yield return new WaitForSecondsRealtime(5f);

        if (core != null) {
            Vector3 corePos = core.transform.position;
            // Create effects to make it look cool
            GameObject flash = Instantiate(spawn_flash);
            flash.transform.position = corePos + new Vector3(-0.1f, 0.5f);
            flash.GetComponent<LoopingAnimation>().StartAnimation();

            GameObject sizzle = Instantiate(lightning_fizzle);
            sizzle.transform.position = corePos;
            sizzle.GetComponent<LoopingAnimation>().StartAnimation();

            GameObject smoke = Instantiate(smoke_plume);
            smoke.transform.position = corePos;
            smoke.GetComponent<LoopingAnimation>().StartAnimation();

            // Move the player to the core
            obj.GetComponent<Rigidbody2D>().velocity = Vector3.zero;
            obj.transform.position = corePos;

            obj.GetComponent<Player> ().form = PlayerForm.Normal;
        }
    }

    // Functions used in "build-to-height" game mode

    /*    
    // Switches to build phase:
    // Checks if either team is above the goal line (if yes, end game)
    // Checks if the number of rounds have reached the total selected (if yes, end game)
    // If neither of these are true, reset timer, change divider to opaque and stop projectiles
    public void SwitchToBuildPhase() {
        int winner = CheckForWinner(7f);
        if(winner != 0) {
            EndGame(winner);
            return;
        }
        currentRound++;
        if(currentRound >= rounds_to_play) {
            winner = FindWinner();
            EndGame(winner);
            return;
        }
        inBuildPhase = true;
        timeLeft = build_time;
        divider.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f);
        divider.layer = LayerMask.NameToLayer("Default");
        ui_phase.text = "BUILD";
    }

    // Casts a raycast left and a raycast right at the desired height to see if a tower is at that height
    // Only left    -> 1
    // Only right   -> 2
    // Both         -> -1
    // Neither      -> 0
    public int CheckForWinner(float height) {
        RaycastHit2D hitLeft = Physics2D.Raycast(new Vector3(0, height), -Vector2.right, 14, layerMask);
        RaycastHit2D hitRight = Physics2D.Raycast(new Vector3(0, height), Vector2.right, 14, layerMask);

        if (hitLeft.collider != null && hitRight.collider == null) {
            return 1;
        }
        else if (hitRight.collider != null && hitLeft.collider == null) {
            return 2;
        }
        else if(hitLeft.collider != null && hitRight.collider != null) {
            return -1;
        }
        else return 0;
    }

    // calls CheckForWinner at each block height moving down from the goalline until it finds higher tower
    public int FindWinner() {
        int winningTeam = 0;
        float raycastHeight = 8;

        while(winningTeam == 0 && raycastHeight > -6) {
            winningTeam = CheckForWinner(raycastHeight);
            raycastHeight--;
        }
        return winningTeam;
    }

    // Stops the timer, displays the winning team
    public void EndGame(int winner) {
        gameOver = true;
        ui_timeLeft.enabled = false;
        print("Winner was " + winner);
        if(winner == -1 || winner == 0) {
            ui_phase.text = "DRAW!";
        }
        else {
            ui_phase.text = "TEAM " + winner + " WINS!";
        }
    }
    */
}
