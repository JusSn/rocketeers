using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerForm {
    Normal,        // Running, Jumping, Idle
    Holding,       // Holding an item
    Throwing,      // Charging up object to throw
    Sitting        // Used when manning a weapon
}

public class Player : MonoBehaviour {
    // Insepctor manipulated attributes
    public float                            xSpeed = 7f;
    public float                            ySpeed = 5f;
    public float                            itemDetectRadius = 0.5f;
    public float                            placementDetectRadius = 0.3f;
    public float                            throwChargeMax = 2.5f;
    public float                            throwChargeRatio = 10f;
    public bool                             debugMode = false;

    public LayerMask                        placementMask;
    public bool                             ________________;
    // Encapsulated attributes
    public PlayerForm                       _form = PlayerForm.Normal;
    public bool                             grounded;
    public Item                             heldItem;                  
    public WeaponBlock                      weapon;

    private bool                            left = false;

    // Counters
    public float                            throwChargeCount = 0f;

    // GameObject components & child objects
    private BoxCollider2D                   coll;
    private Rigidbody2D                     rigid;
    private GameObject                      sprite;
    private SpriteRenderer                  sprend;

    // JF: Highlight object
    public GameObject                       highlightObject;
    private SpriteRenderer[]                highlightSprends;

    // Detection parameters
    private float                           groundCastLength;
    private Vector3                         groundCastOffset;
    private int                             groundMask;
    private int                             blockMask;
    private int                             itemLayer;

    // Function maps
    private Dictionary<PlayerForm, Action>  stateUpdateMap;

    // Use this for initialization
    void Start () {
        // Get GameObject components & children
        coll = GetComponent<BoxCollider2D> ();
        rigid = GetComponent<Rigidbody2D> ();
        sprite = transform.Find ("Sprite").gameObject;
        sprend = sprite.GetComponent<SpriteRenderer> ();

        // JF: Get highlightObject and disable. Enable if item is held later
        highlightObject = transform.Find ("Highlight").gameObject;
        highlightSprends = highlightObject.GetComponentsInChildren<SpriteRenderer> ();
        highlightObject.SetActive(false);

        // Raycast parameters
        groundCastLength = 0.6f*coll.size.y;
        groundCastOffset = new Vector3 (0.5f*coll.size.x, 0f, 0f);
        itemLayer = LayerMask.GetMask ("Items");
        groundMask = LayerMask.GetMask ("Ground");
        blockMask = LayerMask.GetMask ("Blocks");

        // Filling the function behavior map
        stateUpdateMap = new Dictionary<PlayerForm, Action> ();
        stateUpdateMap.Add (PlayerForm.Normal, NormalUpdate);
        stateUpdateMap.Add (PlayerForm.Holding, HoldingUpdate);
        stateUpdateMap.Add (PlayerForm.Throwing, ThrowingUpdate);
        stateUpdateMap.Add (PlayerForm.Sitting, SittingUpdate);
    }

    // Update is called once per frame
    void Update () {
        // Update general attributes
        grounded = IsGrounded ();

        // Call the proper update function
        stateUpdateMap [form] ();
    }

    /******************** State Modifiers & Behaviors ********************/

    // General behavior of the player when not holding anything
    // Entered from: Throwing(thrown)
    // Exit to: Holding(pickup)
    void NormalUpdate() {
        CalculateMovement ();

        // Check if an item is within reach
        Collider2D itemCol;
        if (itemCol = Physics2D.OverlapCircle (transform.position, itemDetectRadius, itemLayer)) {
            if (Input.GetButtonDown ("Pickup_P1")) {
                Item held = itemCol.GetComponent<Item> ();
                held.Attach (this);
                heldItem = held;
                form = PlayerForm.Holding;
            }
        } else if (Input.GetButtonDown("Pickup_P1") && TryToSitInWeapon()) {
            // there's a weapon underneath us, so sit in it
        }

    }

    // Behavior when holding an item; Transitions to either throw or set
    // Entered from: Normal(pickup), Throwing(cancel)
    // Exit to: Throwing(throw button)
    void HoldingUpdate() {
        CalculateMovement ();

        // Switch to either throwing or setting
        if (Input.GetButtonDown ("Throw_P1")) {
            form = PlayerForm.Throwing;
        } else if (heldItem.IsSettable()) {
            // JF: Change location of highlight guide
            Vector3 setPos = GetGridPosition ();
            highlightObject.transform.position = setPos;

            // Check if highlighted position is valid for placement
            Collider2D blocker = Physics2D.OverlapCircle (setPos, placementDetectRadius, placementMask);
            // Obstruction here 
            if (blocker) {
                foreach (SpriteRenderer sp in highlightSprends) {
                    sp.color = Color.red;
                }

                // TODO: Play buzzer sound if player attempts to set item here
            }
            else {
                foreach (SpriteRenderer sp in highlightSprends) {
                    sp.color = Color.white;
                }
                if (Input.GetButtonDown ("Pickup_P1")) {
                    if (debugMode) {
                        Debug.DrawLine (transform.position, setPos, Color.red);
                    }

                    heldItem.Set (setPos);
                    heldItem.Detach (this);
                    heldItem = null;
                    form = PlayerForm.Normal;
                }
            }
        // TODO: This if statement will probably never be called because
        //       the user will always put down the block before they attempt to
        //       sit in a weapon. If we want the weapon to take priority, then
        //       move this check above the heldItem.IsSettable() check.
        } else if (Input.GetButtonDown("Pickup_P1") && TryToSitInWeapon()) {
            heldItem.Thrown (this, Vector3.left + Vector3.up);
        }
    }

    // Behavior when charging a throw; let go of the throw button to throw
    // Entered from: Holding(throw button)
    // Exit to: Holding(cancel), Normal(throw)
    void ThrowingUpdate() {
        CalculateMovement ();
        throwChargeCount += Time.deltaTime;
        sprend.color = Color.Lerp (Color.white, Color.red, throwChargeCount / throwChargeMax);

        if (Input.GetButtonUp ("Throw_P1")) {
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
        } else if (Input.GetButtonDown ("Cancel_P1")) {
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
        if (Input.GetButtonDown("Pickup_P1")){
            DetachFromWeapon ();
            return;
        }

        if (Input.GetButtonDown ("Throw_P1")) {
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
                _form = value;
                break;
            case PlayerForm.Holding:
                // JF: Toggle highlight guide
                highlightObject.SetActive(true);
                _form = value;
                break;
            case PlayerForm.Throwing:
                if (_form == PlayerForm.Holding) {
                    _form = value;
                } else {
                    Debug.LogError ("Transition to Holding state from " + gameObject.name + " from " + _form);
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
        if (vel.x < 0 && !left || vel.x > 0 && left) {
            transform.RotateAround(sprend.transform.position, Vector3.up, 180);
            left = !left;
        }

        rigid.velocity = vel;
    }

    // Calculate and return magnitude of any changes to x velocity from player input
    float GetXInputSpeed(float currentX) {
        float direction = Input.GetAxis ("MoveX_P1");
        if (direction > 0) {
            if (grounded) {
                currentX = Mathf.Lerp(currentX, xSpeed, Time.deltaTime * 10);
            }
            else {
                currentX = Mathf.Lerp(currentX, xSpeed, Time.deltaTime * 2);
            }
        } else if (direction < 0) {
            if (grounded) {
                currentX = Mathf.Lerp(currentX, -xSpeed, Time.deltaTime * 10);
            }
            else {
                currentX = Mathf.Lerp(currentX, -xSpeed, Time.deltaTime * 2);
            }
        }
        return currentX;
    }

    // Calculate and return magnitude of any changes to y velocity from player input
    float GetYInputSpeed(float currentY) {
        if (grounded && Input.GetButtonDown ("Jump_P1")) {
            currentY = ySpeed;
        }
        return currentY;
    }

    // Returns a normalized vector pointed toward the direction of the aiming joystick
    Vector3 GetAimDirection() {
        Vector3 inputDir = new Vector3 (Input.GetAxisRaw ("AimX_P1"), Input.GetAxisRaw ("AimY_P1"));
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
        return rigid.IsTouchingLayers(groundMask);
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
