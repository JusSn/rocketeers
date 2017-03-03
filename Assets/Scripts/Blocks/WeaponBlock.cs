using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponBlock : MonoBehaviour {



    private Player                              controller = null;


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    // calling condition: a user is in the weapon and wants to
    //                    fire ze bullets
    // called by: Player.SittingUpDate(Fire button);
    public void Fire(){
        print ("Firing");
    }

    // calling condition: when a user is near a weapon and
    //                    presses the correct button to use
    //                    the weapon
    // called by: Player.CanSitInWeapon()
    public void AttachUser(Player user){
        user.transform.position = this.transform.position;
        SetUser (user);
    }
    // calling condition: when a user is done using a weapon
    //                    and presses the correct button to
    //                    exit the weapon
    // called by: Player.SittingUpdate(release button)
    public void DetachUser(){
        controller = null;
    }

    // calling condition: when a user wants to attach to a weapon
    //                    a check is performed to see if the weapon
    //                    is already occupied
    // called by: Player.CanSitInWeapon()
    public bool IsOccupied(){
        // if the controller is not empty, then it is occupied
        return controller != null;
    }


    /****************** Utility **********************/
    // called by: this.AttachUser(user);
    void SetUser(Player user){
        controller = user;
    }
}
