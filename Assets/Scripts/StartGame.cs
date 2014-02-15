using UnityEngine;
using System.Collections;

public class StartGame : MonoBehaviour {

	public GameObject playerSoloPrefab;

	// Use this for initialization
	void Start () {
	
		if (PlayerPrefs.GetString ("mode") == "solo") {
			Debug.Log ("Mode Solo");
			Instantiate(playerSoloPrefab, playerSoloPrefab.transform.position, playerSoloPrefab.transform.rotation);
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
