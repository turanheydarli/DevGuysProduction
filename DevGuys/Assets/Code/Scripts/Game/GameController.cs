using System.Collections.Generic;
using Cinemachine;
using Photon.Pun;
using UnityEngine;

public class GameController : MonoBehaviourPunCallbacks
{
    [SerializeField] private List<Transform> playerTransforms;
    [SerializeField] private CinemachineVirtualCamera cinemachineVirtualCamera;

    private void Awake()
    {
        GameObject player = PhotonNetwork.Instantiate("Prefabs/Characters/TurtleGuy",
            playerTransforms[PhotonNetwork.LocalPlayer.ActorNumber].position, Quaternion.identity);
        cinemachineVirtualCamera.m_Follow = player.transform.GetChild(2).transform;
    }
}