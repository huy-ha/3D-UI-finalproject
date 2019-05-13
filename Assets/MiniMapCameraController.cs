using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMapCameraController : MonoBehaviour
{
    PlayerController LocalPlayer;
    GameController GC;
    // Start is called before the first frame update
    void Start()
    {
        GC = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
    }

    // Update is called once per frame
    void Update()
    {
        if(LocalPlayer == null && GC.GetGameHappening())
        {
            LocalPlayer = GC.GetLocalPlayer();
        }
        else if (LocalPlayer != null)
        {
            transform.position = LocalPlayer.transform.position + LocalPlayer.transform.up * 1.45f;
            transform.LookAt(LocalPlayer.transform);
        }

    }
}
