using UnityEngine;
using System.Collections;

public class StartGame : MonoBehaviour {

	public GameObject playerSoloPrefab;
	public GameObject player1Prefab;
	public GameObject player2Prefab;

	// Use this for initialization
	void Start () {
	
		if (PlayerPrefs.GetString ("mode") == "solo") {
			Debug.Log ("Mode Solo");
			Instantiate(playerSoloPrefab, playerSoloPrefab.transform.position, playerSoloPrefab.transform.rotation);
		}
		else{
			if (Network.isServer) {
				Network.Instantiate (player1Prefab, player1Prefab.transform.position, player1Prefab.transform.rotation, 0);
			}
			else{
				Network.Instantiate (player2Prefab, player2Prefab.transform.position, player2Prefab.transform.rotation, 0);
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (PlayerPrefs.GetString ("mode") == "solo") {
			// Back to the Main Menu scene
			if (Input.GetKey(KeyCode.Escape))
				Application.LoadLevel ("TestMenu");
		}
	
	}
	
}
