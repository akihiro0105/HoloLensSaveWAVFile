# SaveWAVFile
- Twitter : [@akihiro01051](https://twitter.com/akihiro01051)

## 動作環境
- Windows10 Creators Update
- Unity 2017.4
- Visual Studio 2017
- HoloLens RS4
- Windows MixedReality Device

----------

## 概要
- 音声データをWAVファイルに保存，再生できる
- ネットワーク越しの音声データ送受信のためのStreamデータ送受信

## 利用パッケージ
- [HoloLensModule](https://github.com/akihiro0105/HoloLensModule)

## 内容
- SaveWAVFileSample
    + マイクからの音声を5秒間録音しWAVファイルで保存
    + 保存されたWAVファイルを再生

- Streamによる音声データの送受信はbyteリストの生成とbyteリストからの再生機能のみ提供
