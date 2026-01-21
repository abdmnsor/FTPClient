using System.ComponentModel;
using System.IO;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FluentFTP;
using Microsoft.Win32;
using Path = System.IO.Path;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SharpCompress.Archives;
using System.Diagnostics;
using System.Xml.Linq;



namespace FTPClient;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private BackgroundWorker worker;
    private string localFolderPath;
    public List<string> SelectedfilesToDownload = new List<string>();
    public string FTPFolderPath;

    public MainWindow()
    {
        InitializeComponent();
        InitializeWorker();

        // Set up local folder path (where application is running)
        localFolderPath = AppDomain.CurrentDomain.BaseDirectory;
        LogMessage($"Application folder: {localFolderPath}");
    }

    private void InitializeWorker()
    {
        worker = new BackgroundWorker();
        worker.WorkerReportsProgress = true;
        worker.DoWork += Worker_DoWork;
        worker.ProgressChanged += Worker_ProgressChanged;
        worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
    }

    private void btnDownload_Click(object sender, RoutedEventArgs e)
    {
        // Clear extracted files list
        lstExtractedFiles.Items.Clear();

        foreach (var process in Process.GetProcessesByName("MSACCESS"))
        {
            // Temp is a document which you need to kill.
            if (process.MainWindowTitle.Contains("Optimum.Point"))
                process.Kill();
        }

        // Check if worker is busy
        if (worker.IsBusy)
        {
            MessageBox.Show("A task is already in progress. Please wait for it to complete.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Disable buttons
        btnDownloadStor2003.IsEnabled = false;
        btnUploadStor2003.IsEnabled = false;
        btnDownloadCustomerData.IsEnabled = false;
        btnDownloadAnyFile.IsEnabled = false;

        // Reset progress bar
        progressBar.Value = 0;
        txtStatus.Text = "Downloading files...";

        // Start worker with download task
        worker.RunWorkerAsync(new WorkerArgs
        {
            Task = TaskType.Download,
            Server = txtServer.Text,
            Username = txtUsername.Text,
            Password = txtPassword.Password
        });
    }

    private void btnUpload_Click(object sender, RoutedEventArgs e)
    {
        // Check if worker is busy
        if (worker.IsBusy)
        {
            MessageBox.Show("A task is already in progress. Please wait for it to complete.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Open file dialog to select file to upload
        OpenFileDialog openFileDialog = new OpenFileDialog();
        openFileDialog.Filter = "All files (*.*)|*.*";
        openFileDialog.Title = "Select a file to upload";
        openFileDialog.Multiselect = true;

        if (openFileDialog.ShowDialog() == true)
        {
            // Disable buttons
            btnDownloadStor2003.IsEnabled = false;
            btnUploadStor2003.IsEnabled = false;
            btnDownloadCustomerData.IsEnabled = false;
            btnDownloadAnyFile.IsEnabled = false;

            // Reset progress bar
            progressBar.Value = 0;
            txtStatus.Text = "Uploading file...";
            foreach (var item in openFileDialog.FileNames)
            {
                SelectedfilesToDownload.Add(item);
            }
            // Start worker with upload task
            worker.RunWorkerAsync(new WorkerArgs
            {
                Task = TaskType.Upload,
                Server = txtServer.Text,
                Username = txtUsername.Text,
                Password = txtPassword.Password,
            });
        }
    }

    private void Worker_DoWork(object sender, DoWorkEventArgs e)
    {
        WorkerArgs args = (WorkerArgs)e.Argument;
        BackgroundWorker bw = sender as BackgroundWorker;

        try
        {
            LogMessageFromThread("clear");
            if (args.Task == TaskType.Download)
            {
                // Files to download
                List<string> filesToDownload = new List<string> { "stor2003_MDB.rar" };
                localFolderPath = @"c:\access\stor\";
                // Download files from FTP
                DownloadFilesFromFtp(args, bw, filesToDownload, "/program/");

            } 
            else if (args.Task == TaskType.Download_Customer_Data)
            {
                //// Files to download
                //List<string> filesToDownload = new List<string> { "stor2003_MDB.rar" };
                localFolderPath = @"y:\access\stor\";
                // Download files from FTP
                DownloadFilesFromFtp(args, bw, SelectedfilesToDownload, FTPFolderPath);
            } else if (args.Task == TaskType.Download_Any_File)
            { 
            }
            else if (args.Task == TaskType.Upload)
            {
                // Upload file to FTP
                foreach (var item in SelectedfilesToDownload)
                {
                    args.LocalFilePath = item;
                    UploadFileToFtp(args, bw);
                }
                
            }
        }
        catch (Exception ex)
        {
            e.Result = new WorkerResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    private void DownloadFilesFromFtp(WorkerArgs args, BackgroundWorker worker, List<string> filesToDownload , string files_path)
    {
        // Files to download
      //  List<string> filesToDownload = new List<string> { "stor_new_setup.zip", "gib.rar" };

        worker.ReportProgress(0, "Connecting to FTP server...");

        // Create FTP client with FluentFTP
        using (FtpClient client = new FtpClient(args.Server))
        {
            // Set credentials
            client.Credentials = new System.Net.NetworkCredential(args.Username, args.Password);

            // Connect to the server
            client.Connect();
            LogMessageFromThread($"Connected to {args.Server}");

            // Download files
            int totalFiles = filesToDownload.Count;
            for (int i = 0; i < totalFiles; i++)
            {
                string fileName = filesToDownload[i];
                string localFilePath = Path.Combine(localFolderPath, fileName);

                worker.ReportProgress((i * 40) / totalFiles, $"Downloading {fileName}...");

                // Download file with progress tracking
                client.DownloadFile(
                    localFilePath,           // Local file path
                    files_path + fileName,                // Remote file path
                    FtpLocalExists.Overwrite,// Overwrite if exists
                    FtpVerify.None,          // No verification
                    progress =>              // Progress reporting
                    {
                        int fileProgress = (int)progress.Progress;
                        int totalProgress = (i * 40) / totalFiles + (fileProgress * 40) / (100 * totalFiles);
                        worker.ReportProgress(totalProgress, $"Downloading {fileName}... {fileProgress}%");
                    }
                );

                LogMessageFromThread($"Downloaded {fileName} to {localFilePath}");
            }

            // Disconnect from server
            client.Disconnect();
            LogMessageFromThread("Disconnected from FTP server");
        }

        worker.ReportProgress(80, "Extracting RAR file...");

        foreach (var item in filesToDownload)
        {
            // Extract content of file1.rar
            string rarFilePath = Path.Combine(localFolderPath, item);
            List<string> extractedFiles = ExtractRarFile(rarFilePath);
            // Return result
            WorkerResult result = new WorkerResult
            {
                Success = true,
                Message = "Download and extraction completed successfully.",
                ExtractedFiles = extractedFiles
            };

            worker.ReportProgress(100, "Download and extraction completed successfully.");
            worker.ReportProgress(100, result);
        }



    }

    private List<string> ExtractRarFile(string rarFilePath)
    {
        List<string> extractedFiles = new List<string>();

        try
        {
            FileInfo fileInfo = new FileInfo(rarFilePath);

            switch (fileInfo.Extension.ToLower())
            {
                case ".zip":
                    // For a real implementation, you would use SharpCompress or similar library:
                    using (var archive = SharpCompress.Archives.Zip.ZipArchive.Open(rarFilePath))
                    {
                        foreach (var entry in archive.Entries)
                        {
                            if (!entry.IsDirectory)
                            {
                                entry.WriteToDirectory(localFolderPath, new SharpCompress.Common.ExtractionOptions
                                {
                                    ExtractFullPath = false,
                                    Overwrite = true
                                });
                                extractedFiles.Add(entry.Key);
                            }
                        }
                    }
                    break;
                case ".rar":
                    // For a real implementation, you would use SharpCompress or similar library:
                    using (var archive = SharpCompress.Archives.Rar.RarArchive.Open(rarFilePath))
                    {
                        foreach (var entry in archive.Entries)
                        {
                            if (!entry.IsDirectory)
                            {
                                entry.WriteToDirectory(localFolderPath, new SharpCompress.Common.ExtractionOptions
                                {
                                    ExtractFullPath = false,
                                    Overwrite = true
                                });
                                extractedFiles.Add(entry.Key);
                            }
                        }
                    }
                    break;
                case ".arj":
                    break;
                default:
                    LogMessageFromThread("Error With The Extension File :  This Extension Not Supported");
                    break;
            }



            // For demonstration purposes, we'll simulate extracting 3 files
            //for (int i = 0; i < 3; i++)
            //{
            //    string extractedFileName = $"extracted_file_{i + 1}.txt";
            //    string extractedFilePath = Path.Combine(localFolderPath, extractedFileName);

            //    // Create a dummy file to simulate extraction
            //    using (FileStream fs = File.Create(extractedFilePath))
            //    {
            //        byte[] info = System.Text.Encoding.UTF8.GetBytes($"This is content of extracted file {i + 1}");
            //        fs.Write(info, 0, info.Length);
            //    }

            //    extractedFiles.Add(extractedFileName);
            //    LogMessageFromThread($"Extracted {extractedFileName}");
            //}

            return extractedFiles;
        }
        catch (Exception ex)
        {
            LogMessageFromThread($"Error extracting RAR: {ex.Message}");
            return extractedFiles;
        }
    }

    private void UploadFileToFtp(WorkerArgs args, BackgroundWorker worker)
    {
        worker.ReportProgress(0, "Connecting to FTP server...");

        string fileName = Path.GetFileName(args.LocalFilePath);

        // Create FTP client with FluentFTP
        using (FtpClient client = new FtpClient(args.Server))
        {
            // Set credentials
            client.Credentials = new System.Net.NetworkCredential(args.Username, args.Password);

            // Connect to the server
            client.Connect();
            LogMessageFromThread($"Connected to {args.Server}");

            worker.ReportProgress(10, $"Uploading {fileName}...");

            // Upload file with progress tracking
            client.UploadFile(
                args.LocalFilePath,      // Local file path
                @"/abd/" + fileName,                // Remote file path
                FtpRemoteExists.Overwrite,// Overwrite if exists
                true,                    // Create directory if doesn't exist
                FtpVerify.None,          // No verification
                progress =>              // Progress reporting
                {
                    int fileProgress = (int)progress.Progress;
                    int totalProgress = 10 + (fileProgress * 90) / 100;
                    worker.ReportProgress(totalProgress, $"Uploading {fileName}... {fileProgress}%");
                }
            );

            LogMessageFromThread($"Uploaded {fileName} to server");

            // Disconnect from server
            client.Disconnect();
            LogMessageFromThread("Disconnected from FTP server");
        }

        // Return result
        WorkerResult result = new WorkerResult
        {
            Success = true,
            Message = $"File {fileName} uploaded successfully."
        };

        worker.ReportProgress(100, result);
    }

    private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
    {
        if (e.UserState is string message)
        {
            txtStatus.Text = message;
            progressBar.Value = e.ProgressPercentage;
        }
        else if (e.UserState is WorkerResult result)
        {
            // Process result
            if (result.ExtractedFiles != null)
            {
                foreach (string file in result.ExtractedFiles)
                {
                    lstExtractedFiles.Items.Add(file);
                }
            }
        }
    }

    private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
    {
        // Enable buttons
        btnDownloadStor2003.IsEnabled = true;
        btnUploadStor2003.IsEnabled = true;
        btnDownloadCustomerData.IsEnabled = true;
        btnDownloadAnyFile.IsEnabled = true;


        if (e.Error != null)
        {
            txtStatus.Text = "Error occurred";
            LogMessage($"Error: {e.Error.Message}");
            MessageBox.Show($"An error occurred: {e.Error.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        else if (e.Result is WorkerResult result)
        {
            if (result.Success)
            {
                txtStatus.Text = result.Message;
                LogMessage(result.Message);
            }
            else
            {
                txtStatus.Text = "Error occurred";
                LogMessage($"Error: {result.ErrorMessage}");
                MessageBox.Show($"An error occurred: {result.ErrorMessage}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void LogMessage(string message)
    {
        if (message == "clear")
        {
            txtLog.Text = "";
            return;
        }
        string timestamp = DateTime.Now.ToString("HH:mm:ss");
        txtLog.AppendText($"[{timestamp}] {message}{Environment.NewLine}");
        txtLog.ScrollToEnd();
    }

    private void LogMessageFromThread(string message)
    {
        Dispatcher.Invoke(() => LogMessage(message));
    }

    private void btnDownloadCustomerData_Click(object sender, RoutedEventArgs e)
    {
        // Clear extracted files list
        lstExtractedFiles.Items.Clear();



        // Check if worker is busy
        if (worker.IsBusy)
        {
            MessageBox.Show("A task is already in progress. Please wait for it to complete.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Disable buttons
        btnDownloadStor2003.IsEnabled = false;
        btnUploadStor2003.IsEnabled = false;
        btnDownloadCustomerData.IsEnabled = false;
        btnDownloadAnyFile.IsEnabled = false;

        FilesList FilesView = new FilesList();

        FilesView.server = txtServer.Text;
        FilesView.username = txtUsername.Text;
        FilesView.password = txtPassword.Password;
        FilesView.Owner = this;

        FilesView.ShowDialog();
        if (SelectedfilesToDownload.Count < 1)
        {
            // Enable buttons
            btnDownloadStor2003.IsEnabled = true;
            btnUploadStor2003.IsEnabled = true;
            btnDownloadCustomerData.IsEnabled = true;
            btnDownloadAnyFile.IsEnabled = true;

            return;
        }
        // Reset progress bar
        progressBar.Value = 0;
        txtStatus.Text = "Downloading files...";

        // Start worker with download task
        worker.RunWorkerAsync(new WorkerArgs
        {
            Task = TaskType.Download_Customer_Data,
            Server = txtServer.Text,
            Username = txtUsername.Text,
            Password = txtPassword.Password
        });
    }

    private void btnDownloadAnyFile_Click(object sender, RoutedEventArgs e)
    {

    }

    private void btnRunStor2003MDB_Click(object sender, RoutedEventArgs e)
    {
        //System.Diagnostics.Process.Start(@"c:\access\stor\stor2003.mdb");
        Process notePad = new Process();
        notePad.StartInfo.FileName = "C:\\Program Files (x86)\\Microsoft Office\\OFFICE11\\MSACCESS.exe";
        notePad.StartInfo.Arguments = @"c:\access\stor\stor2003.mdb";
        notePad.Start();
    }
}

public enum TaskType
{
    Download,
    Download_Customer_Data,
    Download_Any_File,
    Upload
}

public class WorkerArgs
{
    public TaskType Task { get; set; }
    public string Server { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string LocalFilePath { get; set; }
    public string RemotePath { get; set; }
}

public class WorkerResult
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public string ErrorMessage { get; set; }
    public List<string> ExtractedFiles { get; set; }
}
