using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Settable : Item {
	public GameObject			setObject;

	public override bool IsSettable() {
		return true;
	}

	// Update is called once per frame
	public override void Set(Vector3 setPos) {
		Instantiate<GameObject> (setObject, setPos, Quaternion.identity);
		Destroy (gameObject);
	}
}
