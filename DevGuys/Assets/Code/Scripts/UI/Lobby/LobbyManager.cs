using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;

namespace Code.Scripts.UI.Lobby
{
    public class LobbyManager : MonoBehaviourPunCallbacks
    {
        [Header("User Data")] [SerializeField] private TMP_Text usernameText;


        [SerializeField] private GameObject roomListingsMenu;

        public void OnClickPlayButton()
        {
            roomListingsMenu.SetActive(true);
        }

        public override void OnCustomAuthenticationResponse(Dictionary<string, object> data)
        {
            if (usernameText == null) return;
            usernameText.text = data["username"]?.ToString();
        }
    }
}