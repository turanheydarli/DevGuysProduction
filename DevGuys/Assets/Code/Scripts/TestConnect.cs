using Code.Scripts.Managers;
using Code.Scripts.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

namespace Code.Scripts
{
    public class TestConnect : MonoBehaviourPunCallbacks
    {
        [Header("Login")] [SerializeField] private TMP_Text loginFormUsername;
        [SerializeField] private TMP_Text loginFormPassword;

        void Start()
        {
            PlayerPrefs.SetString("username", null);
            PlayerPrefs.SetString("password", null);
            StartGame();
        }

        private void StartGame()
        {
            UIManager.Instance.ShowLoadingPanel();

            string username = PlayerPrefs.GetString("username", null);
            string password = PlayerPrefs.GetString("password", null);
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                UIManager.Instance.ShowLoginPanel();
            }
            else
            {
                if (PhotonNetwork.IsConnected) return;
                PhotonNetwork.AutomaticallySyncScene = true;

                PhotonNetwork.GameVersion = MasterManager.GameSettings.GameVersion;
                PhotonNetwork.NickName = username;

                AuthenticationValues authValues = new AuthenticationValues
                {
                    AuthType = CustomAuthenticationType.Custom
                };

                authValues.AddAuthParameter("username", username);
                authValues.AddAuthParameter("password", password);

                authValues.AuthGetParameters = authValues.AuthGetParameters;
                PhotonNetwork.AuthValues = authValues;

                PhotonNetwork.ConnectUsingSettings();
            }
        }

        public void OnClickLoginButton()
        {
            UIManager.Instance.CloseLoginPanel();
            PlayerPrefs.SetString("username", loginFormUsername.text);
            PlayerPrefs.SetString("password", loginFormPassword.text);
            StartGame();
        }

        public override void OnConnectedToMaster()
        {
            PhotonNetwork.JoinLobby();
            UIManager.Instance.CloseLoadingPanel();
            Debug.Log($"{PhotonNetwork.LocalPlayer.NickName} is connected to server");
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            Debug.Log("Disconnected: " + cause);
        }

        public override void OnCustomAuthenticationFailed(string debugMessage)
        {
            PhotonNetwork.Disconnect();

            PlayerPrefs.SetString("username", null);
            PlayerPrefs.SetString("password", null);

            UIManager.Instance.ShowLoginErrorPanel(debugMessage);
        }

        // public override void OnCustomAuthenticationResponse(Dictionary<string, object> data)
        // {
        //    foreach(var item in data){
        //    Debug.Log(item.Key);
        //    }
        // }
    }
}