using UnityEngine;

namespace Code.Scripts.UI.Rooms
{
    public class JoinRoomCanvas : MonoBehaviour
    {
        [SerializeField] private CreateRoomMenu createRoomMenu;
        [SerializeField] private RoomListingsMenu roomListingsMenu;

        private RoomsCanvases _roomsCanvases;

        public void FirstInitialize(RoomsCanvases canvases)
        {
            _roomsCanvases = canvases;
            createRoomMenu.FirstInitialize(canvases);
            roomListingsMenu.FirstInitialize(canvases);
        }
    }
}