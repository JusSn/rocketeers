using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Utils {
    // JF: Serializable struct for string/Sprite pairs. Useful for editing dictionaries in the editor.

    [System.SerializableAttribute]
    public struct NamedSprite {
        public string name;
        public Sprite sprite;
    }

    public static int LEFT_CLICK_BTN = 0;
    public static float MAX_BUILD_HEIGHT = 9f;
    // Returns the opposite direction
    // North -> South, East -> West
    public static Direction GetOppositeDirection(Direction dir) {
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

    // Returns the opposite direction, but as a vector instead of direction
    public static Vector3 GetOppositeDirectionAsVector(Direction dir){
        Direction opposite_dir = GetOppositeDirection (dir);
        Vector3 vec = DirToVec (opposite_dir);
        return vec;
    }

    // Returns the vector corresponding with a direction
    public static Vector3 DirToVec(Direction dir){
        switch (dir) {
        case Direction.EAST:
            return Vector3.right;
        case Direction.WEST:
            return Vector3.left;
        case Direction.NORTH:
            return Vector3.up;
        case Direction.SOUTH:
            return Vector3.down;
        }
        Debug.Log ("Something went wrong with DirToVec");
        return Vector3.zero;
    }

    // Returns true if the directions are opposite of each other, otherwise
    // returns false
    public static bool DirsAreOpposite(Direction dir1, Direction dir2){
        switch (dir1) {
        // our first direction is East, so our second direction must be West to be true
        case Direction.EAST:
            return dir2 == Direction.WEST;
        case Direction.WEST:
            return dir2 == Direction.EAST;
        case Direction.NORTH:
            return dir2 == Direction.SOUTH;
        case Direction.SOUTH:
            return dir2 == Direction.NORTH;
        }

        Debug.Log ("Something went wrong with DirsAreOpposite");
        return false;
    }

    // JF: returns true if layer is in Layermask.
    public static bool IsInLayerMask(int layer, LayerMask layermask)
    {
    return layermask == (layermask | (1 << layer));
    }

    // [CG]
    // Returns a hashset of all directions. Not efficient, but using the .Contains method in a
    // few places is a bit cleaner
    public static HashSet<Direction> GetAllDirections(){
        HashSet<Direction> dir_set = new HashSet<Direction> { Direction.NORTH, Direction.SOUTH,
                                                              Direction.EAST, Direction.WEST };
        return dir_set;
    }

    // [CG]
    // Calling condition: Called when placing a block to see if it connects to another
    //                    valid block
    // Called by: Player.SettingUpdate()
    public static bool ValidBlockPlacement(Vector3 setPos, LayerMask mask){
        HashSet<Direction> all_dirs = Utils.GetAllDirections ();

        foreach (Direction dir in all_dirs) {
            if (CheckForObj(setPos + Utils.DirToVec(dir), mask)){
                return true;
            }
        }

        return false;
    }

    // [CG]
    // Calling condition: checks for *an* object at the offset from the blocks location and returns that object if found.
    // Called by: Block.CheckForGround, Block.CheckForNeighbor
    public static Collider2D CheckForObj(Vector3 pos, LayerMask mask){
        return Physics2D.OverlapPoint(pos, mask);
    }
}
