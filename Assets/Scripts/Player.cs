using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerForm {
	Normal,		// Running, Jumping, Idle
	Holding, 	// Holding an item
	Throwing, 	// Charging up object to throw
	Setting		// Finding location to set item
}

public class Player : MonoBehaviour {
	// Insepctor manipulated attributes
	public float 							xSpeed = 4f;
	public float							ySpeed = 5f;
	public float 							itemDetectRadius = 0.5f;
	public float 							throwChargeMax = 2.5f;
	public float 							throwChargeRatio = 10f;
	public bool 							debugMode = false;
	public bool 							________________;
	// Encapsulated attributes
	public PlayerForm						_form = PlayerForm.Normal;
	public bool								grounded;
	public Item 							heldItem;

	// Counters
	public float							throwChargeCount = 0f;

	// GameObject components & child objects
	private BoxCollider2D					coll;
	private Rigidbody2D						rigid;
	private GameObject 						sprite;
	private SpriteRenderer					sprend;

	// Detection parameters
	private float							groundCastLength;
	private Vector3							groundCastOffset;
	private int								blockLayer;
	private int								itemLayer;

	// Function maps
	private Dictionary<PlayerForm, Action>	stateUpdateMap;

	// Use this for initialization
	void Start () {
		// Get GameObject componenets & chidlren
		coll = GetComponent<BoxCollider2D> ();
		rigid = GetComponent<Rigidbody2D> ();
		sprite = transform.Find ("Sprite").gameObject;
		sprend = sprite.GetComponent<SpriteRenderer> ();

		// Raycast parameters
		groundCastLength = 0.6f*coll.size.y;
		groundCastOffset = new Vector3 (0.5f*coll.size.x, 0f, 0f);
		blockLayer = LayerMask.GetMask ("Blocks");
		itemLayer = LayerMask.GetMask ("Items");


		// Filling the function behavior map
		stateUpdateMap = new Dictionary<PlayerForm, Action> ();
		stateUpdateMap.Add (PlayerForm.Normal, NormalUpdate);
		stateUpdateMap.Add (PlayerForm.Holding, HoldingUpdate);
		stateUpdateMap.Add (PlayerForm.Throwing, ThrowingUpdate);
		stateUpdateMap.Add (PlayerForm.Setting, SettingUpdate);
	}
	
	// Update is called once per frame
	void Update () {
		// Update general attributes
		grounded = GetGrounded ();

		// Call the proper update function
		stateUpdateMap [form] ();
	}

	/******************** State Modifiers & Behaviors ********************/

	// General behavior of the player when not holding anything
	// Entered from: Throwing(thrown), Setting(set)
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
		}
	}

	// Behavior when holding an item; Transitions to either throw or set
	// Entered from: Normal(pickup), Throwing(cancel), Setting(cancel)
	// Exit to: Throwing(throw button), Setting(set button)
	void HoldingUpdate() {
		CalculateMovement ();

		// Switch to either throwing or setting
		if (Input.GetAxis ("Throw_P1") > 0f) {
			form = PlayerForm.Throwing;
		} else if (heldItem.IsSettable() && Input.GetAxis ("Set_P1") > 0f) {
			form = PlayerForm.Setting;
		}
	}

	// Behavior when charging a throw; let go of the throw button to throw
	// Entered from: Holding(throw button)
	// Exit to: Holding(cancel), Normal(throw)
	void ThrowingUpdate() {
		CalculateMovement ();
		throwChargeCount += Time.deltaTime;
		sprend.color = Color.Lerp (Color.white, Color.red, throwChargeCount / throwChargeMax);

		if (Input.GetAxis ("Throw_P1") == 0f) {
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

	// Behavior when preparing to set an item; let go of the set button to set
	// Entered from: Holding(set button)
	// Exit to: Holding(cancel), Normal(set)
	void SettingUpdate() {
		CalculateMovement ();

		Vector3 setPos = GetGridPosition ();
		if (debugMode) {
			Debug.DrawLine (transform.position, setPos, Color.red);
		}

		if (Input.GetAxis ("Set_P1") == 0f) {
			heldItem.Set (setPos);
			form = PlayerForm.Normal;
		} else if (Input.GetButtonDown ("Cancel_P1")) {
			// Setting was cancelled
			form = PlayerForm.Holding;
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
				_form = value;
				break;
			case PlayerForm.Holding:
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
				if (_form == PlayerForm.Holding) {
					_form = value;
				} else {
					Debug.LogError ("Transition to Holding state from " + gameObject.name + " from " + _form);
				}
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
		float direction = Input.GetAxis ("MoveX_P1");
		if (0 < direction) {
			currentX = xSpeed;
		} else if (direction < 0) {
			currentX = -xSpeed;
		} 
		return currentX;
	}

	// Calculate and return magnitude of any changes to y velocity from player input
	float GetYInputSpeed(float currentY) {
		if (Input.GetButtonDown ("Jump_P1") && grounded) {
			currentY = ySpeed;
		}
		return currentY;
	}

	// Returns a normalized vector pointed toward the direction of the aiming joystick
	Vector3 GetAimDirection() {
		Vector3 inputDir = new Vector3 (Input.GetAxis ("AimX_P1"), Input.GetAxis ("AimY_P1"), 0f);
		return inputDir.normalized;
	}

	// Return a vector3 of the location pointed to by the aiming joystick
	// Rounded to nearest 0.5 (e.g. 1.2 rounds to 1.5, 0.8 rounds to 0.5, etc.)
	Vector3 GetGridPosition() { 
		Vector3 gridPos = transform.position + GetAimDirection ();
		gridPos.x = Mathf.Floor (gridPos.x) + 0.5f;
		gridPos.y = Mathf.Floor (gridPos.y) + 0.5f;
		return gridPos;

	}

	// Return a bool checking if player object is standing on top of a block or ground
	bool GetGrounded() {
		if (debugMode) {
			Debug.DrawRay (transform.position + groundCastOffset, Vector3.down * groundCastLength, Color.red);
			Debug.DrawRay (transform.position - groundCastOffset, Vector3.down * groundCastLength, Color.red);
		}
		return Physics2D.Raycast (transform.position + groundCastOffset, Vector3.down, groundCastLength, blockLayer)
			|| Physics2D.Raycast (transform.position + groundCastOffset, Vector3.down, groundCastLength, blockLayer);
	}
}
