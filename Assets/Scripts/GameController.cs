using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GameController : MonoBehaviour
{
    //Players Variables
    const string PLAYER_ID_PREFIX = "Player ";
    static Dictionary<string, PlayerController> Players = new Dictionary<string, PlayerController>();
    static LinkedList<PlayerController> PlayersList = new LinkedList<PlayerController>();
    PlayerController LocalPlayer;

    //Other Game Variables
    UIController UI;
    ARDebugger d;
    private static bool GameHappening; //if GameIsHappening, then no new players can join

    // Start is called before the first frame update
    private void Awake()
    {
        GameHappening = false;
        LocalPlayer = null;
    }
    void Start()
    {
        UI = GetComponent<UIController>();
        d = GetComponent<ARDebugger>();
    }

    public static PlayerController GetPlayer(string _ID)
    {
        return Players[_ID];
    }

    public static void RegisterPlayer(string _ID, PlayerController _player)
    {
        if (!GameHappening)
        {
            string _playerId = PLAYER_ID_PREFIX + _ID;
            Players.Add(_playerId, _player);
            PlayersList.AddLast(_player);
            _player.transform.name = _playerId;
            _player.gameObject.transform.parent = AssetManager.Instance.Get("GroundImageTarget").transform;
            return;
        }
        //TODO handle kicking the new player out of the server
       
    }

    public static void UnRegisterPlayer(string _ID)
    {
        PlayerController p;
        if (Players.TryGetValue(_ID, out p))
        {
            Players.Remove(_ID);
            PlayersList.Remove(p);
        }
    }

    public void ToggleReady()
    {
        //Toggles local player ready
        //finds local player if don't have reference to it yet
        if (!GameHappening)
        {
            if (LocalPlayer == null)
            {
                foreach (PlayerController p in PlayersList)
                {
                    if (p._isLocalPlayer)
                    {
                        LocalPlayer = p;
                    }
                }
            }
            if (LocalPlayer != null) {
                LocalPlayer.CmdSetReady(!LocalPlayer.GetReady());
            }
        }
    }

    public bool AreAllPlayersReady()
    {
        bool allPlayersReady = true;
        foreach(PlayerController p in PlayersList)
        {
            if (!p.GetReady())
            {
                allPlayersReady = false;
            }
        }
        return allPlayersReady;
    }

    //Called from PlayerController RpcStartGame
    //Since there is only one instance of GC
    //When localPlayer calls GC, it affects 
    //all clients games
    public void StartGame(GameObject Planet)
    {
        UI.SetWaitRoomPanel(false);
        GameHappening = true;
        GameObject.Find("TurnController").GetComponent<TurnController>().InitPlayers(PlayersList);
        //GetComponent<TurnController>().InitPlayers(PlayersList);
    }

    void StopGame()
    {
        UI.SetWaitRoomPanel(true);
        GameHappening = false;
    }

    public bool GetGameHappening()
    {
        return GameHappening;
    }

    //Called by server to find the correct player, change that player's name
    //PlayerName is SyncVar, so is updated on all clients
    public void InitPlayerName(string name,string _playerID)
    {
        GetPlayer(PLAYER_ID_PREFIX + _playerID).InitPlayerName(name);
    }

    public PlayerController GetLocalPlayer()
    {
        return LocalPlayer;
    }
}
