using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {
    
    // inspector tunables
    public float                    bullet_speed = 4f;
    public float                    damage_amount = 25f;
    public int                      teamNum;
    public float                    lifeTime;

    // Use this for initialization
	void Start () {
        Invoke("DestroyThis", lifeTime);
	}

    void Update(){
        
        // SK: shots wrap around to other side
        if((transform.position.x < MainCamera.S.leftBorder && teamNum == 1) || (transform.position.x > MainCamera.S.rightBorder && teamNum == 2)) {
                transform.position = new Vector3(-transform.position.x, transform.position.y);
        }
        else if(!MainCamera.S.IsOnScreen(transform.position)) {
            Destroy(gameObject);
        }
    }
	
    // changes the collider to be active instead of just a trigger once it
    // crosses the center line
    // protected virtual void OnTriggerEnter2D(Collider2D other){
    //     if (other.gameObject.CompareTag ("ConvertProjectile")) {
    //         GetComponent<CircleCollider2D> ().isTrigger = false;
    //     }
    // }

    protected virtual void OnCollisionEnter2D(Collision2D other){
        Debug.Log("other layer");
        Debug.Log(LayerMask.LayerToName(other.gameObject.layer));
        Debug.Log("this layer:");
        Debug.Log(LayerMask.LayerToName(gameObject.layer));
        // check if we came in contact with another block/weaponblock
        if (other.gameObject.CompareTag ("Block") || other.gameObject.CompareTag("Core")) {
            other.gameObject.GetComponent<Block> ().TakeDamage (damage_amount);
        }
        Destroy(gameObject);
    }

    protected void DestroyThis() {
        Destroy(gameObject);
    }
}
