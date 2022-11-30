using Photon.Realtime;
using TMPro;
using UnityEngine;

namespace Code.Scripts.Game
{
    public class LeadboardListing : MonoBehaviour
    {
        [SerializeField] private TMP_Text positionText;
        [SerializeField] private TMP_Text usernameText;

        public void SetBoardInfo(string username, int position)
        {
            usernameText.text = username;
            positionText.text = position.ToString();
        }
    }
}