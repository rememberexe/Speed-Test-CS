# ⚡ Speed Test CS

C# ve WPF teknolojileri ile geliştirilmiş; modern, şık ve son derece premium bir arayüze sahip gerçek zamanlı internet ağ hızı test programıdır.

## ✨ Özellikler

* **Gerçek Zamanlı Ölçüm:** Ping, İndirme (Download) ve Yükleme (Upload) hızlarını asenkron olarak, canlı veri akışıyla ölçer.
* **Güçlü Altyapı:** Takılmaları ve engellemeleri önlemek adına doğrudan yüksek hızlı **Cloudflare** uç noktalarını (endpoints) kullanır.
* **Premium Arayüz:** Standart Windows çerçevelerinden arındırılmış (borderless), yuvarlatılmış köşeler ve pürüzsüz gölgelendirmelere sahip UI tasarımı.
* **Akıcı Animasyonlar:** Yumuşak açılış/kapanış efektleri, aktif menü belirteçleri ve test esnasında "nefes alan" (pulse) animasyonlu etkileşimler.
* **Çift Tema Desteği:** Tek tıklamayla anında uygulanan (programı yeniden başlatmaya gerek duymadan) **Koyu (OLED Dark)** ve **Açık (Light)** tema seçenekleri.
* **Çoklu Dil Desteği:** Ayarlar üzerinden anlık olarak değiştirilebilen **Türkçe** ve **İngilizce** arayüz.

## 📸 Ekran Görüntüleri

![Ana Sayfa (Koyu Tema)]( <img width="900" height="580" alt="1" src="https://github.com/user-attachments/assets/770447f3-c9ce-4678-a1d1-bb65bdcca6dd" />
 )
![Ayarlar (Açık Tema)]( <img width="900" height="580" alt="2" src="https://github.com/user-attachments/assets/c43f089b-7e9b-4a9f-a3ad-618f06e12750" />
 )



## 🚀 Kurulum ve Çalıştırma

1. Bu depoyu bilgisayarınıza klonlayın:
   ```bash
   git clone [https://github.com/KULLANICI_ADINIZ/SpeedTestCS.git](https://github.com/KULLANICI_ADINIZ/SpeedTestCS.git)
   ```
2. Klonladığınız klasördeki `SpeedTestCS.sln` çözüm dosyasını **Visual Studio** ile açın.
3. Projeyi derlemek ve çalıştırmak için üst menüden **Start (Başlat)** butonuna basın veya `F5` tuşunu kullanın.

## 📁 Proje Yapısı

```text
SpeedTestCS/
├── Core/
│   └── SpeedTestEngine.cs       # Cloudflare üzerinden gerçek zamanlı test yapan backend motoru
├── Dictionaries/
│   ├── StringResources.tr.xaml  # Türkçe metin sözlüğü
│   ├── StringResources.en.xaml  # İngilizce metin sözlüğü
│   ├── Theme.Dark.xaml          # OLED koyu tema renk paleti
│   └── Theme.Light.xaml         # Akıcı açık tema renk paleti
├── App.xaml                     # Uygulama başlangıcı ve resource haritalandırması
├── MainWindow.xaml              # SVG ikonlu, animasyonlu arayüz tasarımı
└── MainWindow.xaml.cs           # Sayfa geçişleri, tema/dil değişimi ve test yönetim mantığı
```

## 🛠️ Kullanılan Teknolojiler

* **C# / .NET:** Asenkron programlama, ağ işlemleri ve bellek yönetimi.
* **WPF (Windows Presentation Foundation):** Donanım hızlandırmalı UI, Vector/SVG render ve Storyboard animasyonları.
* **HttpClient & Custom Streams:** Ağa giden ve gelen paketlerin byte seviyesinde anlık olarak okunup UI thread'ine (Dispatcher) iletilmesi.
