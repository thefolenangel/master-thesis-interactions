using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DataSetGenerator;

namespace ManualLogger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        bool commentButtonActive = true;
        bool logging = false;
        int userID;
        private static string directory;

        public MainWindow()
        {
            directory = DataGenerator.TestFileDirectory(DataSource.Target);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            InitializeComponent();
            currentUser.Text = userID.ToString();        
        }

        private void comment_Click(object sender, RoutedEventArgs e)
        {
            if (logging)
            {
                if (commentButtonActive)
                {
                    commentLog.IsEnabled = true;
                    commentLog.Focus();
                    commentButton.Content = "End comment";
                }
                else
                {
                    using (StreamWriter sw = File.AppendText(directory + userID + "_comments.txt"))
                    {
                        sw.WriteLine("[{0}]: {1}", DateTime.Now.ToString("HH:mm:ss"), commentLog.Text);
                    }
                    commentLog.Clear();
                    commentButton.Focus();
                    commentLog.IsEnabled = false;
                    commentButton.Content = "Start comment";
                }
                commentButtonActive = !commentButtonActive;
            }
            
        }

        private void startLog_Click(object sender, RoutedEventArgs e)
        {
            if (logging)
            {
                MessageBoxResult result = MessageBox.Show("Stop logging this user?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.No)
                {
                    return;
                }
                startLog.Content = "Start log";
                logging = !logging;
                userID = default(int);
                currentUser.Text = userID.ToString();
            }
            else
            {
                var dialog = new Dialog();
                if (dialog.ShowDialog() == true)
                {
                        try
                        {
                            userID = Int32.Parse(dialog.ResponseText);
                            currentUser.Text = userID.ToString();
                        }
                        catch (FormatException ex)
                        {
                            MessageBox.Show("Error: " + ex.Message);
                            return;
                        }
                    startLog.Content = "End log";
                    commentButton.Focus();
                    logging = !logging;
                }
            }
        }

        private void commentLog_KeyUp(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                commentButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                commentButton.Focus();
            }
        }
    }
}
