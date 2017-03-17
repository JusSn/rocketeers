using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine;

public class MenuController : MonoBehaviour {

	public void StartButton(){
		SceneManager.LoadScene ("main");
	}

	public void ExitButton(){
		Application.Quit ();
	}
}
