using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Basic class for switching sprites when a function is called
public class SwitchSprites : MonoBehaviour {
	public Sprite				onSprite;
	public Sprite				offSprite;
	public bool					switched;
	public SpriteRenderer		sprend;

	// Use this for initialization
	void Start () {
		switched = false;

		sprend = transform.GetComponent<SpriteRenderer>();
		sprend.sprite = offSprite;
	}

	public void SwitchOn() {
		switched = true;
		sprend.sprite = onSprite;
	}

	public void SwitchOff() {
		switched = false;
		sprend.sprite = offSprite;
	}

	public bool	CheckSwitchOn() {
		return switched;
	}
}
