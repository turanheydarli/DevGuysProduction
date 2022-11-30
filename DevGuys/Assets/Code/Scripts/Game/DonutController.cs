using DG.Tweening;
using UnityEngine;

namespace Code.Scripts.Game
{
    public class DonutController : MonoBehaviour
    {
        [SerializeField] private float duration = 5;
        [SerializeField] private float goTo = -27;

        void Start()
        {
            transform.DOMoveX(goTo, duration).SetLoops(-1, LoopType.Yoyo);
        }
    }
}