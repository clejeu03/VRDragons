using UnityEngine;
using System.Collections;

/**
 * NetworkManager handles hosting a server or connecting to an existing host
 */
public class NetworkManager : MonoBehaviour
{
	public GameObject player1Prefab;
	public GameObject player2Prefab;

    private const string typeName = "UniqueGameName";
    private const string gameName = "RoomName";

    private bool isRefreshingHostList = false;
    private HostData[] hostList;

	private bool displayNetworkManager = false;

	private int width = Screen.width;
	private int height = Screen.height;
	
	private Texture2D blackTexture;
	private GUIStyle styleMenu = new GUIStyle();

	public bool isHost = false;
	
	void Start () {
		// Create a "black screen" texture
		blackTexture = new Texture2D(width,height);
		Color blackColor = new Color(0, 0, 0);
		
		for(int i = 0; i<width; i++)
		{
			for(int j = 0; j<height; j++)
			{
				blackTexture.SetPixel(i, j, blackColor);
			}
		}
		blackTexture.Apply();		
		
		// Reset the background padding
		styleMenu.padding = new RectOffset(0,0,0,0);
	}

    void OnGUI()
    {
		// If the player hasn't started or joined a server yet
		if (!Network.isClient && !Network.isServer && displayNetworkManager)
        {
			// Background
			GUI.Box(new Rect (0 , 0, width, height),blackTexture,styleMenu);

			// Button to start a server
            if (GUI.Button(new Rect(100, 100, 250, 100), "Start Server"))
                StartServer();

			// Button to refresh the host list
            if (GUI.Button(new Rect(100, 250, 250, 100), "Refresh Hosts"))
                RefreshHostList();

			// Display the list of available hosts if it isn't empty
            if (hostList != null)
            {
                for (int i = 0; i < hostList.Length; i++)
                {
					// Button to join a server
                    if (GUI.Button(new Rect(400, 100 + (110 * i), 300, 100), hostList[i].gameName))
                        JoinServer(hostList[i]);
                }
            }
        }
    }

	public void setDisplayNetworkManager(){
		displayNetworkManager = true;
	}
	/*Server creation*/
    private void StartServer()
    {
		// Initialize the server on the network : 
		// InitializeServer(MaxPlayerAmount, PortNumber, Use NAT punchthrough if no public IP present)
        Network.InitializeServer(2, 25000, !Network.HavePublicAddress());
		// Register the host to the Master : ServerRegisterHost(UniqueGameName, RoomName)
        MasterServer.RegisterHost(typeName, gameName);
    }

	/*Actions when the server is succesfully initialized*/
    void OnServerInitialized()
    {
		//PlayerPrefs.SetString("type","host");
		isHost = true;
		SpawnPlayer();
    }


    void Update()
    {
		// If the list of host has been refreshed and isn't empty, the host list is stored
        if (isRefreshingHostList && MasterServer.PollHostList().Length > 0)
        {
            isRefreshingHostList = false;
            hostList = MasterServer.PollHostList();
        }
    }

	/*Send a request to the master server to get the list of host, contening all the data to join a server*/
    private void RefreshHostList()
    {
        if (!isRefreshingHostList)
        {
            isRefreshingHostList = true;
            MasterServer.RequestHostList(typeName);
        }
    }

	/*Join a server*/
    private void JoinServer(HostData hostData)
    {
        Network.Connect(hostData);
    }

	/*Function called after the player has actually joined the server*/
    void OnConnectedToServer()
    {
		//PlayerPrefs.SetString("type","client");
		isHost = false;
		SpawnPlayer();
    }
	private void SpawnPlayer()
	{
		if (isHost == true) {
			Network.Instantiate (player1Prefab, player1Prefab.transform.position + Vector3.left, player1Prefab.transform.rotation, 0);
		}
		else{
			Network.Instantiate (player2Prefab, player2Prefab.transform.position + Vector3.left, player2Prefab.transform.rotation, 0);
		}
	}

}
