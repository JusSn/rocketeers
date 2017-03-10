/*  Author: SK && CG
 *
 *  Update (3/5/17): Make block stick to any neighbors it has on creation
 */

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
    private BlockStates                         state = BlockStates.FALLING;
    private LayerMask                           ground_mask;

    private float                               SLEEPING_THRESHOLD = 0.1f;


    // Use this for initialization
    void Start () {
        rigid = GetComponent<Rigidbody2D> ();
        health = GetComponent<Health> ();
        ground_mask = LayerMask.GetMask ("Ground");
        health.SetParent (this);
        states.Add (BlockStates.FALLING, Falling);
        states.Add (BlockStates.FALLING_TO_STILL, FallingToStill);
        states.Add (BlockStates.STILL, Still);
        CheckForAnyNeighbors ();
    }

    protected virtual void Update(){
        // run the correct state function each update
        states [state] ();
        DestroyIfOffScreen ();
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
            CheckAndConnectToNeighbor (dir);
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


    // Calling condition: check and destroy this block if it's offscreen
    // Called by: this.Update()
    void DestroyIfOffScreen(){
        if (!MainCamera.S.IsOnScreen (transform.position)) {
            Destroy (gameObject);
        }
    }

    // Calling condition: Checking for any block in all four directions to connect to
    // Called by: this.Start()
    void CheckForAnyNeighbors(){
        HashSet<Direction> all_dirs = new HashSet<Direction>{ Direction.NORTH, Direction.SOUTH, Direction.EAST, Direction.WEST };
        foreach (Direction dir in all_dirs) {
            CheckAndConnectToNeighbor (dir);
        }
    }

    // Calling Condition: Check for a surrounding neighbor and connect to it
    // Called by: this.FallingToStill()
    void CheckAndConnectToNeighbor(Direction dir){
        Block neighbor = null;
        if (CheckForNeighbor (dir, out neighbor)) {
            // we have a neighbor, so connect to it
            ConnectToNeighbor (dir, neighbor);
            // connect the neighbor in the opposite direction, since that's the side
            // this block is on
            neighbor.ConnectToNeighbor (Utils.GetOppositeDirection (dir), this);
        } else {
            CheckForGround ();
        }
    }

    // Calling condition: when a block is placed, it could be on the
    //                    ground and a rigid body attachment needs to be made
    // Called by: this.CheckAndConnectToNeighbor()
    void CheckForGround(){
        Collider2D obj = CheckForObj (Vector3.down, ground_mask);
        if (obj != null && obj.gameObject.CompareTag("Ground")) {
            AddFixedJoint (obj.gameObject);
        }
    }

    // Calling condition: checks for an object at the offset from the blocks location and returns that object if found.
    // Called by: this.CheckForGround, this.CheckForNeighbor
    Collider2D CheckForObj(Vector3 offset, LayerMask specific_mask){
        return Physics2D.OverlapPoint(transform.position + offset, specific_mask);
    }

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
        Collider2D neighbor = CheckForObj(Utils.DirToVec(dir), mask);
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
        AddFixedJoint (other.gameObject);
    }

    // Calling condition: when needing to add a fixed joint from this gameObject to another gameObject
    // Called by: ConnectToNeighbor()
    void AddFixedJoint(GameObject other_go){
        gameObject.AddComponent<FixedJoint2D> ().connectedBody = other_go.GetComponent<Rigidbody2D> ();
    }
}
