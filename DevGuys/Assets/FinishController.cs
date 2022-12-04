using Code.Scripts.Game;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ToonyColorsPro.ShaderGenerator.Enums;

public class FinishController : MonoBehaviour
{
    [SerializeField] private Transform rayTrans;
    [SerializeField] LayerMask layer;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        
        if (Physics.Raycast(rayTrans.position, rayTrans.forward, out hit, 100f))
        {
            Debug.DrawRay(rayTrans.position, rayTrans.forward*100f, Color.green);
            if (hit.transform.CompareTag("Player"))
            {
                Debug.Log(hit.transform.name);
                PlayerController player = hit.transform.GetComponent<PlayerController>();
                hit.transform.tag = "End";
                GameUIManager.Instance.OpenEndMenu(player._photonView.Owner.NickName, player._leaderboard.IndexOf(player.onPath.z) + 1);
            }
        }
        
    }


}
