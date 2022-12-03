using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Code.Scripts.UI.Rooms
{
    public class PlayerListingsMenu : MonoBehaviourPunCallbacks
    {
        [SerializeField] private Transform content;
        [SerializeField] private PlayerListing playerListing;
        [SerializeField] private Button startGameButton;

        private List<PlayerListing> _listings = new();


        public override void OnEnable()
        {
            startGameButton.interactable = PhotonNetwork.IsMasterClient;
            base.OnEnable();
            GetCurrentRoomPlayers();
        }

        public override void OnDisable()
        {
            base.OnDisable();
            foreach (var t in _listings)
            {
                Destroy(t.gameObject);
            }

            _listings.Clear();
        }

        private void GetCurrentRoomPlayers()
        {
            if (!PhotonNetwork.IsConnected) return;

            if (PhotonNetwork.CurrentRoom == null || PhotonNetwork.CurrentRoom.Players == null) return;

            foreach (KeyValuePair<int, Player> playerInfo in PhotonNetwork.CurrentRoom.Players)
            {
                AddPlayerListing(playerInfo.Value);
            }
        }

        private void AddPlayerListing(Player player)
        {
            int index = _listings.FindIndex(x => Equals(x.Player, player));
            if (index != -1)
            {
                _listings[index].SetPlayerInfo(player);
            }
            else
            {
                PlayerListing listing = Instantiate(playerListing, content);
                if (listing != null)
                {
                    listing.SetPlayerInfo(player);
                    _listings.Add(listing);
                }
            }
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            AddPlayerListing(newPlayer);
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            int index = _listings.FindIndex(x => Equals(x.Player, otherPlayer));

            if (index != -1)
            {
                Destroy(_listings[index].gameObject);
                _listings.RemoveAt(index);
            }
        }

        public void OnClickStartGame()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                SceneManager.LoadScene(1);
                //PhotonNetwork.LoadLevel(1);
                PhotonNetwork.CurrentRoom.IsOpen = false;
                PhotonNetwork.CurrentRoom.IsVisible = false;
            }
        }

        // IEnumerator MoveToGameScene()
        // {
        //     PhotonNetwork.IsMessageQueueRunning = false;
        //     
        //     PhotonNetwork.LoadLevel(1);
        //     PhotonNetwork.CurrentRoom.IsOpen = false;
        //     PhotonNetwork.CurrentRoom.IsVisible = false;
        //     
        //     while(newSceneDidNotFinishLoading)
        //     {
        //         yield return null;
        //     }
        //     PhotonNetwork.IsMessageQueueRunning = true;
        // }
    }
}