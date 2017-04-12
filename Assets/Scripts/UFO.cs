using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UFO : MonoBehaviour {
    private float               GRABBING_THRESHOLD = 2f;
    private float               DROPPING_THRESHOLD = 1f;
    private float               DISAPPEARING_THRESHOLD = 2f;

    private Vector3             HELD_POS = new Vector3 (0, -1.25f, 0);
    private Player              player_to_respawn;
    private Dictionary<UFOForm, Action>  state_map = new Dictionary<UFOForm, Action>();
    private UFOForm             state;
    private Transform           players_original_parent;
    private LayerMask           players_original_layer;
    private Vector3             spawn_pos = new Vector3(0f, 30f, 0f); // default UFO pos


	// Use this for initialization
	void Start () {
        state_map.Add (UFOForm.FlyingTowards, FlyingTowardsUpdate);
        state_map.Add (UFOForm.Towing, TowingUpdate);
        state_map.Add (UFOForm.FlyingAway, FlyingAwayUpdate);
	}
	
	// Update is called once per frame
	void Update () {
        state_map [state] ();
	}

    // when the UFO is flying towards a player that has fallen off the ship
    void FlyingTowardsUpdate(){
        transform.position = Vector3.Lerp (transform.position,
                                           player_to_respawn.transform.position,
                                           Time.deltaTime * 0.8f);
        if (Vector3.Distance(transform.position, player_to_respawn.transform.position) <= GRABBING_THRESHOLD){
            state = UFOForm.Towing;
            transform.position = player_to_respawn.transform.position;
            player_to_respawn.transform.SetParent (transform);
            player_to_respawn.transform.localPosition = HELD_POS;
        }
    }

    // when the UFO is towing a player back to the core of the ship
    void TowingUpdate(){
        transform.position = Vector3.Lerp (transform.position,
                                           Utils.GetCorePosition(player_to_respawn.teamNum),
                                           Time.deltaTime * 1.5f);
        
        if (Vector3.Distance (transform.position, Utils.GetCorePosition(player_to_respawn.teamNum)) <= DROPPING_THRESHOLD) {
            player_to_respawn.transform.SetParent (players_original_parent);
            player_to_respawn.gameObject.layer = players_original_layer;
            player_to_respawn.GetComponent<Rigidbody2D>().gravityScale = 2f;
            player_to_respawn.form = PlayerForm.Normal;
            state = UFOForm.FlyingAway;
        }
    }

    // when the UFO has dropped the player and is flying away
    void FlyingAwayUpdate(){
        transform.position = Vector3.Lerp (transform.position,
                                           spawn_pos,
                                           Time.deltaTime * 0.75f);

        if (Vector3.Distance (transform.position, spawn_pos) <= DISAPPEARING_THRESHOLD) {
            Destroy (gameObject);
        }
    }

    /************* Called externally ***********************/

    // called before the UFO really does anything. Lets the UFO know who the target is
    public void SetPlayer(Player player){
        player_to_respawn = player;
        state = UFOForm.FlyingTowards;
        players_original_parent = player.transform.parent;
        players_original_layer = player.gameObject.layer;
        player_to_respawn.gameObject.layer = LayerMask.NameToLayer ("TransparentFX");
    }
}
