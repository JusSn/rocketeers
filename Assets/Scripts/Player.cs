using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InControl;

public enum PlayerForm {
    Normal,        // Running, Jumping, Idle
    Setting,       // Placing settable object
    Controlling,   // Used when driving or firing a block weapon
    Respawning,
}

public class Player : MonoBehaviour {
    // Insepctor manipulated attributes
    public string                           playerNumStr;
    public int                              teamNum;
    public float                            xSpeed = 7f;
    public float                            ySpeed = 10f;
    public float                            itemDetectRadius = 0.5f;
    public float                            blockDetectRadius = 0.1f;
    public float                            placementDetectRadius = 0.3f;
    public float                            throwChargeMax = 2.5f;
    public float                            throwChargeRatio = 10f;
    public GameObject                       projectilePrefab;
    public GameObject                       offscreenArrowPrefab;
    public float                            projSpeed;
	public float 							projCDCounter = 0f;
	public float							projCDTime = 0.5f;
    public float                            DRIVE_SPEED_X = 4f;
    public float                            DRIVE_SPEED_Y = 4f;
    public float                            DOWN_JUMP_THRESHOLD = 0.3f;
    public float                            JETPACK_ACTIVE_THRESHOLD = 0.3f;
	public float							jetpackMaxSpeed = 8f;
	public float 							jetpackAccel = 40f;
	public float							jetpackFuelMax = 5f;
	public float 							jetpackRefuelRate = 5f;
    public float                            ABSOLUTE_LOWER_BOUND = -23.5f;

	public bool                             debugMode = false;

    public LayerMask                        placementMask;
    public LayerMask                        platformsMask;
    public LayerMask                        groundedMask;
    public bool                             ________________;
    // Encapsulated attributes
    public PlayerForm                       _form = PlayerForm.Normal;
    public bool                             grounded;
    public bool                             canDownJump;
    private bool                            ducking;
    public Item                             heldItem;
    public Controllable                     controlled_block;
    private bool                            doubleJumped;
	private bool 							buildPhase = true;
    private UFOManager                      ufo_manager;

    private GameObject                      nearestBlockObj; 
    private Block                           selectedBlock;
    // Internal Support Variables
	private float 							jetpackFuelCurrent;

    // GameObject components & child objects
    private Rigidbody2D                     rigid;
    private GameObject                      sprite;
	private GameObject 						charSprite;
    private SpriteRenderer                  sprend;
    private Animator                        animator;
    private PointManager                    point_manager;

    private BoxCollider2D                   bodyCollider;

    // JF: Highlight object
    public GameObject                       highlightObject;
    public GameObject                       swapArrowIndicator;

    // AW: Aim arrow
    private GameObject                      aimArrowObject;
    private GameObject                      projSource;

    // JF: Aim aimSprend
    private SpriteRenderer                  aimSprend;

	// AW: Jetpack objects
    private GameObject                      jetpackObj;
	private GameObject 						jetpackFire;
    private JetpackBar                      jetpack_bar;
	private GameObject 						jetpackFuelBar;

    // CG: offscreen arrow indicator
    private ArrowIndicator                      offscreen_arrow_manager;

    // Detection parameters
    private int                             blockMask;
    private int                             itemLayer;
    private int                             groundLayer;

    // Internal maps
    private Dictionary<PlayerForm, Action>  stateUpdateMap;

	// AW: Player Settings | CG: input object
    private InputDevice                     input;
	private CharacterSettings				charSettings = null;
	private TeamSettings					teamSettings = null;

    // Use this for initialization
    void Start () {
        // Get GameObject components & children
        rigid = GetComponent<Rigidbody2D> ();
        sprite = transform.Find ("Sprite").gameObject;
		charSprite = sprite.transform.Find ("Character").gameObject;
		animator = charSprite.GetComponent<Animator> ();
		sprend = charSprite.GetComponent<SpriteRenderer> ();
        bodyCollider = GetComponent<BoxCollider2D> ();
        point_manager = GetComponent<PointManager> ();
        ufo_manager = GetComponent<UFOManager> ();
        offscreen_arrow_manager = GetComponent<ArrowIndicator> ();

        jetpackObj = sprite.transform.Find ("Jetpack").gameObject;
        jetpack_bar = GetComponent<JetpackBar> ();
        jetpack_bar.SetMaxFuel (jetpackFuelMax);

        // JF: Get highlightObject and disable. Enable if item is held later
        highlightObject = transform.Find ("Highlight").gameObject;
        // highlightSprends = highlightObject.GetComponentsInChildren<SpriteRenderer> ();
        highlightObject.SetActive (false);

        // AW: Get arrow sprite for aiming shots and proj source
        aimArrowObject = transform.Find("Aiming").gameObject;
        aimArrowObject.SetActive (false);
        projSource = aimArrowObject.transform.Find ("ProjectileSource").gameObject;

        // JF: Arrow and gun sprite for flipping
        aimSprend = aimArrowObject.transform.Find("ArrowSprite").GetComponent<SpriteRenderer> ();

		// AW: Get jetpack related variables
		jetpackFire = sprite.transform.Find("Jetpack").transform.Find("Fire").gameObject;
		jetpackFire.SetActive (false);
		jetpackFuelBar = sprite.transform.Find ("Jetpack").transform.Find ("JetpackBar").gameObject;
		jetpackFuelBar.SetActive (false);

        // Initializing internal
		jetpackFuelCurrent = jetpackFuelMax;

        // Raycast parameters
        itemLayer = LayerMask.GetMask ("Items");
        groundLayer = LayerMask.GetMask ("Ground");

        // Filling the function behavior map
        stateUpdateMap = new Dictionary<PlayerForm, Action> ();
        stateUpdateMap.Add (PlayerForm.Normal, NormalUpdate);
        stateUpdateMap.Add (PlayerForm.Setting, SettingUpdate);
        stateUpdateMap.Add (PlayerForm.Controlling, ControllingUpdate);
        stateUpdateMap.Add (PlayerForm.Respawning, RespawningUpdate);


		if (charSettings == null) {
			try {
				input = InputManager.Devices [int.Parse (playerNumStr) - 1];
			} catch {
				Debug.Log ("4 controllers are not connected. Assigning extra players to first player");
				input = InputManager.Devices [0];
			}
		} else {
			InitPlayerSettings ();
		}

        input.LeftStickX.LowerDeadZone = 0.2f;
        input.LeftStickY.LowerDeadZone = 0.5f;
		blockMask = LayerMask.GetMask ("Team" + teamNum.ToString() + "Block");

        // Have the UFOs bring in the players!
        Respawn (transform.position);
    }

    // Update is called once per frame
    void Update () {
		// Update general attributes
		grounded = IsGrounded ();
		if (grounded) {
			doubleJumped = false;
		}
		ducking = IsDucking ();
		canDownJump = CanDownJump ();
		RespawnPlayerIfBelowScreen ();
		// Call the proper update function
		stateUpdateMap [form] ();
    }

    /// <summary>
    /// Sent when an incoming collider makes contact with this object's
    /// collider (2D physics only).
    /// </summary>
    /// <param name="other">The Collision2D data associated with this collision.</param>
    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.transform.CompareTag("Bullet")) {
            animator.SetTrigger("hurt");
			SFXManager.GetSFXManager ().PlaySFX (SFX.HitPlayer);
        }
    }

    /******************** State Modifiers & Behaviors ********************/

    // General behavior of the player when not holding anything during the building phase
    // Entered from: Throwing(thrown)
    // Exit to: Setting(pickup)
    void NormalUpdate() {
        CalculateMovement ();
        // Scan all blocks within range of player
        ScanForBlocks ();

        if (buildPhase) {
            TryToPickUpItem ();

        } else {
            // Aiming shot trajectory with the right stick
            projCDCounter += Time.deltaTime;
            if (IsRightJoyActive()) {
				Vector3 trajectory = GetRightJoyDirection ();
				float angle = Vector3.Angle (Vector3.right, trajectory);
                angle = (trajectory.y < 0) ? -angle : angle; 

                // JF: Flip ray gun if aiming to left
                aimSprend.flipY = trajectory.x < 0;
				aimArrowObject.transform.rotation = Quaternion.AngleAxis (angle, Vector3.forward);

				// CG: Shoot gun every projCDTime seconds
				if (projCDCounter >= projCDTime) {
					FireBurst ();
					// FireProjectile ();
					projCDCounter = 0f;
				}
			}
            if (nearestBlockObj != null){
                if (input.RightBumper.WasPressed) {
                    if (TryToSitInBlock()) {
                        // there's a weapon underneath us, so sit in it
                        form = PlayerForm.Controlling;
						SFXManager.GetSFXManager ().PlaySFX (SFX.StartPilot);
                    }
                }
            }
        }

        // // JF: Cancel block swap with B button
        // if (selectedBlock != null && input.Action2.WasPressed) {
        //     EndBlockSwap ();
        // }
    }

    // setting update occurs when a player is holding a block
    // in this case it is most of the build phase
    void SettingUpdate() {
        CalculateMovement ();
        // CG: still need to be able to pick up the other items if we're close to them
        TryToPickUpItem ();

        // JF: Change location of highlight guide
        Vector3 setPos = GetGridPosition ();
        highlightObject.transform.position = setPos;

        // JF: Check if highlighted position is valid for placement
        Collider2D blocker = Physics2D.OverlapCircle (setPos, placementDetectRadius, placementMask);
        // [CG]: Check if we're by another block forcing us to start connections with the core
        bool valid_neighbor = Utils.ValidBlockPlacement (setPos, blockMask);

        // show or don't show the valid placement object
        if (!blocker && valid_neighbor && Utils.ValidBlockLocation(setPos)) {
            highlightObject.SetActive (true);
            if (input.RightBumper.WasPressed) {
                SetItem (setPos);
            }
        } else {
            highlightObject.SetActive (false);
        }
    }

    // Behavior when controlling a block
    // Entered from: Normal(sit button)
    // Exit to: Normal(sit button)
    void ControllingUpdate() {

        // used to steer the ship
        float x_val = input.LeftStickX * DRIVE_SPEED_X;
        float y_val = input.LeftStickY * DRIVE_SPEED_Y;
        controlled_block.GetComponent<Rigidbody2D>().velocity = new Vector3 (x_val, y_val, 0f);
        transform.position = controlled_block.transform.position;

        FlipAllSprites (x_val);

        // Detach from the block if the user wants to
        if (!input.RightBumper.IsPressed){
            DetachFromBlock ();
            return;
        }
    }


    // Calling Condition: when player falls out of the camera and is being respawned by UFOLakitu
    // Exits to: Player.NormalUpdate()
    void RespawningUpdate(){
        // do nothing while we respawn since the UFOManager takes care of everything
    }

    // Getting/Setting form property. Modify statespecific values here
    // e.g. Variables, Animations, etc.
    public PlayerForm form {
        get { return _form; }
        set {
            if (_form == value) {
                return;
            }

            switch (value) {
            case PlayerForm.Normal:
                // JF: Toggle highlight guide
                highlightObject.SetActive(false);

                jetpackObj.SetActive(true);

                if (!buildPhase) {
                    aimArrowObject.SetActive(true);
                }

                animator.SetBool("piloting", false);
                _form = value;
                break;
            case PlayerForm.Setting:
                // JF: Toggle highlight guide
                _form = value;
                break;
            case PlayerForm.Controlling:
                animator.SetBool("piloting", true);

                aimArrowObject.SetActive(false);
                jetpackObj.SetActive(false);
                _form = value;
                break;

            case PlayerForm.Respawning:
                _form = value;
                break;
            }
        }
    }

	/******************** Public Interface ********************/

	public void SetPlayerSettings (InputDevice device, CharacterSettings charSet, TeamSettings teamSet) {
		input = device;
		charSettings = charSet;
		teamSettings = teamSet;
	}

	public void SwitchToBattle () {
		buildPhase = false;
        aimArrowObject.SetActive(true);
		if (heldItem) {
			heldItem.Detach (this);
        }
        offscreen_arrow_manager.EnableIndicator ();
	}

    /******************** Utility ********************/

	void InitPlayerSettings () {
		foreach (InputDevice id in InputManager.Devices) {
			if (id.Meta == input.Meta) {
				input = id;
				print ("What's going on");
			}
		}

		// Init Player Settings
		sprend.sprite = charSettings.GetSprite();
		animator.runtimeAnimatorController = charSettings.GetAnimator ();
		projectilePrefab = charSettings.GetProjectile ();
		ufo_manager.GetComponent<UFOManager>().UFOLakitu = charSettings.GetUFOManager ();
		offscreen_arrow_manager.GetComponent<ArrowIndicator>().arrowPrefab = charSettings.GetOffscreenIndicator ();

		// Init Team Settings
		teamNum = teamSettings.teamNum;
		placementMask = teamSettings.GetPlacementMask ();
		platformsMask = teamSettings.GetPlatformsMask ();
		gameObject.layer = LayerMask.NameToLayer("Team" + teamNum + "Player");
		jetpackObj.transform.Find ("Sprite").GetComponent<SpriteRenderer>().sprite = teamSettings.GetJetpack();
		aimSprend.sprite = teamSettings.GetWeapon();
		if (teamNum == 1)
			point_manager.ui_pts_left = GameObject.Find ("Team1Points").GetComponent<UnityEngine.UI.Text> ();
		else 
			point_manager.ui_pts_left = GameObject.Find ("Team2Points").GetComponent<UnityEngine.UI.Text> ();
	}

    // Retrieve and apply any changes to the players movement
    void CalculateMovement() {
        Vector3 vel = rigid.velocity;

        // JF: enable walking Animation
        if (Mathf.Abs(vel.x) > 0.1f) {
            animator.SetBool("walking", true);
        }
        else if (vel.x == 0) {
            animator.SetBool("walking", false);
        }

        vel.x = GetXInputSpeed (vel.x);
        vel.y = GetYInputSpeed (vel.y);

        rigid.velocity = vel;
    }

    // Calculate and return magnitude of any changes to x velocity from player input
    float GetXInputSpeed(float currentX) {
        float direction = input.LeftStickX;

        FlipAllSprites (direction);

        if (grounded) {
            // JF: Provide acceleration to allow finer movement
            currentX = Mathf.Lerp(currentX, direction * xSpeed, Time.deltaTime * 10);

            if (Mathf.Abs(currentX) < 0.5f) {
                currentX = 0;
            }
        }
        else {
            // JF: Decrease maneuverability while in the air
            currentX = Mathf.Lerp(currentX, direction * xSpeed, Time.deltaTime * 2);
        }

        return currentX;
    }

    // Calculate and return magnitude of any changes to y velocity from player input
    // JF: Jump and down-jump
    // AW: Jetpack
    float GetYInputSpeed(float currentY) {
        // Down jump
        if (grounded && input.LeftStick.Down > DOWN_JUMP_THRESHOLD && canDownJump){
            // tt_manager.downJumped = true;
            bodyCollider.isTrigger = true;
            Invoke ("RestoreCollision", 0.3f);
        }

		// Jetpack calculations
		if (grounded) {
			jetpackFuelCurrent += jetpackRefuelRate * Time.deltaTime;
			if (jetpackFuelCurrent > jetpackFuelMax) {
				jetpackFuelCurrent = jetpackFuelMax;
				jetpackFuelBar.SetActive (false);
			}
        }
		
        if (input.LeftStickY > JETPACK_ACTIVE_THRESHOLD && jetpackFuelCurrent > 0f) {
			jetpackFuelBar.SetActive (true);
			jetpackFuelCurrent -= Time.deltaTime;
			currentY = GetJetpackThrust ();
			jetpackFire.SetActive (true);

            animator.SetBool("flying", true);
 
		} else {
			jetpackFire.SetActive (false);

            animator.SetBool("flying", false);
		}
        jetpack_bar.SetFuel (jetpackFuelCurrent);

        return currentY;
    }

    void RestoreCollision () {
        bodyCollider.isTrigger = false;
    }

    // Returns a normalized vector pointed toward the direction of the left joystick
    Vector3 GetLeftJoyDirection() {
        Vector3 inputDir = new Vector3 (input.LeftStickX, input.LeftStickY);
        return inputDir.normalized;
    }

	// Returns a normalized vector pointed toward the direction of the left joystick
	Vector3 GetRightJoyDirection() {
		Vector3 inputDir = new Vector3 (input.RightStickX, input.RightStickY);
		return inputDir.normalized;
	}

    // Return a vector3 of the location pointed to by the aiming joystick
    // Rounded to nearest 0.5 (e.g. 1.2 rounds to 1.5, 0.8 rounds to 0.5, etc.)
    Vector3 GetGridPosition() {
		Vector3 gridPos = sprend.transform.position + GetRightJoyDirection ();
        gridPos.x = Mathf.Round (gridPos.x);
        gridPos.y = Mathf.Round (gridPos.y);
        return gridPos;
    }

    bool IsRightJoyActive(){
        return input.RightStickX != 0 || input.RightStickY != 0;
    }

    // JF: Return a bool checking if player object is standing on top of a block or ground
    bool IsGrounded() {
        bool val = rigid.IsTouchingLayers(groundedMask);
        animator.SetBool("grounded", val);
        return val;
    }

    bool IsDucking() {
        bool val = input.LeftStickY < 0;
        animator.SetBool("ducking", val);
        return val;
    }

    bool CanDownJump () {
        return rigid.IsTouchingLayers (platformsMask)
               && !rigid.IsTouchingLayers (groundLayer);
    }

    GameObject ScanForBlocks () {
        Collider2D[] blockCols = Physics2D.OverlapCircleAll (transform.position, blockDetectRadius, blockMask);
        
        // Highlight the nearest block
        nearestBlockObj = GetNearestBlock (blockCols);
        return nearestBlockObj;
    }

    // JF: Returns gameobject of closest collider in range of player
    // Assumes cols_in_range is not empty
    GameObject GetNearestBlock (Collider2D[] cols_in_range) {
        if (cols_in_range.Length > 0) {
            float closest_dist = Vector3.Distance(cols_in_range[0].transform.position, transform.position);
            GameObject closest_block = cols_in_range[0].gameObject;
            foreach(Collider2D coll in cols_in_range) {
                float temp_dist = Vector3.Distance(coll.transform.position, transform.position);
                if(Mathf.Abs(temp_dist) < Mathf.Abs(closest_dist)) {
                    closest_dist = temp_dist;
                    closest_block = coll.gameObject;
                }
            }
            return closest_block;
        }
        return null;
    }

    // when picking up an item, check to see if we have enough points to be able to
    // pick up the item in the first place
    void TryToHoldBlock(Collider2D itemCol){
        Item itemScript = itemCol.GetComponent<Item> ();

        if (point_manager.UsePoints (itemScript.GetCost ())) {
            itemScript.Attach (this);
            heldItem = itemScript;
            form = PlayerForm.Setting;
        }
    }

    bool TryToSitInBlock(){
        Collider2D[] blockCols = Physics2D.OverlapCircleAll (transform.position, blockDetectRadius, blockMask);

        foreach (Collider2D coll in blockCols) {
            // check if there's a weapon in front of us
            if (coll != null && coll.CompareTag ("Core")) {
                // returns whether or not we successfully attached to the block
                return AttachToBlock (coll);
            }
        }

        return false;
    }
/* 
    JF: If within range, selects the closest block to the player and enables
    the player to swap it with another block on their ship. This function handles
    selection of both the first and second blocks to be moved, and swaps them when 
    the second block is successfully selected. */
    void SelectBlockForSwap (GameObject blockObj) {
        // Select block 1
        Block blockScript = blockObj.GetComponent<Block> ();
        if (selectedBlock == null && blockScript != null) {
            selectedBlock = blockScript;

            // Turn on arrow indicator
            swapArrowIndicator.SetActive (true);

            SpriteRenderer blockSprend = blockObj.GetComponent<SpriteRenderer> ();

            if (blockSprend == null) {
                blockSprend = blockObj.GetComponentInChildren<SpriteRenderer> ();
            }

            blockSprend.color = Color.cyan;
        }
        // Swap block 2 with block 1
        else {
            if (blockScript != null) {
                selectedBlock.SwapWithBlock(blockScript);
            }

            EndBlockSwap ();
        }
    }

    // JF: Resets color of block 1 in swap and removes the player's reference to it
    // Assumes that selectedBlockObj is not null
    void EndBlockSwap () {
        SpriteRenderer blockSprend = selectedBlock.GetComponent<SpriteRenderer> ();

        if (blockSprend == null) {
            blockSprend = selectedBlock.GetComponentInChildren<SpriteRenderer> ();
        }

        blockSprend.color = Color.white;

        selectedBlock = null;

        swapArrowIndicator.SetActive (false);
    }

    void RepairBlock(GameObject closest_block) {
        closest_block.GetComponent<Block>().RepairBlock();
    }

    // used when attaching to a controllable block if one is below us
    bool AttachToBlock(Collider2D potential_controllable){
        controlled_block = potential_controllable.gameObject.GetComponent<Controllable>();
        // check if someone is already in the weapon
        if (controlled_block.IsOccupied() || PhaseManager.S.gameOver) {
            return false;
        }

        controlled_block.AttachUser(this);
        rigid.velocity = Vector3.zero;
        form = PlayerForm.Controlling;

        // JF: Disable tooltip
        controlled_block.image.enabled = false;

        // successfully attached!
        return true;
    }

    // Check if there's an item around us (the main 3 item spawners) and pick it up if so
    void TryToPickUpItem(){
        Collider2D itemCol = Physics2D.OverlapCircle (transform.position, itemDetectRadius, itemLayer);
        if (itemCol) {

            GameObject duplicate_item = Instantiate<GameObject>(itemCol.gameObject,
                                                                itemCol.gameObject.transform.position,
                                                                Quaternion.identity);
            if (heldItem) {
                heldItem.Detach (this);
            }

            heldItem = duplicate_item.GetComponent<Item> ();
            heldItem.Attach (this);
        }
    }

    // performs all steps necessary to set an item once a valid position has been found
    void SetItem(Vector3 set_pos){
        if (point_manager.UsePoints (heldItem.GetCost ())) {
            heldItem.Set (set_pos);
			SFXManager.GetSFXManager ().PlaySFX (SFX.BlockSet);
        }
        form = PlayerForm.Setting;
    }

    // used to release from a controllable block and update the correct attributes
    // Calling condition: user presses button to leave block or block is destroyed
    // called by: this.ControllingUpdate(release btn), Controllable.OnDestroy()
    public void DetachFromBlock(){
        sprend.color = Color.white;
        controlled_block.DetachUser ();

        // JF: Re-enable tooltip
        controlled_block.image.enabled = true;

        controlled_block = null;
        form = PlayerForm.Normal;
		SFXManager.GetSFXManager ().PlaySFX (SFX.StopPilot);
    }

    void FlipAllSprites (float x_dir) {
        float flip = (x_dir < 0) ? 180f : 0f;
        if (input.LeftStickX != 0) {
            sprite.transform.rotation = Quaternion.Euler (0f, flip, 0f);
        }
    }

    IEnumerator SpinSprite(){
        float rate = 30f;
        for(float i = 0f; i < 360f; i += rate) {
			charSprite.transform.Rotate(Vector3.forward, rate);
            yield return null;
        }
		Quaternion rot = charSprite.transform.rotation;
        rot.z = 0f;
		charSprite.transform.rotation = rot;
    }

    // JF: Fire burst of three rounds for 
    void FireBurst () {
        Invoke ("FireProjectile", 0);
        Invoke ("FireProjectile", 0.085f);
        Invoke ("FireProjectile", 0.17f);
    }

    void FireProjectile() {
        GameObject proj = Instantiate<GameObject> (projectilePrefab);
        proj.transform.position = projSource.transform.position;
        proj.transform.rotation = projSource.transform.rotation;
        proj.GetComponent<Projectile>().teamNum = teamNum;
        proj.layer = LayerMask.NameToLayer("Team" + teamNum + "Projectiles");
        proj.GetComponent<Rigidbody2D> ().velocity = projSource.transform.right * projSpeed;

		SFXManager.GetSFXManager ().PlaySFX (SFX.ShootLaser, 0.5f);

        // tt_manager.fired = true;
    }

	float GetJetpackThrust() {
        float downVel = rigid.velocity.y;
        // JF: If downward velocity is negative, lerp to 0 in addition to jetpack thrust. 
        // Greatly increases perceived power of jetpack if player is falling
        if (downVel < 3) {
            downVel = Mathf.Lerp(downVel, 5, 5 * Time.deltaTime);
        }

		float magnitude = jetpackAccel * Time.deltaTime;
		if (magnitude + downVel > jetpackMaxSpeed) {
			magnitude = jetpackMaxSpeed;
		} else {
			magnitude += downVel;
		}
		return magnitude;
	}

    public bool IsRespawning(){
        return form == PlayerForm.Respawning;
    }

    public void Respawn(Vector3 spawn_pos){
        SetRespawnSettings ();
        ufo_manager.SetPlayerToRespawn (this, spawn_pos);
    }

    // Respawns the player
    public void Respawn(){
        SetRespawnSettings ();
        ufo_manager.SetPlayerToRespawn (this);
    }

    void SetRespawnSettings(){
        jetpackFuelCurrent = jetpackFuelMax;
        form = PlayerForm.Respawning;
        rigid.gravityScale = 0f;
        rigid.velocity = Vector2.zero;
    }

    // JF: Calling condition: check and respawn this player if it's fallen too far
    // SK: Changed to respawning and added effects for JUICE
    // Called by: this.Update()
    void RespawnPlayerIfBelowScreen(){
        if (MainCamera.S.IsBelowScreen (transform.position) && !IsRespawning() && !PhaseManager.S.gameOver) {
            SFXManager.GetSFXManager ().PlaySFX (SFX.PlayerDied);
            // move the player so they're out of the range of the camera, even if it's all the way zoomed out
            Vector3 pos = transform.position;
            pos.y = ABSOLUTE_LOWER_BOUND;
            transform.position = pos;
            Respawn ();
        }
    }

    public GameObject GetSprite(){
        return sprite;
    }

    public void DisableIndicator(){
        offscreen_arrow_manager.DisableIndicator ();
    }

    public void EnableIndicator(){
        offscreen_arrow_manager.EnableIndicator ();
    }
}
