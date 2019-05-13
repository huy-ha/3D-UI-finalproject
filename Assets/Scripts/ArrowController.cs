using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowController : MonoBehaviour
{
    TurnController TC;
    GameController GC;

    LineRenderer arrow;
    Vector3 direction;

    const float PERCENT_ARROW_HEAD = 0.1f;
    const float ARROW_WIDTH = 0.5f;

    void Start()
    {
        GC = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
        arrow = GetComponent<LineRenderer>();
    }
    

    public void OnAttackDown()
    {
        arrow.enabled = true;
    }

    public void OnAttackUp()
    {
        arrow.enabled = false;
        TC.Attack(direction); // Shoot grenade in the direction of the arrow
    }


    void Update()
    {
        if (!GC.GetGameHappening())
            arrow.enabled = false;


        else if (arrow.enabled)
        {
            // Obtain reference to TurnController (must be done late when TC is active)
            if (!TC)
                TC = GameObject.Find("TurnController").GetComponent<TurnController>();

            // Arrow drawn between player model and tracked wand
            else
            {
                Vector3 arrowTarget = TC.GetCurrentPlayer().gameObject.transform.position + new Vector3(0, .1f, 0);
                Vector3 arrowOrigin = transform.position;

                // Set line renderer interpolated widths
                arrow.widthCurve = new AnimationCurve(
                    new Keyframe(0, 0.1f),
                    new Keyframe(0.999f - PERCENT_ARROW_HEAD, 0.1f),
                    new Keyframe(1 - PERCENT_ARROW_HEAD, ARROW_WIDTH),
                    new Keyframe(1, 0f)
                );

                arrow.SetPositions(new Vector3[] {
                    transform.position,
                    Vector3.Lerp(arrowOrigin, arrowTarget, 0.999f - PERCENT_ARROW_HEAD),
                    Vector3.Lerp(arrowOrigin, arrowTarget, 1 - PERCENT_ARROW_HEAD),
                    arrowTarget
                
                });

                direction = arrowTarget - arrowOrigin;
            }
        }
    }
}
