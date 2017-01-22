using Microsoft.Win32;
using PlusPlayer.Controls;
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
using System.Windows.Threading;

namespace PlusPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {        
        public Player Player = new Player();
        public Random RNG = new Random();
        
        const double PANELWIDTH = 320;

        public const string CONFIGFILENAME = "plusPlayer.cfg";

        public FolderItem RootFolder;
        
        public List<string> MusicPaths = new List<string>();

        private List<SolidColorBrush> AlternateSwatches = new List<SolidColorBrush>
        {
            (SolidColorBrush)(new BrushConverter().ConvertFrom("#BCE1EF")),
            (SolidColorBrush)(new BrushConverter().ConvertFrom("#E5EDF0")),
            (SolidColorBrush)(new BrushConverter().ConvertFrom("#688E9D")),
            (SolidColorBrush)(new BrushConverter().ConvertFrom("#6D838C")),
            (SolidColorBrush)(new BrushConverter().ConvertFrom("#738192"))
        };

        public MainWindow()
        {
            InitializeComponent();

            if (!LoadMusicPaths())
                ShowMusicPathPanel();
            else
                BuildRootList();
        }

        /// <summary>
        /// Builds a list of Paths used to create the music library
        /// </summary>
        /// <returns>If successful return True</returns>
        public bool LoadMusicPaths()
        {
            string applicationDir = AppDomain.CurrentDomain.BaseDirectory;

            if (File.Exists(applicationDir + CONFIGFILENAME))
            {
                MusicPaths = File.ReadAllLines(applicationDir + CONFIGFILENAME).Distinct().ToList();

                bool allFoldersExist = true;
                for (int i = MusicPaths.Count - 1; i >= 0; i--)
                {
                    if (!Directory.Exists(MusicPaths[i]))
                    {
                        allFoldersExist = false;

                        MusicPaths.RemoveAt(i);
                    }
                }

                if (!allFoldersExist)
                {
                    File.WriteAllLines(applicationDir + CONFIGFILENAME, MusicPaths);
                }                 
            }
                
            else
                return false;

            return (MusicPaths.Count > 0);
        }

        /// <summary>
        /// Builds the root list of folders and files from the previously set MusicPaths
        /// </summary>
        public void BuildRootList()
        {
            List<SortedPath> sortedFolders = new List<SortedPath>();
            List<SortedPath> sortedFiles = new List<SortedPath>();

            MainPanel.Children.Clear();

            foreach (string path in MusicPaths)
            {   
                List<string> tempFolders = Folders.GetSubFolders(path);
                List<string> tempFiles = Files.GetFiles(path);
                
                int pathIndex = path.Length -1;

                foreach (string folder in tempFolders)
                {
                    string compareString = folder.Substring(pathIndex);
                    SortedPath sortedPathObj = new SortedPath(folder, compareString);

                    int insertIndex = -1;

                    for (int i = 0; i < sortedFolders.Count; i++)
                    {
                        if (String.Compare(sortedFolders[i].ComparePath, sortedPathObj.ComparePath) >= 0)
                        {
                            insertIndex = i;
                            break;
                        }
                    }

                    if (insertIndex >= 0)
                        sortedFolders.Insert(insertIndex, sortedPathObj);
                    else
                        sortedFolders.Add(sortedPathObj);
                }

                foreach (string file in tempFiles)
                {
                    string compareString = file.Substring(pathIndex);
                    SortedPath sortedPathObj = new SortedPath(file, compareString);

                    int insertIndex = -1;

                    for (int i = 0; i < sortedFiles.Count; i++)
                    {
                        if (String.Compare(sortedFiles[i].ComparePath, sortedPathObj.ComparePath) >= 0)
                        {
                            insertIndex = i;
                            break;
                        }
                    }

                    if (insertIndex >= 0)
                        sortedFiles.Insert(insertIndex, sortedPathObj);
                    else
                        sortedFiles.Add(sortedPathObj);
                }
            }

            List<string> folders = new List<string>();
            
            foreach (SortedPath sortedPath in sortedFolders)            
                folders.Add(sortedPath.AbsolutePath);

            List<string> files = new List<string>();

            foreach (SortedPath sortedPath in sortedFiles)
                files.Add(sortedPath.AbsolutePath);            
            
            DirectoryList rootList = new DirectoryList(folders, files, MainPanel, null, this, false);

            RootFolder = new FolderItem(rootList);
        }

        /// <summary>
        /// Begins the animated showing of the Settings panel
        /// </summary>
        public void ShowMusicPathPanel()
        {
            SpinIcon();

            SettingsPanel settingsPanel = new SettingsPanel(this);
            settingsPanel.HorizontalAlignment = HorizontalAlignment.Right;
            foreach (string path in MusicPaths)
            {
                PathItem pathItem = new PathItem(path, settingsPanel);
                settingsPanel.MusicFolders_SP.Children.Add(pathItem);
            }

            DoubleAnimation doubleAnimation = new DoubleAnimation(0, PANELWIDTH, new Duration(TimeSpan.FromSeconds(0.2)));

            settingsPanel.BeginAnimation(UserControl.WidthProperty, doubleAnimation);
            MenuPanel.Children.Add(settingsPanel);
        }

        /// <summary>
        /// Animates spinning of the Plus Icon
        /// </summary>
        /// <param name="_forwards">Dictates direction of the spin</param>
        public void SpinIcon(bool _forwards = true)
        {
            DoubleAnimation rotateAnimation = new DoubleAnimation();

            if (_forwards)
            {
                rotateAnimation.From = 0;
                rotateAnimation.To = 360;
            }
            else
            {
                rotateAnimation.From = 360;
                rotateAnimation.To = 0;
            }
            
            rotateAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.2));

            RotateTransform rotateTransform = new RotateTransform();
            PlusIcon.RenderTransform = rotateTransform;
            PlusIcon.RenderTransformOrigin = new Point(0.5, 0.5);
            rotateTransform.BeginAnimation(RotateTransform.AngleProperty, rotateAnimation);
        }
        
        /// <summary>
        /// Chooses a random colour swatch from the preset list
        /// </summary>
        /// <returns>SolidcolorBrush</returns>
        public SolidColorBrush GetAlternateSwatch()
        {
            return AlternateSwatches[RNG.Next(0, 5)];
        }

        /// <summary>
        /// Shows the root directory list if available
        /// </summary>        
        private void PlusPlayer_But_Click(object sender, RoutedEventArgs e)
        {
            // AddTracks_But.Visibility = Visibility.Collapsed;
            SpinIcon();

            if (RootFolder != null)
                RootFolder.ChildList.ShowDirectoryList(); 

        }
          
        /// <summary>
        /// Attaches the player to the current active FolderItem
        /// </summary>
        /// <param name="_folderItem">The active FolderItem</param>
        public void ShowPlayer(FolderItem _folderItem)
        {
            SpinIcon();

            if (Player.Parent != null)            
                ((StackPanel)Player.Parent).Children.Remove(Player);
            
            if (Player.FolderItem != null)                                       
                Player.FolderItem.PlayerRemoved();
                
            Player.FolderItem = _folderItem;

            if (_folderItem == null)            
                MenuPanel.Children.Add(Player);            
            else
            {
                int index = _folderItem.ParentList.DirectoryPanel.Children.IndexOf(_folderItem) + 1;
                
                _folderItem.ParentList.DirectoryPanel.Children.Insert(index, Player);
            }
            
            DoubleAnimation doubleAnimation = new DoubleAnimation();
            doubleAnimation.From = 0;
            doubleAnimation.To = Player.PLAYER_HEIGHT;
            doubleAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.2));
            Player.BeginAnimation(UserControl.HeightProperty, doubleAnimation);
        }

        /// <summary>
        /// Begins the animated removal of the Player from the active FolderIem
        /// </summary>
        public void HidePlayer()
        {
            Player.MediaPlayer.Stop();

            SpinIcon(false);

            DoubleAnimation doubleAnimation = new DoubleAnimation();
            doubleAnimation.From = Player.PLAYER_HEIGHT;
            doubleAnimation.To = 0;
            doubleAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.2));
            doubleAnimation.Completed += RemoveMediaPlayer;
            Player.BeginAnimation(UserControl.HeightProperty, doubleAnimation);
        }

        /// <summary>
        /// Removes the Player object from the FolderItem when the animation completes
        /// </summary>        
        private void RemoveMediaPlayer(object sender, EventArgs e)
        {
            ((StackPanel)Player.Parent).Children.Remove(Player);
            Player.FolderItem = null;
        }

        /// <summary>
        /// If the Ctrl button is held, allows dragging of the main window
        /// </summary>        
        private void Window_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
                this.DragMove();
            else
                e.Handled = false;
        }

        private void Quit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Help_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SettingsMenu_Click(object sender, RoutedEventArgs e)
        {          
            ShowMusicPathPanel();
        }
    }
}

/// <summary>
/// Helper class for building the sorted root list 
/// </summary>
public class SortedPath
{
    public string AbsolutePath;
    public string ComparePath;

    public SortedPath(string _absolutePath, string _comparePath)
    {
        AbsolutePath = _absolutePath;
        ComparePath = _comparePath;
    }
}