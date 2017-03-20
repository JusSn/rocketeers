using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToolTipManager : MonoBehaviour {

    // set these in the inspector
    public GameObject                   spend_pts_prefab;
    public Sprite[]                     spritesArray;
    public bool _______________________;

    public bool                         jumped = false;
    public bool                         downJumped = false;
    private GameObject                  playerObj;
    private Player                      playerScript;
    private Image                       tooltipImage;
    private Vector3                     SPEND_PTS_OFFSET_BEGIN = Vector3.up;
    private Vector3                     DISPLAY_PTS_OFFSET = Vector3.up;

    private LayerMask                   blockMask;

    private float                       fBLOCK_DETECT_RAD = 2f;


	// Use this for initialization
	void Start () {
        blockMask = LayerMask.GetMask ("Team1Block", "Team2Block");
	}
	
	// Update is called once per frame
	void Update () {
		// JF: Shows the prompt above the player to press the jump button 
        // When: player reaches any set block and has yet to jump once
        if (!jumped && isNearBlock ()) {
            tooltipImage.sprite = spritesArray[0]; //A button
            tooltipImage.enabled = true;
        }
        else if (!downJumped && playerScript.canDownJump) {
            tooltipImage.sprite = spritesArray[1]; //down A 
            tooltipImage.enabled = true;
        }
        else {
            tooltipImage.enabled = false;
        }

        // JF: Shows the prompt above the player to down jump
        // When: player first lands on a platform than can be down jumped

        // JF : Shows the prompt above the player to press the jetpack button
        // When: Player is grounded after jumping for the first time

        // JF: Removes the rocket button prompt
        // When: Player has used the jetpack for longer than 0.5s

        // JF: Shows the prompt above the player to press the fire button
        // When: Battle phase has begun 

        // JF: Removes the fire button prompt
        // When: Player has fired their weapon for the first time

	}

    // Sets the player gameObject to be of the corresponding
    // player. This can be used to put tool tips relative to the
    // players position.
    public void SetPlayer(GameObject player_go){
        playerObj = player_go;
        playerScript = playerObj.GetComponent<Player> ();

        // JF: Get tooltip object in player 
        GameObject ttImageObj = playerObj.transform
                                .Find("TooltipCanvas")
                                .Find("ButtonImage").gameObject; 
        tooltipImage = ttImageObj.GetComponent<Image> ();
        
        tooltipImage.enabled = false;
    }

    // instantiate the spendPoints tool tip object and let the TTSpendPoints script do the rest
    // Calling condition: When a player purchases an item, the points float upwards to signify the purchase
    // Called by: Player.TryToHoldItem()
    public void SpendPoints(int spend_amt){
        GameObject go = Instantiate<GameObject> (spend_pts_prefab, SPEND_PTS_OFFSET_BEGIN, Quaternion.identity);
        go.transform.parent = playerObj.transform;
        go.transform.localPosition = SPEND_PTS_OFFSET_BEGIN;
        go.GetComponent<TTSpendPoints> ().Purchase (spend_amt);
    }

    // shows the price of the item floating above the item itself
    // Calling condition: when a player is close to an object, player calls this function to
    //                    show the 
    // Called by: Player.NormalUpdate()
    public void DisplayPrice(GameObject item){
        GameObject go = Instantiate<GameObject> (spend_pts_prefab, DISPLAY_PTS_OFFSET, Quaternion.identity);
        go.transform.parent = item.transform;
        go.transform.localPosition = DISPLAY_PTS_OFFSET;
        go.GetComponent<TTSpendPoints> ().Display (item.GetComponent<Item>().GetCost());
    }

    private bool isNearBlock () {
        return Physics2D.OverlapCircle (transform.position, fBLOCK_DETECT_RAD, blockMask) != null;
    }
}
