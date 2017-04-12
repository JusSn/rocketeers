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
    public static void Shake (float duration, float magnitude, bool win_shake = false) {
		if (IsShaking ()) {
			return;
		}
		instance._originalPos = instance.gameObject.transform.localPosition;
		instance.StopAllCoroutines();
        instance.StartCoroutine (instance.cShake (duration, magnitude, win_shake));
	}

    // used for internal class purposes
    private IEnumerator cShake (float duration, float magnitude, bool win_shake) {
		shaking = true;
		while (duration > 0) {
            Vector3 offset = GetOffset (win_shake);
            Vector3 new_pos = offset + Random.insideUnitSphere * magnitude;
            new_pos.z = -10f;
            transform.localPosition = new_pos;

            duration -= Time.deltaTime;

			yield return null;
		}
		shaking = false;
        // if we are win shaking we don't want to return to the original position,
        // since this messes up the zooming effect
        if (!win_shake){
            transform.localPosition = _originalPos;
        }
	}

    Vector3 GetOffset(bool win_shake){
        // if we are win shaking we don't want to return to the original position,
        // since this messes up the zooming effect
        if (win_shake){
            return GetCurPosition ();
        } else {
            return _originalPos;
        }
    }

    Vector3 GetCurPosition(){
        return instance.gameObject.transform.position;
    }
}