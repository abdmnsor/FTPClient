# FTP Manager

A modern, feature-rich FTP client built with WPF and .NET 8, designed for efficient file management and archive handling.

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)
![WPF](https://img.shields.io/badge/WPF-Windows-0078D4?logo=windows)
![License](https://img.shields.io/badge/license-MIT-green)

## 📋 Overview

FTP Manager is a Windows desktop application that provides a user-friendly interface for managing FTP file transfers. Built with modern WPF architecture, it supports downloading, uploading, browsing remote directories, and automatic archive extraction.

## ✨ Features

### Core Functionality
- **🔐 Secure FTP Connection** - Connect to FTP servers with username/password authentication
- **⬇️ File Download** - Download files with real-time progress tracking
- **⬆️ File Upload** - Upload single or multiple files to remote servers
- **📂 Directory Browser** - Browse and navigate remote FTP directories
- **📦 Archive Extraction** - Automatic extraction of RAR and ZIP archives
- **📊 Progress Tracking** - Real-time progress bars and status updates
- **📝 Activity Logging** - Detailed timestamped operation logs

### Advanced Features
- **Multi-file Selection** - Select and download/upload multiple files at once
- **Sortable File Lists** - Sort files by name, size, type, or date
- **Background Processing** - Non-blocking UI with background worker threads
- **Error Handling** - Comprehensive error handling with user-friendly messages
- **File Information** - Display file sizes, modification dates, and types

## 🛠️ Technology Stack

- **Framework**: .NET 8.0
- **UI**: Windows Presentation Foundation (WPF)
- **FTP Library**: [FluentFTP](https://github.com/robinrodricks/FluentFTP) v52.1.0
- **Archive Library**: [SharpCompress](https://github.com/adamhathcock/sharpcompress) v0.39.0
- **Architecture**: MVVM pattern with BackgroundWorker for async operations

## 📦 Installation

### Prerequisites
- Windows 10 or later
- .NET 8.0 Runtime or SDK

### Build from Source

1. **Clone the repository**
   ```bash
   git clone https://github.com/abdmnsor/FTPClient.git
   cd FTPClient
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Build the project**
   ```bash
   dotnet build --configuration Release
   ```

4. **Run the application**
   ```bash
   dotnet run --project FTPClient/FTPClient.csproj
   ```

## 🚀 Usage

### Connecting to an FTP Server

1. Enter your FTP server details:
   - **Server**: FTP server address (e.g., `ftp.example.com`)
   - **Username**: Your FTP username
   - **Password**: Your FTP password

2. Click one of the action buttons to perform operations

### Downloading Files

**Download Stor2003 Files**
- Click the "Download Stor2003 Files" button
- Files are downloaded to `c:\access\stor\`
- Archives are automatically extracted

**Download Customer Data**
- Click "Download Customer Data"
- Browse and select files from the remote directory
- Files are downloaded to `y:\access\stor\`

### Uploading Files

1. Click "Upload Stor2003 File"
2. Select one or more files from your local system
3. Files are uploaded to the `/abd/` directory on the server

### Browsing Remote Directories

The File Browser window allows you to:
- Navigate remote FTP directories
- View file details (name, size, date modified)
- Sort files by any column
- Select multiple files for download

## 📸 Screenshots

### Main Window
The main interface provides easy access to all FTP operations with real-time progress tracking and activity logs.

### File Browser
Browse remote directories with a sortable, multi-column view showing file details.

## 🏗️ Project Structure

```
FTPClient/
├── FTPClient/
│   ├── MainWindow.xaml          # Main application window
│   ├── MainWindow.xaml.cs       # Main window logic
│   ├── FilesList.xaml           # File browser dialog
│   ├── FilesList.xaml.cs        # File browser logic
│   ├── App.xaml                 # Application resources
│   └── FTPClient.csproj         # Project file
└── FTPClient.sln                # Solution file
```

## 🔧 Configuration

### Default Paths

The application uses the following default paths:
- **Stor2003 Downloads**: `c:\access\stor\`
- **Customer Data Downloads**: `y:\access\stor\`
- **Upload Directory**: `/abd/` (on FTP server)
- **Stor2003 Files**: `/program/` (on FTP server)

These can be modified in the source code as needed.

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🐛 Known Issues

- The "Download Any File" button is not yet implemented
- Application requires specific MS Access installation path for the "Run Stor2003.MDB" feature

## 🔮 Future Enhancements

- [ ] SFTP/FTPS support
- [ ] Drag-and-drop file transfers
- [ ] Connection profiles/bookmarks
- [ ] Transfer queue management
- [ ] Bandwidth throttling
- [ ] File synchronization
- [ ] Dark mode theme

## 📧 Contact

For questions, suggestions, or issues, please open an issue on GitHub.

## 🙏 Acknowledgments

- [FluentFTP](https://github.com/robinrodricks/FluentFTP) - Excellent FTP library for .NET
- [SharpCompress](https://github.com/adamhathcock/sharpcompress) - Archive extraction library

---

**Made with ❤️ using WPF and .NET 8**
