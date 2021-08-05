using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using Path = System.IO.Path;
using System.Threading;

namespace Opera3SEImporter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string FileIncPath { get; private set; }
        public string FileMask { get; private set; }
        public string HeaderMask { get; private set; }
        public string DetailsMask { get; private set; }

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void BtnImport_Click(object sender, RoutedEventArgs e)
        {
            lblFileCount.Content = "File Count: 0";
            lblFileCurrent.Content = "Current File: 0";
            lblCompleted.Content = "";
            lblErrors.Content = "Errors: 0";
            Thread.Sleep(200);
            FileMask = TxtFileMask.Text;
            FileIncPath = TxtFilesPath.Text;
            HeaderMask = TxtHeaderMask.Text;
            DetailsMask = TxtDetailsMask.Text;
            Progress<int> FileCount = new(response => { lblFileCount.Content = "File Count: " + response; });
            Progress<int> FileCurrent = new(response => { lblFileCurrent.Content = "Current File: " + response; });            
            Progress<int> Errors = new(response => { lblErrors.Content = "Errors: " + response; });
            Progress<string> Completed = new(response => { lblCompleted.Content = "Complete: " + response; });
            await Task.Run(() =>
                    ImportCls.DoWork(FileCount, FileCurrent, Errors, Completed, FileMask, FileIncPath, HeaderMask, DetailsMask)
                );
        }

        private void BtnBrowse_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new()
            {
                InitialDirectory = "C:\\",
                IsFolderPicker = true
            };
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                TxtFilesPath.Text = dialog.FileName;
        }
    }
}
