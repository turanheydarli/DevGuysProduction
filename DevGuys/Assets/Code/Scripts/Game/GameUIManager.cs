using Code.Scripts.Game;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour
{
    [SerializeField] public TMP_Text positionText;
    [SerializeField] public LeadboardListingMenu leadboardListingMenu;

    [SerializeField] private GameObject generalPanel;
    [SerializeField] private GameObject endPanel;
    [SerializeField] private Image endPanelSlider;

    [SerializeField] private GameObject goBackLobbyButton;


    public static GameUIManager Instance;

    private void Awake()
    {
        Instance ??= this;
    }

    public void SetPositionInfo(string position)
    {
        positionText.text = position;
    }

    public void OpenEndMenu(string username, int position)
    {
        generalPanel.SetActive(false);
        endPanel.SetActive(true);
        leadboardListingMenu.AddUserToLeadboard(username, position);

        goBackLobbyButton.SetActive(true);
    }

    public void GoBackLobby()
    {
        SceneManager.LoadScene(0);
    }
}