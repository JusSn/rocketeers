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

	public void Attach(Player _player){
		rigid.velocity = Vector3.zero;
		rigid.isKinematic = true;
		transform.parent = _player.transform;
		transform.localPosition = heldPos;
	}

	public void Detach(Player _player){
		rigid.isKinematic = false;
		transform.parent = _player.transform.parent;
	}

	public void Thrown(Player _player, Vector3 throwVel){
		Detach (_player);
		rigid.velocity = throwVel;
	}
}
