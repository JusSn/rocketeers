using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosive : Block {

	private LayerMask enemyBlockMask;

	private float EXPLOSION_RADIUS = 2f;

	/// <summary>
	/// Sent when an incoming collider makes contact with this object's
	/// collider (2D physics only).
	/// </summary>
	/// <param name="other">The Collision2D data associated with this collision.</param>

	// On contact with enemy block, explode this block, the block it contacts, and deal lesser area damage to surrounding enemy blocks
	public override void OnCollisionEnter2D(Collision2D other)
	{
		enemyBlockMask = LayerMask.NameToLayer("Team" + Utils.GetOppositeTeamNum (teamNum) + "Block");
		if (other.gameObject.layer == enemyBlockMask) {
			// Explode this block
			ExplodeBlock ();
		}
	}

	private void ExplodeBlock () {
		// JF: EXPLOSIONS!
        if (explosion != null) {
            GameObject boom0 = Instantiate(explosion, transform.position, Quaternion.identity);
            boom0.GetComponent<LoopingAnimation>().StartAnimation();
        }

		print (LayerMask.LayerToName (enemyBlockMask));

		Collider2D[] blockCols = Physics2D.OverlapCircleAll (transform.position, EXPLOSION_RADIUS, LayerMask.NameToLayer("Team2Block"));
		foreach (Collider2D col in blockCols) {
			print ("boom1");
			Block blockScript = col.gameObject.GetComponent<Block> ();
			
			if (blockScript != null) {
				print ("boom2");
				blockScript.ExplosionDamage ();
			}
		}

		UnhingeAndFall ();
	}
}
