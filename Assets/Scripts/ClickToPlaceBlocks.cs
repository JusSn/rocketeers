using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickToPlaceBlocks : MonoBehaviour {

    public GameObject blockPrefab;
    GameObject created_block;
    bool holding_block;

	// Use this for initialization
	void Start () {
        holding_block = false;
	}
	
	// Update is called once per frame
	void Update () {
		if(!holding_block && Input.GetMouseButtonDown(0)) {
            created_block = Instantiate(blockPrefab);
            created_block.layer = LayerMask.NameToLayer("Items");
            holding_block = true;

            Vector3 block_pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            block_pos.z = 0;
            created_block.transform.position = block_pos;
        }
        if(Input.GetMouseButton(0)) {
            created_block.GetComponent<Block>().being_manipulated = true;
        }
        if(Input.GetMouseButtonUp(0)) {
            if (created_block.GetComponent<Block>().in_placeable_spot) {
                created_block.GetComponent<Block>().being_manipulated = false;
                holding_block = false;
                created_block.GetComponent<Block>().ConnectBlock();
            }
            else {

            }
        }
	}
}
