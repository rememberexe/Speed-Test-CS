using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;

namespace SpeedTestCS.Core
{
    public class SpeedTestEngine
    {
        // Cloudflare'in resmi, yüksek hızlı ve engellenmeyen test uçları
        private const string DownloadTestUrl = "https://speed.cloudflare.com/__down?bytes=50000000"; // 50 MB
        private const string UploadTestUrl = "https://speed.cloudflare.com/__up";

        private readonly HttpClient _client;

        public SpeedTestEngine()
        {
            _client = new HttpClient();
            _client.Timeout = TimeSpan.FromSeconds(20); // Maksimum test süresi
            _client.DefaultRequestHeaders.Add("User-Agent", "SpeedTestCS/2.0 Premium");
            _client.DefaultRequestHeaders.ConnectionClose = false;
        }

        public async Task<long> TestPingAsync()
        {
            try
            {
                Ping ping = new Ping();
                long bestPing = long.MaxValue;

                // Daha doğru sonuç için 3 kez ping atıp en iyisini alıyoruz
                for (int i = 0; i < 3; i++)
                {
                    PingReply reply = await ping.SendPingAsync("1.1.1.1", 2000);
                    if (reply.Status == IPStatus.Success && reply.RoundtripTime < bestPing)
                    {
                        bestPing = reply.RoundtripTime;
                    }
                    await Task.Delay(50);
                }

                return bestPing == long.MaxValue ? 0 : bestPing;
            }
            catch { return 0; }
        }

        public async Task TestDownloadAsync(Action<double> onProgress, CancellationToken token)
        {
            try
            {
                using var response = await _client.GetAsync(DownloadTestUrl, HttpCompletionOption.ResponseHeadersRead, token);
                response.EnsureSuccessStatusCode();

                using var stream = await response.Content.ReadAsStreamAsync();
                var buffer = new byte[65536]; // 64KB Chunk
                int bytesRead;
                long totalBytesRead = 0;

                var sw = Stopwatch.StartNew();

                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, token)) > 0)
                {
                    totalBytesRead += bytesRead;
                    double elapsedSeconds = sw.Elapsed.TotalSeconds;

                    if (elapsedSeconds > 0.1)
                    {
                        // Anlık hızı (Mbps) hesapla
                        double mbps = (totalBytesRead * 8.0) / 1_000_000.0 / elapsedSeconds;
                        onProgress?.Invoke(mbps);
                    }

                    if (elapsedSeconds > 10) break; // Testi 10 saniyede bitir
                }
            }
            catch (TaskCanceledException) { }
            catch { onProgress?.Invoke(0); }
        }

        public async Task TestUploadAsync(Action<double> onProgress, CancellationToken token)
        {
            try
            {
                // 25 MB'lık boş veri oluşturuyoruz (Upload için)
                var buffer = new byte[25 * 1024 * 1024];
                using var ms = new MemoryStream(buffer);

                var sw = Stopwatch.StartNew();

                // Custom Stream ile byte'lar ağa aktarıldıkça anlık progress alıyoruz
                using var progressStream = new ProgressStream(ms, totalBytesSent =>
                {
                    double elapsedSeconds = sw.Elapsed.TotalSeconds;
                    if (elapsedSeconds > 0.1)
                    {
                        double mbps = (totalBytesSent * 8.0) / 1_000_000.0 / elapsedSeconds;
                        onProgress?.Invoke(mbps);
                    }
                });

                using var content = new StreamContent(progressStream);
                var response = await _client.PostAsync(UploadTestUrl, content, token);
                response.EnsureSuccessStatusCode();
            }
            catch (TaskCanceledException) { }
            catch { onProgress?.Invoke(0); }
        }
    }

    // --- GERÇEK ZAMANLI UPLOAD İÇİN ÖZEL STREAM SINIFI ---
    // (Ağa giden veriyi canlı olarak yakalamamızı sağlar)
    public class ProgressStream : Stream
    {
        private readonly Stream _innerStream;
        private readonly Action<long> _onReadProgress;
        private long _totalRead = 0;

        public ProgressStream(Stream innerStream, Action<long> onReadProgress)
        {
            _innerStream = innerStream;
            _onReadProgress = onReadProgress;
        }

        public override bool CanRead => _innerStream.CanRead;
        public override bool CanSeek => _innerStream.CanSeek;
        public override bool CanWrite => _innerStream.CanWrite;
        public override long Length => _innerStream.Length;
        public override long Position { get => _innerStream.Position; set => _innerStream.Position = value; }
        public override void Flush() => _innerStream.Flush();
        public override long Seek(long offset, SeekOrigin origin) => _innerStream.Seek(offset, origin);
        public override void SetLength(long value) => _innerStream.SetLength(value);
        public override void Write(byte[] buffer, int offset, int count) => _innerStream.Write(buffer, offset, count);

        // Asenkron okuma yapıldıkça (ağa gönderildikçe) tetiklenir
        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            int read = await _innerStream.ReadAsync(buffer, offset, count, cancellationToken);
            if (read > 0)
            {
                _totalRead += read;
                _onReadProgress?.Invoke(_totalRead);
            }
            return read;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int read = _innerStream.Read(buffer, offset, count);
            if (read > 0)
            {
                _totalRead += read;
                _onReadProgress?.Invoke(_totalRead);
            }
            return read;
        }
    }
}