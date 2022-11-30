using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

namespace Code.Scripts.UI.Rooms
{
    public class RoomListing : MonoBehaviour
    {
        [SerializeField] private TMP_Text roomNameText;
        [SerializeField] private TMP_Text roomInfoText;
        [SerializeField] private TMP_Text roomIdText;

        public RoomInfo RoomInfo { get; private set; }

        public void SetRoomInfo(RoomInfo roomInfo)
        {
            RoomInfo = roomInfo;
            roomNameText.text = roomInfo.Name;
            roomInfoText.text = $"{roomInfo.PlayerCount}/{roomInfo.MaxPlayers}";
            roomIdText.text = roomInfo.masterClientId.ToString();
        }

        public void OnClickButton()
        {
            PhotonNetwork.JoinRoom(RoomInfo.Name);
        }
    }
}