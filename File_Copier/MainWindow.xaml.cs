using Microsoft.Win32;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace File_Copier
{
    public partial class MainWindow : Window
    {
        private Thread thread;
        private bool isThreadSuspended;

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void CopyFileAsync(string sourceFilePath, string destinationFilePath)
        {
            try
            {
                await Task.Run(() =>
                {
                    if (File.Exists(sourceFilePath))
                    {
                        using (FileStream sourceStream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read))
                        using (FileStream destinationStream = new FileStream(destinationFilePath, FileMode.Create, FileAccess.Write))
                        {
                            byte[] buffer = new byte[4096];
                            int bytesRead;

                            while ((bytesRead = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                destinationStream.Write(buffer, 0, bytesRead);

                                Dispatcher.Invoke(() =>
                                {
                                    progress.Value += bytesRead;
                                });

                                Thread.Sleep(50);
                                destinationStream.Flush();
                            }

                            Dispatcher.Invoke(() =>
                            {
                                MessageBox.Show("Copy Successful");
                                progress.Value = 0;
                                thread = null;
                                fromtextbox.Text = string.Empty;
                                totextbox.Text = string.Empty;
                                isThreadSuspended = false;
                            });
                        }
                    }
                    else
                    {
                        Dispatcher.Invoke(() =>
                        {
                            MessageBox.Show("Source file does not exist");
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"An error occurred: {ex.Message}");
                });
            }
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(fromtextbox.Text) || string.IsNullOrWhiteSpace(totextbox.Text))
            {
                MessageBox.Show("Please enter valid file paths");
                return;
            }

            if (thread == null)
            {
                CopyFileAsync(fromtextbox.Text, totextbox.Text);
            }
        }

        private void SuspendButton_Click(object sender, RoutedEventArgs e)
        {
            if (thread != null && !isThreadSuspended)
            {
                isThreadSuspended = true;
                thread.Suspend();
            }
        }

        private void ResumeButton_Click(object sender, RoutedEventArgs e)
        {
            if (thread != null && isThreadSuspended)
            {
                isThreadSuspended = false;
                thread.Resume();
            }
        }

        private void FromOpenButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "Text Files (*.txt)|*.txt";
            if (fileDialog.ShowDialog() == true)
            {
                fromtextbox.Text = fileDialog.FileName;
            }
        }

        private void ToOpenButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "Text Files (*.txt)|*.txt";
            if (fileDialog.ShowDialog() == true)
            {
                totextbox.Text = fileDialog.FileName;
            }
        }
    }
}