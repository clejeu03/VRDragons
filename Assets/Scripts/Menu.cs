using UnityEngine;
using System.Collections;

public class Menu : MonoBehaviour {

	// Network variables
	public GameObject networkPrefab;
	public GameObject mainCamera;
	private GameObject networkManager;
	private NetworkManager networkManagerScript;

	private HostData[] hostList = null;
	private bool refreshHostList = false;

	private bool displayMessage = false;
	private string message;
		
	// Menu states variables
	enum MenuState { MainMenu, NetworkManager, WaitingRoom, Play };
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
		//Initiate the PlayerPref "mode"
		if (!PlayerPrefs.HasKey("mode")){
			PlayerPrefs.SetString("mode","solo");
		}
	}

	void Update(){
		mainCamera.transform.Rotate (new Vector3 (0, 1, 0), 0.05f);
	}


	/*******************************************************
	 * GUI functions
	 ******************************************************/
	
	void OnGUI()
	{
		// Menus
		if (currentMenu == MenuState.MainMenu)
			displayMainMenu();
		else if(currentMenu == MenuState.NetworkManager)
			displayNetworkMenu();
		else if (currentMenu == MenuState.WaitingRoom)
			displayWaitingRoom();

		if (displayMessage) {
			GUI.Label (new Rect (0, 0,500,100), message);

		}
	}


	public void displayMainMenu(){

		// Button to launch Solo mode 
		if (GUI.Button(new Rect(100, 100, 250, 100), "Solo")){
			PlayerPrefs.SetString("mode","solo");
			Play();
		}
		// Button to launch Multiplayer mode
		if (GUI.Button(new Rect(100, 250, 250, 100), "Multiplayer"))
		{
			PlayerPrefs.SetString("mode","multi");
			currentMenu = MenuState.NetworkManager;
		}
	}


	public void displayNetworkMenu(){
		
		// Button to start a server
		if (GUI.Button(new Rect(100, 100, 250, 100), "Start Server")){
			networkManagerScript.StartServer();
			refreshHostList = false;
		}
		
		// Button to refresh the host list
		if (GUI.Button(new Rect(100, 250, 250, 100), "Join Server")){
			networkManagerScript.RefreshHostList();
			refreshHostList = true;
		}
		
		// Button to go bakc to the main menu
		if (GUI.Button(new Rect(100, 400, 250, 100), "Main Menu")){
			currentMenu = MenuState.MainMenu;
			refreshHostList = false;
		}
		
		// If the player want to join a server 
		if(refreshHostList){
			// Refresh the hosts list
			networkManagerScript.RefreshHostList();
			// Display the list of available hosts if it isn't empty
			if (hostList != null)
			{
				for (int i = 0; i < hostList.Length; i++)
				{
					// Button to join a server
					if (GUI.Button(new Rect(400, 100 + (110 * i), 300, 100), hostList[i].gameName))
						networkManagerScript.JoinServer(hostList[i]);
				}
			}
		}
	}


	void displayWaitingRoom(){
		GUI.Label (new Rect (100, 100,500,100), "Waiting a client...");

		// Button to close the server and go back to the main menu
		if (GUI.Button(new Rect(100, 400, 250, 100), "Close Server")){
			currentMenu = MenuState.NetworkManager;
			networkManagerScript.CloseServer();
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
		currentMenu = MenuState.NetworkManager;
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
