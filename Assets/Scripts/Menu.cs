using UnityEngine;
using System.Collections;

public class Menu : MonoBehaviour {
	public GameObject networkManager;

	private int width = Screen.width;
	private int height = Screen.height;

	private Texture2D blackTexture;
	private GUIStyle styleMenu = new GUIStyle();

	enum MenuState { MainMenu, NetworkManager, Play };
	private MenuState currentMenu = MenuState.MainMenu;


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

		//Initiate the PlayerPref mode
		if (!PlayerPrefs.HasKey("mode")){
			PlayerPrefs.SetString("mode","solo");
		}

	}

	void OnGUI()
	{
		// Display MainMenu
		if (currentMenu == MenuState.MainMenu)
		{
			// Background
			GUI.Box(new Rect (0 , 0, width, height),blackTexture,styleMenu);

			// Solo mode button
			if (GUI.Button(new Rect(100, 100, 250, 100), "Solo")){
				PlayerPrefs.SetString("mode","solo");
				Play();
			}
			// Multiplayer mode button
			if (GUI.Button(new Rect(100, 250, 250, 100), "Multiplayer"))
			{
				PlayerPrefs.SetString("mode","multi");
				networkManager.GetComponent<NetworkManager>().setDisplayNetworkManager();
				Play();
			}
		}
		else if(currentMenu == MenuState.Play){
			GUI.Box(new Rect (0 , 0, width, height),blackTexture,styleMenu);
		}
	}
	
	public void Play(){
		currentMenu = MenuState.Play;
		if(PlayerPrefs.GetString ("mode") == "multi") {
			// The networkManager won't be destroy
			DontDestroyOnLoad (networkManager);
		}

		Application.LoadLevel ("TestGame");
	}

	// Update is called once per frame
	void Update () {
	
	}
}
