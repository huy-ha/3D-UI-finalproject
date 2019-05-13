using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;

//TODO change date time from system to unity engine
public class TurnController : NetworkBehaviour
{
    //Turn Variables
    [SyncVar]
    int currentPlayer;
    const int TURN_TIME_LIMIT = 30;
    [SyncVar]
    int TimeLeftInTurn;
    int TurnStartTime; //time when the current turn started
    bool Walking;
    public bool Shooting;

    //Player Network Variables
    Vector3 TargetLocalPos;
    Quaternion TargetLocalRot;
    string TargetPlayerName;
    bool HasEnergy;

    //Player Variables
    PlayerController[] Players;

    //Other Game Variables
    GameController GC;
    ARDebugger d;
    UIController UI;

    // Start is called before the first frame update
    void Start()
    {
        GC = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
        d = GC.gameObject.GetComponent<ARDebugger>();
        UI = GC.gameObject.GetComponent<UIController>();
        currentPlayer = -1;
        TurnStartTime = -1;
        HasEnergy = true;
        Shooting = false;
    }

    public void InitPlayers(LinkedList<PlayerController> PlayersList)
    {
        //Called when Game Starts
        Players = new PlayerController[PlayersList.Count];
        LinkedListNode<PlayerController> cur = PlayersList.First;
        for (int i = 0; i < PlayersList.Count; i++)
        {
            Players[i] = cur.Value;
            cur = cur.Next;
        }
        UI.SetTurnPanel(true);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        //Make sure that Turn Controller is Init
        if (Players != null && Players.Length > 0)
        {
            //Only update Time Left In Turn if you are the server
            if (isServer)
            {
                TimeLeftInTurn = TURN_TIME_LIMIT - (TurnStartTime - CurrentTime());
            }
            //If there is no current player, then init current player
            //If there is no time left in turn, then end the turn
            if ((currentPlayer == -1) ||
                TimeLeftInTurn < 0 ||
                Players[currentPlayer].GetEnergy() < 0)
            {
                if (HasEnergy)
                {
                    EndTurn();
                }
            }
            else
            {
                if (Shooting)
                {
                    //Player just threw grenade
                    UI.SetTurnTime(false);
                    UI.SetPlayerTurnPanel(false);
                }
                else
                {
                    //Player has now thrown grenade this turn (walking, standing).
                    UI.SetTurnTimeText(TimeLeftInTurn.ToString() + " seconds left");
                }
                HandleSyncTransforms();
            }
        }
    }


    #region Networking & Transformations
    void HandleSyncTransforms()
    {
        if (Players[currentPlayer]._isLocalPlayer)
        {
            if (Walking)
            {
                HandleWalk();
            }
            else
            {
                Players[currentPlayer].GetComponent<Rigidbody>().angularDrag = 2;
            }
            AlignPlayerWithCamera();
            //if is local player then send position to other clients via server
            Players[currentPlayer].CmdSendPlayerTransform(
                Players[currentPlayer].transform.localPosition,
                Players[currentPlayer].transform.localRotation,
                Players[currentPlayer].GetPlayerName()
                );
        }
        else
        {
            SyncCurrentPlayerTransform();
        }
        SpinPlanetToPlayer();
    }

    void HandleWalk()
    {
        Players[currentPlayer].GetComponent<Rigidbody>().angularDrag = 10f;
        Players[currentPlayer].transform.position = Players[currentPlayer].transform.position + GetCurrentPlayerForward() * Time.deltaTime;
        Players[currentPlayer].CmdDecreaseEnergy();
        //Straighten the player
        AssetManager.Instance.Get("Planet").GetComponent<Planet>().ClampPlayerUpright(Players[currentPlayer]);
    }

    public void UpdatePlayerNetworkTransforms(Vector3 localPos, Quaternion localRot, string PlayerName)
    {
        TargetLocalPos = localPos;
        TargetLocalRot = localRot;
        TargetPlayerName = PlayerName;
    }

    public void SyncCurrentPlayerTransform()
    {
        if (!Players[currentPlayer]._isLocalPlayer &&
            TargetLocalPos != null && TargetLocalRot != null && TargetPlayerName != null &&
            Players[currentPlayer].GetPlayerName() == TargetPlayerName)
        {
            if (Vector3.Distance(Players[currentPlayer].transform.localPosition, TargetLocalPos) > 0.01) //TODO experiment with changing value
            {
                Walking = true;
                Players[currentPlayer].transform.localPosition =
                    Vector3.Lerp(
                        Players[currentPlayer].transform.localPosition,
                        TargetLocalPos,
                        Time.deltaTime * 15
                        );
            }
            else
            {
                Walking = false;
                Players[currentPlayer].transform.localPosition = TargetLocalPos;
            }
            Players[currentPlayer].transform.localRotation = Quaternion.Slerp(
                Players[currentPlayer].transform.localRotation,
                TargetLocalRot,
                Time.deltaTime * 10
                );
        }
    }

    void SpinPlanetToPlayer()
    {
        GameObject planet = AssetManager.Instance.Get("Planet");
        Quaternion original = planet.transform.rotation;
        Vector3 n = Players[currentPlayer].transform.position - planet.transform.position;
        Quaternion target = Quaternion.FromToRotation(n, Vector3.up) * original;
        planet.transform.rotation = Quaternion.Slerp(original, target, Time.deltaTime * 2);
    }
    #endregion

    //returns time in terms of seconds
    private int CurrentTime()
    {
        return -(int)(System.DateTime.UtcNow.Ticks / 10000000);
    }

    public PlayerController[] GetPlayers()
    {
        return Players;
    }

    //Player's Local Device tells the Local Player Controller
    //To send a command to the server
    public void EndTurn()
    {
        int NewPlayer = currentPlayer;
        if (NewPlayer == -1) NewPlayer = 0;
        else NewPlayer = (NewPlayer + 1) % Players.Length;
        HasEnergy = false;
        Walking = false;
        //End Last Player's Turn
        if (currentPlayer == -1)
        {
            GC.GetLocalPlayer().CmdEndTurn(NewPlayer, CurrentTime());
        }
        else
        {
            Players[currentPlayer].CmdEndTurn(NewPlayer, CurrentTime());
        }
    }

    public void DoEndTurn(int curPlayer, int TimeStartTurn)
    {
        //Start Next Player's Turn
        currentPlayer = curPlayer;
        TurnStartTime = TimeStartTurn;
        HasEnergy = true;
        Walking = false;
        Shooting = false;
        PlayerController currentPlayerController = Players[currentPlayer];
        if (currentPlayerController == GC.GetLocalPlayer())
        {
            UI.SetTurnText("Your Turn");
            UI.SetPlayerTurnPanel(true);
        }
        else
        {
            UI.SetTurnText(currentPlayerController.GetPlayerName() + "'s Turn");
            UI.SetPlayerTurnPanel(false);
        }
        UI.SetTurnTime(true);
    }


    void AlignPlayerWithCamera()
    {
        Players[currentPlayer].transform.rotation = Quaternion.LookRotation(GetCurrentPlayerForward(), GetCurrentPlayerUp());
    }

    Vector3 GetCurrentPlayerForward()
    {
        Vector3 right = Vector3.Cross(
            Players[currentPlayer].transform.position - Camera.main.transform.position,
            GetCurrentPlayerUp());
        right.Normalize();
        Vector3 forward = Vector3.Cross(GetCurrentPlayerUp(), right);
        return forward.normalized;
    }

    Vector3 GetCurrentPlayerUp()
    {
        Vector3 up = Players[currentPlayer].transform.position - AssetManager.Instance.Get("Planet").transform.position;
        return up.normalized;
    }

    public void SetWalking(bool WalkButtonHoldDown)
    {
        Walking = WalkButtonHoldDown;
    }

    public bool GetWalking()
    {
        return Walking;
    }

    public void Attack(Vector3 direction)
    {
        Players[currentPlayer].CmdShoot(direction);
    }

    public PlayerController GetCurrentPlayer()
    {
        if (currentPlayer > -1 && currentPlayer < Players.Length)
            return Players[currentPlayer];
        else
            return null;
    }
}
