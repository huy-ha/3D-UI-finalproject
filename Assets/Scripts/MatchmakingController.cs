using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Networking.Match;


public class MatchmakingController : MonoBehaviour
{
    NetworkManager networkManager;
    UIController ui;

    private const uint roomSize = 6;
    private string roomName;

    List<GameObject> roomList;
    public GameObject roomButtonPrefab;

    GameObject inputField;
    GameObject createMatchBtn;

    private void Awake()
    {
        roomName = "";
        roomList = new List<GameObject>();
    }

    void Start()
    {
        // Disable the default Unity HUD
        NetworkManagerHUD hud = FindObjectOfType<NetworkManagerHUD>();
        if (hud != null) {
            hud.showGUI = false;
        }

        // Set up network manager
        networkManager = NetworkManager.singleton;
        ui = GameObject.Find("GameController").GetComponentInChildren<UIController>();


        if (!networkManager.matchMaker)
            networkManager.StartMatchMaker();
    }

    /// <summary>
    /// Sets the name of the matchmaking room.
    /// </summary>
    /// <param name="newName">User inputted text</param>
    public void SetRoomName(string newName)
    {
        roomName = newName;
    }

    public void RefreshRoomList()
    {
        // Remove all rooms
        for (int i = 0; i < roomList.Count; ++i)
            Destroy(roomList[i]);

        roomList.Clear();

        networkManager.matchMaker.ListMatches(0, 20, "", true, 0, 0, OnMatchList);

    }

    public void OnMatchList(bool success, string extendedInfo, List<MatchInfoSnapshot> matches)
    {
        GameObject parent = GameObject.Find("Content");

        foreach (MatchInfoSnapshot match in matches)
        {
            // Add rooms
            GameObject roomBtn = Instantiate(roomButtonPrefab, parent.transform);
            string roomName = match.name + " (" + match.currentSize + "/" + match.maxSize + ")";
            roomBtn.GetComponent<Button>().onClick.AddListener(() =>
            {
                JoinMatch(match.networkId);
            });


            roomBtn.GetComponentsInChildren<Text>()[0].text = roomName;
            roomList.Add(roomBtn);
        }
    }

    void JoinMatch(UnityEngine.Networking.Types.NetworkID netId)
    {
        // Join match
        networkManager.matchMaker.JoinMatch(netId, "", "", "", 0, 0, networkManager.OnMatchJoined);
        // Disable menus
        ui.EnableBackground(false);
        ui.EnableJoinMenu(false);
        ui.SetWaitRoomPanel(true);

    }

    /// <summary>
    /// Create a matchmaking room with a given name and max number of players.
    /// </summary>
    public void CreateRoom()
    {
        print("Creating Room");
        // Rooms must be named
        if (roomName.Equals(""))
            return;

        Debug.Log("Creating lobby " + roomName + "...");
        networkManager.matchMaker.CreateMatch(roomName, roomSize, true, "", "", "", 0, 0, networkManager.OnMatchCreate);

        ui.EnableHostMenu(false);
        ui.EnableBackground(false);
        ui.SetWaitRoomPanel(true);
    }
}
