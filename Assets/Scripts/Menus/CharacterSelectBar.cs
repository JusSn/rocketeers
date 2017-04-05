using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InControl;

public enum CharacterSelectState {
	Waiting,
	Selecting,
	Confirmed
}

public class CharacterSelectBar : MonoBehaviour {	
	public CharacterSettings[]		playerOptions;
	public GameObject 				smokeEffect;

	private GameObject 				ufo;
	private GameObject				beam;
	private SpriteRenderer			sprite;
	private GameObject 				waiting;
	private GameObject				selecting;
	private GameObject				confirm;

	private int 					playerChoice = 0;
	public CharacterSelectState	state;
	private InputDevice				input;
	public bool 					switched = false;


	// Use this for initialization
	void Start () {
		ufo = transform.Find ("UFO").gameObject;
		beam = ufo.transform.Find ("selection").gameObject;
		sprite = beam.transform.Find ("sprite").GetComponent<SpriteRenderer> ();
		waiting = transform.Find ("Waiting").gameObject;
		selecting = transform.Find ("Selecting").gameObject;
		confirm = transform.Find ("Confirm").gameObject;


		ufo.SetActive (false);
		waiting.SetActive (true);
		selecting.SetActive (false);
		confirm.SetActive (false);
		state = CharacterSelectState.Waiting;
	}

	public void AssignPlayer (InputDevice controller) {
		input = controller;
		state = CharacterSelectState.Selecting;

		waiting.SetActive (false);
		selecting.SetActive (true);

		ufo.SetActive (true);
		UpdateSprite ();
	}

	public void RemovePlayer () {
		input = null;
		state = CharacterSelectState.Waiting;

		ufo.SetActive (false);
		selecting.SetActive (false);
		confirm.SetActive (false);
		waiting.SetActive (true);
	}

	void SwitchCharacter(bool right) {
		if (right) {
			++playerChoice;
			if (playerChoice >= playerOptions.Length)
				playerChoice = 0;
		} else {
			--playerChoice;
			if (playerChoice < 0)
				playerChoice = playerOptions.Length - 1;
		}
		switched = true;
		UpdateSprite ();
	}

	void UpdateSprite () {
		sprite.sprite = playerOptions [playerChoice].sprite;
	}

	public bool	CharacterSelected () {
		return state == CharacterSelectState.Confirmed;
	}
	public bool WaitingForPlayer () {
		return state == CharacterSelectState.Waiting;
	}

	public InputDevice GetDevice() {
		return input;
	}

	// Update is called once per frame
	void Update () {
		if (state == CharacterSelectState.Selecting) {
			if (!switched) {
				if (input.LeftStick.Left) {
					SwitchCharacter (false);
				} else if (input.LeftStick.Right) {
					SwitchCharacter (true);
				} 
			} else if (input.LeftStickX == 0) {
				switched = false;
			}

			if (input.Action1.WasPressed) {
				selecting.SetActive (false);
				confirm.SetActive (true);
				StartCoroutine ("RetractBeam");
			}
		} else if (state == CharacterSelectState.Confirmed) {
			if (input.Action2.WasPressed) {
				selecting.SetActive (true);
				confirm.SetActive (false);
				StartCoroutine ("ExtendBeam");
			}
		}
	}

	IEnumerator ExtendBeam () {
		SpriteRenderer domeSprite = ufo.transform.Find("dome").Find("sprite").GetComponent<SpriteRenderer>();
		domeSprite.sprite = null;
		Vector3 skinny = new Vector3 (1f, 1f, 1f);
		while (beam.transform.localScale.x < .99f) {
			beam.transform.localScale = Vector3.Lerp (beam.transform.localScale, skinny, 0.75f);
			yield return null;
		}
		beam.transform.localScale = Vector3.one;

		state = CharacterSelectState.Selecting;
		MenuController.GetMenuController ().ResetDeviceCharacter (input);
	}

	IEnumerator RetractBeam () {
		Vector3 skinny = new Vector3 (0f, 1f, 1f);
		while (beam.transform.localScale.x > .01f) {
			beam.transform.localScale = Vector3.Lerp (beam.transform.localScale, skinny, 0.75f);
			yield return null;
		}
		beam.transform.localScale = new Vector3 (0f, 1f, 1f);

		SpriteRenderer domeSprite = ufo.transform.Find("dome").Find("sprite").GetComponent<SpriteRenderer>();
		domeSprite.sprite = playerOptions[playerChoice].sprite;
		GameObject smoke = Instantiate(smokeEffect, domeSprite.transform.position, Quaternion.identity);
		smoke.GetComponent<LoopingAnimation>().StartAnimation();

		state = CharacterSelectState.Confirmed;
		MenuController.GetMenuController ().SetDeviceCharacter (input, playerOptions [playerChoice]);
	}
}
