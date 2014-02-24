using UnityEngine;
using System.Collections;

public class Menu : MonoBehaviour {

	// Network variables
	public GameObject networkPrefab;
	private GameObject networkManager;
	private NetworkManager networkManagerScript;

	private HostData[] hostList = null;
	private bool refreshHostList = false;

	// GUI variables
	private int width = Screen.width;
	private int height = Screen.height;
	private Texture2D blackTexture;
	private GUIStyle styleMenu = new GUIStyle();

	// Loading next scene variable
	private AsyncOperation async;
		
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
		// Create a "black screen" texture
		blackTexture = new Texture2D(width,height);
		Color blackColor = new Color(0, 0, 0);

		for(int i = 0; i<width; i++)
		{
			for(int j = 0; j<height; j++)
				blackTexture.SetPixel(i, j, blackColor);
		}
		blackTexture.Apply();		

		// Reset the background padding
		styleMenu.padding = new RectOffset(0,0,0,0);


		//Initiate the PlayerPref "mode"
		if (!PlayerPrefs.HasKey("mode")){
			PlayerPrefs.SetString("mode","solo");
		}

		//Load the next level asynchronically
		Load();
	}


	/*******************************************************
	 * GUI functions
	 ******************************************************/
	
	void OnGUI()
	{
		// Background
		GUI.Box(new Rect (0 , 0, width, height),blackTexture,styleMenu);

		// Menus
		if (currentMenu == MenuState.MainMenu)
			displayMainMenu();
		else if(currentMenu == MenuState.NetworkManager)
			displayNetworkMenu();
		else if (currentMenu == MenuState.WaitingRoom)
			displayWaitingRoom();
		else if(currentMenu == MenuState.Play)
			displayBlackScreen(); 
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
			currentMenu = MenuState.MainMenu;
			networkManagerScript.CloseServer();
		}
	}

	
	public void displayBlackScreen(){
		GUI.Box(new Rect (0 , 0, width, height),blackTexture,styleMenu);
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


	/*******************************************************
	 * Loading next scene functions
	 ******************************************************/

	//Load the next scene asynchronically
	void Load() {
		Debug.LogWarning("ASYNCLOAD STARTED  - Do not exit play mode (Unity could crashed)");
		async = Application.LoadLevelAsync("TestGame");
		async.allowSceneActivation = false;
	}

	//Activate the next scene
	void ActivateScene() {
		Debug.Log("Activate !");
		async.allowSceneActivation = true;
	}
	
	public void Play(){
		currentMenu = MenuState.Play;

		if(PlayerPrefs.GetString ("mode") == "multi") {
			// The networkManager won't be destroy
			DontDestroyOnLoad (networkManager);
		}
		ActivateScene ();
	}
}
