using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour {

    // Anything with a healthbar/health should add this script to the object and
    // then save it for use throughout the gameobjects lifetime (look to Block to
    // see how it is handled there)

    // inspector tunables
    public float                                MAX_HEALTH;

    public float                                DAMAGE_FROM_LASER;
    public float                                BASE_DAMAGE_FROM_RAM;
    public float                                DAMAGE_FROM_EXPLOSION;
    public bool                                 is_core;


    public GameObject                           health_bar_prefab;
    public Sprite[]                             damage_sprites;

    // health_bar attributes
    public GameObject                             health_bar;

    private SpriteRenderer                      sprend;
    protected Transform                           greenBG;
    protected Block                               parent_block;
    protected Vector3                             health_bar_pos = new Vector3(0f, -0.25f, 0f);

    // health attributes
    protected float                               cur_health;

    protected virtual void Start(){
        cur_health = MAX_HEALTH;
        sprend = GetComponent<SpriteRenderer> ();
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
    public void Damage(float dmg){
        // Debug.Assert (dmg_amount > 0f, "Pass in a positive number to TakeDamage");
        UpdateHealthByAmount (-dmg);
        CheckToDestroy ();
        FlashHealthBar ();
    }

    public void ExplosiveDamage(){
        Damage (DAMAGE_FROM_EXPLOSION);
    }
    public void RammingDamage(float bonus){
        Damage (BASE_DAMAGE_FROM_RAM + bonus);
    }

    public float GetHealth() {
        return cur_health;
    }

    void CheckToDestroy(){
        if (cur_health <= 0f) {
            // SK: add logic for core ending the game
			//SFXManager.GetSFXManager().PlaySFX(SFX.BlockDestroyed);
            if (is_core) {
                PhaseManager.S.EndGame(parent_block);
                // JF: Destroy entire base
                Destroy(gameObject.transform.parent.gameObject);
            }
            // remove all the hinges on the block to let it fall offscreen
            parent_block.Kill ();
        }
    }

    // Called by: this.TakeDamage(dmg_amount) and potentially other objects
    // NOTE: This value can be positive or negative (could adjust health up if repaired)
    public void UpdateHealthByAmount(float health){
        cur_health = Mathf.Max(Mathf.Min(cur_health + health, MAX_HEALTH), 0);

         // Update sprite to reflect current state of damage
         if (damage_sprites.Length > 0) {
             sprend.sprite = damage_sprites[CurrentSprite()];
         }
    }

    // Returns appropriate index into damage_sprites to represent damage dealt to block
    // Assumes that damage_sprites have been set in the inspector with size > 0
    // Assumes that damage_sprites is sorted from most damaged -> undamaged
    private int CurrentSprite() {
        return (int) ((damage_sprites.Length - 1) * (cur_health / MAX_HEALTH));
    }

    // Calling condition: when taking damage the health bar will appear
    // Called by: this
    // displays the health bar slightly below center of the parent block
    protected virtual void FlashHealthBar(){
        // for the case that the health_bar will be displayed and destroyed, don't create
        // additional health_bars if one is already present
        if (health_bar == null){
            InstantiateHealthBar ();
            // JF: Don't have to do a Find every time you want to update green bar
            AssignGreenBG ();
        }

        // adjust the green health bar so that the red background shows
        greenBG.transform.localScale = new Vector3 (Mathf.Max(cur_health/MAX_HEALTH, 0f), 1f, 0f);

        // TODO: IF WE DON'T LIKE HAVING HEALTH BARS AROUND WE CAN MAKE THEM DISAPPEAR AFTER A COUPLE SECONDS
        // OR THEY CAN EXIST ONLY ONCE A BLOCK IS DAMAGED THE FIRST TIME (THIS IS THE CURRENT SITUATION)
        // destroy the health_bar after 2 seconds
        //Destroy (health_bar, 2f);
    }

    void InstantiateHealthBar(){
        health_bar = Instantiate<GameObject> (health_bar_prefab, Vector3.down, Quaternion.identity);

        SetPosition (); // sets the health bar as being a child of the parent_block gameObject

    }

    void SetPosition(){
        health_bar.transform.parent = parent_block.transform;
        health_bar.transform.localPosition = health_bar_pos;
    }

    void AssignGreenBG(){
        greenBG = health_bar.transform.Find ("Canvas/GreenBackground");
    }
}
