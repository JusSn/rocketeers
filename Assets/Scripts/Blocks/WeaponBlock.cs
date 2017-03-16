using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponBlock : Controllable {

    // inspector manipulated variables
    public GameObject                            bullet_prefab;
    public bool                                  __________________;

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
}
