using UnityEngine;
using System.Collections;
using Leap;

public class LeapFly : MonoBehaviour {
  
	Controller m_leapController;

	private float forceMult = 0.0f;
	private Vector3 newRot = Vector3.zero;
  
	void Start () {
		// Get the leapController
		m_leapController = new Controller();
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
			newRot.y += handDiff.z * 3.0f - newRot.z * 0.03f * transform.rigidbody.velocity.magnitude;
			newRot.x = -(avgPalmForward.y - 0.1f) * 100.0f;

			float forceMult = 10.0f;
      
			// If closed fist, then stop the plane (and slowly go backwards)
		    if (frame.Fingers.Count < 3) {
		    	forceMult = -3.0f;
		    }
      
			transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.Euler(newRot), 0.1f);
			transform.rigidbody.velocity = transform.forward * forceMult;
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
				if (Input.GetKey(KeyCode.Q))
					newRot.y -= 10f;
				if (Input.GetKey(KeyCode.D))
					newRot.y += 10f;

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
		
			transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.Euler(newRot), 0.1f);
			transform.rigidbody.velocity = transform.forward * forceMult;
		}
	}
  
}
