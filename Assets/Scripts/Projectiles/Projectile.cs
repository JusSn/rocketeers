using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {

    // inspector tunables
    public float                    bullet_speed;
    // public float                    damage_amount = 15f;
    public int                      teamNum;
    public float                    lifeTime;
    public GameObject               hit_effect;

    private Rigidbody2D             rigid;

    // Use this for initialization
    void Start () {
        rigid = GetComponent<Rigidbody2D> ();
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
        transform.rotation = Quaternion.FromToRotation(Vector3.right, rigid.velocity);
    }

    protected virtual void OnCollisionEnter2D(Collision2D other){
        // check if we came in contact with another block/weaponblock
        if (other.gameObject.tag.StartsWith("Block") || other.gameObject.CompareTag("Core")) {
            other.gameObject.GetComponent<Block> ().LaserDamage ();

            if (other.gameObject.CompareTag("BlockReflect")) {
                ReflectLaser (other);
            }
            else {
                Destroy (gameObject);
            }
        }
        else {
            Destroy(gameObject);
        }


        // Create hit effect
        GameObject effect = Instantiate(hit_effect, other.contacts[0].point, Quaternion.identity);
        if (gameObject.GetComponent<Rigidbody2D>().velocity.x > 0) {
            effect.GetComponent<SpriteRenderer>().flipX = true;
        }
        hit_effect.GetComponent<LoopingAnimation>().StartAnimation();
    }

    void ReflectLaser (Collision2D collisionInfo) {
        // Normal of collider surface
        Vector3 normal = collisionInfo.contacts[0].normal;
        // Reflect laser
        rigid.velocity = Vector3.Reflect(rigid.velocity, normal).normalized * bullet_speed;
        // Change layer to other team
        gameObject.layer = (gameObject.layer == LayerMask.NameToLayer("Team1Projectiles")) 
                            ? LayerMask.NameToLayer("Team2Projectiles") 
                            : LayerMask.NameToLayer("Team1Projectiles");
    }

    protected void DestroyThis() {
        Destroy(gameObject);
    }
}
