using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Direction {
    NORTH,
    SOUTH,
    EAST,
    WEST
}

public enum BlockStates {
    FALLING,
    FALLING_TO_STILL,
    STILL
}

public class Block : MonoBehaviour {
    // Inspector manipulated attributes
    public LayerMask                            mask;
    public float                                snap_radius = 0.75f;
    public bool                                 ______________________;
    // Encapsulated attributes
    public bool                                 being_manipulated;
    public bool                                 in_placeable_spot = false;
    public bool                                 block_fell = false;
    protected Health                            health;

    // GameObject components & child objects
    private Rigidbody2D                         rigid;

    // Neighbor joints
    public Dictionary<Direction, Block>         connected_neighbors = new Dictionary<Direction, Block>();

    // Block states
    public Dictionary<BlockStates, Action>      states = new Dictionary<BlockStates, Action>();
    public BlockStates                          state = BlockStates.FALLING;

    private float                               SLEEPING_THRESHOLD = 0.1f;


    // Use this for initialization
    void Start () {
        rigid = GetComponent<Rigidbody2D> ();
        health = GetComponent<Health> ();
        health.SetParent (this);
        states.Add (BlockStates.FALLING, Falling);
        states.Add (BlockStates.FALLING_TO_STILL, FallingToStill);
        states.Add (BlockStates.STILL, Still);
    }

    protected virtual void Update(){
        // run the correct state function each update
        states [state] ();
    }

    /******************** State Modifiers & Behaviors ********************/

    // Block is in this state if it is falling
    // Entered from: Block creation or surrounding block destruction
    // Exits to: FallingToStill()
    void Falling(){

        // check if the block has come to a rest
        if (rigid.velocity.magnitude <= SLEEPING_THRESHOLD) {
            state = BlockStates.FALLING_TO_STILL;
            return;
        }

        // block is still falling, any logic that needs to be performed can be put here.
        // At the moment, I can't think of anything the block needs to do when it is falling though
    }

    // Once the block has come to a rest, this function is called once
    // Entered from: Falling()
    // Exits to: Still() 
    void FallingToStill(){

        // check if there are blocks that can be attached to, this should automatically be called
        // on all the blocks in a clump that fell together, so no need to trigger any other
        // blocks than yourself

        // create a full set of directions, that we will "subtract" from as we go through our
        // direction map
        HashSet<Direction> dir_set = new HashSet<Direction>{ Direction.NORTH, Direction.SOUTH,
                                                             Direction.EAST, Direction.NORTH };
        // remove each direction that is already in our Direction Map
        foreach(KeyValuePair<Direction, Block> dir in connected_neighbors){
            if (dir_set.Contains (dir.Key)) {
                dir_set.Remove (dir.Key);
            }
        }

        // now our original dir_set is left with only directions that we do not already
        // have connections to, so we know blocks weren't previously there, but now there might
        // be blocks there, so we check.
        foreach (Direction dir in dir_set) {
            Block neighbor = null;
            if (CheckForNeighbor (dir, out neighbor)) {
                // we have a neighbor, so connect to it
                ConnectToNeighbor (dir, neighbor);
                // connect the neighbor in the opposite direction, since that's the side
                // this block is on
                neighbor.ConnectToNeighbor(Utils.GetOppositeDirection(dir), this);
            }
        }

        state = BlockStates.STILL;
    }

    // When a block is not moving, or not just previously moving
    // Entered from: FallingToStill()
    // Exits to: Falling(), OnDestroy() (indirectly)
    void Still(){

        // the block is moving again, so transition to the falling state
        if (rigid.velocity.magnitude > SLEEPING_THRESHOLD) {
            state = BlockStates.FALLING;
            return;
        }

    }

    // Calling condition: Whenever the gameObject is destroyed
    //                    (probably when the block's health is <= 0)
    protected virtual void OnDestroy(){
        // for each neighbor around us
        foreach (KeyValuePair<Direction, Block> dir in connected_neighbors) {
            // remove ourselves from our neighbors... RIP us :'(
            // since we're removing ourself from our neighbors, the directions
            // are reversed
            dir.Value.DeleteNeighboringConnection (Utils.GetOppositeDirection(dir.Key));
        }
    }

    // Calling condition: when a projectile collides with a block
    // Called by: Projectile.OnTriggerEnter2D()
    public virtual void TakeDamage(float dmg_amt){
        health.TakeDamage(dmg_amt);
    }


    /******************** Utility ********************/

    // Calling condition: A neighboring block dies
    // Called by: neighboring block - not invoked by this on itself.
    public void DeleteNeighboringConnection(Direction dir){
        // delete our connection to a now deceased neighbor
        // RIP our neighbor :'(
        Debug.Assert(connected_neighbors.ContainsKey(dir), "Trying to remove a direction from a map that doesn't contain the direction");
        connected_neighbors.Remove(dir);
    }


    // Calling condition: When a block falls. The direction passed in is not
    //                    already present in the direction map
    // Called by: FallingToStill()
    public bool CheckForNeighbor(Direction dir, out Block neighbor_block) {
        Vector2 pt_to_check = transform.position + Utils.DirToVec (dir);
        Collider2D neighbor = Physics2D.OverlapPoint(pt_to_check, mask);

        if (neighbor != null && (neighbor.gameObject.layer == LayerMask.NameToLayer("Blocks") ||
                                 neighbor.gameObject.layer == LayerMask.NameToLayer("WeaponBlocks"))) {
            neighbor_block = neighbor.gameObject.GetComponent<Block>();
            return true;
        }
        neighbor_block = null; // to appease the compiler, we must assign to it regardless
        return false;
    }

    // Calling condition: When either another block or this block has fallen and
    //                    finds an existing neighbor to connect to.
    // Called by: this and neighboring block simultaneously
    public void ConnectToNeighbor(Direction dir, Block other){
        // add the fixedjoints and update the direction map
        connected_neighbors.Add(dir, other);
        AddFixedJoint (other);
    }

    // Called by: ConnectToNeighbor()
    void AddFixedJoint(Block other){
        gameObject.AddComponent<FixedJoint2D> ().connectedBody = other.gameObject.GetComponent<Rigidbody2D> ();
    }

    /* 
     CODE THAT COULD BE USED TO DETERMINE WHERE VALID POSITIONS ARE AND HIGHLIGHT THEM. THIS WAS PREVIOUSLY USED WHEN CLICKING
     AND PLACING A BLOCK, BUT SINCE WE DON'T NEED TO CLICK AND PLACE BLOCKS IN OUR GAME, THIS CODE COULD BE USEFUL IN ACHIEVING
     THAT SIMILAR "GUIDING THE PLAYER" ABILITY.
        if(being_manipulated) {
            Vector3 block_pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            block_pos.z = 0;
            transform.position = block_pos;
            rigid.velocity = Vector3.zero;

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
        */
}
