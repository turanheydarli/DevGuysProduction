using Code.Scripts.Game;
using UnityEngine;

public class FinishController : MonoBehaviour
{
    [SerializeField] private Transform rayTrans;
    int winnerCount = 0;

    void Update()
    {
        RaycastHit hit;

        if (Physics.Raycast(rayTrans.position, rayTrans.forward, out hit, 100f))
        {
            Debug.DrawRay(rayTrans.position, rayTrans.forward * 100f, Color.green);
            if (hit.transform.CompareTag("Player"))
            {
                winnerCount++;
                Debug.Log(hit.transform.name);
                PlayerController player = hit.transform.GetComponent<PlayerController>();
                hit.transform.tag = "End";
                GameUIManager.Instance.OpenEndMenu(player.photonView.Owner.NickName, winnerCount);
            }
        }
    }
}