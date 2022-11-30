using DG.Tweening;
using UnityEngine;

namespace Code.Scripts.Game
{
    public class LeadboardListingMenu : MonoBehaviour
    {
        [SerializeField] private Transform content;
        [SerializeField] private LeadboardListing leadboardListing;

        public void AddUserToLeadboard(string username, int position)
        {
            LeadboardListing listing = Instantiate(leadboardListing, content);
            
            // DOTween.To(() => listing.transform.GetComponent<RectTransform>().DOMoveX(), x =>
            // {
            //     loadingSlider.value = x;
            //     loadingSliderText.text = $"{((int)(x * 100))}%";
            // }, 1, 5);
            //
            //
            // listing.transform.GetComponent<RectTransform>().DOMoveX(-150, 0);
            // listing.transform.GetComponent<RectTransform>().DOMoveX(150, 3);
                
            // listing.gameObject.SetActive(false);
            //
            // var sequence = DOTween.Sequence();
            //
            // sequence.Append(listing.transform.DOMoveX(-150, 0));
            // sequence.AppendCallback(() =>
            // {
            //     listing.gameObject.SetActive(true);
            // });
            //
            // sequence.Append(listing.transform.DOMoveX(150, 1));

            listing.SetBoardInfo(username, position);
        }
    }
}