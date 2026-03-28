# MornTweet

<p align="center">
  <img src="Editor/MornTweet.png" alt="MornTweet" width="640" />
</p>

<p align="center">
  <img src="https://img.shields.io/github/license/TsukumiStudio/MornTweet" alt="License" />
</p>

## 概要

Unity ゲームから Twitter/X にツイートするユーティリティです。
テキストのみ、またはスクリーンショット付き (Imgur 経由) で投稿できます。

## 導入方法

Unity Package Manager で以下の Git URL を追加:

```
https://github.com/TsukumiStudio/MornTweet.git
```

### 依存パッケージ

- [UniTask](https://github.com/Cysharp/UniTask)
- [MornWebUtil](https://github.com/TsukumiStudio/MornWebUtil)

## 使い方

### コンポーネント

`MornTweetButton` を Button にアタッチし、Inspector でテキスト・ハッシュタグ・画像添付の有無を設定してください。

### スクリプト

```csharp
using MornLib;

// テキストのみ
MornTweetService.Tweet("ゲームをプレイしました！", "MyGame,Unity");

// スクリーンショット付き
await MornTweetService.TweetWithScreenShotAsync(
    "ゲームをプレイしました！", "MyGame,Unity", "your_imgur_client_id"
);
```

## ライセンス

[The Unlicense](LICENSE)
