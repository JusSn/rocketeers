using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour {
	public Vector3 					heldPos = new Vector3 (0f, 0f, 0f);
    // how much this resource costs to pickup/use
    public int                      resource_cost;

	public float					repoolTime = 10f;
	public bool 					________________;
	public bool 					held = false;
	private Rigidbody2D 			rigid;
	private BoxCollider2D  			boxCollider;

	// Use this for initialization
	void Awake () {
		rigid =	GetComponent<Rigidbody2D>();
		boxCollider = GetComponent<BoxCollider2D> ();
	}

	// Set this object as the child of _player at local position heldPos
	public void Attach(Player _player) {
		rigid.velocity = Vector3.zero;
		rigid.isKinematic = true;
		transform.parent = _player.transform;
		transform.localPosition = heldPos;
        transform.localScale = Vector3.one / 2f;
        _player.form = PlayerForm.Setting;

		// JF: Disable collider when held to enable down jumping and disable other people from picking it up
		boxCollider.enabled = false;

		CancelInvoke ();
	}

	// Assumes _player is the parent of this object
	// Puts this object and _player at the same level
	public void Detach(Player _player) {
        _player.heldItem = null;
        _player.form = PlayerForm.Normal;

        Destroy (gameObject);
    }

	// Detach item from parent and add velocity throwVel to this object
	public void Thrown(Player _player, Vector3 throwVel) {
		Detach (_player);
		rigid.velocity = throwVel;

		// JF: Disappears after 10 s if not picked up 
		ScheduleRepool (repoolTime);
	}

    public int GetCost(){
        return resource_cost;
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

	// JF: Repools object if required, else destroys it
	public void ScheduleRepool (float time) {
		CancelInvoke ();
		Invoke ("PoolDestroy", time);
	}

    public void PoolDestroy () {
		Destroy (gameObject);
	}
}
