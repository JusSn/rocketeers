using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InControl;

public enum CharacterSelectState {
	Waiting,
	Character,
	Team,
	Confirmed
}

public class CharacterSelectBar : MonoBehaviour {	
	public GameObject 				smokeEffect;
	public float 					ufoStartHeight = 2.75f;
	public float 					ufoBottomHeight = -2.75f;

	private GameObject 				ufo;
	private SpriteRenderer 			ufoSprend;
	private GameObject				beam;
	private SpriteRenderer			sprite;
	private GameObject 				waiting;
	private GameObject				selecting;
	private GameObject				team;
	private GameObject				confirm;

	private int 					playerChoice = 0;
	private CharacterSettings 		charSettings = null;
	private int						teamNum = 0;

	public CharacterSelectState		state;
	private InputDevice				input;
	private bool 					switched = false;
	private Dictionary<CharacterSelectState, Action>  stateUpdateMap;
	private bool 					animating = false;


	// Use this for initialization
	void Awake () {
		ufo = transform.Find ("UFO").gameObject;
		ufoSprend = ufo.GetComponent<SpriteRenderer> ();
		beam = ufo.transform.Find ("selection").gameObject;
		sprite = beam.transform.Find ("sprite").GetComponent<SpriteRenderer> ();
		waiting = transform.Find ("Waiting").gameObject;
		selecting = transform.Find ("Selecting").gameObject;
		team = transform.Find ("Team").gameObject;
		confirm = transform.Find ("Confirm").gameObject;

		ufo.SetActive (false);
		waiting.SetActive (true);
		selecting.SetActive (false);
		team.SetActive (false);
		confirm.SetActive (false);

		state = CharacterSelectState.Waiting;
		stateUpdateMap = new Dictionary<CharacterSelectState, Action> ();
		stateUpdateMap.Add(CharacterSelectState.Waiting, WaitingUpdate);
		stateUpdateMap.Add(CharacterSelectState.Character, CharacterUpdate);
		stateUpdateMap.Add (CharacterSelectState.Team, TeamUpdate);
		stateUpdateMap.Add(CharacterSelectState.Confirmed, ConfirmedUpdate);
	}

	// Update is called once per frame
	void Update () {
		if (!animating) {
			stateUpdateMap [state] ();
		}
	}

	void WaitingUpdate() {
		if (input != null) {
			if (input.Action1.WasPressed) {
				SFXManager.GetSFXManager ().PlaySFX (SFX.StartPilot);
				StartCoroutine ("ExtendBeam");
				state = CharacterSelectState.Character;

				waiting.SetActive (false);
				selecting.SetActive (true);

				ufo.SetActive (true);

				SwitchCharacter (true);
				UpdateSprite ();
			}
		}
	}

	void CharacterUpdate() {
		if (input.Action2.WasPressed) {
			state = CharacterSelectState.Waiting;
			charSettings = null;

			waiting.SetActive (true);
			selecting.SetActive (false);
			confirm.SetActive (false);
			ufo.SetActive (false);
			SFXManager.GetSFXManager ().PlaySFX (SFX.StopPilot);
		}

		if (input.Action1.WasPressed
			&& !MenuController.GetMenuController().CharacterIsSelected(charSettings.GetCharacterType())) {
			MenuController.GetMenuController ().SetCharacterSelect (charSettings.GetCharacterType (), true);
			selecting.SetActive (false);

			SpriteRenderer domeSprite = ufo.transform.Find("dome").Find("sprite").GetComponent<SpriteRenderer>();
			domeSprite.sprite = charSettings.GetSprite();
			GameObject smoke = Instantiate(smokeEffect, domeSprite.transform.position, Quaternion.identity);
			smoke.GetComponent<LoopingAnimation>().StartAnimation();
			SFXManager.GetSFXManager ().PlaySFX (SFX.StartPilot);
			StartCoroutine ("RetractBeam");

			ufoSprend.color = Color.red;
			teamNum = 2;
			state = CharacterSelectState.Team;
			team.SetActive (true);
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

	void TeamUpdate () {
		if (input.Action2.WasPressed) {
			MenuController.GetMenuController ().SetCharacterSelect (charSettings.GetCharacterType (), false);
			selecting.SetActive (true);
			team.SetActive (false);

			teamNum = 0;
			SFXManager.GetSFXManager ().PlaySFX (SFX.StopPilot);
			StartCoroutine ("Ascend");
			StartCoroutine ("ExtendBeam");
			ufoSprend.color = Color.white;
			state = CharacterSelectState.Character;
		}

		if (input.Action1.WasPressed) {
			team.SetActive (false);
			SFXManager.GetSFXManager ().PlaySFX (SFX.StartPilot);
			if (ufoSprend.color.r == 1f)
				teamNum = 2;
			else
				teamNum = 1;

			state = CharacterSelectState.Confirmed;
			confirm.SetActive (true);
			return;
		}

		if (!switched) {
			if (input.LeftStick.Up) {
				ufoSprend.color = Color.red;
				StartCoroutine ("Ascend");
				switched = true;
			} else if (input.LeftStick.Down) {
				ufoSprend.color = Color.blue;
				StartCoroutine ("Descend");
				switched = true;
			}
		} else if (input.LeftStickX == 0) {
			switched = false;
		}
	}

	void ConfirmedUpdate() {
		if (input.Action2.WasPressed) {
			team.SetActive (true);
			confirm.SetActive (false);
			SFXManager.GetSFXManager ().PlaySFX (SFX.StopPilot);
			state = CharacterSelectState.Team;
		}
	}

	/**************** Public Interface ****************/

	public void ResetPlayerSelect () {
		input = null;
		state = CharacterSelectState.Waiting;
		charSettings = null;
		animating = false;
		switched = false;
		teamNum = 0;
		ufoSprend.color = Color.white;
		ufo.transform.localPosition = new Vector3 (0f, ufoStartHeight, ufo.transform.localPosition.z);

		ufo.SetActive (false);
		selecting.SetActive (false);
		team.SetActive (false);
		confirm.SetActive (false);
		waiting.SetActive (true);
	}

	// Assumes ResetPlayerSelect as been called
	public void AssignPlayer (InputDevice controller) {
		input = controller;
	}

	void UpdateSprite () {
		sprite.sprite = charSettings.GetSprite ();
	}

	public CharacterSettings GetSelectedCharacter () {
		return charSettings;
	}

	public bool WaitingForPlayer () {
		return state == CharacterSelectState.Waiting;
	}

	public bool IsReady () {
		return state == CharacterSelectState.Confirmed;
	}

	public int GetSelectedTeam () {
		return teamNum;
	}

	public InputDevice GetDevice() {
		return input;
	}

	/**************** Internal Interface ****************/

	void SwitchCharacter(bool right) {
		CharacterSettings sets = null;
		if (right) {
			do {
				++playerChoice;
				if (playerChoice >= MenuController.GetMenuController().characters.Length)
					playerChoice = 0;
				sets = MenuController.GetMenuController().characters[playerChoice];
			} while(MenuController.GetMenuController().CharacterIsSelected(sets.GetCharacterType()));
		} else {
			do {
				--playerChoice;
				if (playerChoice < 0)
					playerChoice = MenuController.GetMenuController().characters.Length - 1;
				sets = MenuController.GetMenuController().characters[playerChoice];
			} while (MenuController.GetMenuController().CharacterIsSelected(sets.GetCharacterType()));
		}
		switched = true;
		charSettings = sets;
		UpdateSprite ();
	}

	IEnumerator Descend () {
		animating = true;

		Vector3 pos = new Vector3 (ufo.transform.position.x, ufoBottomHeight, ufo.transform.position.z);
		while (ufo.transform.position.y > ufoBottomHeight + 0.01f) {
			ufo.transform.position = Vector3.Lerp (ufo.transform.position, pos, 0.5f);
			yield return null;
		}
		ufo.transform.position = pos;

		animating = false;
	}

	IEnumerator Ascend () {
		animating = true;

		Vector3 pos = new Vector3 (ufo.transform.position.x, ufoStartHeight, ufo.transform.position.z);
		while (ufo.transform.position.y < ufoStartHeight - 0.01f) {
			ufo.transform.position = Vector3.Lerp (ufo.transform.position, pos, 0.5f);
			yield return null;
		}
		ufo.transform.position = pos;

		animating = false;
	}

	IEnumerator ExtendBeam () {
		animating = true;
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
		animating = false;
	}

	IEnumerator RetractBeam () {
		animating = true;
		Vector3 skinny = new Vector3 (0f, 1f, 1f);
		while (beam.transform.localScale.x > .01f) {
			beam.transform.localScale = Vector3.Lerp (beam.transform.localScale, skinny, 0.5f);
			yield return null;
		}
		beam.SetActive (false);
		beam.transform.localScale = new Vector3 (1f, 1f, 1f);
		animating = false;
	}
}
