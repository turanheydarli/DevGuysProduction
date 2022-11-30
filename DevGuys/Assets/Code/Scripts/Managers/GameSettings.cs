using UnityEngine;

namespace Code.Scripts.Managers
{
    [CreateAssetMenu(menuName = "Singletons/GameSettings", order = -51)]
    public class GameSettings : ScriptableObject
    {
        [SerializeField] private string gameVersion;
        public string GameVersion => gameVersion;

        [SerializeField] private string username;
        public string Username => username + Random.Range(0, 9999);
    }
}