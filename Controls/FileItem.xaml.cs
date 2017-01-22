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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TagLib;
using Microsoft.WindowsAPICodePack.Shell;
using PlusPlayer.Utility;

namespace PlusPlayer.Controls
{
    /// <summary>
    /// Interaction logic for FileItem.xaml
    /// </summary>
    public partial class FileItem : UserControl
    {
        public string AbsolutePath;

        private SolidColorBrush background;
        public DirectoryList ParentList;
        public TagLib.File TagFile;

        /// <summary>
        /// Constructor for FileItem representing a file in a directory structure
        /// </summary>        
        /// <param name="_absolutePath">Absolute path to the parent directory</param>
        /// <param name="_parentList">DirectoryList object parenting this child FolderItem</param>        
        /// <param name="_parentPanel">The StackPanel to which this File Item object will be attached</param>param>
        /// <param name="_background">The background SolidColorBrush</param>
        public FileItem(string _absolutePath, DirectoryList _parentList, StackPanel _parentPanel, SolidColorBrush _background)
        {
            InitializeComponent();

            AbsolutePath = _absolutePath;
            ParentList = _parentList;

            background = _background;
            Background = background;

            File_ICB.CheckUpdated += FileCheckBoxUpdated;

            try
            {
                TagFile = TagLib.File.Create(AbsolutePath);

                if (TagFile.Tag.Title != "" && TagFile.Tag.Title != null)
                    SongTitle.Content = TagFile.Tag.Title;

                string artists = "";
                foreach (string artist in TagFile.Tag.AlbumArtists)
                {
                    if (artists != "")
                        artists += ", ";

                    artists += artist;
                }
                
                Album.Content = TagFile.Tag.Album;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + " - " + e.InnerException);
            }

            if (SongTitle.Content == null)
            {
                SongTitle.Content = System.IO.Path.GetFileNameWithoutExtension(AbsolutePath);
            }          
                
            try
            {                
                ShellFile so = ShellFile.FromFilePath(AbsolutePath);

                double nanoseconds;
                double.TryParse(so.Properties.System.Media.Duration.Value.ToString(), out nanoseconds);

                double s = 0, m = 0, h = 0;
                ConvertTime.NanosecondsToHours(nanoseconds, out nanoseconds, out s, out m, out h);

                SongLength.Content = ConvertTime.TimeUnitsToString(h, m, s);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + " - " + e.InnerException);
            }
        }

        /// <summary>
        /// Called when file item is visually highlighted
        /// </summary>
        public void Select()
        {            
            Background = ParentList.PlayingHighlight;
        }

        /// <summary>
        /// Called when file item is visually returned to default visual state
        /// </summary>
        public void Deselect()
        {
            Background = background;            
        }

        /// <summary>
        /// Add/remove item from play que
        /// </summary>
        /// <param name="_value">Add/remove based on True/False value</param>
        public void FileCheckBoxUpdated(bool _value)
        {            
            ParentList.MainWindow.Player.RebuildPlaylist();            
        }

        private void HitBox_MouseEnter(object sender, MouseEventArgs e)
        {

        }

        private void HitBox_MouseLeave(object sender, MouseEventArgs e)
        {

        }

        private void HitBox_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            HitBox.CaptureMouse();
        }

        /// <summary>
        /// On left mouse button click, play if in que otherwise show player
        /// </summary>        
        private void HitBox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            HitBox.ReleaseMouseCapture();
            if (HitBox.IsMouseDirectlyOver)
            {
                if (ParentList.ParentFolder != null)
                {
                    ParentList.SelectedFileItem = this;
                    ParentList.MainWindow.Player.PlaySong(this);
                }
                else
                {
                    ParentList.SelectedFileItem = this;

                    if (ParentList.MainWindow.Player.Parent != ParentList.MainWindow.MenuPanel)
                    {
                        ParentList.MainWindow.ShowPlayer(null);
                        ParentList.MainWindow.Player.Background = ParentList.PlayerBackGround;
                    }

                    List<FileItem> localList = new List<FileItem>();

                    foreach (UserControl uc in ParentList.DirectoryPanel.Children)
                        if (uc is FileItem)
                        {
                            localList.Add(uc as FileItem);
                            (uc as FileItem).File_ICB.IsChecked = true;
                        }
                    ParentList.MainWindow.Player.BuildLocalPlaylist(localList);
                    
                    ParentList.MainWindow.Player.PlaySong(this);
                }          
            }
        }
    }
}
