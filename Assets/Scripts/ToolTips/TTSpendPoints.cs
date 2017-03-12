using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TTSpendPoints : MonoBehaviour {


    public Text                  ui_spend_amt;

    private Vector3              SPEND_PTS_OFFSET_END = Vector3.up;
    private float                STEP_AMT = 10f;
    private Quaternion           original_rotation;

	// Use this for initialization
    void Start () {
        original_rotation = transform.rotation;
    }
	
	// Update is called once per frame
	void Update () {
        float step = Time.deltaTime * STEP_AMT;
        transform.localPosition = Vector3.Lerp (transform.localPosition, SPEND_PTS_OFFSET_END, step);
        if (transform.localPosition == SPEND_PTS_OFFSET_END) {
            Destroy (gameObject);
        }
        // used to prevent the text from flipping when the player changes direction
        transform.rotation = original_rotation;
    }

    public void SetPointAmt(int spend_amt){
        ui_spend_amt.text = spend_amt.ToString ();
    }
}
