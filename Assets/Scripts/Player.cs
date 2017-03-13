using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerForm {
    Normal,        // Running, Jumping, Idle
	Shooting,	   // Aiming to shoot held weapon
    Holding,       // Holding an item
    Throwing,      // Charging up object to throw
    Setting,       // Placing settable object
    Sitting        // Used when manning a weapon
}

public class Player : MonoBehaviour {
    // Insepctor manipulated attributes
	public string							playerNum;
    public float                            xSpeed = 7f;
    public float                            ySpeed = 10f;
    public float                            itemDetectRadius = 0.5f;
    public float                            placementDetectRadius = 0.3f;
    public float                            throwChargeMax = 2.5f;
    public float                            throwChargeRatio = 10f;
	public GameObject 						projectilePrefab;
	public float							projSpeed = 10f;
    public bool                             debugMode = false;

    public LayerMask                        placementMask;
    public LayerMask                        groundedMask;
    public bool                             ________________;
    // Encapsulated attributes
    public PlayerForm                       _form = PlayerForm.Normal;
    public bool                             grounded;
    public Item                             heldItem;                  
    public WeaponBlock                      weapon;

    // Internal Support Variables
	private string							playerNumStub;
	private bool                            left = false;
    private float                           throwChargeCount = 0f;

    // GameObject components & child objects
    private Rigidbody2D                     rigid;
    private GameObject                      sprite;
    private SpriteRenderer                  sprend;
    private PointManager                    point_manager;
    private ToolTipManager                  tt_manager;

    private BoxCollider2D                   bodyCollider;

    // JF: Highlight object
    public GameObject                       highlightObject;
    private SpriteRenderer[]                highlightSprends;

	// AW: Aim arrow
	private GameObject						aimArrowObject;
	private GameObject 						projSource;

    // Detection parameters
    private int                             blockMask;
    private int                             itemLayer;
    private int                             platformLayer;
    private int                             groundLayer;

    // Internal maps
    private Dictionary<PlayerForm, Action>  buildUpdateMap;
	private Dictionary<PlayerForm, Action>  battleUpdateMap;

    // Use this for initialization
    void Start () {
        // Get GameObject components & children
        rigid = GetComponent<Rigidbody2D> ();
        sprite = transform.Find ("Sprite").gameObject;
        sprend = sprite.GetComponent<SpriteRenderer> ();
        bodyCollider = GetComponent<BoxCollider2D> ();
        point_manager = GetComponent<PointManager> ();
        tt_manager = GetComponent<ToolTipManager> ();
        tt_manager.SetPlayer (gameObject);


        // JF: Get highlightObject and disable. Enable if item is held later
        highlightObject = transform.Find ("Highlight").gameObject;
        highlightSprends = highlightObject.GetComponentsInChildren<SpriteRenderer> ();
        highlightObject.SetActive(false);

		// AW: Get arrow sprite for aiming shots and proj source
		aimArrowObject = transform.Find("AimArrow").gameObject;
		aimArrowObject.SetActive (false);
		projSource = aimArrowObject.transform.Find ("ProjectileSource").gameObject;

		// Initializing internal
		playerNumStub = "_P" + playerNum;

        // Raycast parameters
        itemLayer = LayerMask.GetMask ("Items");
        blockMask = LayerMask.GetMask ("Blocks");
        platformLayer = LayerMask.GetMask ("Platform");
        groundLayer = LayerMask.GetMask ("Ground");

        // Filling the function behavior map
		buildUpdateMap = new Dictionary<PlayerForm, Action> ();
		buildUpdateMap.Add (PlayerForm.Normal, NormalBuildUpdate);
		buildUpdateMap.Add (PlayerForm.Holding, HoldingUpdate);
		buildUpdateMap.Add (PlayerForm.Throwing, ThrowingUpdate);
		buildUpdateMap.Add (PlayerForm.Setting, SettingUpdate);
		buildUpdateMap.Add (PlayerForm.Sitting, SittingUpdate);

		battleUpdateMap = new Dictionary<PlayerForm, Action> ();
		battleUpdateMap.Add (PlayerForm.Normal, NormalBattleUpdate);
		battleUpdateMap.Add (PlayerForm.Shooting, ShootingUpdate);
    }

    // Update is called once per frame
    void Update () {
        // Update general attributes
        grounded = IsGrounded ();
        // Call the proper update function
		if (PhaseManager.S.inBuildPhase) {
			buildUpdateMap [form] ();
		} else {
			battleUpdateMap [form] ();
		}
    }

    /******************** State Modifiers & Behaviors ********************/

    // General behavior of the player when not holding anything during the building phase
    // Entered from: Throwing(thrown)
    // Exit to: Holding(pickup)
    void NormalBuildUpdate() {
        CalculateMovement ();

        // Check if an item is within reach
        Collider2D itemCol;
        if (itemCol = Physics2D.OverlapCircle (transform.position, itemDetectRadius, itemLayer)) {
            tt_manager.DisplayPrice (itemCol.gameObject);
            if (Input.GetButtonDown ("X" + playerNumStub)) {
                TryToHoldWeapon (itemCol);
            }
        } else if (Input.GetButtonDown("X" + playerNumStub) && TryToSitInWeapon()) {
            // there's a weapon underneath us, so sit in it
        }

    }

	// General behavior of the player during the battle phase
	// Entered from: Shooting(shot/cancel)
	// Exit to: Shooting(shoot)
	void NormalBattleUpdate() {
		CalculateMovement ();

		if (Input.GetButtonDown ("X" + playerNumStub)) {
			form = PlayerForm.Shooting;
			aimArrowObject.SetActive (true);
		}
	}

	// Aiming the projectile; Let go of the shoot button to fire
	// Entered from: Normal(shoot)
	// Exit to: Normal(shoot)
	void ShootingUpdate() {

		if (Input.GetAxis ("LeftJoyX" + playerNumStub) != 0 || Input.GetAxis ("LeftJoyX" + playerNumStub) != 0) {
			Vector3 trajectory = GetAimDirection ();
			float angle = Vector3.Angle (Vector3.right, trajectory);
			if (trajectory.y < 0) {
				angle *= -1;
			}
			aimArrowObject.transform.rotation = Quaternion.AngleAxis (angle, Vector3.forward);
		}

		// JF: Attempt to place block
		if (Input.GetButtonUp ("X" + playerNumStub)) {	
			GameObject proj = Instantiate<GameObject> (projectilePrefab);
			proj.transform.position = projSource.transform.position;
			proj.transform.rotation = projSource.transform.rotation;
			proj.GetComponent<Rigidbody2D> ().velocity = projSource.transform.right * projSpeed;
			form = PlayerForm.Normal;
		} else if (Input.GetButtonDown ("A" + playerNumStub)) {
			CalculateMovement ();
			form = PlayerForm.Normal;
		}
	}

    // Behavior when holding an item; Transitions to either throw or set
    // Entered from: Normal(pickup), Throwing(cancel)
    // Exit to: Throwing(throw button)
    void HoldingUpdate() {
        CalculateMovement ();

        // Switch to either throwing or setting
        if (Input.GetAxis ("Trig" + playerNumStub) < 0) {
            form = PlayerForm.Throwing;
        } else if (heldItem.IsSettable() && Input.GetButtonDown ("X" + playerNumStub)) {
            form = PlayerForm.Setting;
        }
        // TODO: This if statement will probably never be called because
        //       the user will always put down the block before they attempt to
        //       sit in a weapon. If we want the weapon to take priority, then
        //       move this check above the heldItem.IsSettable() check.
        // else if (Input.GetButtonDown("B" + playerNumStub) && TryToSitInWeapon()) {
        //     heldItem.Thrown (this, Vector3.left + Vector3.up);
        // }

        // JF: Release held item
        else if (Input.GetButtonDown("B" + playerNumStub)) {
            heldItem.Thrown (this, Vector3.left + Vector3.up);
            form = PlayerForm.Normal;
        }
    }

    void SettingUpdate() {
        // CalculateMovement ();

        // JF: Change location of highlight guide
        Vector3 setPos = GetGridPosition ();
        highlightObject.transform.position = setPos;

        // JF: Check if highlighted position is valid for placement
        Collider2D blocker = Physics2D.OverlapCircle (setPos, placementDetectRadius, placementMask);
        // Obstruction here 
        if (blocker) {
            foreach (SpriteRenderer sp in highlightSprends) {
                sp.color = Color.red;
            }
        }
        else {
            foreach (SpriteRenderer sp in highlightSprends) {
                sp.color = Color.white;
            }
        }

        // JF: Attempt to place block
        if (Input.GetButtonUp ("X" + playerNumStub)) {
            if (debugMode) {
                Debug.DrawLine (transform.position, setPos, Color.red);
            }

            if (blocker) { // Cannot place here
                form = PlayerForm.Holding;
                // TODO: JF: Play buzzer sound if player attempts to set item here
            }
            else {
                heldItem.Set (setPos);
                heldItem.Detach (this);
                heldItem = null;
                form = PlayerForm.Normal;
            }
        }
    }

    // Behavior when charging a throw; let go of the throw button to throw
    // Entered from: Holding(throw button)
    // Exit to: Holding(cancel), Normal(throw)
    void ThrowingUpdate() {
        CalculateMovement ();
        throwChargeCount += Time.deltaTime;
        sprend.color = Color.Lerp (Color.white, Color.red, throwChargeCount / throwChargeMax);

        if (Input.GetAxis ("Trig" + playerNumStub) == 0) {
            // Item is thrown
            if (throwChargeCount > throwChargeMax) {
                throwChargeCount = throwChargeMax;
            }
            Vector3 throwVel = GetAimDirection()*throwChargeCount*throwChargeRatio;
            heldItem.Thrown (this, throwVel);
            heldItem = null;
            throwChargeCount = 0f;
            sprend.color = Color.white;
            form = PlayerForm.Normal;
        } else if (Input.GetButtonDown ("B" + playerNumStub)) {
            // Throwing was cancelled
            throwChargeCount = 0f;
            sprend.color = Color.white;
            form = PlayerForm.Holding;
        }
    }
		
    // Behavior when sitting in a weapon;
    // Entered from: Holding(set button), Normal(set button)
    // Exit to: Normal(set)
    void SittingUpdate() {

        // if the user uses the "use" button while in the weapon, it will
        // detach them from the weapon
        if (Input.GetButtonDown("X" + playerNumStub)){
            DetachFromWeapon ();
            return;
        }

        if (Input.GetAxis ("Trig" + playerNumStub) < 0) {
            weapon.Fire (GetAimDirection());
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
				aimArrowObject.SetActive(false);
                _form = value;
                break;
			case PlayerForm.Shooting:
				aimArrowObject.SetActive(true);
				_form = value;
				break;
            case PlayerForm.Holding:
                // JF: Toggle highlight guide
                highlightObject.SetActive(false);
                _form = value;
                break;
            case PlayerForm.Throwing:
                if (_form == PlayerForm.Holding) {
                    _form = value;
                } else {
                    Debug.LogError ("Transition to Holding state from " + gameObject.name + " from " + _form);
                }
                break;
            case PlayerForm.Setting:
                // JF: Toggle highlight guide
                highlightObject.SetActive(true);
                if (_form == PlayerForm.Holding) {
                    _form = value;
                }
                break;
            case PlayerForm.Sitting:
                sprend.color = Color.blue;
                _form = value;
                break;
            }
        }
    }

    /******************** Utility ********************/

    // Retrieve and apply any changes to the players movement
    void CalculateMovement() {
        Vector3 vel = rigid.velocity;
        vel.x = GetXInputSpeed (vel.x);
        vel.y = GetYInputSpeed (vel.y);

        // Flip Player 
        if (vel.x < -0.1f && !left || vel.x > 0.1f && left) {
            transform.RotateAround(sprend.transform.position, Vector3.up, 180);
            left = !left;
        }

        rigid.velocity = vel;
    }

    // Calculate and return magnitude of any changes to x velocity from player input
    float GetXInputSpeed(float currentX) {
		float direction = Input.GetAxis ("LeftJoyX" + playerNumStub);

        float flip = (direction < 0) ? 180f : 0f;

        if (grounded) {
            currentX = Mathf.Lerp(currentX, direction * xSpeed, Time.deltaTime * 10);
        }
        else {
            currentX = Mathf.Lerp(currentX, direction * xSpeed, Time.deltaTime * 2);
        }

        sprite.transform.rotation = Quaternion.Euler(0f, flip, 0f);
        
        return currentX;
    }

    // Calculate and return magnitude of any changes to y velocity from player input
    // JF: Jump and down-jump
    float GetYInputSpeed(float currentY) {
		if (grounded && Input.GetButtonDown ("A" + playerNumStub)) {
            // Down jump
            if (-Input.GetAxisRaw ("LeftJoyY" + playerNumStub) < 0 
                            && rigid.IsTouchingLayers(platformLayer) 
                            && !rigid.IsTouchingLayers(groundLayer)) {
                bodyCollider.isTrigger = true;
                Invoke ("RestoreCollision", 0.2f);
            }
            else {
                currentY = ySpeed;
            }
        }
        return currentY;
    }

    void RestoreCollision () {
        bodyCollider.isTrigger = false;
    }

    // Returns a normalized vector pointed toward the direction of the aiming joystick
    Vector3 GetAimDirection() {
        Vector3 inputDir = new Vector3 (Input.GetAxisRaw ("LeftJoyX" + playerNumStub), -Input.GetAxisRaw ("LeftJoyY" + playerNumStub));
        return inputDir.normalized;
    }

    // Return a vector3 of the location pointed to by the aiming joystick
    // Rounded to nearest 0.5 (e.g. 1.2 rounds to 1.5, 0.8 rounds to 0.5, etc.)
    Vector3 GetGridPosition() {
        Vector3 gridPos = sprend.transform.position + GetAimDirection ();
        gridPos.x = Mathf.Round (gridPos.x);
        gridPos.y = Mathf.Round (gridPos.y);
        return gridPos;
    }

    // Return a bool checking if player object is standing on top of a block or ground
    bool IsGrounded() {
        return rigid.IsTouchingLayers(groundedMask);
    }

    // when picking up an item, check to see if we have enough points to be able to 
    // pick up the item in the first place
    void TryToHoldWeapon(Collider2D itemCol){
        Item held = itemCol.GetComponent<Item> ();

        if (point_manager.UsePoints (held.GetCost ())) {
            // show the tooltip of the player spending points on picking up the item
            tt_manager.SpendPoints(held.GetCost());
            held.Attach (this);
            heldItem = held;
            form = PlayerForm.Holding;
        }
    }

    bool TryToSitInWeapon(){
        // check if there's a weapon in front of us
        Collider2D potential_weapon = Physics2D.OverlapCircle (transform.position, itemDetectRadius, blockMask);
        if (potential_weapon != null && potential_weapon.gameObject.CompareTag ("WeaponBlock")) {
            // returns whether or not we successfully attached to the weapon
            return AttachToWeapon (potential_weapon);
        }

        return false;
    }

    // used when attaching to a weapon if one is below us
    bool AttachToWeapon(Collider2D potential_weapon){
        weapon = potential_weapon.gameObject.GetComponent<WeaponBlock>();
        // check if someone is already in the weapon
        if (weapon.IsOccupied()) {
            return false;
        }

        weapon.AttachUser(this);
        rigid.velocity = Vector3.zero;
        form = PlayerForm.Sitting;

        // successfully attached!
        return true;
    }

    // used to release from a weapon and update the correct attributes
    // Calling condition: user presses button to leave weapon or weapon is destroyed
    // called by: this.SittingUpdate(release btn), WeaponBlock.OnDestroy()
    public void DetachFromWeapon(){
        sprend.color = Color.white;
        weapon.DetachUser ();
        weapon = null;
        form = PlayerForm.Normal;
    }
}