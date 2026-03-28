using System;
using System.Xml.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace MornLib
{
    /// <summary>
    /// スクリーンショットを撮影し、Imgur にアップロードして Twitter/X で共有する。
    /// </summary>
    public static class MornTweetService
    {
        /// <summary>
        /// スクリーンショットを撮影 → Imgur へアップロード → Twitter Intent URL を開く。
        /// </summary>
        /// <param name="tweetText">ツイート本文。アップロード成功時は画像URLが末尾に付与される。</param>
        /// <param name="hashtags">ハッシュタグ（カンマ区切り、#不要）。</param>
        /// <param name="imgurClientId">Imgur API の Client-ID。</param>
        public static async UniTask TweetWithScreenShotAsync(string tweetText, string hashtags, string imgurClientId)
        {
            await UniTask.WaitForEndOfFrame();
            var tex = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            tex.Apply();

            var uploadedUrl = "";
            try
            {
                var form = new WWWForm();
                form.AddField("image", Convert.ToBase64String(tex.EncodeToJPG()));
                form.AddField("type", "base64");
                using var www = UnityWebRequest.Post("https://api.imgur.com/3/image.xml", form);
                www.SetRequestHeader("AUTHORIZATION", $"Client-ID {imgurClientId}");
                await www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    var xDoc = XDocument.Parse(www.downloadHandler.text);
                    var url = xDoc.Element("data")?.Element("link")?.Value ?? "";
                    var lastDot = url.LastIndexOf('.');
                    if (lastDot >= 0) url = url[..lastDot];
                    uploadedUrl = url;
                }
                else
                {
                    Debug.LogError($"[MornTweet] Imgur upload failed: {www.error}");
                }
            }
            finally
            {
                UnityEngine.Object.Destroy(tex);
            }

            var body = string.IsNullOrEmpty(uploadedUrl)
                ? tweetText
                : $"{tweetText}\n{uploadedUrl}";
            var text = UnityWebRequest.EscapeURL(body);
            var tweetUrl = $"http://twitter.com/intent/tweet?text={text}&hashtags={hashtags}";
            MornWebUtil.Open(tweetUrl);
        }
    }
}
