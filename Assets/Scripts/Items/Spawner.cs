// Justin Fan
// To be attached to Spawner GameObject
// Spawns items (blocks and weapons) in a set direction
// Features item pooling to recycle items that travel off screen

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour {
	// Item to be spawned
	public GameObject itemPrefab;
	// Direction for spawned objects to travel in
	public Vector3 outputDir;
	// Maximum number of items in pool
	public int maxCount;

	public bool __________________;

	// Stack of itemPrefabs in pool	
	Stack<GameObject> poolStack;

	// Use this for initialization
	void Start () {
		// Pool instances of itemPrefab
		poolStack = new Stack<GameObject> ();

		for (int i = 0; i < maxCount; ++i) {
			GameObject item = Instantiate (itemPrefab, transform.position, Quaternion.identity) as GameObject;

			item.SetActive(false);
			item.GetComponent<Item> ().spawnerScript = this;
			poolStack.Push(item);
		}
		// Invoke repeating spawns of prefab
		InvokeRepeating ("SpawnItem", 0f, 2f);
	}
	
	// Update is called once per frame
	void Update () {
		// Not much to do here?
	}

	void SpawnItem () {
		if (poolStack.Count == 0) {
			return;
		}

		GameObject item = poolStack.Pop();

		item.SetActive(true);
		item.transform.position = transform.position;
		item.GetComponent<Rigidbody> ().velocity = outputDir;
	}

	public void Repool (GameObject go) {
		go.SetActive(false);
		poolStack.Push(go);
	}
}
