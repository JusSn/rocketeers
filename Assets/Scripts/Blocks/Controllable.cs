using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controllable : Block {

    private Player                               controller = null;

    // Calling condition: when a user is near a block and
    //                    presses the correct button to use
    //                    the controllable block
    // Called by: Player.CanSitInBlock()
    public void AttachUser(Player user){
        user.gameObject.layer = LayerMask.NameToLayer ("TransparentFX");
        user.transform.position = this.transform.position;
        rigid.constraints = RigidbodyConstraints2D.None;
        SetUser (user);
    }

    // Calling condition: when a user is done using a controllable
    //                    and presses the correct button to
    //                    exit the block
    // Called by: Player.SittingUpdate(release button)
    public void DetachUser(){
        controller.gameObject.layer = LayerMask.NameToLayer ("Players");
        controller = null;
        rigid.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY;
    }

    // Calling condition: when a user wants to attach to a controllable
    //                    a check is performed to see if the weaponblock
    //                    is already occupied
    // Called by: Player.CanSitInBlock()
    public bool IsOccupied(){
        // if the controller is not empty, then it is occupied
        return controller != null;
    }


    /****************** Utility **********************/
    // Called by: this.AttachUser(user);
    void SetUser(Player user){
        controller = user;
    }

    public override void UnhingeAndFall(){
        // if someone is in the block when it is destroyed
        // detach the user from the block
        if (controller != null) {
            controller.DetachFromBlock ();
        }
        base.UnhingeAndFall ();
    }
}
