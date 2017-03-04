using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour {

    // Anything with a healthbar/health should add this script to the object and
    // then save it for use throughout the gameobjects lifetime (look to Block to
    // see how it is handled there)

    public GameObject                           health_bar_prefab;

    // health_bar attributes
    private GameObject                          health_bar;
    private Block                               parent_block;
    private Vector3                             health_bar_pos = new Vector3(0f, -0.25f, 0f);

    // health attributes
    private float                               cur_health;
    private float                               MAX_HEALTH = 100f;

    void Start(){
        cur_health = MAX_HEALTH;
    }


    // Calling condition: when a new object is created, it should
    //                    set itself as the parent of the health/healthbar
    // Called by: anyone who wants a health system/healthbar
    public void SetParent(Block in_parent_block){
        parent_block = in_parent_block;
    }

    // Calling condition: when a block/any item that has a health system
    //                    takes damage and calls the "TakeDamage" function
    // Called by: the parent block/object
    // NOTE: pass in a positive number to take damage by
    public void TakeDamage(float dmg_amount){
        Debug.Assert (dmg_amount > 0f, "Pass in a positive number to TakeDamage");
        UpdateHealthByAmount (-dmg_amount);
        CheckToDestroy ();
        FlashHealthBar ();
    }

    void CheckToDestroy(){
        if (cur_health <= 0f) {
            Destroy (gameObject);
        }
    }

    // Called by: this.TakeDamage(dmg_amount) and potentially other objects
    // NOTE: This value can be positive or negative (could adjust health up if repaired)
    public void UpdateHealthByAmount(float health){
        cur_health += health;
    }

    // Calling condition: when taking damage the health bar will appear
    // Called by: this
    // displays the health bar slightly below center of the parent block
    void FlashHealthBar(){
        // for the case that the health_bar will be displayed and destroyed, don't create
        // additional health_bars if one is already present
        if (health_bar == null){
            health_bar = Instantiate<GameObject> (health_bar_prefab, Vector3.down, Quaternion.identity);
            // sets the health bar as being a child of the parent_block gameObject
            health_bar.transform.parent = parent_block.transform;
            health_bar.transform.localPosition = health_bar_pos;
        }

        // adjust the green health bar so that the red background shows
        health_bar.transform.Find("Canvas/GreenBackground").transform.localScale = new Vector3 (cur_health/MAX_HEALTH, 1f, 0f);

        // TODO: IF WE DON'T LIKE HAVING HEALTH BARS AROUND WE CAN MAKE THEM DISAPPEAR AFTER A COUPLE SECONDS
        // OR THEY CAN EXIST ONLY ONCE A BLOCK IS DAMAGED THE FIRST TIME (THIS IS THE CURRENT SITUATION)
        // destroy the health_bar after 2 seconds
        //Destroy (health_bar, 2f);
    }

}
