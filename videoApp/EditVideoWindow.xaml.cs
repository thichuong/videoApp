using Microsoft.Win32;
using System.Windows;
using System.IO;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Drawing;
using System.Windows.Media.Media3D;

namespace videoApp
{
    /// <summary>
    /// Interaction logic for EditVideoWindow.xaml
    /// </summary>
    public partial class EditVideoWindow : Window
    {
        private Editor editor;
        private String outputVideoPath = String.Format("video_out.mp4");
        private string AudioFolderPath;
        private string ImageFolderPath;
        public EditVideoWindow()
        {
            InitializeComponent();
            editor = new Editor();
        }
        private void btnImportVideo_Click(object sender, RoutedEventArgs e)
        {
            // Mở hộp thoại chọn file video
            // Mở hộp thoại chọn file video
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Video Files|*.avi;*.mp4;*.mkv;*.wmv;*.mov|All Files|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                string selectedFileName = openFileDialog.FileName;
                editor.ProcessVideoImport(selectedFileName,outputVideoPath, 720, 1280);
                mediaPlayer.Source = new Uri(AppDomain.CurrentDomain.BaseDirectory+"\\"+outputVideoPath);
            }
        }

        private void btnSelectImageFolder_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog folderDialog = new CommonOpenFileDialog();
            folderDialog.IsFolderPicker = true;

            if (folderDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                ImageFolderPath = folderDialog.FileName;

                // Lưu đường dẫn folder hình ảnh vào TextBox
                txtImageFolderPath.Text = ImageFolderPath;

                // Đoạn mã xử lý để sử dụng đường dẫn folder hình ảnh
            }
        }

        private void btnSelectAudioFolder_Click(object sender, RoutedEventArgs e)
        {
            
            CommonOpenFileDialog folderDialog = new CommonOpenFileDialog();
            folderDialog.IsFolderPicker = true;

            if (folderDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                AudioFolderPath = folderDialog.FileName;

                // Lưu đường dẫn folder hình ảnh vào TextBox
                txtAudioFolderPath.Text = AudioFolderPath;

                // Đoạn mã xử lý để sử dụng đường dẫn folder hình ảnh
            }
        }

        private void btnExportVideo_Click(object sender, RoutedEventArgs e)
        {
            // Mở hộp thoại chọn file video
            string pathvideo = AppDomain.CurrentDomain.BaseDirectory + outputVideoPath;
            String pathExport = String.Format("Export.mp4");
            var listPathImage = editor.GetImageFilesFromDirectory(ImageFolderPath);
            var listPathAudio = editor.GetAudioFilesFromDirectory(AudioFolderPath);
            editor.InsertImagesIntoVideo(pathvideo, listPathImage, pathExport);
            editor.InsertAudioEveryIntervalAsync(pathvideo, AudioFolderPath, pathExport, 2);
            mediaPlayer.Source = new Uri(AppDomain.CurrentDomain.BaseDirectory + "\\" + pathExport);
        }
    }
}
