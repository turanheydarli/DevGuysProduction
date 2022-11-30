using System;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

namespace Code.Scripts.UI.Rooms
{
    public class CreateRoomMenu : MonoBehaviourPunCallbacks
    {
        [SerializeField] private TMP_Text roomName;
        
        private RoomsCanvases _roomsCanvases;
        public void FirstInitialize(RoomsCanvases canvases)
        {
            _roomsCanvases = canvases;
        }

        private void OnValidate()
        {
            if (roomName.text.Length > 10)
            {
                roomName.text = String.Empty;
            }
        }

        public void OnClickCreateRoom()
        {
            if (!PhotonNetwork.IsConnected) return;

            RoomOptions options = new RoomOptions
            {
                MaxPlayers = 10
            };

            PhotonNetwork.JoinOrCreateRoom(roomName.text, options, TypedLobby.Default);
        }

        public override void OnCreatedRoom()
        {
            Debug.Log("Room created successfully.");
            _roomsCanvases.CurrentRoomCanvas.Show();
        }

        public override void OnCreateRoomFailed(short returnCode, string message)
        {
            Debug.Log("Room creation failed: " + message);
        }
    }
}