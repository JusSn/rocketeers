using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocationArrow : MonoBehaviour {

    public GameObject           arrowPrefab;

    GameObject                  arrow;
    private float               Y_OFFSET = 1.2f;
	private float				SCALE_RATIO = 8f;


	// Use this for initialization
	void Start () {
		arrow = Instantiate(arrowPrefab, transform);
		Vector3 pos = transform.position;
		pos.y += Y_OFFSET;
		arrow.transform.position = pos;

		HideIndicator ();
	}
	
	// Update is called once per frame
	void Update () {
		if (!PhaseManager.S.inBuildPhase) {
			if (!MainCamera.S.IsOnScreen (transform.position)) {
				HideIndicator ();
			} else {
				ShowIndicator ();

				// Calculate Scale
				Vector3 arrow_scale = arrow.transform.localScale;
				float scale = MainCamera.S.GetComponent<Camera> ().orthographicSize / SCALE_RATIO;
				arrow_scale.x = arrow_scale.y = scale;
				arrow.transform.localScale = arrow_scale;
			}
		}
	}

    void HideIndicator(){
        if (arrow)
            arrow.SetActive (false);
    }

    void ShowIndicator(){
        if (arrow)
            arrow.SetActive (true);
    }
}
