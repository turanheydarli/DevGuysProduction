using System;
using UnityEngine;

namespace Code.Scripts.UI.Rooms
{
    public class RoomsCanvases : MonoBehaviour
    {
        [SerializeField] private JoinRoomCanvas joinRoomCanvas;
        public JoinRoomCanvas JoinRoomCanvas => joinRoomCanvas;
        
        [SerializeField] private CurrentRoomCanvas currentRoomCanvas;
        public CurrentRoomCanvas CurrentRoomCanvas => currentRoomCanvas;

        private void Awake()
        {
            FirstInitialize();
        }

        private void FirstInitialize()
        {
            CurrentRoomCanvas.FirstInitialize(this);
            JoinRoomCanvas.FirstInitialize(this);
        }
    }
}
