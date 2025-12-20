using StudentAdWindowsApp.Models;
using StudentAdWindowsApp.Services;
using System;
using System.Windows;
using System.ComponentModel; // เพิ่มสำหรับ CancelEventArgs
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;

namespace StudentAdWindowsApp
{
    public partial class MainWindow : Window
    {
        private readonly AdService _adService = new();
        private readonly ConfigService _configService = new();

        // ตัวแปรเช็คว่าเป็นการกด "Exit" จริงๆ หรือแค่กดปิดหน้าต่าง (X)
        private bool _isExplicitClose = false;

        public MainWindow()
        {
            InitializeComponent();

            // 1. โหลด Config เดิมเข้าสู่ UI
            var cfg = _configService.Load();
            txtDomain.Text = cfg.Domain;
            txtOU.Text = cfg.OuPath;
            txtUser.Text = cfg.AdminUser;
            txtPass.Password = cfg.AdminPassword;
            txtPort.Text = cfg.ApiPort.ToString();

            // 2. รันกระบวนการตรวจสอบและเริ่ม API แบบอัตโนมัติ
            AutoStartApiProcess();

            // 3. ตั้งค่าการซ่อนหน้าต่างเมื่อเริ่มโปรแกรม
            this.ContentRendered += (s, e) => this.Hide();
        }

        private async void AutoStartApiProcess()
        {
            try
            {
                // 1. ดึงค่าคอนฟิก
                var config = GetConfig();
                txtStatus.Text = "⏳ กำลังตรวจสอบการเชื่อมต่อ AD...";
                txtStatus.Foreground = System.Windows.Media.Brushes.Orange;

                // 2. จำลองหรือเรียกใช้งาน Test Connection
                // หาก AD เชื่อมต่อไม่ได้ บรรทัดนี้จะโยน Exception และกระโดดไปที่ catch ทันที
                await Task.Run(() => _adService.TestConnection(config));

                // 3. ถ้าผ่านมาถึงบรรทัดนี้ได้ แปลว่า Test Connection สำเร็จ
                txtStatus.Text = "✅ เชื่อมต่อสำเร็จ กำลังเริ่ม API...";
                txtStatus.Foreground = System.Windows.Media.Brushes.Green;

                // 4. เริ่ม Start API
                // เราเรียกฟังก์ชัน Start_Click โดยตรง หรือแยก Logic การ Start ออกมาก็ได้
                Start_Click(null, null);

                // 5. เมื่อทุกอย่างเรียบร้อย ให้ซ่อนหน้าต่าง
                this.Hide();
            }
            catch (Exception ex)
            {
                // กรณี Test Connection ไม่ผ่าน หรือมีข้อผิดพลาดอื่นๆ
                txtStatus.Text = "❌ การเชื่อมต่อล้มเหลว API ไม่ทำงาน";
                txtStatus.Foreground = System.Windows.Media.Brushes.Red;

                // แสดงหน้าต่างขึ้นมาเพื่อให้ผู้ใช้เห็น Error และแก้ไขค่า
                this.Show();
                this.WindowState = WindowState.Normal;
                this.Activate();

                MessageBox.Show($"ไม่สามารถเริ่มระบบอัตโนมัติได้เนื่องจาก:\n{ex.Message}",
                                "Connection Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        // --- เพิ่มส่วนจัดการ System Tray ---

        // เมื่อดับเบิลคลิกไอคอนที่มุมจอ ให้แสดงหน้าต่างขึ้นมา
        private void NotifyIcon_DoubleClick(object sender, RoutedEventArgs e)
        {
            this.Show();
            this.WindowState = WindowState.Normal;
            this.Activate();
        }

        // เมื่อกดปิดหน้าต่าง (X) ให้ซ่อนลงมุมจอแทน
        protected override void OnClosing(CancelEventArgs e)
        {
            if (!_isExplicitClose)
            {
                e.Cancel = true;
                this.Hide();
            }
            base.OnClosing(e);
        }

        // สำหรับปุ่ม Exit ใน Context Menu (คลิกขวาที่ไอคอนมุมจอ)
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            _isExplicitClose = true;
            Application.Current.Shutdown();
        }

        // --- โค้ดเดิมของคุณ ---

        private AdConfig GetConfig()
        {
            if (!int.TryParse(txtPort.Text, out int port) || port < 1 || port > 65535)
                throw new Exception("Port ต้องเป็นตัวเลข 1 - 65535");

            return new AdConfig
            {
                Domain = txtDomain.Text.Trim(),
                OuPath = txtOU.Text.Trim(),
                AdminUser = txtUser.Text.Trim(),
                AdminPassword = txtPass.Password,
                ApiPort = port
            };
        }

        private void Test_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var config = GetConfig();
                _adService.TestConnection(config);
                _configService.Save(config);
                txtStatus.Text = "✅ เชื่อมต่อ Active Directory สำเร็จ";
            }
            catch (Exception ex)
            {
                MessageBox.Show("เชื่อมต่อไม่สำเร็จ\n\n" + ex.Message, "AD Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void Start_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var config = GetConfig();
                App.CurrentConfig = config;

                App.ApiHost = Host.CreateDefaultBuilder()
                    .ConfigureWebHostDefaults(webBuilder =>
                    {
                        webBuilder
                            .UseKestrel()
                            .UseUrls($"http://0.0.0.0:{config.ApiPort}")
                            .UseStartup<StudentAdWindowsApp.Api.ApiStartup>();
                    })
                    .Build();

                await App.ApiHost.StartAsync();
                txtStatus.Text = $"🚀 API Started (http://localhost:{config.ApiPort})";
                _configService.Save(config);

                // (Option) เมื่อกด Start แล้วให้ย่อหน้าต่างเก็บทันที
                // this.Hide(); 
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Start API Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}