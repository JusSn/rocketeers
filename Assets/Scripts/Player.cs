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
	public GameObject 						projectile;
	public float 							itemDetectRadius = 0.5f;
	public bool 							debugMode = false;
	public bool 							________________;
	// Encapsulated attributes
	public PlayerForm						_form = PlayerForm.Normal;
	public bool								grounded;
	public Item 						heldItem;

	// GameObject components & child objects
	private BoxCollider2D					coll;
	private Rigidbody2D						rigid;

	// Detection parameters
	private float							groundCastLength;
	private Vector3							groundCastOffset;
	private int								blockLayer;
	private int								itemLayer;

	// Function maps
	private Dictionary<PlayerForm, Action>	stateUpdateMap;

	// Use this for initialization
	void Start () {
		// Get GameObject componenets
		coll = GetComponent<BoxCollider2D> ();
		rigid = GetComponent<Rigidbody2D> ();

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

	/******************** State Modifiers & Updaters ********************/

	void NormalUpdate() {
		CalculateMovement ();

		// Check if an item is within reach
		Collider2D itemCol;
		if (itemCol = Physics2D.OverlapCircle (transform.position, itemDetectRadius, itemLayer)) {
			if (Input.GetKey (KeyCode.A)) {
				Item held = itemCol.GetComponent<Item> ();
				held.PickedUp (this);
				heldItem = held;
				form = PlayerForm.Holding;
			}
		}
	}

	void HoldingUpdate() {
		CalculateMovement ();
		if (Input.GetKeyDown (KeyCode.S)) {
			form = PlayerForm.Throwing;
		} else if (Input.GetKeyDown (KeyCode.D)) {
			form = PlayerForm.Setting;
		}
	}

	void ThrowingUpdate() {
		CalculateMovement ();
		if (Input.GetKeyUp (KeyCode.S)) {
			heldItem.Thrown (this);
			heldItem = null;
			form = PlayerForm.Normal;
		}
	}

	void SettingUpdate() {
		CalculateMovement ();
		if (Input.GetKeyUp (KeyCode.D)) {
			heldItem.Thrown (this);
			heldItem = null;
			form = PlayerForm.Normal;
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
		if (Input.GetKey (KeyCode.LeftArrow)) {
			currentX = -xSpeed;
		} else if (Input.GetKey (KeyCode.RightArrow)) {
			currentX = xSpeed;
		} 
		return currentX;
	}

	// Calculate and return magnitude of any changes to y velocity from player input
	float GetYInputSpeed(float currentY) {
		if (Input.GetKey (KeyCode.Space) && grounded) {
			currentY = ySpeed;
		}
		return currentY;
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
