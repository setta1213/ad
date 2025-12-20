using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using StudentAdWindowsApp.Models;
using Microsoft.Win32; // สำหรับ Registry
using System.Reflection; // สำหรับ GetExecutingAssembly
using System.Windows;

namespace StudentAdWindowsApp
{
    public partial class App : Application
    {
        public static IHost? ApiHost;
        public static AdConfig? CurrentConfig;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 1. ตั้งค่าให้เปิดแอปอัตโนมัติเมื่อเปิดเครื่อง (เรียกใช้ฟังก์ชันด้านล่าง)
            SetAutoStart(true);

            // 2. (ตัวเลือกเสริม) ถ้าต้องการให้เปิดมาแล้วซ่อนไปที่มุมจอทันที
            // MainWindow.WindowState = WindowState.Minimized;
            // MainWindow.Hide();
        }

        private void SetAutoStart(bool enable)
        {
            try
            {
                string appName = "StudentAdWindowsApp";
                // ดึง Path ของไฟล์ .exe
                string appPath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;

                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
                {
                    if (enable)
                        key.SetValue(appName, $"\"{appPath}\"");
                    else
                        key.DeleteValue(appName, false);
                }
            }
            catch { /* จัดการ error กรณีสิทธิ์การเข้าถึงไม่ได้ */ }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            ApiHost?.StopAsync().Wait();
            base.OnExit(e);
        }
    }
}