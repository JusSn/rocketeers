using System;
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
	public GameObject 				smokeEffect;

	private GameObject 				ufo;
	private GameObject				beam;
	private SpriteRenderer			sprite;
	private GameObject 				waiting;
	private GameObject				selecting;
	private GameObject				confirm;

	private int 					playerChoice = 0;
	public CharacterSelectState		state;
	private InputDevice				input;
	public bool 					switched = false;
	private Dictionary<CharacterSelectState, Action>  stateUpdateMap;
	private bool 					beaming = false;


	// Use this for initialization
	void Awake () {
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
		stateUpdateMap = new Dictionary<CharacterSelectState, Action> ();
		stateUpdateMap.Add(CharacterSelectState.Waiting, WaitingUpdate);
		stateUpdateMap.Add(CharacterSelectState.Selecting, SelectingUpdate);
		stateUpdateMap.Add(CharacterSelectState.Confirmed, ConfirmedUpdate);
	}

	// Update is called once per frame
	void Update () {
		if (!beaming) {
			stateUpdateMap [state] ();
		}
	}

	void WaitingUpdate() {
		if (input != null) {
			if (input.Action1.WasPressed) {
				state = CharacterSelectState.Selecting;
				SFXManager.GetSFXManager ().PlaySFX (SFX.StartPilot);
				StartCoroutine ("ExtendBeam");

				waiting.SetActive (false);
				selecting.SetActive (true);

				ufo.SetActive (true);
				UpdateSprite ();
			}
		}
	}

	void SelectingUpdate() {
		if (input.Action2.WasPressed) {
			state = CharacterSelectState.Waiting;
			waiting.SetActive (true);
			selecting.SetActive (false);
			confirm.SetActive (false);
			ufo.SetActive (false);
			SFXManager.GetSFXManager ().PlaySFX (SFX.StopPilot);
		}

		if (input.Action1.WasPressed
			&& !MenuController.GetMenuController ().characters [playerChoice].selected) {
			selecting.SetActive (false);
			confirm.SetActive (true);
			SFXManager.GetSFXManager ().PlaySFX (SFX.StartPilot);
			MenuController.GetMenuController ().characters [playerChoice].selected = true;

			SpriteRenderer domeSprite = ufo.transform.Find("dome").Find("sprite").GetComponent<SpriteRenderer>();
			domeSprite.sprite = MenuController.GetMenuController().characters[playerChoice].sprite;
			GameObject smoke = Instantiate(smokeEffect, domeSprite.transform.position, Quaternion.identity);
			smoke.GetComponent<LoopingAnimation>().StartAnimation();
			state = CharacterSelectState.Confirmed;
			StartCoroutine ("RetractBeam");
			return;
		}

		if (!switched) {
			if (input.LeftStick.Left) {
				SwitchCharacter (false);
			} else if (input.LeftStick.Right) {
				SwitchCharacter (true);
			} 
		} else if (input.LeftStickX == 0) {
			switched = false;
		}
	}

	void ConfirmedUpdate() {
		if (input.Action2.WasPressed) {
			selecting.SetActive (true);
			confirm.SetActive (false);
			SFXManager.GetSFXManager ().PlaySFX (SFX.StopPilot);
			MenuController.GetMenuController ().characters [playerChoice].selected = false;
			state = CharacterSelectState.Selecting;
			StartCoroutine ("ExtendBeam");
		}
	}

	/**************** Public Interface ****************/

	public void ResetPlayerSelect () {
		input = null;
		state = CharacterSelectState.Waiting;
		beaming = false;

		ufo.SetActive (false);
		selecting.SetActive (false);
		confirm.SetActive (false);
		waiting.SetActive (true);
	}

	// Assumes ResetPlayerSelect as been called
	public void AssignPlayer (InputDevice controller) {
		input = controller;
	}

	void UpdateSprite () {
		sprite.sprite = MenuController.GetMenuController().characters [playerChoice].sprite;
	}

	public CharacterSettings GetSelectedCharacter () {
		if (state == CharacterSelectState.Confirmed)
			return MenuController.GetMenuController ().characters [playerChoice];
		else
			return null;
	}

	public bool WaitingForPlayer () {
		return state == CharacterSelectState.Waiting;
	}

	public InputDevice GetDevice() {
		return input;
	}

	/**************** Internal Interface ****************/

	void SwitchCharacter(bool right) {
		if (right) {
			do {
				++playerChoice;
				if (playerChoice >= MenuController.GetMenuController().characters.Length)
					playerChoice = 0;
			} while(MenuController.GetMenuController().characters[playerChoice].selected);
		} else {
			do {
				--playerChoice;
				if (playerChoice < 0)
					playerChoice = MenuController.GetMenuController().characters.Length - 1;

			} while (MenuController.GetMenuController().characters[playerChoice].selected);
		}
		switched = true;
		UpdateSprite ();
	}

	IEnumerator ExtendBeam () {
		beaming = true;
		SpriteRenderer domeSprite = ufo.transform.Find("dome").Find("sprite").GetComponent<SpriteRenderer>();
		domeSprite.sprite = null;

		Vector3 scale = beam.transform.localScale;
		scale.x = 0f;
		beam.transform.localScale = scale;
		beam.SetActive (true);

		while (beam.transform.localScale.x < .99f) {
			beam.transform.localScale = Vector3.Lerp (beam.transform.localScale, Vector3.one, 0.5f);
			yield return null;
		}
		beam.transform.localScale = Vector3.one;
		beaming = false;
	}

	IEnumerator RetractBeam () {
		beaming = true;
		Vector3 skinny = new Vector3 (0f, 1f, 1f);
		while (beam.transform.localScale.x > .01f) {
			beam.transform.localScale = Vector3.Lerp (beam.transform.localScale, skinny, 0.5f);
			yield return null;
		}
		beam.SetActive (false);
		beam.transform.localScale = new Vector3 (1f, 1f, 1f);
		beaming = false;
	}
}
