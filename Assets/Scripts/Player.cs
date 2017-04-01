using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InControl;

public enum PlayerForm {
    Normal,        // Running, Jumping, Idle
    Setting,       // Placing settable object
    Controlling,   // Used when driving or firing a block weapon
}

public class Player : MonoBehaviour {
    // Insepctor manipulated attributes
    public string                           playerNumStr;
    public int                              teamNum;
    public float                            xSpeed = 7f;
    public float                            ySpeed = 10f;
    public float                            itemDetectRadius = 0.5f;
    public float                            placementDetectRadius = 0.3f;
    public float                            throwChargeMax = 2.5f;
    public float                            throwChargeRatio = 10f;
    public GameObject                       projectilePrefab;
    public float                            projSpeed = 10f;
	public float 							projCDCounter = 0f;
	public float							projCDTime = 0.5f;
    public float                            DRIVE_SPEED_X = 4f;
    public float                            DRIVE_SPEED_Y = 4f;
	public float							jetpackMaxSpeed = 8f;
	public float 							jetpackAccel = 40f;
	public float							jetpackFuelMax = 5f;
	public float 							jetpackRefuelRate = 5f;
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


    // Internal Support Variables
	private float 							jetpackFuelCurrent;

    // GameObject components & child objects
    private Rigidbody2D                     rigid;
    private GameObject                      sprite;
	private GameObject 						charSprite;
    private SpriteRenderer                  sprend;
    private Animator                        animator;
    private PointManager                    point_manager;
    private ToolTipManager                  tt_manager;

    private BoxCollider2D                   bodyCollider;

    // JF: Highlight object
    public GameObject                       highlightObject;
    // private SpriteRenderer[]                highlightSprends;

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

    // Detection parameters
    private int                             blockMask;
    private int                             itemLayer;
    private int                             groundLayer;

    // Internal maps
    private Dictionary<PlayerForm, Action>  stateUpdateMap;

    // CG: input object
    private InputDevice                     input;

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

        jetpackObj = sprite.transform.Find ("Jetpack").gameObject;
        jetpack_bar = GetComponent<JetpackBar> ();
        jetpack_bar.SetMaxFuel (jetpackFuelMax);
        tt_manager = GetComponent<ToolTipManager> ();
        tt_manager.SetPlayer (gameObject);

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
        blockMask = LayerMask.GetMask ("Team" + teamNum.ToString() + "Block");
        groundLayer = LayerMask.GetMask ("Ground");

        // Filling the function behavior map
        stateUpdateMap = new Dictionary<PlayerForm, Action> ();
        stateUpdateMap.Add (PlayerForm.Normal, NormalUpdate);
        stateUpdateMap.Add (PlayerForm.Setting, SettingUpdate);
        stateUpdateMap.Add (PlayerForm.Controlling, ControllingUpdate);
        try {
            input = InputManager.Devices [int.Parse (playerNumStr) - 1];
        } catch {
            Debug.Log ("4 controllers are not connected. Assigning extra players to first player");
            input = InputManager.Devices [0];
        }

        input.LeftStickX.LowerDeadZone = 0.2f;
        input.LeftStickY.LowerDeadZone = 0.5f;
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
			}

			// CG: shooting occurs with right trigger
			if (input.RightTrigger > 0){
				// CG: Shoot gun every projCDTime seconds
				if (projCDCounter >= projCDTime) {
					FireBurst ();
					// FireProjectile ();
					projCDCounter = 0f;
				}
			}

			// Check if a block is within reach
            Collider2D[] blockCols = Physics2D.OverlapCircleAll (transform.position, itemDetectRadius, blockMask);
            if (blockCols.Length != 0){
                if (input.Action3.WasPressed) {
                    if (TryToSitInBlock(blockCols)) {
                        // there's a weapon underneath us, so sit in it
                        form = PlayerForm.Controlling;
						SFXManager.GetSFXManager ().PlaySFX (SFX.StartPilot);
                    }
                    else {
                        TryToRepairBlock(blockCols);
                    }
                }
            }
        }
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
        if (!blocker && valid_neighbor && setPos.x != 0 && setPos.y < Utils.MAX_BUILD_HEIGHT) {
            highlightObject.SetActive (true);
            if (input.RightTrigger > 0) {
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
        if (input.Action3.WasPressed){
            DetachFromBlock ();
            return;
        }

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
            }
        }
    }

	/******************** Public Interface ********************/

	public void SwitchToBattle () {
		buildPhase = false;
        aimArrowObject.SetActive(true);
		if (heldItem)
			heldItem.Detach (this);		
	}

    /******************** Utility ********************/

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
        if (grounded && input.Action1.IsPressed) {
            // Down jump
            if (ducking && canDownJump) {
                tt_manager.downJumped = true;
                bodyCollider.isTrigger = true;
                Invoke ("RestoreCollision", 0.3f);
            } else {
                // Has jumped  
                tt_manager.jumped = true;
                currentY = ySpeed;
				SFXManager.GetSFXManager ().PlaySFX (SFX.Jump, 0.25f);
            }
        }

        // Check for double jump
        if (!doubleJumped && !grounded && input.Action1.WasPressed){
            tt_manager.doubleJumped = true;

            currentY = ySpeed;
			SFXManager.GetSFXManager ().PlaySFX (SFX.Jump, 0.25f);
            StartCoroutine("SpinSprite");
            doubleJumped = true;
        }

		// Jetpack calculations
		if (grounded) {
			jetpackFuelCurrent += jetpackRefuelRate * Time.deltaTime;
			if (jetpackFuelCurrent > jetpackFuelMax) {
				jetpackFuelCurrent = jetpackFuelMax;
				jetpackFuelBar.SetActive (false);
			}
        }
		
        if (input.LeftTrigger > 0 && jetpackFuelCurrent > 0f) {
			jetpackFuelBar.SetActive (true);
			jetpackFuelCurrent -= Time.deltaTime;
			currentY = GetJetpackThrust ();
			jetpackFire.SetActive (true);

            animator.SetBool("flying", true);

            // JF: Disable tooltip once player has used jetpack a sufficient amount
            if (jetpackFuelCurrent < 3.5f) {
                tt_manager.jetpacked = true;
            }
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

    // when picking up an item, check to see if we have enough points to be able to
    // pick up the item in the first place
    void TryToHoldBlock(Collider2D itemCol){
        Item itemScript = itemCol.GetComponent<Item> ();

        if (point_manager.UsePoints (itemScript.GetCost ())) {
            // show the tooltip of the player spending points on picking up the item
            tt_manager.SpendPoints(itemScript.GetCost());
            itemScript.Attach (this);
            heldItem = itemScript;
            form = PlayerForm.Setting;
        }
    }

    bool TryToSitInBlock(Collider2D[] potential_controllable){
        foreach (Collider2D coll in potential_controllable) {
            // check if there's a weapon in front of us
            if (potential_controllable != null && coll.CompareTag ("Core")) {
                // returns whether or not we successfully attached to the block
                return AttachToBlock (coll);
            }
        }

        return false;
    }

    void TryToRepairBlock(Collider2D[] repairable_block) {
        float closest_dist = Vector3.Distance(repairable_block[0].transform.position, transform.position);
        GameObject closest_block = repairable_block[0].gameObject;
        foreach(Collider2D coll in repairable_block) {
            float temp_dist = Vector3.Distance(coll.transform.position, transform.position);
            if(coll.gameObject.GetComponent<Health>().GetHealth() < coll.gameObject.GetComponent<Health>().MAX_HEALTH && Mathf.Abs(temp_dist) < Mathf.Abs(closest_dist)) {
                closest_dist = temp_dist;
                closest_block = coll.gameObject;
            }
        }
        closest_block.GetComponent<Block>().RepairBlock();
    }

    // used when attaching to a controllable block if one is below us
    bool AttachToBlock(Collider2D potential_controllable){
        controlled_block = potential_controllable.gameObject.GetComponent<Controllable>();
        // check if someone is already in the weapon
        if (controlled_block.IsOccupied()) {
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
            // show the tooltip of the player spending points on picking up the item
            tt_manager.SpendPoints (heldItem.GetCost ());
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

		SFXManager.GetSFXManager ().PlaySFX (SFX.ShootLaser);

        tt_manager.fired = true;
    }

	float GetJetpackThrust() {
        float downVel = rigid.velocity.y;
        // JF: If downward velocity is negative, lerp to 0 in addition to jetpack thrust. 
        // Greatly increases perceived power of jetpack if player is falling
        if (downVel < 0) {
            downVel = Mathf.Lerp(downVel, 0, 5 * Time.deltaTime);
        }

		float magnitude = jetpackAccel * Time.deltaTime;
		if (magnitude + downVel > jetpackMaxSpeed) {
			magnitude = jetpackMaxSpeed;
		} else {
			magnitude += downVel;
		}
		return magnitude;
	}
}
