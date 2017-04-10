using UnityEngine;
using System.Collections;

// CG: screen shake courtesy of: https://gist.github.com/ftvs/5822103#gistcomment-1844085
public class CameraShake : MonoBehaviour {

	public static CameraShake instance;

    private Vector3 _originalPos;
	private bool shaking = false;

	void Awake()
	{
		instance = this;
	}

	public static bool IsShaking(){
		return instance.shaking;
	}

    // CG: Shake the screen for a specific duration and a certain magnitude
	public static void Shake (float duration, float magnitude) {
		if (IsShaking ()) {
			return;
		}
		instance._originalPos = instance.gameObject.transform.localPosition;
		instance.StopAllCoroutines();
		instance.StartCoroutine(instance.cShake(duration, magnitude));
	}

    // used for internal class purposes
	private IEnumerator cShake (float duration, float magnitude) {
		shaking = true;
		while (duration > 0) {
			transform.localPosition = _originalPos + Random.insideUnitSphere * magnitude;

            duration -= Time.deltaTime;

			yield return null;
		}
		shaking = false;
		transform.localPosition = _originalPos;
	}
}