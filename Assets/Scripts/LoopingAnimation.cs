using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Created by SK on 3/13/17

public class LoopingAnimation : MonoBehaviour {

    public  Sprite[]        sprites;
    public int              current = 0;
    public bool             loop;
    float                   animationSpeed = .07f;
    float                   startTime;
    public bool            animating;
    

	// Use this for initialization
	void Start () {
        
	}
	
	// Update is called once per frame
	void Update () {
        // Hides sprite while in build phase
        if(!animating) {
            GetComponent<SpriteRenderer>().enabled = false;
            return;
        }
        GetComponent<SpriteRenderer>().enabled = true;
        if (Time.time - startTime >= animationSpeed) {
            startTime = Time.time;
            current++;
            if(current == sprites.Length) {
                if (loop) {
                    current = 0;
                }
                else {
                    Destroy(gameObject);
                    return;
                }
            }
            GetComponent<SpriteRenderer>().sprite = sprites[current];
        }
	}

    public void StartAnimation() {
        startTime = Time.time;
        animating = true;
    }
}
