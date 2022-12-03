using System;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using Code.Scripts.Game;
using Photon.Pun;
using TMPro;
using UnityEngine;

public class GameController : MonoBehaviourPunCallbacks
{
    [SerializeField] private List<Transform> playerTransforms;
    [SerializeField] private CinemachineVirtualCamera cinemachineVirtualCamera;

    private void Awake()
    {
        GameObject player = PhotonNetwork.Instantiate("Prefabs/Characters/Player",
            playerTransforms[PhotonNetwork.LocalPlayer.ActorNumber].position, Quaternion.identity);
        cinemachineVirtualCamera.m_Follow = player.transform.GetChild(2).transform;
    }

    public void FixedUpdate()
    {
       
    }
}