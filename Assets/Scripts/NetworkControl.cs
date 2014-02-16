﻿using UnityEngine;
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

	private float forceMult = 0f;
	private float syncForceMult = 0f;

	private Vector3 newRot = Vector3.zero;
	private Vector3 syncNewRot = Vector3.zero;

	private Quaternion syncStartRotation = Quaternion.identity;
	private Quaternion syncEndRotation = Quaternion.identity;

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
		// If the Dragon is the player's one, get the leapController
		if(networkView.isMine){
			m_leapController = new Controller();
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

	/* Update the rotation and velocity of the Dragon according to the movements of the Dragon of the player's opponent*/
	private void SyncedMovement()
	{
		syncTime += Time.deltaTime;

		transform.localRotation = Quaternion.Slerp(syncStartRotation, syncEndRotation, syncTime /syncDelay);
		transform.rigidbody.velocity = transform.forward * syncForceMult;

	}

	/* Get the left Hand*/
	Hand GetLeftMostHand(Frame f) {
		float smallestVal = float.MaxValue;
		Hand h = null;
		for(int i = 0; i < f.Hands.Count; ++i) {
			if (f.Hands[i].PalmPosition.ToUnity().x < smallestVal) {
				smallestVal = f.Hands[i].PalmPosition.ToUnity().x;
				h = f.Hands[i];
			}
		}
		return h;
	}

	/* Get the right Hand*/
	Hand GetRightMostHand(Frame f) {
		float largestVal = -float.MaxValue;
		Hand h = null;
		for(int i = 0; i < f.Hands.Count; ++i) {
			if (f.Hands[i].PalmPosition.ToUnity().x > largestVal) {
				largestVal = f.Hands[i].PalmPosition.ToUnity().x;
				h = f.Hands[i];
			}
		}
		return h;
	}

	/*Handle player's input*/
	void InputMovement() {

		Frame frame = m_leapController.Frame();

		// Leap controls
		if (frame.Hands.Count >= 2) {
			// Get the hands
			Hand leftHand = GetLeftMostHand(frame);
			Hand rightHand = GetRightMostHand(frame);
			
			// Takes the average direction (vector from the palm position toward the fingers) between the two hands 
			// TODO It will be used for the pitch of the Dragon ???
			Vector3 avgPalmForward = (frame.Hands[0].Direction.ToUnity() + frame.Hands[1].Direction.ToUnity()) * 0.5f;

			// Get the difference between the hands position
			// PalmPosition : center position of the palm from the leap motion controller origin in millimeter
			Vector3 handDiff = leftHand.PalmPosition.ToUnityScaled() - rightHand.PalmPosition.ToUnityScaled();

			// Gets the rotation of the transform relative to the parent transform's rotation (should be 
			//null in our case at the beginning, as the Dragon hasn't any parent)
			newRot = transform.localRotation.eulerAngles;

			newRot.z = -handDiff.y * 20.0f;
			
			// Adding the rot.z as a way to use banking (rolling) to turn.
			newRot.y += handDiff.z * 3.0f - newRot.z * 0.03f * transform.rigidbody.velocity.magnitude;
			newRot.x = -(avgPalmForward.y - 0.1f) * 100.0f;
			
			forceMult = 10.0f;
			
			// if closed fist, then stop the plane (and slowly go backwards)
			if (frame.Fingers.Count < 3) {
				forceMult = -3.0f;
				//forceMult = 0f;
			}
			
			transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.Euler(newRot), 0.1f);
			transform.rigidbody.velocity = transform.forward * forceMult;
		}

		// Keyboard controls
		else{
			if (Input.GetKey(KeyCode.Z))
				rigidbody.MovePosition(rigidbody.position + Vector3.forward * speed * Time.deltaTime);
			
			if (Input.GetKey(KeyCode.S))
				rigidbody.MovePosition(rigidbody.position - Vector3.forward * speed * Time.deltaTime);
			
			if (Input.GetKey(KeyCode.D))
				rigidbody.MovePosition(rigidbody.position + Vector3.right * speed * Time.deltaTime);
			
			if (Input.GetKey(KeyCode.Q))
				rigidbody.MovePosition(rigidbody.position - Vector3.right * speed * Time.deltaTime);
		}

	}

	//transform.localRotation = Quaternion.Slerp(syncStartRotation, Quaternion.Euler(syncEndRotation), syncTime);
	//transform.rigidbody.velocity = transform.forward * forceMult;

	/* This function is automatically called every time it sends or receives datas.
	(To use for data that constantly changed) */
	void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
	{
		Quaternion syncRotation = Quaternion.identity;
		Vector3 syncVelocity = Vector3.zero;
		
		// The player is writing to the stream (= he moves its own Dragon...)
		if (stream.isWriting)
		{
			syncRotation = transform.localRotation;
			stream.Serialize(ref syncRotation);
			
			syncVelocity = transform.rigidbody.velocity;
			stream.Serialize(ref syncVelocity);

			syncForceMult = forceMult;
			stream.Serialize(ref syncForceMult);

			syncNewRot = newRot;
			stream.Serialize(ref syncNewRot);

		}
		// The dragon of the player's opponent need to be moved
		else
		{
			stream.Serialize(ref syncRotation);
			stream.Serialize(ref syncVelocity);
			stream.Serialize(ref syncForceMult);
			stream.Serialize(ref syncNewRot);
			
			// Interpolation : smoothing the transition from the old to the new data values
			syncTime = 0f;
			syncDelay = Time.time - lastSynchronizationTime;
			lastSynchronizationTime = Time.time;
			
			// Prediction : the rotation is "updated" before the new data is received
			syncEndRotation = Quaternion.Slerp(syncRotation, Quaternion.Euler(syncNewRot), syncDelay);
			syncStartRotation = transform.localRotation;
		}
	}
}
