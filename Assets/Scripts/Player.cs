using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerForm {
    Normal,        // Running, Jumping, Idle
    Shooting,      // Aiming to shoot held weapon
    Holding,       // Holding an item
    Throwing,      // Charging up object to throw
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
    public bool                             debugMode = false;
    public float                            DRIVE_SPEED_X = 4f;
    public float                            DRIVE_SPEED_Y = 4f;

    public LayerMask                        placementMask;
    public LayerMask                        groundedMask;
    public bool                             ________________;
    // Encapsulated attributes
    public PlayerForm                       _form = PlayerForm.Normal;
    public bool                             grounded;
    private bool                            ducking;
    public Item                             heldItem;
    public Controllable                     controlled_block;

    // Internal Support Variables
    private string                          playerNumStub;
    private float                           throwChargeCount = 0f;

    // GameObject components & child objects
    private Rigidbody2D                     rigid;
    private GameObject                      sprite;
    private SpriteRenderer                  sprend;
    private Animator                        animator;
    private PointManager                    point_manager;
    private ToolTipManager                  tt_manager;

    private BoxCollider2D                   bodyCollider;

    // JF: Highlight object
    public GameObject                       highlightObject;
    private SpriteRenderer[]                highlightSprends;

    // AW: Aim arrow
    private GameObject                      aimArrowObject;
    private GameObject                      projSource;

    // Detection parameters
    private int                             blockMask;
    private int                             itemLayer;
    private int                             platformLayer;
    private int                             groundLayer;

    // Internal maps
    private Dictionary<PlayerForm, Action>  stateUpdateMap;

    // Use this for initialization
    void Start () {
        // Get GameObject components & children
        rigid = GetComponent<Rigidbody2D> ();
        sprite = transform.Find ("Sprite").gameObject;
        animator = sprite.GetComponent<Animator> ();
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
        playerNumStub = "_P" + playerNumStr;

        // Raycast parameters
        itemLayer = LayerMask.GetMask ("Items");
        blockMask = LayerMask.GetMask ("Blocks");
        platformLayer = LayerMask.GetMask ("Platform");
        groundLayer = LayerMask.GetMask ("Ground");

        // Filling the function behavior map
        stateUpdateMap = new Dictionary<PlayerForm, Action> ();
        stateUpdateMap.Add (PlayerForm.Normal, NormalUpdate);
        stateUpdateMap.Add (PlayerForm.Shooting, ShootingUpdate);
        stateUpdateMap.Add (PlayerForm.Holding, HoldingUpdate);
        stateUpdateMap.Add (PlayerForm.Throwing, ThrowingUpdate);
        stateUpdateMap.Add (PlayerForm.Setting, SettingUpdate);
        stateUpdateMap.Add (PlayerForm.Controlling, ControllingUpdate);
    }

    // Update is called once per frame
    void Update () {
        // Update general attributes
        grounded = IsGrounded ();
        ducking = IsDucking ();
        // Call the proper update function
        stateUpdateMap [form] ();
    }

    /******************** State Modifiers & Behaviors ********************/

    // General behavior of the player when not holding anything during the building phase
    // Entered from: Throwing(thrown)
    // Exit to: Holding(pickup)
    void NormalUpdate() {
        CalculateMovement ();

        if (PhaseManager.S.inBuildPhase) {
            // Check if an item is within reach
            Collider2D itemCol;
            if (itemCol = Physics2D.OverlapCircle (transform.position, itemDetectRadius, itemLayer)) {
                tt_manager.DisplayPrice (itemCol.gameObject);
                if (Input.GetButtonDown ("Y" + playerNumStub)) {
                    TryToHoldBlock (itemCol);
                }
            }

        } else {
            // Check if a block is within reach
            Collider2D[] blockCols = Physics2D.OverlapCircleAll (transform.position, itemDetectRadius, blockMask);
            if (blockCols.Length != 0){
                if (Input.GetButtonDown ("Y"+ playerNumStub) && TryToSitInBlock (blockCols)) {
                    // there's a weapon underndeath us, so sit in it
                    form = PlayerForm.Controlling;
                }
            }
            if (Input.GetButtonDown ("X" + playerNumStub)) {
                form = PlayerForm.Shooting;
                aimArrowObject.SetActive (true);
            }
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

        if (Input.GetButtonUp ("X" + playerNumStub)) {
            GameObject proj = Instantiate<GameObject> (projectilePrefab);
            proj.transform.position = projSource.transform.position;
            proj.transform.rotation = projSource.transform.rotation;
            proj.GetComponent<Projectile>().teamNum = teamNum;
            proj.layer = LayerMask.NameToLayer("Team" + teamNum + "Projectiles");
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
        } else if (heldItem.IsSettable() && Input.GetButtonDown ("Y" + playerNumStub)) {
            form = PlayerForm.Setting;
        }

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
        if (Input.GetButtonUp ("Y" + playerNumStub)) {
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
        // JF: Cancel setting
        else if (Input.GetButtonDown("B" + playerNumStub)) {
            form = PlayerForm.Holding;
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

    // Behavior when controlling a block
    // Entered from: Holding(set button), Normal(set button)
    // Exit to: Normal(set)
    void ControllingUpdate() {

        // used to steer the ship
        float x_val = Input.GetAxis ("LeftJoyX" + playerNumStub) * DRIVE_SPEED_X;
        float y_val = -Input.GetAxis ("LeftJoyY" + playerNumStub) * DRIVE_SPEED_Y;
        controlled_block.GetComponent<Rigidbody2D>().velocity = new Vector3 (x_val, y_val, 0f);
        transform.position = controlled_block.transform.position;

        // Detach from the block if the user wants to
        if (Input.GetButtonDown("Y" + playerNumStub)){
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
            case PlayerForm.Controlling:
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

        rigid.velocity = vel;
    }

    // Calculate and return magnitude of any changes to x velocity from player input
    float GetXInputSpeed(float currentX) {
        float direction = Input.GetAxis ("LeftJoyX" + playerNumStub);

        float flip = (direction < 0) ? 180f : 0f;

        if (grounded) {
            // JF: Provide acceleration to allow finer movement
            currentX = Mathf.Lerp(currentX, direction * xSpeed, Time.deltaTime * 10);

            // JF: enable walking Animation
            if (Mathf.Abs(currentX) > 0.1f) {
                animator.SetBool("walking", true);
            }
            else {
                animator.SetBool("walking", false);
            }
        }
        else {
            // JF: Decrease maneuverability while in the air
            currentX = Mathf.Lerp(currentX, direction * xSpeed, Time.deltaTime * 2);
        }

        if (Input.GetAxis ("LeftJoyX" + playerNumStub) != 0) {
            sprite.transform.rotation = Quaternion.Euler (0f, flip, 0f);
        }

        return currentX;
    }

    // Calculate and return magnitude of any changes to y velocity from player input
    // JF: Jump and down-jump
    float GetYInputSpeed(float currentY) {
        if (grounded && Input.GetButtonDown ("A" + playerNumStub)) {
            // Down jump
            if (ducking
                    && rigid.IsTouchingLayers(platformLayer)
                    && !rigid.IsTouchingLayers(groundLayer)) {
                bodyCollider.isTrigger = true;
                Invoke ("RestoreCollision", 0.3f);
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
        bool val = rigid.IsTouchingLayers(groundedMask);
        animator.SetBool("grounded", val);
        return val;
    }

    bool IsDucking() {
        bool val = -Input.GetAxisRaw ("LeftJoyY" + playerNumStub) < 0;
        animator.SetBool("ducking", val);
        return val;
    }

    // when picking up an item, check to see if we have enough points to be able to
    // pick up the item in the first place
    void TryToHoldBlock(Collider2D itemCol){
        Item held = itemCol.GetComponent<Item> ();

        if (point_manager.UsePoints (held.GetCost ())) {
            // show the tooltip of the player spending points on picking up the item
            tt_manager.SpendPoints(held.GetCost());
            held.Attach (this);
            heldItem = held;
            form = PlayerForm.Holding;
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

        // successfully attached!
        return true;
    }

    // used to release from a controllable block and update the correct attributes
    // Calling condition: user presses button to leave block or block is destroyed
    // called by: this.ControllingUpdate(release btn), Controllable.OnDestroy()
    public void DetachFromBlock(){
        sprend.color = Color.white;
        controlled_block.DetachUser ();
        controlled_block = null;
        form = PlayerForm.Normal;
    }
}
