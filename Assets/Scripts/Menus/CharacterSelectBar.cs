using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InControl;

public enum CharacterSelectState {
	Waiting,
	Selecting,
	Selected
}

public class CharacterSelectBar : MonoBehaviour {	
	public CharacterSettings[]		playerOptions;

	private CharacterSelectState	state;
	private InputDevice				input;


	// Use this for initialization
	void Start () {
		state = CharacterSelectState.Waiting;
	}

	public void AssignPlayer (InputDevice controller) {
		input = controller;
		state = CharacterSelectState.Selecting;
	}

	public void RemovePlayer () {
		input = null;
		state = CharacterSelectState.Waiting;
	}

	// Update is called once per frame
	void Update () {
		if (state == CharacterSelectState.Selecting) {
			
		} else if (state == CharacterSelectState.Selected) {
			
		}
	}
}
