using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Settable : Item {
	public GameObject			setObject;

	private GameObject 			highlightObj = null;

	public override bool IsSettable() {
		return true;
	}

	public override void Set(Vector3 setPos) {
		GameObject go = Instantiate<GameObject> (setObject, setPos, Quaternion.identity);
        // SK: keep track of blocks that have been placed
        if (PhaseManager.S)
            // give it the new big block and the settable object
            PhaseManager.S.AddBlock (go, gameObject);
        // JF: push back into pool
	}

	/// <summary>
	/// Update is called every frame, if the MonoBehaviour is enabled.
	/// </summary>
	void Update()
	{	
		// JF: Move item sprite around to match building highlight guide
		if (held) {
			if (highlightObj == null) {
                // CG: the blocks are now children of the sprite of the character so
                // they flip when the character does. So now we have to find the parent
                // of a parent to get the 'Highlight' object
                highlightObj = transform.parent.parent.Find ("Highlight").gameObject;
				transform.localPosition = heldPos;
			}
			else if (highlightObj.activeInHierarchy) {
				transform.position = highlightObj.transform.position;
			}
			else {
				transform.localPosition = heldPos;
			}
		}
	}
}
