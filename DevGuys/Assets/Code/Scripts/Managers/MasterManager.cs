using UnityEngine;

namespace Code.Scripts.Managers
{
    [CreateAssetMenu(menuName = "Singletons/MasterManager", order = -51)]
    public class MasterManager : SingletonScriptableObject<MasterManager>
    {
        [SerializeField] private GameSettings gameSettings;
        public static GameSettings GameSettings => Instance.gameSettings;
        
    }
}