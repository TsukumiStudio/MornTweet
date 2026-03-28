using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace MornLib
{
    /// <summary>
    /// ボタン押下でツイートする。スクリーンショット添付の有無を切り替えられる。
    /// </summary>
    [RequireComponent(typeof(Button))]
    public sealed class MornTweetButton : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] [TextArea] private string _tweetText;
        [SerializeField] private string[] _hashtags;
        [SerializeField] private bool _includeScreenshot;
        [SerializeField] private string _imgurClientId;

        private bool _isTweeting;

        private void Awake()
        {
            _button.onClick.AddListener(OnClick);
        }

        private void OnDestroy()
        {
            _button.onClick.RemoveListener(OnClick);
        }

        private void Reset()
        {
            _button = GetComponent<Button>();
        }

        private void OnClick()
        {
            var hashtags = string.Join(",", _hashtags);
            if (_includeScreenshot)
            {
                TweetWithScreenShotAsync(hashtags).Forget();
            }
            else
            {
                MornTweetService.Tweet(_tweetText, hashtags);
            }
        }

        private async UniTaskVoid TweetWithScreenShotAsync(string hashtags)
        {
            if (_isTweeting) return;
            _isTweeting = true;
            try
            {
                await MornTweetService.TweetWithScreenShotAsync(_tweetText, hashtags, _imgurClientId);
            }
            finally
            {
                _isTweeting = false;
            }
        }
    }
}
