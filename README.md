<div align=center>
<img src="ConvertZZ/icon.ico"><br>
<h1>ConvertZZ</h1>
  
<b>非官方分支，原項目鏈接</b>
[ConvertZZ](https://github.com/flier268/ConvertZZ)

![Release](https://img.shields.io/github/v/release/zouhuidong/ConvertZZ)
[![Blog](https://img.shields.io/badge/blog-huidong.xyz-green.svg)](http://huidong.xyz)
![Stars](https://img.shields.io/github/stars/zouhuidong/ConvertZZ)
![Forks](https://img.shields.io/github/forks/zouhuidong/ConvertZZ)
![Downloads](https://img.shields.io/github/downloads/zouhuidong/ConvertZZ/total)
</div><br>

## 鄭重聲明
秦王掃六合，虎視何雄哉。而今稱二國，無乃豎子乎！

## 簡介

ConvertZZ 是一款 Windows 上的簡繁轉換工具，原作者 flier268，前身是 ConvertZ。現在 flier268 發佈的最後一個版本停留在 2019 年，此後未有更新，故 huidong 接替之。

**特色**
1. 可使用 OpenCC 簡繁轉換引擎
2. 支持帶格式文本內容轉換（CF_HTML 格式）
3. 可以設置快捷鍵（默認 `Ctrl + /` 和 `Ctrl + Shift + /`）一鍵進行剪切板簡繁轉換
4. 可以進行文件簡繁體批量轉換

**其它功能**
1. 文件編碼轉換
2. MP3 Tag 轉換
3. ……

## ConvertZ 的故事

「ConvertZ」是一款 Windows 上的簡繁轉換工具，作者李志成，以其功能多樣、簡潔實用而頗有名氣，但該軟件已有二十年未有更新，最後一個版本是 V8.02（2005 年發佈）。

ConvertZ 本身並不開源，爲修正 ConvertZ 的 bug，flier268 用 C# 重新寫了 [ConvertZZ](https://github.com/flier268/ConvertZZ)，修復了 ConvertZ 的若干 bug，實現了 ConvertZZ 90% 的功能，並開源了代碼。 

但現在 ConvertZZ 也不再爲原作者所維護了，最後一個發佈版本停留在 2019 年。其功能猶待完善。

鑑 flier268 之 [ConvertZZ](https://github.com/flier268/ConvertZZ) 久未更新，故我基於 v1.0.0.8 爲其繼續維護。自 v1.0.1.0 始，乃非官方之更新也。

v1.0.1.0 重點更新內容：

* 支持 OpenCC 轉換引擎

* 支持剪切板 CF_HTML 轉換（即轉換時保留網頁內容格式）

**爲什麼要加入 OpenCC 轉換引擎**

ConvertZZ 自帶的詞典轉換不夠專業，轉換的問題不少（見 ConvertZZ 之 issue 甚多可知矣）。本來簡繁轉換就有做得很好的引擎 OpenCC，何不用之？

用家可以打開 OpenCC 配置面板，自定義 OpenCC 轉換偏好。

![1738215874141875](https://github.com/user-attachments/assets/a26ca57e-b370-47b2-b9cb-78e73f4171be)

我在發佈版的 ConvertZZ 文件夾中附上了 OpenCC 自帶的所有配置文件（.json 和 .ocd2），方便用戶各取所需，如下圖所示（部分文件截圖）：

![图片](https://github.com/user-attachments/assets/54c05cc0-bd70-4b05-9a0a-57428087d795)

您若需要臺灣用字轉換，就選取 `s2tw.json` 作爲 Unicode 簡轉繁的配置文件，`tw2s.json` 作爲繁轉簡的配置文件，至於其他地區用字，自可知之矣。

## 贊助原作者

用家若感不錯，不妨贊助一下原作者。

* [Paypal](http://paypal.me/flier268)(台灣以外地區)
* [街口支付](https://i.imgur.com/IKowON0.png)(台灣地區)
