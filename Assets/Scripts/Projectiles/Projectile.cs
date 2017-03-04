﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {
    
    // inspector tunables
    public float                                    bullet_speed = 4f;
    public float                                    damage_amount = 25f;


    // Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    // calling condition: when the projectile hits another block
    protected virtual void OnTriggerEnter2D(Collider2D other){

        // check if we came in contact with another block/weaponblock
        if (other.gameObject.CompareTag ("Block") || other.gameObject.CompareTag ("WeaponBlock")) {
            other.gameObject.GetComponent<Block> ().TakeDamage (damage_amount);
            Destroy(gameObject);
        }
    }
}