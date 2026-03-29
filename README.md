# MornTweet

<p align="center">
  <img src="Editor/MornTweet.png" alt="MornTweet" width="640" />
</p>

<p align="center">
  <img src="https://img.shields.io/github/license/TsukumiStudio/MornTweet" alt="License" />
</p>

## 概要

Unity ゲームから Twitter/X にツイートするユーティリティです。
テキストのみ、またはスクリーンショット付き (freeimage.host 経由) で投稿できます。
外部パッケージへの依存はありません。

## 導入方法

Unity Package Manager で以下の Git URL を追加:

```
https://github.com/TsukumiStudio/MornTweet.git
```

### スクリーンショット付きツイートを使う場合

[freeimage.host API](https://freeimage.host/api) から API Key を取得してください。
2026年3月29日現在、アカウント登録不要で API Key を取得できます。

> **注意:** freeimage.host は外部の無料サービスです。サービスの停止・仕様変更・レート制限等について、本ライブラリの作者は一切の責任を負いません。

## 使い方

### コンポーネント

`MornTweetButton` を Button にアタッチし、Inspector で設定:

| プロパティ | 説明 |
|---|---|
| **Tweet Text** | ツイート本文 |
| **Hashtags** | ハッシュタグ (配列、#不要) |
| **Include Screenshot** | スクリーンショットを添付するか |
| **Api Key** | freeimage.host の API Key (スクリーンショット使用時のみ) |

### スクリプト

```csharp
using MornLib;

// テキストのみ
MornTweetService.Tweet("ゲームをプレイしました！", "MyGame,Unity");

// スクリーンショット付き (コルーチン)
StartCoroutine(MornTweetService.TweetWithScreenShotCoroutine(
    "ゲームをプレイしました！", "MyGame,Unity", "your_api_key"
));
```

## ライセンス

[The Unlicense](LICENSE)
