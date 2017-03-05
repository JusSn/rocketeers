using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Settable : Item {
	public GameObject			setObject;

	public override bool IsSettable() {
		return true;
	}

	public override void Set(Vector3 setPos) {
		Instantiate<GameObject> (setObject, setPos, Quaternion.identity);
		// JF: push back into pool
		CancelInvoke ();
		PoolDestroy ();
	}
}
