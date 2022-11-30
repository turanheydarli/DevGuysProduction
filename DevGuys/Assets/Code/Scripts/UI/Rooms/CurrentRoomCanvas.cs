using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Code.Scripts.UI.Rooms
{
    public class CurrentRoomCanvas : MonoBehaviour
    {
        private RoomsCanvases _roomsCanvases;

        public void FirstInitialize(RoomsCanvases canvases)
        {
            _roomsCanvases = canvases;
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void StartGame()
        {
            SceneManager.LoadScene(1);
        }
    }
}