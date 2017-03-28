// SK

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    public AudioClip                    countdownAudio;
    public GameObject[]                 rockets;
    public GameObject                   explosion;

    // JF: References to core and player objects
    public GameObject[]                 players;
    public int[]                        TeamLives;
    public GameObject[]                 cores;

    // Encapsulated attributes
    public static PhaseManager          S;
    public bool                         inBuildPhase = true;
    public bool                         countdownNotStarted = true;
    public int                          currentRound = 0;
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
    public int                         rounds_to_play = 2;

	// Components
	private AudioSource					audioSource;

    private void Awake() {
        S = this;
    }
    // Use this for initialization
    void Start () {
        placedBlocks = new List<GameObject>();
        timeLeft = build_time;
        groundDestination = new Vector2(0, -25f);
        groundStartPosition = new Vector2(0, -7f);

		audioSource = GetComponent<AudioSource> ();
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
                        DestroyPlayerIfBelowScreen (player);
                    }
                }
            }
        }
	}

    public void SwitchToCountdownPhase() {
        countdownNotStarted = false;
        audioSource.PlayOneShot(countdownAudio, 3.0f);
        Invoke ("SwitchToBattlePhase", 10);
        InvokeRepeating ("FlashCautionUI", 0, 1);
        ui_timeLeft.fontSize = 50;
        MainCamera.S.SwitchToBattlePhase ();
    }

    // Switches to battle phase:
    // Resets timer, makes divider more transparent and allows projectiles through
    public void SwitchToBattlePhase() {
        inBuildPhase = false;
        
        // JF: Stop caution UI from flashing
        LaunchCautionUI.SetActive(false);
        CancelInvoke ("FlashCautionUI");

        StartCoroutine(moveGround(Vector3.down, groundDestination));
        timeLeft = battle_time;
        ui_phase.text = "BATTLE";
        ui_timeLeft.text = "";
		audioSource.clip = battleBgm;
		audioSource.Play ();

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
        }

        // JF: Enable tooltips on cores
        foreach (GameObject obj in cores) {
            obj.GetComponent<Block> ().image.enabled = true;
        }

        foreach (GameObject player in players) {
            Player player_script = player.GetComponent<Player> ();
            if (player_script.heldItem) {
                player_script.heldItem.Detach (player_script);
            }
        }
    }

    public void SwitchToBuildPhase() {
        inBuildPhase = true;
        timeLeft = build_time;
        StartCoroutine(moveGround(Vector3.up, groundStartPosition));
        ui_phase.text = "BUILD";
		audioSource.clip = buildBgm;
		audioSource.Play ();

        foreach (GameObject obj in backgroundObjects) {
            obj.GetComponent<Rigidbody2D>().gravityScale = 0;
        }

    }

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
    }

    // Sets gameOver to true and creates an explosion at the losing team's core
    public void EndGame(Block destroyedCoreBlock) {
        int winner = (destroyedCoreBlock.teamNum == 1) ? 2 : 1;
        GameObject boom = Instantiate(explosion, destroyedCoreBlock.transform.position, Quaternion.identity);
        boom.GetComponent<LoopingAnimation>().StartAnimation();
        gameOver = true;
        ui_phase.text = "Team " + winner + " wins!";
    }

    private void FlashCautionUI () {
        LaunchCautionUI.SetActive(!LaunchCautionUI.activeInHierarchy);
    }

    // JF: Calling condition: check and destroy this player if it's fallen too far
    // Called by: this.Update()
    private void DestroyPlayerIfBelowScreen(GameObject obj){
        if (MainCamera.S.IsBelowScreen (obj.transform.position)) {
            // Get TeamNum and decrement lives
            int teamNum = obj.GetComponent<Player> ().teamNum;
            TeamLives[teamNum - 1]--;
            Destroy (obj);

            // If out of lives, destroy team's core
            if (TeamLives[teamNum - 1] <= 0) {
                float damage = cores[teamNum - 1].gameObject.GetComponent<Health> ().MAX_HEALTH;
                cores[teamNum - 1].gameObject.GetComponent<Block> ().TakeDamage (damage);
            }

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
