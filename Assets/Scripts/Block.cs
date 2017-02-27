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
    // Inspector manipulated attributes
    public LayerMask mask;
    public float snap_radius = 0.75f;
    public bool ______________________;
    // Encapsulated attributes
    public bool being_manipulated;
    public bool in_placeable_spot = false;
    public bool block_fell = false;

    // Neighbor joints
    public Dictionary<Direction, FixedJoint2D> connected_neighbors;

    // Use this for initialization
    void Start () {
        connected_neighbors = new Dictionary<Direction, FixedJoint2D>();
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
            in_placeable_spot = false;
            for (int i = 0; i < nearby_blocks.Length && !in_placeable_spot; i++) {
                if (nearby_blocks[i].tag == "Block") {
                    in_placeable_spot = true;
                }
            }
        }

        if(!being_manipulated && GetComponent<Rigidbody2D>().velocity.magnitude >= 1f) {
            block_fell = true;
        }

        if(block_fell && GetComponent<Rigidbody2D>().velocity.magnitude <= 0.05f) {
            // Jump block to closest half position
            //transform.position = new Vector3(Mathf.Round(transform.position.x * 2f) * 0.5f, Mathf.Round(transform.position.y * 2f) * 0.5f);

            // Check each neighbor to see if one exists
            // If not, add it as a joint and add it to the dictionary
            if(!connected_neighbors.ContainsKey(Direction.NORTH)) {
                ConnectSingleNeighbor(Direction.NORTH);
            }
            if (!connected_neighbors.ContainsKey(Direction.SOUTH)) {
                ConnectSingleNeighbor(Direction.SOUTH);
            }
            if (!connected_neighbors.ContainsKey(Direction.WEST)) {
                ConnectSingleNeighbor(Direction.WEST);
            }
            if (!connected_neighbors.ContainsKey(Direction.EAST)) {
                ConnectSingleNeighbor(Direction.EAST);
            }

            block_fell = false;
        }

        // Check if it has lost any connections to neighbors
        FixedJoint2D neighbor_joint;
        if (connected_neighbors.TryGetValue(Direction.NORTH, out neighbor_joint)) {
            if (neighbor_joint.connectedBody == null) {
                Destroy(neighbor_joint);
                connected_neighbors.Remove(Direction.NORTH);
            }
        }
        if (connected_neighbors.TryGetValue(Direction.SOUTH, out neighbor_joint)) {
            if (neighbor_joint.connectedBody == null) {
                Destroy(neighbor_joint);
                connected_neighbors.Remove(Direction.SOUTH);
            }
        }
        if (connected_neighbors.TryGetValue(Direction.EAST, out neighbor_joint)) {
            if (neighbor_joint.connectedBody == null) {
                Destroy(neighbor_joint);
                connected_neighbors.Remove(Direction.EAST);
            }
        }
        if (connected_neighbors.TryGetValue(Direction.WEST, out neighbor_joint)) {
            if (neighbor_joint.connectedBody == null) {
                Destroy(neighbor_joint);
                connected_neighbors.Remove(Direction.WEST);
            }
        }
    }

    // Connects this block to closest edge via Fixed Joint 2D
    // Also connects neighbors. Only used when first placing block
    // (not for blocks falling)
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
        //print(direction);
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

        ConnectSingleNeighbor(Direction.NORTH);
        ConnectSingleNeighbor(Direction.SOUTH);
        ConnectSingleNeighbor(Direction.EAST);
        ConnectSingleNeighbor(Direction.WEST);

        // reset block layer
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

    // Connects a single neighbor if a block exists in that direction
    public void ConnectSingleNeighbor(Direction dir) {
        Vector2 pt;
        switch (dir) {
            case Direction.NORTH:
                pt = transform.position + new Vector3(0, 1);
                break;
            case Direction.SOUTH:
                pt = transform.position + new Vector3(0, -1);
                break;
            case Direction.EAST:
                pt = transform.position + new Vector3(1, 0);
                break;
            default:
                pt = transform.position + new Vector3(-1, 0);
                break;
        }

        Collider2D neighbor = Physics2D.OverlapPoint(pt, mask);
        if (neighbor != null) {
            FixedJoint2D joint = gameObject.AddComponent<FixedJoint2D>();
            joint.connectedBody = neighbor.GetComponent<Rigidbody2D>();
            connected_neighbors[dir] = joint;

            joint = neighbor.gameObject.AddComponent<FixedJoint2D>();
            joint.connectedBody = gameObject.GetComponent<Rigidbody2D>();
            neighbor.GetComponent<Block>().connected_neighbors[GetOppositeDirection(dir)] = joint;
        }
    }

    // Returns the opposite direction
    // North -> South, East -> West
    public Direction GetOppositeDirection(Direction dir) {
        switch(dir) {
            case Direction.NORTH:
                return Direction.SOUTH;
            case Direction.SOUTH:
                return Direction.NORTH;
            case Direction.EAST:
                return Direction.WEST;
            case Direction.WEST:
                return Direction.EAST;
        }

        Debug.Log("Something went wrong with GetOppositeDirection");
        return Direction.NORTH;
    }
}
