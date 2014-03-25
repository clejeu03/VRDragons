using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Leap;

public class LeapFly : MonoBehaviour {
  
	Controller m_leapController;

	private float forceMult = 0.0f;
	private Vector3 newRot = Vector3.zero;
	private AudioSource m_audio;
	private Dictionary<string,AudioClip> m_actionSounds;
	private bool m_isGoingLeft;
	private bool m_isGoingRight;
  
	void Start () {
		// Get the leapController
		m_leapController = new Controller();

		m_isGoingLeft = false;
		m_isGoingRight = false;

		//Manage sounds
		m_audio = gameObject.GetComponent<AudioSource>();

		m_actionSounds = new Dictionary<string, AudioClip>();
		m_actionSounds["Acceleration"] = Resources.Load("Audio/acceleration") as AudioClip;
		m_actionSounds["TurnLeft"] = Resources.Load("Audio/turn_left") as AudioClip;
		m_actionSounds["TurnRight"] = Resources.Load("Audio/turn_right") as AudioClip;
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
	void FixedUpdate () {
    
		Frame frame = m_leapController.Frame();
  
		// Leap controls
    	if (frame.Hands.Count >= 2) {
			// Get the hands
			Hand leftHand = GetLeftMostHand(frame);
			Hand rightHand = GetRightMostHand(frame);
      
			// Takes the average direction (vector from the palm position toward the fingers) between the two hands 
			Vector3 avgPalmForward = (frame.Hands[0].Direction.ToUnity() + frame.Hands[1].Direction.ToUnity()) * 0.5f;

			// Get the difference between the hands position
			// PalmPosition : center position of the palm from the leap motion controller origin in millimeter
			Vector3 handDiff = leftHand.PalmPosition.ToUnityScaled() - rightHand.PalmPosition.ToUnityScaled();

			// Gets the rotation of the transform relative to the parent transform's rotation (should be 
			//null in our case at the beginning, as the Dragon hasn't any parent)
			newRot = transform.localRotation.eulerAngles;

			newRot.z = -handDiff.y * 20.0f;

			// Adding the rot.z as a way to use banking (rolling) to turn.
			if(handDiff.z * 3.0f - newRot.z * 0.03f * transform.rigidbody.velocity.magnitude > 0.1){
				m_isGoingRight = true;
				m_isGoingLeft = false;
			}else if(handDiff.z * 3.0f - newRot.z * 0.03f * transform.rigidbody.velocity.magnitude < 0.1){
				m_isGoingRight = false;
				m_isGoingLeft = true;
			}else{
				m_isGoingRight = false;
				m_isGoingLeft = false;
			}
			newRot.y += handDiff.z * 3.0f - newRot.z * 0.03f * transform.rigidbody.velocity.magnitude;
			newRot.x = -(avgPalmForward.y - 0.1f) * 200.0f;

			float forceMult = 10.0f;
      
			// If closed fist, then stop the plane (and slowly go backwards)
		    if (frame.Fingers.Count < 3) {
		    	forceMult = -3.0f;
		    }
      
			transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.Euler(newRot), 0.1f);
			transform.rigidbody.velocity = transform.forward * forceMult;

			computeSounds();
		}

		// Keyboard controls
		else{
			// Gets the rotation of the transform relative to the parent transform's rotation (should be 
			//null in our case at the beginning, as the Dragon hasn't any parent)
			newRot = transform.localRotation.eulerAngles;

			// We need to press the UpArrow or DownArrow(= we have the hands over the LeapMotion controller) to make the plane move
			if((Input.GetKey(KeyCode.UpArrow)) || (Input.GetKey(KeyCode.DownArrow))){
			
				// If the UpArrow is pressed, then go forward ("positive" speed)
				if(Input.GetKey(KeyCode.UpArrow))
					forceMult = 10.0f;
				// If the DownArrow is pressed (= if closed fist) then stop the plane and slowly go back ("negative" speed)
				else if(Input.GetKey(KeyCode.DownArrow))
					forceMult = -3.0f;

				// Rotation around the x axis
				if (Input.GetKey(KeyCode.Z))
					newRot.x -= 10f;
				if (Input.GetKey(KeyCode.S))
					newRot.x += 10f;

				// Rotation around the y axis
				if (Input.GetKey(KeyCode.Q)){
					newRot.y -= 10f;
					m_isGoingLeft = true;
					m_isGoingRight = false;
				}
				if (Input.GetKey(KeyCode.D)){
					newRot.y += 10f;
					m_isGoingRight = true;
					m_isGoingLeft = false;
				}


				// Rotation around the z axis
				if (Input.GetKey(KeyCode.A))
					newRot.z += 10f;			
				if (Input.GetKey(KeyCode.E))
					newRot.z -= 10f;
			}
		
			// If the upArrow or DownArrow aren't pressed (= we haven't the hands over the LeapMotion controller) then stop the plane
			else {
				forceMult = 0f;
			}

			//Get rotation states
//			if(Input.GetKeyDown(KeyCode.D) && Input.GetKeyDown(KeyCode.UpArrow)){
//				m_isGoingRight = true;
//				m_isGoingLeft = false;
//			}else if(Input.GetKeyDown(KeyCode.Q) && Input.GetKeyDown(KeyCode.UpArrow) ){
//				m_isGoingLeft = true;
//				m_isGoingRight = false;
//			}else if( (!Input.GetKeyDown(KeyCode.Q) && !Input.GetKeyDown(KeyCode.D)) ) {
//				m_isGoingRight = false;
//				m_isGoingLeft = false;
//			}
		
			transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.Euler(newRot), 0.1f);
			transform.rigidbody.velocity = transform.forward * forceMult;

			computeSounds();
		}
	
	}

	void computeSounds(){
		//Retrieve the sound according to the movement
		Vector3 rotation = transform.localRotation.eulerAngles;
		rotation.x = transform.rigidbody.velocity.magnitude * 7.0f;

		if(Vector3.Dot(transform.rigidbody.velocity, transform.forward) > 0) {
			if( !(m_audio.clip == m_actionSounds["Acceleration"] && m_audio.isPlaying) ){
				StartCoroutine(fadeOut());
				m_audio.Stop();
				m_audio.clip = m_actionSounds["Acceleration"];
				m_audio.volume = 0.0f;
				m_audio.Play();
				StartCoroutine(fadeIn());
			}
//			if(m_isGoingRight) {
//				Debug.Log ("right !");
//				if(!(m_audio.clip == m_actionSounds["TurnRight"] && m_audio.isPlaying) ){
//					StartCoroutine(fadeOut());
//					m_audio.Stop();
//					m_audio.clip = m_actionSounds["TurnRight"];
//					m_audio.volume = 0.0f;
//					m_audio.Play();
//					m_audio.loop = true;
//					StartCoroutine(fadeIn());
//				}
//			}else if(m_isGoingLeft) {
//				Debug.Log ("left !");
//				if( !(m_audio.clip == m_actionSounds["TurnLeft"] && m_audio.isPlaying) ){
//					StartCoroutine(fadeOut());
//					m_audio.Stop();
//					m_audio.clip = m_actionSounds["TurnLeft"];
//					m_audio.volume = 0.0f;
//					m_audio.Play();
//					m_audio.loop = true;
//					StartCoroutine(fadeIn());
//				}
//			}
		}else{
			m_audio.Stop();
		}
	}

	IEnumerator fadeIn() {
		for(int i = 0; i < 10; ++i){
			yield return new WaitForSeconds (0.2f);
			m_audio.volume = i*0.1f;
		}
	}
	
	IEnumerator fadeOut() {
		for(int i = 10 ; i > 0; --i){
			yield return new WaitForSeconds (0.2f);
			m_audio.volume = i* 0.1f;
		}
	}
  
}
