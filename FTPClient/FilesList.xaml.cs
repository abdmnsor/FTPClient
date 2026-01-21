using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using FluentFTP;
using FTPClient;

namespace FTPClient
{
    /// <summary>
    /// Interaction logic for FilesList.xaml
    /// </summary>
    public partial class FilesList : Window
    {
        public string server = string.Empty;
        public string password = string.Empty;
        public string username = string.Empty;
        private BackgroundWorker worker;
        private string sorted = "Ascending";
        private string localFolderPath;
        List<DirectoryItem> AlldirectoryItems = new List<DirectoryItem>();

        public string files_path = string.Empty;
        List<string> AlldirectoryItemsToDownload = new List<string>();

        public FilesList()
        {
            InitializeComponent();
            InitializeWorker();

            // Set up local folder path (where application is running)
            localFolderPath = AppDomain.CurrentDomain.BaseDirectory;
            this.Closing += SecondaryWindow_Closing;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Start worker with browse task
            worker.RunWorkerAsync(new WorkerArgs
            {
                Task = TaskType.Download_Customer_Data,
                Server = server,
                Username = username,
                Password = password,
                RemotePath = txtFtpPath.Text
            });
            
        }
        private List<DirectoryItem> BrowseFtpDirectory(WorkerArgs args, BackgroundWorker worker)
        {
        

            List<DirectoryItem> directoryItems = new List<DirectoryItem>();

            using (FtpClient client = new FtpClient(args.Server))
            {
                // Set credentials
                client.Credentials = new System.Net.NetworkCredential(args.Username, args.Password);

                try
                {
                    // Connect to the server
                    client.Connect();
                    

                 

                    // Get a list of all items (files and directories) from the specified path
                    FtpListItem[] items = client.GetListing(args.RemotePath);

                    foreach (FtpListItem item in items)
                    {
                        DirectoryItem dirItem = new DirectoryItem();

                        if (item.Type == FtpObjectType.Directory)
                        {
                            dirItem.Type = "DIR";
                            dirItem.Name = item.Name;
                            dirItem.Size = "";
                            dirItem.DateAndTime = item.Modified.ToString("dd/MM/yyyy hh:mm:ss tt");
                            dirItem.sortDate = Convert.ToDecimal(item.Modified.ToString("yyyyMMddhhmmss"));
                        }
                        else if (item.Type == FtpObjectType.File)
                        {
                            dirItem.Type = "FILE";
                            dirItem.Name = item.Name;
                            dirItem.Size = FormatFileSize(item.Size);
                            dirItem.DateAndTime = item.Modified.ToString("dd/MM/yyyy hh:mm:ss tt");
                            dirItem.sortDate = Convert.ToDecimal( item.Modified.ToString("yyyyMMddhhmmss"));
                        }
                        else
                        {
                            dirItem.Type = "LINK";
                            dirItem.Name = item.Name;
                            dirItem.Size = "";
                            dirItem.DateAndTime = item.Modified.ToString("dd/MM/yyyy hh:mm:ss tt");
                            dirItem.sortDate = Convert.ToDecimal(item.Modified.ToString("yyyyMMddhhmmss"));
                        }

                        directoryItems.Add(dirItem);
                    }

                    // Disconnect from server
                    client.Disconnect();
                   // MessageBox.Show("Disconnected from FTP server");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error browsing directory: {ex.Message}");
                    throw;
                }
            }

            // Return result
            //WorkerResult result = new WorkerResult
            //{
            //    Success = true,
            //    Message = $"Successfully listed contents of {args.RemotePath}",
            //    DirectoryItems = directoryItems
            //};
           return directoryItems;


        }
        private void InitializeWorker()
        {
            worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += Worker_DoWork;
            worker.ProgressChanged += Worker_ProgressChanged;
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
        }
    
        private void Worker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            lstDirectoryContents.ItemsSource = AlldirectoryItems;
            Sort("DateTime");

        }

        private void Worker_ProgressChanged(object? sender, ProgressChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            WorkerArgs args = (WorkerArgs)e.Argument;
            BackgroundWorker bw = sender as BackgroundWorker;

            if (args.Task == TaskType.Download_Customer_Data)
            {
                // Browse FTP directory
                AlldirectoryItems = BrowseFtpDirectory(args, bw);
            }


          
        }


        //private void ListFtpDirectoryContents(string remotePath, FtpClient client)
        //{
        //    try
        //    {
        //        // Get a list of all items (files and directories) from the specified path
        //        FtpListItem[] items = client.GetListing(remotePath);

        //        LogMessageFromThread($"Contents of {remotePath}:");

        //        foreach (FtpListItem item in items)
        //        {
        //            if (item.Type == FtpFileSystemObjectType.Directory)
        //            {
        //                LogMessageFromThread($"[DIR] {item.Name}");
        //            }
        //            else if (item.Type == FtpFileSystemObjectType.File)
        //            {
        //                LogMessageFromThread($"[FILE] {item.Name} - Size: {FormatFileSize(item.Size)}");
        //            }
        //            else
        //            {
        //                LogMessageFromThread($"[LINK] {item.Name}");
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        LogMessageFromThread($"Error listing directory contents: {ex.Message}");
        //    }
        //}
        private void GridViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader header = sender as GridViewColumnHeader;
            if (header != null && header.Column != null)
            {
                string sortBy = header.Column.Header as string;
                Sort(sortBy);
            }
        }

        private void Sort(string sortBy)
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(lstDirectoryContents.ItemsSource);
            if (view != null)
            {
                view.SortDescriptions.Clear();
                ListSortDirection direction = ListSortDirection.Descending;

                // Check if already sorted by this column
                if (sorted == "Descending")
                {
                    direction = ListSortDirection.Ascending;
                    sorted = "Ascending";
                }
                else 
                {
                    direction = ListSortDirection.Descending;
                    sorted = "Descending";
                }
                if (sortBy == "DateTime")
                {
                    view.SortDescriptions.Add(new SortDescription("sortDate", direction));
                }
                else
                {
                    view.SortDescriptions.Add(new SortDescription(sortBy, direction));
                }
                
            }
        }

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;

            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            return $"{len:0.##} {sizes[order]}";
        }

        private void btnDownload_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = lstDirectoryContents.SelectedItems;
            if (selectedItems != null)
            {
                foreach (DirectoryItem item in selectedItems)
                {
                    AlldirectoryItemsToDownload.Add(item.Name);
                }
               
                // Use the column value as needed
            }
            files_path = txtFtpPath.Text;
           
            this.Close();
        }
        private void SecondaryWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (this.Owner is MainWindow mainWindow)
            {
                foreach (var item in this.AlldirectoryItemsToDownload)
                {
                    mainWindow.SelectedfilesToDownload.Add(item);
                }
                if (!this.files_path.EndsWith("\\"))
                {
                    this.files_path += "\\";
                }
                mainWindow.FTPFolderPath = this.files_path;
            }
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            // Start worker with browse task
            worker.RunWorkerAsync(new WorkerArgs
            {
                Task = TaskType.Download_Customer_Data,
                Server = server,
                Username = username,
                Password = password,
                RemotePath = txtFtpPath.Text
            });
        }
    }

}
public class DirectoryItem
{
    public string Type { get; set; }
    public string Name { get; set; }
    public string Size { get; set; }
    public string DateAndTime { get; set; }
    public decimal sortDate { get; set; }

}
