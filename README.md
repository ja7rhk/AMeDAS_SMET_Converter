このアプリはSNOWPACKシミュレーションを行うため、CSV形式のアメダスをSMET形式に変換します。

アメダスの1時間毎の観測データは気象庁の過去の気象データ・ダウンロードからCSV形式でダウンロードできます。 
長い期間を指定できないので1ヶ月毎にダウンロードして、Excelでデータだけ編集して一つのファイルにし、これをSMET形式に変換します。 
このため、Visual C#で『AMeDAS_SMET_Converter』というアプリを作っています(Windows版のみ)。 
アメダスのデータには1時間毎の日照時間が計測されていますが、1時間当たりの日射(W/m^2)が必要なので、『日照時間を用いた時間積算日射量推定モデルの開発』に記載している式を使いました。
また、大気外日射量は『太陽方位、高度、大気外日射量の計算』のサイトの式を使って求めています。

![](converter.png)

