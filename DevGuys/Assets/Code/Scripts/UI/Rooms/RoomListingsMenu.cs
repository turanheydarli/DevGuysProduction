using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace Code.Scripts.UI.Rooms
{
    public class RoomListingsMenu : MonoBehaviourPunCallbacks
    {
        [SerializeField] private Transform content;
        [SerializeField] private RoomListing roomListing;
        [SerializeField] private GameObject roomListingsMenu;

        private readonly List<RoomListing> _listings = new();

        private RoomsCanvases _roomsCanvases;

        public void FirstInitialize(RoomsCanvases canvases)
        {
            _roomsCanvases = canvases;
        }

        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            foreach (RoomInfo info in roomList)
            {
                if (info.RemovedFromList)
                {
                    int index = _listings.FindIndex(x => x.RoomInfo.Name == info.Name);
                    if (index != -1)
                    {
                        Destroy(_listings[index].gameObject);
                        _listings.RemoveAt(index);
                    }
                }
                else
                {
                    RoomListing listing = Instantiate(roomListing, content);
                    listing.SetRoomInfo(info);
                    _listings.Add(listing);
                }
            }
        }

        public override void OnJoinedRoom()
        {
            _roomsCanvases.CurrentRoomCanvas.Show();
        }


        public void OnClickLeaveRoom()
        {
            PhotonNetwork.LeaveRoom(true);
            _roomsCanvases.CurrentRoomCanvas.Hide();
        }
        
        public void OnClickBackToHome()
        {
            roomListingsMenu.SetActive(false);
        }
    }
}