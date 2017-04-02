using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {

    // inspector tunables
    public float                    bullet_speed = 4f;
    public float                    damage_amount = 15f;
    public int                      teamNum;
    public float                    lifeTime;
    public GameObject               hit_effect;

    // Use this for initialization
    void Start () {
        Invoke("DestroyThis", lifeTime);
    }

    void Update(){

        // SK: shots wrap around to other side
		/*
        if((transform.position.x < MainCamera.S.leftBorder && teamNum == 1) || (transform.position.x > MainCamera.S.rightBorder && teamNum == 2)) {
                transform.position = new Vector3(-transform.position.x, transform.position.y);
        }
        */

		/*
		if(!GetComponent<SpriteRenderer>().isVisible) {
            Destroy(gameObject);
        } 
		*/
    }

    protected virtual void OnCollisionEnter2D(Collision2D other){
		print ("Call me in OnCollision");
        Destroy(gameObject);
        // check if we came in contact with another block/weaponblock
        if (other.gameObject.CompareTag ("Block") || other.gameObject.CompareTag("Core")) {
            other.gameObject.GetComponent<Block> ().TakeDamage (damage_amount);
        }

        // Create hit effect
        GameObject effect = Instantiate(hit_effect, other.contacts[0].point, Quaternion.identity);
        if (gameObject.GetComponent<Rigidbody2D>().velocity.x > 0) {
            effect.GetComponent<SpriteRenderer>().flipX = true;
        }
        hit_effect.GetComponent<LoopingAnimation>().StartAnimation();
    }

    protected void DestroyThis() {
		print ("Call me in DestroyThis");
        Destroy(gameObject);
    }
}
