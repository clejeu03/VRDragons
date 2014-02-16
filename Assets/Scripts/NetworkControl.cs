using UnityEngine;
using System.Collections;
using Leap;

/**
 * NetworkControl handles the players actions on the two Dragons movements on the Network
 * ----------------------------------------------
 * Need the Component>Miscellaneous>NetworkView to be add to the Dragon object,
 * which enable us to send data packages over the network to synchronise the player
 * Parameters : 
 * - "State synchronized" set to "reliable delta compressed" 
 * 	 synchronized data will be sent automatically, but only if its value changed
 * - "Observed" contains the component that will be synchronized.
 * 	 The Object Transform is automatically added to this field
 * 	 In order to write our own synchronization method, we drag the 
 * 	 component of the player script into this field
 * -----------------------------------------------
 * The Dragon object need to be add to the bierachy to make it a prefab
 * that can be instantiate on the network
 * -----------------------------------------------
 * Run a server and client on the same computer :
 * Edit -> Project Settings -> Player -> “Run in Background” 
 *(Else you won’t be able to connect to the server, unless you focus on the game instance running the server after client has been connected)
 */
public class NetworkControl : MonoBehaviour {

	Controller m_leapController;
	public float speed = 3.5f;

	public GameObject myCamera;

	// Interpolation values
	private float lastSynchronizationTime = 0f;
	private float syncDelay = 0f;
	private float syncTime = 0f;
	private Vector3 syncStartPosition = Vector3.zero;
	private Vector3 syncEndPosition = Vector3.zero;

	private bool isHost = false;


	void Awake()
	{
		lastSynchronizationTime = Time.time;
	}

	void Start(){
		// Is the game instance the host or the client ?
		isHost = GameObject.Find("NetworkManager").GetComponent<NetworkManager>().isHost;
		
		// If the Dragon isn't the player's one, disable its camera 
		if (!networkView.isMine) {
			myCamera.camera.enabled = false;
		}
	}

	void OnGUI()
	{
		// If the game is the host
		if(isHost){
			GUI.Box(new Rect(10,10,50,20), "Host");
		}
		else
			GUI.Box(new Rect(10,10,50,20), "Client");
	}

	void Update()
	{
		// The input functions are only called if the Dragon is the player's one
		if (networkView.isMine)
		{
			InputMovement();
		}
		// When it's not, we need to use the interpolation between the synchronized values
		// to update the Dragon mouvement according to the movements of the Dragon of the player's opponent
		else
		{
			SyncedMovement();
		}
	}

	/* Update the position of the Dragon according to the movements of the Dragon of the player's opponent*/
	private void SyncedMovement()
	{
		syncTime += Time.deltaTime;
		
		rigidbody.position = Vector3.Lerp(syncStartPosition, syncEndPosition, syncTime / syncDelay);
	}

	/*Handle player's input*/
	void InputMovement() {
		if (Input.GetKey(KeyCode.Z))
			rigidbody.MovePosition(rigidbody.position + Vector3.forward * speed * Time.deltaTime);
		
		if (Input.GetKey(KeyCode.S))
			rigidbody.MovePosition(rigidbody.position - Vector3.forward * speed * Time.deltaTime);
		
		if (Input.GetKey(KeyCode.D))
			rigidbody.MovePosition(rigidbody.position + Vector3.right * speed * Time.deltaTime);
		
		if (Input.GetKey(KeyCode.Q))
			rigidbody.MovePosition(rigidbody.position - Vector3.right * speed * Time.deltaTime);

	}

	/* This function is automatically called every time it sends or receives datas.
	(To use for data that constantly changed) */
	void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
	{
		Vector3 syncPosition = Vector3.zero;
		Vector3 syncVelocity = Vector3.zero;
		
		// The player is writing to the stream (= he moves its own Dragon...)
		if (stream.isWriting)
		{
			syncPosition = rigidbody.position;
			stream.Serialize(ref syncPosition);
			
			syncPosition = rigidbody.velocity;
			stream.Serialize(ref syncVelocity);
		}
		// The dragon of the player's opponent need to be moved
		else
		{
			stream.Serialize(ref syncPosition);
			stream.Serialize(ref syncVelocity);
			
			// Interpolation : smoothing the transition from the old to the new data values
			syncTime = 0f;
			syncDelay = Time.time - lastSynchronizationTime;
			lastSynchronizationTime = Time.time;
			
			// Prediction : the position is "updated" before the new data is received
			syncEndPosition = syncPosition + syncVelocity * syncDelay;
			syncStartPosition = rigidbody.position;
		}
	}
}
