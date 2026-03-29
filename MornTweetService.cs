using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;

namespace MornLib
{
    /// <summary>
    /// Twitter/X への共有ユーティリティ。テキストのみ、またはスクリーンショット付きで投稿できる。
    /// 画像アップロードには imgBB API を使用。
    /// </summary>
    public static class MornTweetService
    {
#if UNITY_WEBGL
        [DllImport("__Internal")]
        private static extern void MornTweetOpenWindow(string url);
#endif

        /// <summary>
        /// Twitter Intent URL を開いてツイートする（テキストのみ）。
        /// </summary>
        /// <param name="tweetText">ツイート本文。</param>
        /// <param name="hashtags">ハッシュタグ（カンマ区切り、#不要）。</param>
        public static void Tweet(string tweetText, string hashtags)
        {
            OpenTweetIntent(tweetText, hashtags);
        }

        /// <summary>
        /// スクリーンショットを撮影 → imgBB へアップロード → Twitter Intent URL を開く。
        /// MonoBehaviour.StartCoroutine で実行すること。
        /// </summary>
        /// <param name="tweetText">ツイート本文。アップロード成功時は画像URLが末尾に付与される。</param>
        /// <param name="hashtags">ハッシュタグ（カンマ区切り、#不要）。</param>
        /// <param name="apiKey">imgBB の API Key。</param>
        public static IEnumerator TweetWithScreenShotCoroutine(string tweetText, string hashtags, string apiKey)
        {
            yield return new WaitForEndOfFrame();

            var tex = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            tex.Apply();

            var uploadedUrl = "";
            var form = new WWWForm();
            form.AddField("image", Convert.ToBase64String(tex.EncodeToJPG()));
            using (var www = UnityWebRequest.Post($"https://api.imgbb.com/1/upload?key={apiKey}", form))
            {
                yield return www.SendWebRequest();
                if (www.result == UnityWebRequest.Result.Success)
                {
                    var json = JsonUtility.FromJson<ImgBBResponse>(www.downloadHandler.text);
                    if (json?.data != null)
                    {
                        uploadedUrl = !string.IsNullOrEmpty(json.data.url_viewer)
                            ? json.data.url_viewer
                            : json.data.url;
                    }
                }
                else
                {
                    Debug.LogError($"[MornTweet] Image upload failed: {www.error}");
                }
            }

            UnityEngine.Object.Destroy(tex);
            var body = string.IsNullOrEmpty(uploadedUrl) ? tweetText : $"{tweetText}\n{uploadedUrl}";
            OpenTweetIntent(body, hashtags);
        }

        private static void OpenTweetIntent(string tweetText, string hashtags)
        {
            var text = Uri.EscapeDataString(tweetText);
            var tags = Uri.EscapeDataString(hashtags);
            OpenUrl($"https://x.com/intent/post?text={text}&hashtags={tags}");
        }

        private static void OpenUrl(string url)
        {
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
#if UNITY_WEBGL
                MornTweetOpenWindow(url);
#endif
            }
            else
            {
#if UNITY_EDITOR
                System.Diagnostics.Process.Start(url);
#else
                Debug.Log($"[MornTweet] {url}");
#endif
            }
        }

        [Serializable]
        private class ImgBBResponse
        {
            public bool success;
            public ImgBBData data;
        }

        [Serializable]
        private class ImgBBData
        {
            public string url;
            public string url_viewer;
        }
    }
}
