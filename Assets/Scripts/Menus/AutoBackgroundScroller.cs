using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoBackgroundScroller : MonoBehaviour {

	public float			scrollSpeed = 1f;
	public bool 			vertical = true;
	private Renderer		rend;

	// Use this for initialization
	void Start () {
		rend = GetComponent<Renderer> ();	
	}
	
	// Update is called once per frame
	void Update () {	
		Vector2 offset;
		if (vertical) {
			offset = new Vector2 (0f, Time.time * scrollSpeed);
		} else {
			offset = new Vector2 (Time.time * scrollSpeed, 0f);
		}
		rend.material.mainTextureOffset = offset;
	}
}
