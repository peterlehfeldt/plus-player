using PlusPlayer.Utility;
using System;
using System.Collections.Generic;
using System.IO;
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
  /// Class object used in describing folder items in a directory structure.    
  /// </summary>

    public partial class FolderItem : UserControl
    {
        public string AbsolutePath;

        private SolidColorBrush background;
        private StackPanel mainPanel;
        public DirectoryList ParentList, ChildList;

        private bool hasPlayer;
        public bool HasPlayer
        {
            get { return hasPlayer; }
            set
            {
                hasPlayer = value;
                Folder_ICB.IsChecked = value;
                DoubleAnimation rotateAnimation = new DoubleAnimation();

                if (hasPlayer)
                {
                    rotateAnimation.From = 0;
                    rotateAnimation.To = -90;


                    if (ChildList == null)                   
                        ChildList = new DirectoryList(AbsolutePath, mainPanel, this, ParentList.MainWindow, true);                                      
                    else
                        ChildList.ShowDirectoryList();

                    ParentList.MainWindow.ShowPlayer(this);

                    ParentList.SelectedFolderItem = this;                    
                }
                else
                {
                    rotateAnimation.From = -90;
                    rotateAnimation.To = 0;

                  
                    ParentList.MainWindow.HidePlayer();

                    if (ParentList.SelectedFolderItem != null)                    
                        ParentList.SelectedFolderItem.Deselect();
                    
                }

                rotateAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.2));

                RotateTransform rotateTransform = new RotateTransform();
                OpenPlayerImage.RenderTransform = rotateTransform;
                OpenPlayerImage.RenderTransformOrigin = new Point(0.5, 0.5);
                rotateTransform.BeginAnimation(RotateTransform.AngleProperty, rotateAnimation);

                
                
            }
        }

        /// <summary>
        /// Constructor for FolderItem representing a folder in a directory structure
        /// </summary>
        /// <param name="_absolutePath">Absolute path to the parent directory</param>
        /// <param name="_parentList">DirectoryList object parenting this child FolderItem</param>
        /// <param name="_mainPanel">The StackPanel to which this Folder Item object will be attached</param>
        /// <param name="_background">The background SolidColorBrush</param>
        public FolderItem(string _absolutePath, DirectoryList _parentList, StackPanel _mainPanel, SolidColorBrush _background)
        {
            InitializeComponent();

            AbsolutePath = _absolutePath;
            List<string> folders = Folders.GetSubFolders(AbsolutePath);
            List<string> files = Files.GetFiles(AbsolutePath);
            List<string> audioFiles = new List<string>();

            Folder_ICB.CheckUpdated += FolderCheckBoxUpdated;

            mainPanel = _mainPanel;
            ParentList = _parentList;

            background = _background;
            Background = background;

            
            bool imageSet = false;

            foreach (string file in files)
            {
                string ext = System.IO.Path.GetExtension(file);

                switch (ext)
                {
                    case ".mp3":
                        audioFiles.Add(file);
                        break;

                    case ".png":
                        if (!imageSet)
                            imageSet = SetImageSource(file);
                        
                        break;

                    case ".jpg":
                        if (!imageSet)
                            imageSet = SetImageSource(file);

                        break;

                    case ".jpeg":
                        if (!imageSet)
                            imageSet = SetImageSource(file);

                        break;

                    case ".bmp":
                        if (!imageSet)
                            imageSet = SetImageSource(file);

                        break;
                }                           
            }

            FolderName.Content = new DirectoryInfo(AbsolutePath).Name;

            if (!imageSet)
                SetAlternateArt();


            string folderCount = "";
            if (folders.Count > 0)
            {
                folderCount = String.Format("{0} Folder", folders.Count);

                if (folders.Count > 1)
                    folderCount += "s";
            }
            
            string fileCount = "";
            if (files.Count > 0)
            {
                fileCount = String.Format("{0} File", audioFiles.Count);

                if (files.Count > 1)
                    fileCount += "s";
            }

            if (folderCount != "" && fileCount != "")
            {
                folderCount += ", ";
            }

            ItemCount.Content = folderCount + fileCount;
        }

        /// <summary>
        /// Constructor for the root folder in a folder structure               
        /// </summary>
        /// <param name="_childList">The sub-folder/file DirectoryList</param>
        public FolderItem(DirectoryList _childList)
        {
            ChildList = _childList;
        }

        /// <summary>
        /// Sets the image associated with the folder             
        /// </summary>
        /// <param name="_file">Absolute path to the image file</param>  
        private bool SetImageSource(string _file)
        {
            if (AlbumArt.Source == null)
            {
                try
                {
                    AlbumArt.Source = new BitmapImage(new Uri(_file));
                    return true;
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);

                    Console.WriteLine("Above error generated trying to open : " + _file);
        
                }                
            }

            return false;
        }

        /// <summary>
        /// Sets the image associated with the folder to a letter or digit associated with the filename        
        /// </summary>
        private void SetAlternateArt()
        {
            char title = char.ToUpper(FolderName.Content.ToString()[0]);
            
            AlbumAlternate.Content = title;
            AlternateBackground.Fill = ParentList.MainWindow.GetAlternateSwatch();
        }

        /// <summary>
        /// Sets the background colour according to the parent DirectoryList
        /// </summary>
        public void Select()
        {
            Background = ParentList.SelectedHighlight;
        }


        /// <summary>
        /// Reset the background colour
        /// </summary>
        public void Deselect()
        {
          
            Background = background;
             
            if (ChildList != null)
            {
                RemoveChildList();
            }
           
        }

        /// <summary>
        /// Sets the background colour according to the parent DirectoryList
        /// </summary>
        public void PlayerRemoved()
        {
            DoubleAnimation rotateAnimation = new DoubleAnimation();
            
            rotateAnimation.From = -90;
            rotateAnimation.To = 0;
            
            rotateAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.2));

            RotateTransform rotateTransform = new RotateTransform();
            OpenPlayerImage.RenderTransform = rotateTransform;
            OpenPlayerImage.RenderTransformOrigin = new Point(0.5, 0.5);
            rotateTransform.BeginAnimation(RotateTransform.AngleProperty, rotateAnimation);

            Folder_ICB.IsChecked = false;
            hasPlayer = false;
        }

        /// <summary>
        /// Begin animated child folder removal
        /// </summary>
        public void RemoveChildList()
        {
            if (ChildList.SelectedFolderItem != null)
            {                
                ChildList.SelectedFolderItem.Deselect();
                ChildList.SelectedFolderItem = null;
            }

            DoubleAnimation doubleAnimation = new DoubleAnimation();
            doubleAnimation.From = ChildList.ActualWidth;
            doubleAnimation.To = 0;
            doubleAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.2));

            doubleAnimation.Completed += RemoveChildListAfterAnimation;
            ChildList.BeginAnimation(UserControl.WidthProperty, doubleAnimation);            
        }

        /// <summary>
        /// Finalise removal of child folder list
        /// </summary>
        void RemoveChildListAfterAnimation(object sender, EventArgs e)
        {
            mainPanel.Children.Remove(ChildList);
            ChildList.AddedToPanel = false;            
        }

        /// <summary>
        /// Add/remove playable items from the que        
        /// </summary>
        /// <param name="_value">Current value of the check box</param>
        public void FolderCheckBoxUpdated(bool _value)
        {
            ParentList.MainWindow.Player.Playlist = null;

            if (ChildList != null)
            {
                foreach (UserControl uc in ChildList.DirectoryPanel.Children)
                {
                    if (uc is FolderItem)
                    {
                        (uc as FolderItem).Folder_ICB.IsChecked = _value;
                    }

                    if (uc is FileItem)
                    {
                        (uc as FileItem).File_ICB.IsChecked = _value;
                    }

                }
            }

            
        }

        
        private void HitBox_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            HitBox.CaptureMouse();
        }

        private void OpenPlayer_But_Click(object sender, RoutedEventArgs e)
        {
            HasPlayer = !hasPlayer;            
        }

        /// <summary>
        /// If shift key is down at folder to playable que
        /// </summary>
        private void HitBox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            HitBox.ReleaseMouseCapture();
            if (HitBox.IsMouseDirectlyOver)
            {
                if ((Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)))
                {
                    Folder_ICB.IsChecked = !Folder_ICB.IsChecked;
                }
                else if (ParentList.SelectedFolderItem != this)
                {
                    ParentList.SelectedFolderItem = this;

                    if (ChildList == null)
                    {
                        ChildList = new DirectoryList(AbsolutePath, mainPanel, this, ParentList.MainWindow, true);                        
                    }
                    else
                        ChildList.ShowDirectoryList();

                }
                else
                {
                    ParentList.SelectedFolderItem.Deselect();
                    ParentList.SelectedFolderItem = null;
                }


            }
        }
    }
}
