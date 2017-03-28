using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Settable : Item {
	public GameObject			setObject;

	public override bool IsSettable() {
		return true;
	}

	public override void Set(Vector3 setPos) {
		GameObject go = Instantiate<GameObject> (setObject, setPos, Quaternion.identity);
        // SK: keep track of blocks that have been placed
        PhaseManager.S.placedBlocks.Add(go);
        // JF: push back into pool
	}
}
