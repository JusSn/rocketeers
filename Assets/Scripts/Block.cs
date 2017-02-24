using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Direction {
    NORTH,
    SOUTH,
    EAST,
    WEST
}

public class Block : MonoBehaviour {

    public bool being_manipulated;
    public bool in_placeable_spot = false;
    public LayerMask mask;
    public float snap_radius = 0.75f;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if(being_manipulated) {
            Vector3 block_pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            block_pos.z = 0;
            transform.position = block_pos;
            GetComponent<Rigidbody2D>().velocity = Vector3.zero;

            // gets array of gameobjects within a circle of radius 1
            // nearby_blocks[0] is this block so we ignore it
            Collider2D[] nearby_blocks = Physics2D.OverlapCircleAll(transform.position, snap_radius, mask);
            for (int i = 0; i < nearby_blocks.Length && !in_placeable_spot; i++) {
                if (nearby_blocks[i].tag == "Block") {
                    in_placeable_spot = true;
                }
            }
        }
	}

    // Connects this block to closest edge via Fixed Joint 2D
    public void ConnectBlock() {
        GameObject connected_block = null;
        float closest_block = Mathf.Infinity;
        Collider2D[] nearby_blocks = Physics2D.OverlapCircleAll(transform.position, snap_radius + 0.25f, mask);
        for (int i = 0; i < nearby_blocks.Length; i++) {
            if (nearby_blocks[i].tag == "Block") {
                if(Vector3.Distance(transform.position, nearby_blocks[i].transform.position) < closest_block) {
                    closest_block = Vector3.Distance(transform.position, nearby_blocks[i].transform.position);
                    connected_block = nearby_blocks[i].gameObject;
                }
            }
        }
        if(connected_block == null) {
            return;
        }
        Direction direction = GetRelativeSide(connected_block.transform.position);
        print(direction);
        transform.position = connected_block.transform.position;
        switch(direction) {
            case Direction.NORTH:
                transform.position += new Vector3(0, -1f);
                break;
            case Direction.SOUTH:
                transform.position += new Vector3(0, 1f);
                break;
            case Direction.EAST:
                transform.position += new Vector3(-1f, 0);
                break;
            case Direction.WEST:
                transform.position += new Vector3(1f, 0);
                break;
        }

        FixedJoint2D joint = gameObject.AddComponent<FixedJoint2D>();
        joint.connectedBody = connected_block.GetComponent<Rigidbody2D>();
        joint = connected_block.AddComponent<FixedJoint2D>();
        joint.connectedBody = gameObject.GetComponent<Rigidbody2D>();


        // Connect the rest of the neighbor blocks
        Collider2D[] neighbors;
        Vector2 overlap_area_topleft = new Vector2(transform.position.x - 0.75f, transform.position.y + 0.25f);
        Vector2 overlap_area_bottomright = new Vector2(transform.position.x + 0.75f, transform.position.y - 0.25f);

        // first change the recently attached block layer to ignore it
        connected_block.layer = LayerMask.NameToLayer("Items");
        // get left and right neighbors by making a short and wide overlap area
        Collider2D[] horizontal_neighbors = Physics2D.OverlapAreaAll(overlap_area_topleft, overlap_area_bottomright, mask);
        overlap_area_topleft += new Vector2(0.5f, 0.5f);
        overlap_area_bottomright -= new Vector2(0.5f, 0.5f);
        // get top and bottom neighbors by making a tall and narrow overlap area
        Collider2D[] vertical_neighbors = Physics2D.OverlapAreaAll(overlap_area_topleft, overlap_area_bottomright, mask);
        // combine these arrays
        neighbors = new Collider2D[horizontal_neighbors.Length + vertical_neighbors.Length];
        horizontal_neighbors.CopyTo(neighbors, 0);
        vertical_neighbors.CopyTo(neighbors, horizontal_neighbors.Length);
        // for each neighbor, create joint on new block and neighbor block
        for (int i = 0; i < neighbors.Length; i++) {
            joint = gameObject.AddComponent<FixedJoint2D>();
            joint.connectedBody = neighbors[i].GetComponent<Rigidbody2D>();
            joint = neighbors[i].gameObject.AddComponent<FixedJoint2D>();
            joint.connectedBody = gameObject.GetComponent<Rigidbody2D>();
        }

        // reset block layers
        connected_block.layer = LayerMask.NameToLayer("Blocks");
        gameObject.layer = LayerMask.NameToLayer("Blocks");
    }

    // Returns which side of the Block that the neighbor is on
    // (If the neighbor is to the left, it will return WEST, etc)
    public Direction GetRelativeSide(Vector3 neighbor_pos) {
        float angle = Vector2.Angle(Vector2.right, neighbor_pos - gameObject.transform.position);
        
        if (angle >= 315 || angle <= 45) {
            return Direction.EAST;
        }
        else if (angle > 45 && angle <= 135) {
            if (neighbor_pos.y < transform.position.y) {
                return Direction.SOUTH;
            }
            return Direction.NORTH;
        }
        else {
            return Direction.WEST;
        }
    }
}
