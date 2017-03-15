using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrapplingHook : MonoBehaviour {
    
    // inspector tunables
	private bool						attached = false;
	private Rigidbody2D					rigid;
	private CapsuleCollider2D 			collide;

    // Use this for initialization
	void Start () {
		rigid = GetComponent<Rigidbody2D> ();
		collide = GetComponent<CapsuleCollider2D> ();
	}

	public bool IsAttached() {
		return attached;
	}

    void OnCollisionEnter2D(Collision2D other){
		if (other.gameObject.layer == LayerMask.NameToLayer ("Blocks")) {
			attached = true;
			transform.parent = other.transform;
			rigid.bodyType = RigidbodyType2D.Kinematic;
			rigid.freezeRotation = true;
			collide.isTrigger = true;
		}
    }
}
