using UnityEngine;
using System.Collections;

/**
 * NetworkManager handles hosting a server or connecting to an existing host
 */
public class NetworkManager : MonoBehaviour
{
	// Main menu variables
	private GameObject mainMenu;
	private Menu mainMenuScript;
	private bool hasMessageToMenu = false;
	private string messageToMenu;

	// Network variables
	private const string typeName = "VRDragon";
	private const string gameName = "RoomName";

    private bool isRefreshingHostList = false;
    private HostData[] hostList = null;
	

	/*******************************************************
	 * Initialisation functions
	 ******************************************************/

	public void FindMenu(){
		// Retrieve the MainMenu gameObject
		mainMenu = GameObject.Find ("MainMenu");
		mainMenuScript = mainMenu.GetComponent<Menu>();

		// The game was played and the server or the client has quitted
		if (hasMessageToMenu) {
			mainMenuScript.setCurrentStateNetwork();
			mainMenuScript.setMessage (messageToMenu);
			hasMessageToMenu = false;
		}
	}

	/*******************************************************
	 * Server actions : Start or Close a server
	 ******************************************************/

    public void StartServer()
    {
		// Initialize the server on the network : 
		// InitializeServer(MaxPlayerAmount, PortNumber, Use NAT punchthrough if no public IP present)
		NetworkConnectionError error  = Network.InitializeServer(2, 25000, !Network.HavePublicAddress());
		if (error != NetworkConnectionError.NoError)
			//Debug.Log (error);
						mainMenuScript.setMessage ("A server has already been started. Try to join it !");
		// Register the host to the Master : ServerRegisterHost(UniqueGameName, RoomName)
        MasterServer.RegisterHost(typeName, gameName);
    }
	
	public void CloseServer(){
		Network.Disconnect();
		MasterServer.UnregisterHost();
	}

	public void CloseServerInGame(){
		// Kick the player off the server before closing it (see OnPlayerDisconnected() function)
		if (Network.connections.Length > 0) {
			Debug.Log("Disconnecting: "+
			          Network.connections[0].ipAddress+":"+Network.connections[0].port);
			Network.CloseConnection(Network.connections[0], true);
		} 
	}


	/*******************************************************
	 * Server and Client actions : quit the game and go back to the menu
	 ******************************************************/
	
	public void QuitGame(){
		// Properly close or quit the server
		if (Network.isServer){
			messageToMenu = "Successfully closed server";
			hasMessageToMenu = true;
			CloseServerInGame();
		}
		else{
			messageToMenu = "Successfully disconnected from server";
			hasMessageToMenu = true;
			QuitServer();
		}
	}

	/*******************************************************
	 * Client actions : Join or Quit a server
	 ******************************************************/

	public void JoinServer(HostData hostData)
	{
		Network.Connect(hostData);
	}

	public void QuitServer()
	{
		Network.Disconnect();
	}


	/*******************************************************
	 * Update the host list
	 ******************************************************/
    void Update()
    {
		// If the list of host has been refreshed 
        if (isRefreshingHostList)
        {
            isRefreshingHostList = false;
			// If the list of host isn't empty, the host list is refresh
			if(MasterServer.PollHostList().Length > 0)
            	hostList = MasterServer.PollHostList();
			else hostList = null;

			// The MainMenu list of hosts need to be refreshed as well
			mainMenuScript.setHostList(hostList);
        }
    }

	// Send a request to the master server to get the list of host contening all the data to join a server
    public void RefreshHostList()
    {
        if (!isRefreshingHostList)
        {
            isRefreshingHostList = true;
            MasterServer.RequestHostList(typeName);
        }
    }


	/***************************************************************************
	 * Messages sent on the server or the client when a specific event occures
	 **************************************************************************/
	
	// Actions called on the server whenever it has been succesfully initialized
	void OnServerInitialized()
	{
		// Display the waiting room for the server-player
		mainMenuScript.setCurrentStateWait();
	}

	// Actions called on the client when a connection attempt fails for some reason
	void OnFailedToConnect(){
	}

	// Actions called on the client when it has successfully joined a server
    void OnConnectedToServer()
    {
		// Launch the game
		mainMenuScript.Play ();
    }
	// Actions called on the server whenever a new player has successfully connected
	void OnPlayerConnected()
	{
		// Launch the game
		mainMenuScript.Play ();
	}

	// Actions called on the server whenever a player is disconnected from the server
	void OnPlayerDisconnected(){
		if(!hasMessageToMenu){
			messageToMenu = "The client has quit";
			hasMessageToMenu = true;
		}
		CloseServer ();
		// Don't destroy the game object on which the script is attached
		DontDestroyOnLoad (gameObject);
		//Load the menu
		Application.LoadLevel ("TestMenu");
	}

	// Actions called on client during disconnection from server, but also on the server when the connection has disconnected
	void OnDisconnectedFromServer(){
		if(Network.isClient){
			if(!hasMessageToMenu){
			   messageToMenu = "The server has been closed";
			   hasMessageToMenu = true;
			}
			// Don't destroy the game object on which the script is attached
			DontDestroyOnLoad (gameObject);
			// Load the menu
			Application.LoadLevel ("TestMenu");
		}
	}

}
