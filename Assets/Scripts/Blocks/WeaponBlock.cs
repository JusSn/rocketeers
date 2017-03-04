using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponBlock : Block {

    // inspector manipulated variables
    public GameObject                            bullet_prefab;
    public bool                                  __________________;

    // encapsulated attributes
    private Player                               controller = null;
    private float                                bullet_speed = 10f;


    // Calling condition: a user is in the weapon and wants to
    //                    fire ze bullets
    // Called by: Player.SittingUpDate(Fire button);
    public void Fire(Vector3 aim_direction){
        // TODO: this needs cleaning up/refactoring so that the bullet details are more separated
        //       from the weaponblock class. That way we could keep the same weapon base but
        //       swap out bullets and make different guns that way?
        GameObject bullet = Instantiate<GameObject> (bullet_prefab, transform.position + aim_direction, Quaternion.identity);
        bullet.GetComponent<Rigidbody2D> ().velocity = aim_direction * bullet_speed;
    }

    // Calling condition: when a user is near a weapon and
    //                    presses the correct button to use
    //                    the weapon
    // Called by: Player.CanSitInWeapon()
    public void AttachUser(Player user){
        user.transform.position = this.transform.position;
        SetUser (user);
    }

    // Calling condition: when a user is done using a weapon
    //                    and presses the correct button to
    //                    exit the weapon
    // Called by: Player.SittingUpdate(release button)
    public void DetachUser(){
        controller = null;
    }

    // Calling condition: when a user wants to attach to a weapon
    //                    a check is performed to see if the weapon
    //                    is already occupied
    // Called by: Player.CanSitInWeapon()
    public bool IsOccupied(){
        // if the controller is not empty, then it is occupied
        return controller != null;
    }


    /****************** Utility **********************/
    // Called by: this.AttachUser(user);
    void SetUser(Player user){
        controller = user;
    }

    protected override void OnDestroy(){
        // if someone is in the weapon when it is destroyed
        // detach the user from the weapon
        if (controller != null) {
            controller.DetachFromWeapon ();
        }
        base.OnDestroy ();
    }
}
