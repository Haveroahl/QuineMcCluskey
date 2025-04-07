using System;
using System.Windows.Forms;

namespace QuineMcCluskeyGUI
{
    static class Program
    {
        /// <summary>
        /// Điểm vào chính của ứng dụng.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();             // Bật giao diện hiện đại hơn (Windows theme)
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());              // Khởi chạy Form chính
        }
    }
}
