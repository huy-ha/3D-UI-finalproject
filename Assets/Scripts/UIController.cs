using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    GameObject WaitRoomPanel;
    GameObject TurnPanel;
    GameObject PlayerTurnPanel;
    TextMeshProUGUI TurnText;
    TextMeshProUGUI TurnTimeText;

    GameObject StartMenu;
    GameObject JoinMenu;
    GameObject HostMenu;

    GameObject MyProfile;

    // so it can be set hidden during edit mode
    [SerializeField]
    GameObject Panels;
    [SerializeField]
    GameObject Menus;
    [SerializeField]
    GameObject Background;
    private void Init()
    {
        // Locate corresponding GameObjects in the scene
        Panels.SetActive(true);
        Menus.SetActive(true);
        Background.SetActive(true);
        StartMenu = GameObject.Find("StartMenu");
        JoinMenu = GameObject.Find("JoinMenu");
        HostMenu = GameObject.Find("HostMenu");

        WaitRoomPanel = GameObject.Find("WaitRoomPanel");
        TurnPanel = GameObject.Find("TurnPanel");
        PlayerTurnPanel = GameObject.Find("PlayerTurnPanel");

        TurnText = TurnPanel.GetComponentsInChildren<TextMeshProUGUI>()[0];
        TurnTimeText = TurnPanel.GetComponentsInChildren<TextMeshProUGUI>()[1];

        MyProfile = GameObject.Find("MyProfile");

        // Set up event handlers
        GameObject.Find("My Profile").GetComponent<Button>().onClick.AddListener(() =>
        {
            EnableMyProfile(true);
            EnableStartMenu(false);
        });

        GameObject.Find("Join Game").GetComponent<Button>().onClick.AddListener(() =>
        {
            EnableJoinMenu(true);
            EnableStartMenu(false);
        });

        GameObject.Find("Host Game").GetComponent<Button>().onClick.AddListener(() =>
        {
            EnableHostMenu(true);
            EnableStartMenu(false);
        });

    }

    public void GoBack()
    {
        EnableHostMenu(false);
        EnableJoinMenu(false);
        EnableMyProfile(false);
        EnableStartMenu(true);
        EnableBackground(true);
    }

    private void Start()
    {
        Init();
        EnableStartMenu(true);

        // Menus disabled at start
        EnableMyProfile(false);
        EnableHostMenu(false);
        EnableJoinMenu(false);
        WaitRoomPanel.SetActive(false);
        TurnPanel.SetActive(false);
        PlayerTurnPanel.SetActive(false);
    }

    public void EnableStartMenu(bool active)
    {
        StartMenu.SetActive(active);
    }

    public void EnableBackground(bool active)
    {
        Background.SetActive(active);
    }

    public void EnableMyProfile(bool active)
    {
        MyProfile.SetActive(active);
    }

    public void EnableJoinMenu(bool active)
    {
        JoinMenu.SetActive(active);
    }

    public void EnableHostMenu(bool active)
    {
        HostMenu.SetActive(active);
    }

    public void SetWaitRoomPanel(bool active)
    {
        WaitRoomPanel.SetActive(active);
    }

    public void SetTurnPanel(bool active)
    {
        TurnPanel.SetActive(active);
    }

    public void SetTurnText(string s)
    {
        TurnText.text = s;
    }

    public void SetTurnTimeText(string s)
    {
        TurnTimeText.text = s;
    }

    public void SetPlayerTurnPanel(bool active)
    {
        PlayerTurnPanel.SetActive(active);
    }

    public void SetTurnTime(bool active)
    {
        TurnTimeText.gameObject.SetActive(active);
    }
}
