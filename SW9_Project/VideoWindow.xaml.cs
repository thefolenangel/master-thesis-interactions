using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;

using DataSetGenerator;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Interop;

namespace SW9_Project
{
    /// <summary>
    /// Interaction logic for VideoWindow.xaml
    /// </summary>
    public partial class VideoWindow : Window {

        static VideoWindow currentVideoWindow;

        static string format = ".mp4";

        public VideoWindow() {
            InitializeComponent();
            MoveWindow(false);
        }

        public static void PlayVideo(GestureDirection direction, GestureType type) {
            canvasWindow.LockScreen(type, direction);
            if(currentVideoWindow == null) {
                currentVideoWindow = new VideoWindow();
            }
            
            string videoPath = GetVideoPath(direction, type);
            if (File.Exists(videoPath)) {
                currentVideoWindow.videoMediaElement.Source = new Uri(videoPath, UriKind.Relative);
                currentVideoWindow.videoMediaElement.Position = TimeSpan.Zero;
                
                currentVideoWindow.videoMediaElement.MediaEnded += (sender, e) => {
                    canvasWindow.UnlockScreen();
                    currentVideoWindow.videoMediaElement.Position = TimeSpan.Zero; 
                    canvasWindow.Activate();
                };
                
            }
            else
            {
                canvasWindow.UnlockScreen();
                currentVideoWindow.videoMediaElement.Position = TimeSpan.Zero;
                currentVideoWindow.WindowState = WindowState.Minimized;
                canvasWindow.Activate();
            }
        }
        



        static CanvasWindow canvasWindow;
        public static void SetCanvasWindow(CanvasWindow window) {
            canvasWindow = window;
        }

        private void MoveWindow(bool primaryScreen) {

            if (Screen.AllScreens.Length > 1) {
                int secScreen = Screen.AllScreens.Length == 2 ? 0 : 2;
                int mainScreen = Screen.AllScreens.Length == 2 ? 1 : 0;
                Screen s = primaryScreen ? Screen.AllScreens[mainScreen] : Screen.AllScreens[secScreen];
                System.Drawing.Rectangle r = s.Bounds;
                this.Topmost = true;
                this.Top = r.Top;
                this.Left = r.Left;
                this.Width = r.Width;
                this.Height = r.Height;
                this.Show();
            }
            else {
                Screen s = Screen.AllScreens[0];
                System.Drawing.Rectangle r = s.WorkingArea;
                this.Top = r.Top;
                this.Left = r.Left;
                this.Show();
            }

        }
        
        private static string GetVideoPath(GestureDirection direction, GestureType type) {

            string videoDirectory = @"techniques/";
            string video = direction.ToString() + "_" + type.ToString() + format;

            return Path.Combine(new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName, videoDirectory + video);
        }

    }
}