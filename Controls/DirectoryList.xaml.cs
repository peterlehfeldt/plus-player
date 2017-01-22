using PlusPlayer.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PlusPlayer.Controls
{
    /// <summary>
    /// Interaction logic for DirectoryList.xaml
    /// </summary>
    public partial class DirectoryList : UserControl
    {
        public string AbsolutePath;
        public MainWindow MainWindow;
        public StackPanel MainPanel;
        public FolderItem ParentFolder;

        public SolidColorBrush SelectedHighlight = (SolidColorBrush)(new BrushConverter().ConvertFrom("#BFD6F0"));
        public SolidColorBrush ParentHighlight = (SolidColorBrush)(new BrushConverter().ConvertFrom("#E6EBF1"));
        public SolidColorBrush PlayingHighlight = (SolidColorBrush)(new BrushConverter().ConvertFrom("#C1CEF1"));
        public SolidColorBrush PlayerBackGround = (SolidColorBrush)(new BrushConverter().ConvertFrom("#979BA0"));
        private SolidColorBrush lightBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#9C9EA5"));
        private SolidColorBrush darkBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#91979A"));
        private SolidColorBrush currentBrush;

        public bool AddedToPanel = false;

        private FolderItem selectedFolderItem;
        public FolderItem SelectedFolderItem
        {
            get { return selectedFolderItem; }
            set
            {
                MainWindow.SpinIcon(true);

                if (selectedFolderItem != null && selectedFolderItem != value)
                {
                    selectedFolderItem.Deselect();
                    selectedFolderItem = null;
                }

                if (selectedFileItem != null)
                {
                    selectedFileItem.Deselect();
                    selectedFileItem = null;
                }

                if (value != null)
                {
                    selectedFolderItem = value;
                    selectedFolderItem.Select();
                }
            }
        }

        private FileItem selectedFileItem;
        public FileItem SelectedFileItem
        {
            get { return selectedFileItem; }
            set
            {
                if (selectedFolderItem != null)
                {
                    selectedFolderItem.Deselect();
                    selectedFolderItem = null;
                }

                if (selectedFileItem != null)
                {
                    selectedFileItem.Deselect();
                    selectedFileItem = null;
                }

                if (value != null)
                {
                    selectedFileItem = value;
                    selectedFileItem.Select();
                }
            }
        }
        /// <summary>
        /// Data object describing one level in a Directory Structure. Directory structure is based by folder and file lists.
        /// </summary>
        /// <param name="_folders">List of Folders at this level</param>
        /// <param name="_files">List of Files at this level</param>
        /// <param name="_parentPanel">Stack panel to display the structure in</param>
        /// <param name="_parentFolder">Parent folder in directory structure</param>
        /// <param name="_mainWindow">Refence to MainWindow object</param>
        /// <param name="_showPanel">Boolean used to hide or show panel</param>
        public DirectoryList(List<string> _folders, List<string> _files, StackPanel _parentPanel, FolderItem _parentFolder, MainWindow _mainWindow, bool _showPanel)
        {
            InitializeComponent();

            AbsolutePath = "";

            MainWindow = _mainWindow;
            MainPanel = _parentPanel;
            ParentFolder = _parentFolder;

            BuildDirectoryList(_folders, _files, _showPanel);
        }

        /// <summary>
        /// Data object describing one level in a Directory Structure. Directory structure is computed from absolute path.
        /// </summary>
        /// <param name="_absolutePath">Absolute path used to calculate the directory structure</param>
        /// <param name="_parentPanel">Stack panel to display the structure in</param>
        /// <param name="_parentFolder">Parent folder in directory structure</param>
        /// <param name="_mainWindow">Refence to MainWindow object</param>
        /// <param name="_showPanel">Boolean used to hide or show panel</param>
        public DirectoryList(string _absolutePath, StackPanel _parentPanel, FolderItem _parentFolder, MainWindow _mainWindow, bool _showPanel)
        {
            InitializeComponent();

            AbsolutePath = _absolutePath;

            List<string> folders = Folders.GetSubFolders(AbsolutePath);
            List<string> files = Files.GetFiles(AbsolutePath);

            MainWindow = _mainWindow;
            MainPanel = _parentPanel;
            ParentFolder = _parentFolder;

            BuildDirectoryList(folders, files, _showPanel);
        }

        /// <summary>
        /// Build the visual directory list and add to stack panel
        /// </summary>
        /// <param name="_folders">List of folders in data structure</param>
        /// <param name="_files">List of files in data structure</param>
        /// <param name="_showPanel">Show or hide panel based on boolean</param>
        private void BuildDirectoryList(List<string> _folders, List<string> _files, bool _showPanel)
        { 
            
            if (ParentFolder != null)
                if (ParentFolder.ParentList != null)
                    if (ParentFolder.ParentList.ParentFolder != null)
                        ParentFolder.ParentList.ParentFolder.Background = ParentHighlight;
            
            currentBrush = lightBrush;

            foreach (string folderPath in _folders)
            {
                FolderItem folderItem = new FolderItem(folderPath,this, MainPanel, currentBrush);
                
                if (currentBrush == lightBrush)
                    currentBrush = darkBrush;
                else
                    currentBrush = lightBrush;

                if (ParentFolder != null)                
                    folderItem.Folder_ICB.IsChecked = ParentFolder.Folder_ICB.IsChecked;
                 

                DirectoryPanel.Children.Add(folderItem);
            }

            List<string> audioFiles = new List<string>();
            foreach (string file in _files)
            {
                if (System.IO.Path.GetExtension(file) == ".mp3")
                    audioFiles.Add(file);
            }

            foreach (string audioFile in audioFiles)
            {
                FileItem fileItem = new FileItem(audioFile,this, MainPanel, currentBrush);
                DirectoryPanel.Children.Add(fileItem);
                
                if (currentBrush == lightBrush)
                    currentBrush = darkBrush;
                else
                    currentBrush = lightBrush;

                if (ParentFolder != null)
                    fileItem.File_ICB.IsChecked = ParentFolder.Folder_ICB.IsChecked;

            }

            if (_showPanel)            
                ShowDirectoryList();
        }

        /// <summary>
        /// Begin animated display of directory list
        /// </summary>
        public void ShowDirectoryList()
        {
            if (!AddedToPanel)
            {
                AddedToPanel = true;

                MainWindow.SpinIcon(false);

                DoubleAnimation doubleAnimation = new DoubleAnimation();
                doubleAnimation.From = 0;
                doubleAnimation.To = 320;
                doubleAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.2));
                this.BeginAnimation(UserControl.WidthProperty, doubleAnimation);

                MainPanel.Children.Add(this);
                MainWindow.LayoutUpdated += ResizeWindow;
            }
        }

        private void ResizeWindow(object sender, EventArgs e)
        {
            MainWindow.LayoutUpdated -= ResizeWindow;

                MainWindow.Width = MainPanel.ActualWidth + 320;

        }
    }
}
