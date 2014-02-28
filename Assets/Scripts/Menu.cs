using UnityEngine;
using System.Collections;

public class Menu : MonoBehaviour {

	// Public variables
	public GameObject networkPrefab;
	public GameObject mainCamera;
	public Texture[] guiTextures;

	// Network variables
	private GameObject networkManager;
	private NetworkManager networkManagerScript;
	private HostData[] hostList = null;
	
	// Gui variables
	private int width = Screen.width;
	private int height = Screen.height;
	private int offset = 60; // Offset between buttons
	private int planeYpos = 20;
	private bool displayMessage = false;
	private string message;
	private GUIStyle mystyle;
		
	// Menu states variables
	enum MenuState { MainMenu, Credits, TwoPlayers, RoomList, WaitingRoom, Play };
	private MenuState currentMenu = MenuState.MainMenu;
	

	/*******************************************************
	 * Initialisation functions
	 ******************************************************/
	
	void Awake () {
		// Retrieve or instantiate the NetworkManager gameobject which wear the "NetworkManager" tag
		if (GameObject.FindGameObjectWithTag ("NetworkManager") == null) {
			Debug.Log ("Instantiating NetworkManager");
			Instantiate(networkPrefab, networkPrefab.transform.position, networkPrefab.transform.rotation);
			networkManager = GameObject.FindGameObjectWithTag ("NetworkManager");
			networkManagerScript = networkManager.GetComponent<NetworkManager>();
		}
		else{
			Debug.Log ("Retrieving NetworkManager ");
			networkManager = GameObject.FindGameObjectWithTag ("NetworkManager");
			networkManagerScript = networkManager.GetComponent<NetworkManager>();
		}

		networkManagerScript.FindMenu();
	}


	void Start () {
		// Set no style
		mystyle = new GUIStyle ();

		//Initiate the PlayerPref "mode"
		if (!PlayerPrefs.HasKey("mode")){
			PlayerPrefs.SetString("mode","solo");
		}
	}

	void Update(){
		// Slowly rotate the camera
		mainCamera.transform.Rotate (new Vector3 (0, 1, 0), 0.05f);

		// Update the screen width / height
		width = Screen.width;
		height = Screen.height;
	}


	/*******************************************************
	 * GUI functions
	 ******************************************************/
	
	void OnGUI()
	{
		// Menus
		if (currentMenu == MenuState.MainMenu)
			displayMainMenu();
		else if(currentMenu == MenuState.TwoPlayers)
			displayTwoPlayersMenu();
		else if (currentMenu == MenuState.WaitingRoom)
			displayWaitingRoom();
		else if (currentMenu == MenuState.RoomList)
			displayRoomList();
		else if (currentMenu == MenuState.Credits)
			displayCreditsMenu();

		if (displayMessage) {
			GUI.Label (new Rect (0, 0,500,100), message);

		}
	}


	private void displayMainMenu(){
		// Check if mouse if over the buttons to display the paperplane icon
		hoverMainMenu();

		// "Solo" button 
		if (GUI.Button(new Rect(width/2 - guiTextures[0].width/2, height/2+offset, guiTextures[0].width, guiTextures[0].height), guiTextures[0], mystyle)){
			PlayerPrefs.SetString("mode","solo");
			Play();
		}
		// "Two players" button
		if (GUI.Button(new Rect(width/2 - guiTextures[1].width/2, height/2+offset*2, guiTextures[1].width, guiTextures[1].height), guiTextures[1], mystyle))
		{
			PlayerPrefs.SetString("mode","multi");
			currentMenu = MenuState.TwoPlayers;
		}
		// "Credits" button
		if (GUI.Button(new Rect(width/2 - guiTextures[2].width/2, height/2+offset*3, guiTextures[2].width, guiTextures[2].height), guiTextures[2], mystyle))
		{
			currentMenu = MenuState.Credits;
		}
		// "Exit" button
		if (GUI.Button(new Rect(width/2 - guiTextures[3].width/2, height/2+offset*4, guiTextures[3].width, guiTextures[3].height), guiTextures[3], mystyle))
		{
			Application.Quit();
		}
	}

	private void hoverMainMenu(){
		// Check if mouse hover one of the buttons in main menu
		for (int i = 0; i < 4; ++i) {
			if (new Rect(width/2 - guiTextures[i].width/2, height/2+(i+1)*offset, guiTextures[i].width, guiTextures[i].height).Contains (Event.current.mousePosition) == true) {
				GUI.Label(new Rect(width/2 + guiTextures[i].width/2 - guiTextures[11].width/2, height/2+(i+1)*offset+planeYpos, guiTextures[11].width, guiTextures[11].height), guiTextures[11], mystyle);
			}
		}
	}


	private void displayTwoPlayersMenu(){
		// Check if mouse if over the buttons to display the paperplane icon
		hoverTwoPlayersMenu ();
		
		// "Create a room" button
		if (GUI.Button(new Rect(width/2 - guiTextures[4].width/2, height/2+offset, guiTextures[4].width, guiTextures[4].height), guiTextures[4], mystyle)){
			networkManagerScript.StartServer();
		}
		
		// "Join a room" button
		if (GUI.Button(new Rect(width/2 - guiTextures[5].width/2, height/2+2*offset, guiTextures[5].width, guiTextures[5].height), guiTextures[5], mystyle)){
			currentMenu = MenuState.RoomList;
		}
		
		// "Back" button
		if (GUI.Button(new Rect(width/2 - guiTextures[10].width/2, height/2+3*offset, guiTextures[10].width, guiTextures[10].height), guiTextures[10], mystyle)){
			currentMenu = MenuState.MainMenu;
		}
	}

	private void hoverTwoPlayersMenu(){
		// Check if mouse hover "Create" or "Join" buttons in Two Players menu
		for (int i = 4; i < 6; ++i) {
			if (new Rect(width/2 - guiTextures[i].width/2, height/2+(i-3)*offset, guiTextures[i].width, guiTextures[i].height).Contains (Event.current.mousePosition) == true) {
				GUI.Label(new Rect(width/2 + guiTextures[i].width/2 - guiTextures[11].width/2, height/2+(i-3)*offset+planeYpos, guiTextures[11].width, guiTextures[11].height), guiTextures[11], mystyle);
			}
		}

		// Check if mouse hover "Back" button in Two Players menu
		if (new Rect(width/2 - guiTextures[10].width/2, height/2 + 3*offset, guiTextures[10].width, guiTextures[10].height).Contains (Event.current.mousePosition) == true) {
			GUI.Label(new Rect(width/2 + guiTextures[10].width/2 - guiTextures[11].width/2, height/2 + 3*offset+planeYpos, guiTextures[11].width, guiTextures[11].height), guiTextures[11], mystyle);
		}

	}

	private void displayWaitingRoom(){
		// Label "Awaiting player"
		GUI.Label(new Rect(width/2 - guiTextures[6].width/2, height/2+offset, guiTextures[6].width, guiTextures[6].height), guiTextures[6], mystyle);

		// Check if mouse if over the "Back" button to display the paperplane icon
		if (new Rect(width/2 - guiTextures[10].width/2, height/2 + 3*offset, guiTextures[10].width, guiTextures[10].height).Contains (Event.current.mousePosition) == true) {
			GUI.Label(new Rect(width/2 + guiTextures[10].width/2 - guiTextures[11].width/2, height/2 + 3*offset+planeYpos, guiTextures[11].width, guiTextures[11].height), guiTextures[11], mystyle);
		}

		// "Back" button : close the server and go back to the main menu
		if (GUI.Button(new Rect(width/2 - guiTextures[10].width/2, height/2+3*offset, guiTextures[10].width, guiTextures[10].height), guiTextures[10], mystyle)){
			currentMenu = MenuState.TwoPlayers;
			networkManagerScript.CloseServer();
		}
	}

	private void displayRoomList(){
		// Label "List of rooms :"
		GUI.Label(new Rect(width/2 - guiTextures[7].width, height/2+offset, guiTextures[7].width, guiTextures[7].height), guiTextures[7], mystyle);

		// Refresh the hosts list
		networkManagerScript.RefreshHostList();
		// Display the list of available hosts if it isn't empty
		if (hostList != null)
		{
			for (int i = 0; i < hostList.Length; i++)
			{
				// Paperplane icon if mouse over a server
				if (new Rect(width/2 + guiTextures[7].width/2 - guiTextures[8].width/2 + 20, height/2+(i+1)*offset+15, guiTextures[8].width, guiTextures[8].height).Contains (Event.current.mousePosition) == true)
					GUI.Label(new Rect(width/2 + guiTextures[7].width/2 + guiTextures[8].width/2 + 20 - guiTextures[11].width/2,
					                   height/2+(i+1)*offset+planeYpos+15,
					                   guiTextures[11].width, guiTextures[11].height),
					          		   guiTextures[11], mystyle);

				// Button to join a server
				if (GUI.Button(new Rect(width/2 + guiTextures[7].width/2 - guiTextures[8].width/2 + 20, height/2+(i+1)*offset+15, guiTextures[8].width, guiTextures[8].height), guiTextures[8], mystyle))
					networkManagerScript.JoinServer(hostList[i]);
			}
		}

		// Check if mouse if over the Back button to display the paperplane icon
		if (new Rect(width/2 - guiTextures[10].width/2, height/2 + guiTextures[7].height + 1.5f*offset, guiTextures[10].width, guiTextures[10].height).Contains (Event.current.mousePosition) == true) {
			GUI.Label(new Rect(width/2 + guiTextures[10].width/2 - guiTextures[11].width/2, height/2 + guiTextures[7].height + 1.5f*offset + planeYpos, guiTextures[11].width, guiTextures[11].height), guiTextures[11], mystyle);
		}
		
		// Button to go back to the main menu
		if (GUI.Button(new Rect(width/2 - guiTextures[10].width/2, height/2 + guiTextures[7].height + 1.5f*offset, guiTextures[10].width, guiTextures[10].height), guiTextures[10], mystyle)){
			currentMenu = MenuState.TwoPlayers;
		}
	}

	private void displayCreditsMenu(){
		// Label "Made by :"
		GUI.Label(new Rect(width/2 - guiTextures[9].width/2, height/2+offset, guiTextures[9].width, guiTextures[9].height), guiTextures[9], mystyle);

		// Check if mouse if over the Back button to display the paperplane icon
		if (new Rect(width/2 - guiTextures[10].width/2, height/2 + guiTextures[9].height + 1.5f*offset, guiTextures[10].width, guiTextures[10].height).Contains (Event.current.mousePosition) == true) {
			GUI.Label(new Rect(width/2 + guiTextures[10].width/2 - guiTextures[11].width/2, height/2 + guiTextures[9].height + 1.5f*offset + planeYpos, guiTextures[11].width, guiTextures[11].height), guiTextures[11], mystyle);
		}
		
		// Button to go back to the main menu
		if (GUI.Button(new Rect(width/2 - guiTextures[10].width/2, height/2 + guiTextures[9].height + 1.5f*offset, guiTextures[10].width, guiTextures[10].height), guiTextures[10], mystyle)){
			currentMenu = MenuState.MainMenu;
		}
	}

	/*******************************************************
	 * Setter functions
	 ******************************************************/

	public void setHostList(HostData[] hosts){
		hostList = hosts;
	}

	public void setCurrentStateWait(){
		currentMenu = MenuState.WaitingRoom;
	}
	public void setCurrentStateNetwork(){
		currentMenu = MenuState.TwoPlayers;
	}

	public void stopDisplayMessage(){
		displayMessage = false;
	}
	public void setMessage(string msg){
		message = msg;
		displayMessage = true;
		Invoke("stopDisplayMessage", 2);
	}

	/*******************************************************
	 * Loading next scene functions
	 ******************************************************/
	public void Play(){
		currentMenu = MenuState.Play;

		if(PlayerPrefs.GetString ("mode") == "multi") {
			// The networkManager won't be destroy
			DontDestroyOnLoad (networkManager);
		}
		Application.LoadLevel("TestGame");
	}
}
