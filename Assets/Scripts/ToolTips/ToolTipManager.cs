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
    public bool                         doubleJumped = false;
    public bool                         jetpacked = false;
    public bool                         setted = false;
    public bool                         fired = false;
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
        // JF: Player-centric tool tips
        if (playerScript != null) {
            // Shows the prompt above the player to place a block
            // When: Player is holding settable block item and has not set before
            if (!setted && playerScript.form == PlayerForm.Setting) {
                tooltipImage.sprite = spritesArray[3]; //Custom set/cancel image
                tooltipImage.enabled = true;
            }
            // Shows the prompt above the player to press the jump button 
            // When: player reaches any set block and has yet to jump once
            else if (!jumped && isNearBlock ()) {
                tooltipImage.sprite = spritesArray[0]; //A button
                tooltipImage.enabled = true;
            }
            // Shows the prompt above the player to down jump
            // When: player first lands on a platform than can be down jumped
            else if (!downJumped && playerScript.canDownJump) {
                tooltipImage.sprite = spritesArray[1]; //down A 
                tooltipImage.enabled = true;
            }
            // Shows the prompt above the player to prompt a double jump
            // When: Player is airborne and has not double jumped
            else if (!doubleJumped && !playerScript.grounded) {
                tooltipImage.sprite = spritesArray[0]; //A button
                tooltipImage.enabled = true;
            }
            // Shows the prompt above the player to press the jetpack button
            // When: Player has doubleJumped
            // Note: Will be disabled after player depletes fuel to certain amount
            else if (!jetpacked && doubleJumped) {
                tooltipImage.sprite = spritesArray[2]; //Left trigger
                tooltipImage.enabled = true;
            }
            // Shows the prompt above the player to press the fire button
            // When: Battle phase has begun 
            else if (!fired && !PhaseManager.S.inBuildPhase) {
                tooltipImage.sprite = spritesArray[4]; //Right trigger
                tooltipImage.enabled = true;
            }
            else {
                tooltipImage.enabled = false;
            }
        }
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
