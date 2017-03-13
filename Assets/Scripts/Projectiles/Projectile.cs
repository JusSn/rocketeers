using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {
    
    // inspector tunables
    public float                    bullet_speed = 4f;
    public float                    damage_amount = 25f;

    // Use this for initialization
	void Start () {
	}

    void Update(){
        if (!MainCamera.S.IsOnScreen (transform.position)) {
            Destroy (gameObject);
        }
    }
	
    // changes the collider to be active instead of just a trigger once it
    // crosses the center line
    protected virtual void OnTriggerEnter2D(Collider2D other){
        if (other.gameObject.CompareTag ("ConvertProjectile")) {
            GetComponent<CircleCollider2D> ().isTrigger = false;
        }
    }

    protected virtual void OnCollisionEnter2D(Collision2D other){
        // check if we came in contact with another block/weaponblock
        if (other.gameObject.CompareTag ("Block") || other.gameObject.CompareTag ("WeaponBlock") || other.gameObject.CompareTag("Core")) {
            other.gameObject.GetComponent<Block> ().TakeDamage (damage_amount);
        }
        Destroy(gameObject);
    }
}
