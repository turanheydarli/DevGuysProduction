using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

namespace Code.Scripts.UI.Rooms
{
    public class PlayerListing : MonoBehaviour
    {
        [SerializeField] private TMP_Text text;
        
        public Player Player { get; private set; }

        public void SetPlayerInfo(Player player)
        {
            Player = player;
            text.text = player.NickName;
        }
        
    }
}