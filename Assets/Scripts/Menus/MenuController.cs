using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine;

public class MenuController : MonoBehaviour {

	public AutoBackgroundScroller		bg;

	private GameObject					homeScreen;
	private GameObject					charSelectScreen;
	private GameObject					teamSelectScreen;
	private GameObject 					settingsScreen;

	private SwitchSprites[]				checkBoxes;
	private GameObject[]				players;

	void Start() {
		homeScreen = transform.Find ("Home").gameObject;
		charSelectScreen = transform.Find ("CharacterSelect").gameObject;
		teamSelectScreen = transform.Find ("TeamSelect").gameObject;
		settingsScreen = transform.Find ("Settings").gameObject;

		InitHomeScreen ();
	}

	/******************** Switch Screen Functions ********************/

	public void InitHomeScreen () {
		homeScreen.SetActive (true);
	}

	/******************** Button Functions ********************/

	public void StartButton(){
		SceneManager.LoadScene ("main");
	}

	public void HowToPlayButton(){
		SceneManager.LoadScene ("tutorial");
	}

	public void ExitButton(){
		Application.Quit ();
	}
}
		