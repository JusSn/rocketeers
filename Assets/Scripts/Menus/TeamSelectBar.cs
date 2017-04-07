using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InControl;

public class TeamSelectBar : MonoBehaviour {
	public float 					shiftDistance;

	private SpriteRenderer			sprend;
	private GameObject 				leftArrow;
	private GameObject				rightArrow;

	public bool					shifting = false;
	private CharacterSettings		character;
	public InputDevice				input;

	// Use this for initialization
	void Awake () {
		// Get Child objects and components
		sprend = GetComponent<SpriteRenderer> ();
		leftArrow = transform.Find ("left").gameObject;
		rightArrow = transform.Find ("right").gameObject;
	}

	void Start () {
		leftArrow.SetActive (true);
		rightArrow.SetActive (true);
	}

	// Update is called once per frame
	void Update () {
		if (!shifting) {
			Vector3 shift = new Vector3 (shiftDistance, 0f, 0f);
			if (character.teamNumber == 0) {
				if (input.LeftStick.Left) {
					StartCoroutine (ShiftTo (transform.position - shift));
					leftArrow.SetActive (false);
					character.teamNumber = 1;
				} else if (input.LeftStick.Right) {
					StartCoroutine (ShiftTo (transform.position + shift));
					rightArrow.SetActive (false);
					character.teamNumber = 2;
				} 
			} else {
				if (input.LeftStick.Left && transform.position.x > 0) {
					StartCoroutine (ShiftTo (transform.position - shift));
					rightArrow.SetActive (true);
					character.teamNumber = 0;
				} else if (input.LeftStick.Right && transform.position.x < 0) {
					StartCoroutine (ShiftTo (transform.position + shift));
					leftArrow.SetActive (true);
					character.teamNumber = 0;
				}
			}
		}
	}

	/**************** Public Interface *****************/

	public void ResetTeamSelect() {
		input = null;

		Vector3 pos = transform.position;
		pos.x = 0f;
		transform.position = pos;
		shifting = false;

		leftArrow.SetActive (true);
		rightArrow.SetActive (true);
		gameObject.SetActive (false);
	}

	// Assumes ResetTeamSelect() has been called prior
	public void AssignPlayer (InputDevice device, CharacterSettings settings) {
		input = device;
		character = settings;
		character.teamNumber = 0;

		sprend.sprite = settings.sprite;

		gameObject.SetActive (true);
	}

	public InputDevice GetDevice() {
		return input;
	}

	/**************** Utility *****************/

	IEnumerator ShiftTo (Vector3 destination) {
		shifting = true;
		SFXManager.GetSFXManager ().PlaySFX (SFX.HitPlayer);
		while (Mathf.Abs (Vector3.Distance (transform.position, destination)) > 0.01f) {
			transform.position = Vector3.Lerp (transform.position, destination, 0.25f);
			yield return null;
		}
		transform.position = destination;
		shifting = false;
	}
}
