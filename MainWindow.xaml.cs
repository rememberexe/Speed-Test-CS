using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media;
using SpeedTestCS.Core;

namespace SpeedTestCS
{
    public partial class MainWindow : Window
    {
        private SpeedTestEngine _engine;
        private CancellationTokenSource _cts;
        private bool _isTesting = false;
        private bool _isInitializing = true;

        public MainWindow()
        {
            InitializeComponent();
            _engine = new SpeedTestEngine();

            // Başlangıç değerleri (Dark ve TR seçili gelsin)
            RbDark.IsChecked = true;
            RbTr.IsChecked = true;
            _isInitializing = false;
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        // --- KAPANIŞ ANİMASYONU ---
        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_cts != null) _cts.Cancel();

            // Kapanış animasyonunu XAML'den bul ve çalıştır
            Storyboard closeAnim = (Storyboard)FindResource("WindowCloseAnim");
            closeAnim.Completed += (s, args) => Application.Current.Shutdown();
            closeAnim.Begin();
        }

        private void MinBtn_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        #region Animasyonlu Sayfa Geçişleri
        private void NavHome_Click(object sender, RoutedEventArgs e)
        {
            if (PageHome.Visibility == Visibility.Visible) return;
            FadeTransition(PageSettings, PageHome);
        }

        private void NavSettings_Click(object sender, RoutedEventArgs e)
        {
            if (PageSettings.Visibility == Visibility.Visible) return;
            FadeTransition(PageHome, PageSettings);
        }

        private void FadeTransition(UIElement from, UIElement to)
        {
            var fadeOut = new DoubleAnimation(0, TimeSpan.FromSeconds(0.2));
            fadeOut.Completed += (s, e) =>
            {
                from.Visibility = Visibility.Collapsed;
                to.Visibility = Visibility.Visible;
                var fadeIn = new DoubleAnimation(1, TimeSpan.FromSeconds(0.2));
                to.BeginAnimation(UIElement.OpacityProperty, fadeIn);
            };
            from.BeginAnimation(UIElement.OpacityProperty, fadeOut);
        }
        #endregion

        #region Ayarlar
        private void Theme_Changed(object sender, RoutedEventArgs e)
        {
            if (_isInitializing) return;
            App.ChangeTheme(RbDark.IsChecked == true);

            // Eğer butonda renk gradientleri kullanılıyorsa, tema değiştiğinde fırçaları güncellemek için App.xaml resource mapping'leri halledecektir.
        }

        private void Lang_Changed(object sender, RoutedEventArgs e)
        {
            if (_isInitializing) return;
            App.ChangeLanguage(RbEn.IsChecked == true);
        }
        #endregion

        #region Hız Testi Yönetimi
        private async void BtnStartTest_Click(object sender, RoutedEventArgs e)
        {
            if (_isTesting)
            {
                _cts?.Cancel();
                ResetUI();
                return;
            }

            _isTesting = true;
            BtnStartTest.Content = "DURDUR";
            StartPulseAnimation();

            TxtPing.Text = "--";
            TxtDownload.Text = "--";
            TxtUpload.Text = "--";

            TxtStatus.Text = (string)FindResource("StrTesting");
            TxtStatus.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation(1, TimeSpan.FromSeconds(0.3)));

            _cts = new CancellationTokenSource();

            try
            {
                long ping = await _engine.TestPingAsync();
                TxtPing.Text = ping > 0 ? ping.ToString() : "Hata";

                if (_cts.IsCancellationRequested) return;

                await _engine.TestDownloadAsync(speed =>
                {
                    Dispatcher.Invoke(() => TxtDownload.Text = speed > 0 ? speed.ToString("0.0") : "0.0");
                }, _cts.Token);

                if (_cts.IsCancellationRequested) return;

                await _engine.TestUploadAsync(speed =>
                {
                    Dispatcher.Invoke(() => TxtUpload.Text = speed > 0 ? speed.ToString("0.0") : "0.0");
                }, _cts.Token);

            }
            catch (Exception ex)
            {
                MessageBox.Show("Ağ bağlantısı koptu veya sunucuya ulaşılamıyor.\n\nDetay: " + ex.Message, "Test Hatası", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            finally
            {
                ResetUI();
            }
        }

        private void ResetUI()
        {
            _isTesting = false;

            Dispatcher.Invoke(() =>
            {
                BtnStartTest.Content = FindResource("StrStartTest");
                TxtStatus.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation(0, TimeSpan.FromSeconds(0.3)));

                // Nabız animasyonunu durdur
                BtnStartTest.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, null);
                BtnStartTest.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, null);
            });
        }

        private void StartPulseAnimation()
        {
            // Tatmin edici, yavaş bir nefes alma/kalp atışı efekti
            var pulse = new DoubleAnimation
            {
                To = 1.05,
                Duration = TimeSpan.FromSeconds(0.9),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever,
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
            };

            BtnStartTest.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, pulse);
            BtnStartTest.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, pulse);
        }
        #endregion
    }
}