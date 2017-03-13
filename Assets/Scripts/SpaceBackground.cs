using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Created by SK on 3/12/17

public class SpaceBackground : MonoBehaviour {

    private float spriteHeight = 90;
    private float drag = 0.5f;
    private float gravity;

	// Use this for initialization
	void Start () {
        gravity = PhaseManager.S.gravityScale;
    }
	
	// Update is called once per frame
	void Update () {
        //Check if the difference along the y axis between the main Camera and the position of the object this is attached to is greater than spriteHeight.
        if (transform.position.y < -spriteHeight) {
            //If true, this means this object is no longer visible and we can safely move it forward to be re-used.
            RepositionBackground();
        }
    }

    public void StartFlying() {
        Rigidbody2D body = GetComponent<Rigidbody2D>();
        body.drag = drag;
        body.gravityScale = gravity;
    }

    public void StopFlying() {
        GetComponent<Rigidbody2D>().gravityScale = 0;
    }

    //Moves the object this script is attached to up in order to create our looping background effect.
    private void RepositionBackground() {
        //This is how far up we will move our background object, in this case, twice its length. This will position it directly above the currently visible background object.
        Vector2 offset = new Vector2(0, spriteHeight * 2f);

        //Move this object from it's position offscreen, behind the player, to the new position off-camera above of the player.
        transform.position = (Vector2)transform.position + offset;
    }
}
