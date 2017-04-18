using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MiddleBanner : MonoBehaviour {
	private static MiddleBanner singleton;

	public GameObject		 	bannerObj;

	private static bool			animating;
	private static GameObject	activeBanner = null;

	void Awake () {
		singleton = this;
	}
		
	public static MiddleBanner GetBanner() {
		return singleton;
	}

	// Create persisting banner
	public void CreateBanner (string text) {
		if (!activeBanner) {
			activeBanner = Instantiate<GameObject> (bannerObj, Vector3.zero, Quaternion.identity);
			activeBanner.transform.Find ("Canvas").Find ("Text").GetComponent<Text>().text = text;
			StartCoroutine ("ExtendBanner", -1f);
		}
	}

	// Create banner that disappears after a duration of time
	public void CreateBanner (string text, float duration) {
		if (!activeBanner) {
			activeBanner = Instantiate<GameObject> (bannerObj, Vector3.zero, Quaternion.identity);
			activeBanner.transform.Find ("Canvas").Find ("Text").GetComponent<Text>().text = text;
			StartCoroutine ("ExtendBanner", duration);
		}
	}

	// Set position of permanent banner
	public void CreateBanner (string text, Vector3 pos) {
		if (!activeBanner) {
			activeBanner = Instantiate<GameObject> (bannerObj, pos, Quaternion.identity);
			activeBanner.transform.Find ("Canvas").Find ("Text").GetComponent<Text>().text = text;
			StartCoroutine ("ExtendBanner", -1f);
		}
	}

	// Set position of temporary banner
	public void CreateBanner (string text, float duration, Vector3 pos) {
		if (!activeBanner) {
			activeBanner = Instantiate<GameObject> (bannerObj, pos, Quaternion.identity);
			activeBanner.transform.Find ("Canvas").Find ("Text").GetComponent<Text>().text = text;
			StartCoroutine ("ExtendBanner", duration);
		}
	}

	public void ChangeText (string text) {
		if(activeBanner && !animating) {
			activeBanner.transform.Find ("Canvas").Find ("Text").GetComponent<Text>().text = text;
		}
	}

	public void DestroyBanner () {
		if (!animating && activeBanner) {
			StartCoroutine ("RetractBanner");
		}
	}

	IEnumerator ExtendBanner (float duration = -1f) {
		animating = true;

		Vector3 skinny = new Vector3 (1f, 0f, 1f);
		activeBanner.transform.localScale = skinny;
		while (activeBanner.transform.localScale.y < .99f) {
			activeBanner.transform.localScale = Vector3.Lerp (activeBanner.transform.localScale, Vector3.one, 0.50f);
			yield return null;
		}
		activeBanner.transform.localScale = Vector3.one;
		animating = false;

		if (duration >= 0) {
			Invoke ("DestroyBanner", duration);
		}
	}

	IEnumerator RetractBanner () {
		animating = true;
		Vector3 skinny = new Vector3 (1f, 0f, 1f);
		activeBanner.transform.localScale = Vector3.one;
		while (activeBanner.transform.localScale.y > .01f) {
			activeBanner.transform.localScale = Vector3.Lerp (activeBanner.transform.localScale, skinny, 0.50f);
			yield return null;
		}
		Destroy (activeBanner);
		activeBanner = null;
		animating = false;
	}
}
