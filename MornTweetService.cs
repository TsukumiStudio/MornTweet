using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;

namespace MornLib
{
    /// <summary>
    /// Twitter/X への共有ユーティリティ。テキストのみ、またはスクリーンショット付きで投稿できる。
    /// 画像アップロードには freeimage.host API を使用。
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
        /// スクリーンショットを撮影 → freeimage.host へアップロード → Twitter Intent URL を開く。
        /// MonoBehaviour.StartCoroutine で実行すること。
        /// </summary>
        /// <param name="tweetText">ツイート本文。アップロード成功時は画像URLが末尾に付与される。</param>
        /// <param name="hashtags">ハッシュタグ（カンマ区切り、#不要）。</param>
        /// <param name="apiKey">freeimage.host の API Key。</param>
        public static IEnumerator TweetWithScreenShotCoroutine(string tweetText, string hashtags, string apiKey)
        {
            yield return new WaitForEndOfFrame();
            var tex = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            tex.Apply();

            var uploadedUrl = "";
            var form = new WWWForm();
            form.AddField("key", apiKey);
            form.AddField("source", Convert.ToBase64String(tex.EncodeToJPG()));
            form.AddField("format", "json");
            using (var www = UnityWebRequest.Post("https://freeimage.host/api/1/upload", form))
            {
                yield return www.SendWebRequest();
                if (www.result == UnityWebRequest.Result.Success)
                {
                    var json = JsonUtility.FromJson<FreeImageResponse>(www.downloadHandler.text);
                    if (json?.image != null)
                    {
                        // ビューアページURL (OGP展開される) を優先、なければ直接URL
                        uploadedUrl = !string.IsNullOrEmpty(json.image.url_viewer)
                            ? json.image.url_viewer
                            : json.image.url;
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
        private class FreeImageResponse
        {
            public int status_code;
            public FreeImageData image;
        }

        [Serializable]
        private class FreeImageData
        {
            public string url;
            public string url_viewer;
        }
    }
}
