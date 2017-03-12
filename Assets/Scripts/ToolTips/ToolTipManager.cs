using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToolTipManager : MonoBehaviour {

    // set these in the inspector
    public GameObject                   spend_pts_prefab;

    private GameObject                  player;
    private Vector3                     SPEND_PTS_OFFSET_BEGIN = Vector3.zero;
	// Use this for initialization
	void Start () {
        
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    // Sets the player gameObject to be of the corresponding
    // player. This can be used to put tool tips relative to the
    // players position.
    public void SetPlayer(GameObject player_go){
        player = player_go;
    }

    // instantiate the spendPoints tool tip object and let the TTSpendPoints script do the rest
    public void SpendPoints(int spend_amt){
        GameObject go = Instantiate<GameObject> (spend_pts_prefab, SPEND_PTS_OFFSET_BEGIN, Quaternion.identity);
        go.transform.parent = player.transform;
        go.transform.localPosition = SPEND_PTS_OFFSET_BEGIN;
        go.GetComponent<TTSpendPoints> ().SetPointAmt (spend_amt);
    }
}
