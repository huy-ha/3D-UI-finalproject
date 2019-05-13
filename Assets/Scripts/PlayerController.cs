using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.UI;

//TODO set up animations
public class PlayerController : NetworkBehaviour
{
    //Warfare variables
    int maxHealth = 100;
    float maxEnergy = 100f;
    [SyncVar]
    float currentHealth;
    [SyncVar]
    float currentEnergy;

    //Networking variables
    public bool _isLocalPlayer;
    [SyncVar]
    private bool _ready; //whether the Game server can start the game
    private bool prevReady;

    //PlayerVariables
    [SyncVar]
    string PlayerName;
    GameObject PlayerMesh;
    Animator anim;

    //Other Game variables
    GameController GC;
    AssetManager AM;
    TurnController TC;

    //Debug Variables
    ARDebugger d;

    //PlayerUI
    public GameObject PlayerNameText;
    GameObject PlayerUICanvas;
    public GameObject HealthBar;
    public GameObject EnergyBar;
    Slider HealthBarSlider;
    Slider EnergyBarSlider;
    public TextMeshProUGUI ReadyText;

    //Shooting
    public GameObject ShootingPoint;

    private void Awake()
    {
        currentHealth = maxHealth;
        currentEnergy = 0;
        //Init Player UI variables
        HealthBarSlider = HealthBar.GetComponent<Slider>();
        EnergyBarSlider = EnergyBar.GetComponent<Slider>();
    }

    private void Start()
    {
        GC = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
        d = GC.GetComponent<ARDebugger>();
        AM = GC.GetComponent<AssetManager>();
        TC = GameObject.Find("TurnController").GetComponent<TurnController>();
        //TC = GC.GetComponent<TurnController>();
        prevReady = false;
        PlayerUICanvas = PlayerNameText.transform.parent.gameObject;
        PlayerUICanvas.GetComponent<Canvas>().worldCamera = Camera.main;
        PlayerMesh = transform.GetChild(0).gameObject;
        anim = GetComponent<Animator>();
    }

    public void InitPlayerName(string name)
    {
        if (name == "") PlayerName = "Bob";
        else PlayerName = name;
        PlayerNameText.GetComponent<TextMeshProUGUI>().text = PlayerName;
    }

    [Command]
    public void CmdTakeDamage(float _amt)
    {
        currentHealth -= _amt;
    }

    public float GetEnergy()
    {
        return currentEnergy;
    }

    public void Update()
    {
        if (GC.GetGameHappening())
        {
            if(TC.GetCurrentPlayer() == this)
                anim.SetBool("Moving", TC.GetWalking());
            else
                anim.SetBool("Moving", false);
        }
        else
        {
            CheckReady();
            PlayerNameText.GetComponent<TextMeshProUGUI>().text = PlayerName;
        }

        PlayerUICanvas.transform.LookAt(Camera.main.transform);
        HealthBarSlider.value = Mathf.Lerp(HealthBarSlider.value, currentHealth / 100, Time.deltaTime*6);
        EnergyBarSlider.value = Mathf.Lerp(EnergyBarSlider.value, currentEnergy / 100, Time.deltaTime*6); 
    }

    [Command]
    //Player on Server is set to ready
    //_ready is SyncVar so syncs to all clients
    public void CmdSetReady(bool ready)
    {
        _ready = ready;
        if (GC.AreAllPlayersReady())
            CmdStartGame();
    }

    [Command]
    public void CmdDecreaseEnergy()
    {
        if(currentEnergy > 0)
        {
            currentEnergy -= Time.deltaTime * 20;
        }
    }

    public bool GetReady()
    {
        return _ready;
    }

    void CheckReady()
    {
        if (_ready != prevReady)
        {
            //User just updated ready
            ReadyText.text = _ready ? "Ready" : "Not Ready";
            ReadyText.color = _ready ? Color.green : Color.red;
            prevReady = _ready;
        }
    }

    public void SetReadyText(bool active)
    {
        ReadyText.gameObject.SetActive(active);
    }

    public string GetPlayerName()
    {
        return PlayerName;
    }

    #region Shooting
    [Command]
    public void CmdShoot(Vector3 direction)
    {
        //spawn grenade.
        GameObject Grenade = Instantiate(AM.Get("Grenade"));
        NetworkServer.Spawn(Grenade);
        Grenade.gameObject.name = "Grenade " + Grenade.GetComponent<NetworkIdentity>().netId.ToString();
        RpcShoot(Grenade, direction);
    }

    [ClientRpc]
    public void RpcShoot(GameObject Grenade, Vector3 direction)
    {
        GameObject Planet = AM.Get("Planet");
        Planet.GetComponent<Planet>().AddRigidbody(Grenade.GetComponent<Rigidbody>());
        Grenade.transform.parent = Planet.transform;
        Grenade.transform.position = TC.GetCurrentPlayer().ShootingPoint.transform.position;
        Vector3 F = direction;
        Vector3 n = TC.GetCurrentPlayer().transform.position - Planet.transform.position;
        n.Normalize();
        F = F + n;
        F = F * 30f;
        Grenade.GetComponent<Grenade>().Throw(F);
        TC.Shooting = true;
        print("Set Shooting to " + TC.Shooting);
    }
    #endregion

    #region Game Initializers
    //Tells server to start game
    [Command]
    private void CmdStartGame()
    {
        GameObject Planet = Instantiate(AM.Get("Planet"));
        NetworkServer.Spawn(Planet);
        Planet.gameObject.name = "Planet " + Planet.GetComponent<NetworkIdentity>().netId.ToString();
        RpcStartGame(Planet);
    }

    //Tells every client to start game
    [ClientRpc]
    private void RpcStartGame(GameObject Planet)
    {
        Planet.transform.parent = AssetManager.Instance.Get("GroundImageTarget").transform;
        AssetManager.Instance.PlanetPrefab = Planet;
        GC.StartGame(Planet);
    }
    #endregion

    #region TurnController Helpers
    [Command]
    public void CmdEndTurn(int currrentPlayer,int TimeStartTurn)
    {
        currentEnergy = 0;
        TC.GetPlayers()[currrentPlayer].currentEnergy = maxEnergy;
        RpcEndTurn(currrentPlayer, TimeStartTurn);
    }

    [ClientRpc]
    public void RpcEndTurn(int currrentPlayer, int TimeStartTurn)
    {
        TC.DoEndTurn(currrentPlayer, TimeStartTurn);
    }

    [Command]
    public void CmdSendPlayerTransform(Vector3 localPos,Quaternion localRot,string PlayerName)
    {
        RpcSendPlayerTransform(localPos,localRot, PlayerName);
    }
    [ClientRpc]
    public void RpcSendPlayerTransform(Vector3 localPos, Quaternion localRot, string PlayerName)
    {
        TC.UpdatePlayerNetworkTransforms(localPos, localRot, PlayerName);
    }
    #endregion

}
