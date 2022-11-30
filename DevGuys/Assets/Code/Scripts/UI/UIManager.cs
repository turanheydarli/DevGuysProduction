using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Code.Scripts.UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance;

        private void Awake()
        {
            Instance ??= this;
        }

        [Header("Loading")] [SerializeField] private GameObject loadingPanel;
        [SerializeField] private Slider loadingSlider;
        [SerializeField] private TMP_Text loadingSliderText;

        [Header("Login/Register")] [SerializeField]
        private GameObject loginPopup;

        [SerializeField] private GameObject loginErrorPopup;
        [SerializeField] private TMP_Text loginErrorMessageText;

        [SerializeField] private GameObject registerPopup;
        [SerializeField] private TMP_Text registerFormUsername;
        [SerializeField] private TMP_Text registerFormPassword;




        public void ShowLoadingPanel()
        {
            loadingPanel.SetActive(true);
            loadingSlider.value = 0;
            DOTween.To(() => loadingSlider.value, x =>
            {
                loadingSlider.value = x;
                loadingSliderText.text = $"{((int)(x * 100))}%";
            }, 1, 5);
        }

        public void CloseLoadingPanel()
        {
            loadingPanel.SetActive(false);
        }

        public void ShowLoginPanel()
        {
            loginPopup.SetActive(true);
        }

        public void CloseLoginPanel()
        {
            loginPopup.SetActive(false);
        }

        public void ShowLoginErrorPanel(string message)
        {
            loginErrorMessageText.text = message;
            loginErrorPopup.SetActive(true);
        }

        public void CloseLoginErrorPanel()
        {
            loginErrorPopup.SetActive(false);
            loginPopup.SetActive(true);
        }

        public void ShowRegisterPanel()
        {
            registerPopup.SetActive(true);
            loginPopup.SetActive(false);
        }

        public void CloseRegisterPanel()
        {
            registerPopup.SetActive(false);
        }

        public void OnClickRegisterButton()
        {
            StartCoroutine(Register(registerFormUsername.text, registerFormPassword.text));
        }

        IEnumerator Register(string username, string password)
        {
            using UnityWebRequest www =
                UnityWebRequest.Post($"https://auth.istu.run/register?username={username}&password={password}", "");

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                CloseRegisterPanel();
                ShowLoginErrorPanel(www.error);
            }
            else
            {
                ShowLoginPanel();
                CloseRegisterPanel();
            }
        }

       
    }
}