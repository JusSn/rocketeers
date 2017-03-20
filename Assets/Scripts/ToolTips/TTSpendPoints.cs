using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum TTSpendPointsStates {
    DISPLAY,
    BOUGHT,
}

public class TTSpendPoints : MonoBehaviour {


    public Text                  ui_spend_amt;
    public Image                 buttonImage;

    private Vector3              SPEND_PTS_OFFSET_END = Vector3.up * 1.5f;
    private float                STEP_AMT = 10f;
    private Quaternion           original_rotation;
    private Dictionary<TTSpendPointsStates, Action> states = new Dictionary<TTSpendPointsStates, Action> ();
    private TTSpendPointsStates state;

    void Start () {
        original_rotation = transform.rotation;
        states.Add (TTSpendPointsStates.DISPLAY, DisplayState);
        states.Add (TTSpendPointsStates.BOUGHT, BoughtState);
    }
	
	void Update () {
        states [state] ();

        // used to prevent the text from flipping when the player changes direction
        transform.rotation = original_rotation;
    }

    // When an item is purchased, the cost of the item is floated upwards as a way to
    // show the item was 'purchased'
    void BoughtState(){
        float step = Time.deltaTime * STEP_AMT;
        transform.localPosition = Vector3.Lerp (transform.localPosition, SPEND_PTS_OFFSET_END, step);
        if (transform.localPosition == SPEND_PTS_OFFSET_END) {
            Destroy (gameObject);
        }
    }

    // Destroys the cost of the item that is floating above a block
    void DisplayState(){
        Destroy (gameObject);
    }

    // Called by: ToolTipManager.DisplayPrice()
    // Causes Item to transition to DisplayState()
    public void Display(int spend_amt){
        ui_spend_amt.text = spend_amt.ToString ();
        state = TTSpendPointsStates.DISPLAY;
    }

    // Called by: ToolTipManager.SpendPoints()
    // Causes Item to transition to BoughtState()
    public void Purchase(int spend_amt){
        ui_spend_amt.text = spend_amt.ToString ();
        // JF: Disable button ToolTip
        buttonImage.enabled = false;
        state = TTSpendPointsStates.BOUGHT;
    }
}
