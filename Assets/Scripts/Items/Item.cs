using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour {
	public Vector3 					heldPos = new Vector3 (0f, 0f, 0f);
	public bool 					________________;
	public bool 					held = false;
	private Rigidbody2D 			rigid;

	// Use this for initialization
	void Start () {
		rigid =	GetComponent<Rigidbody2D>();
	}

	// Set this object as the child of _player at local position heldPos
	public void Attach(Player _player) {
		rigid.velocity = Vector3.zero;
		rigid.isKinematic = true;
		transform.parent = _player.transform;
		transform.localPosition = heldPos;
	}

	// Assumes _player is the parent of this object
	// Puts this object and _player at the same level
	public void Detach(Player _player) {
		rigid.isKinematic = false;
		transform.parent = _player.transform.parent;
	}

	// Detach item from parent and add velocity throwVel to this object
	public void Thrown(Player _player, Vector3 throwVel) {
		Detach (_player);
		rigid.velocity = throwVel;
	}

	/******************** Fat Interface for Derived Classes ********************/

	// Item is settable
	public virtual bool IsSettable() {
		return false;
	}

	// Set item at the input location
	public virtual void Set(Vector3 setPos) {
		return;
	}

	// Item can be used as a weapon
	public virtual bool IsWeapon() {
		return false;
	}
}
