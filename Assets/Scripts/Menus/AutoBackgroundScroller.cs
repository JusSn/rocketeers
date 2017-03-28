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

	public void SetScrollSpeed(float new_speed) {
		scrollSpeed = new_speed;
	}

	public void SetScrollVertical(bool set_vertical){
		vertical = set_vertical;
	}

	// Update is called once per frame
	void Update () {	
		Vector2 offset = rend.material.mainTextureOffset;
		if (vertical) {
			offset.y += Time.deltaTime * scrollSpeed;
		} else {
			offset.x += Time.deltaTime * scrollSpeed;
		}
		rend.material.mainTextureOffset = Vector2.Lerp(rend.material.mainTextureOffset, offset, 0.5f);
	}
}
