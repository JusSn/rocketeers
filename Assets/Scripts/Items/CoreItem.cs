using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoreItem : Settable {

    public override void Set (Vector3 setPos)
    {
        // destroy the item in the way of the core
        Collider2D item_in_way = Physics2D.OverlapPoint (setPos);
        if (item_in_way) {
            Destroy (item_in_way.gameObject);
        }
        // place the core as a regular block
        base.Set (setPos);
    }
}
