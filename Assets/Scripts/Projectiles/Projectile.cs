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
        transform.rotation = Quaternion.FromToRotation(Vector3.right, rigid.velocity);
    }

    protected virtual void OnCollisionEnter2D(Collision2D other){
        print (other.gameObject);
        // check if we came in contact with another block/weaponblock
        if (other.gameObject.tag.StartsWith ("Block") || other.gameObject.tag == "Core"){
            other.gameObject.GetComponent<Block> ().LaserDamage (other, gameObject);
        } else {
            DestroyThis ();
        }

        // Create hit effect
        GameObject effect = Instantiate(hit_effect, other.contacts[0].point, Quaternion.identity);
        if (gameObject.GetComponent<Rigidbody2D>().velocity.x > 0) {
            effect.GetComponent<SpriteRenderer>().flipX = true;
        }
        hit_effect.GetComponent<LoopingAnimation>().StartAnimation();
    }

    protected void DestroyThis() {
        Destroy(gameObject);
    }
}
