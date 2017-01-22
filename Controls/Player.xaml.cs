using PlusPlayer.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;

namespace PlusPlayer.Controls
{
    /// <summary>
    /// Interaction logic for Player.xaml
    /// </summary>
    public partial class Player : UserControl
    {
        private FolderItem folderItem;
        public FolderItem FolderItem
        {
            get { return folderItem; }

            set
            {
                folderItem = value;
                Playlist = null;
                CurrentSong = null;


                MediaPlayer.Stop();
                isPlaying = false;
                isPaused = false;

                AlbumArt.Source = null;
                AlbumAlternate.Content = null;
                AlternateBackground.Fill = null;
                TrackData.Content = null;
                Album.Content = null;
                SongTitle.Content = null;

                if (folderItem != null)
                {
                    //BuildPlaylist(directoryList);
                    
                    SongTitle.Content = folderItem.FolderName.Content;

                    if (folderItem.AlbumArt.Source != null)                            
                        AlbumArt.Source = folderItem.AlbumArt.Source;                            
                    else
                    {
                        AlbumAlternate.Content = folderItem.AlbumAlternate.Content;
                        AlternateBackground.Fill = folderItem.AlternateBackground.Fill;
                    }
                    
                    Background = folderItem.ParentList.PlayerBackGround;                    
                }
            }
        }

        const double FILEITEM_PROGRESS_BAR_MAX_WIDTH = 201;
        const double PLAYER_PROGRESS_BAR_MAX_WIDTH = 276;
        public const double PLAYER_HEIGHT = 205;
        public MediaPlayer MediaPlayer= new MediaPlayer();

        private DispatcherTimer timer;

        private bool isPlaying = false;
        private bool isPaused = false;

        private FileItem currentSong;
        public FileItem CurrentSong
        {
            get { return currentSong; }
            set
            {
                if (currentSong != null)
                {
                    currentSong.ProgressBar_Grid.Visibility = Visibility.Hidden;
                    currentSong.Deselect();
                }               
                    
                currentSong = value;

                if (currentSong != null)
                {
                    currentSong.ProgressBar_Grid.Visibility = Visibility.Visible;
                    currentSong.ProgressBar.Width = 0;
                }
            }
        }

        public List<FileItem> Playlist;

        private MediaData mediaData;

        public Player()
        {
            InitializeComponent();
            
            Playlist = null;
        }

        /// <summary>
        /// When FolderItem is added removed - update playlist
        /// Not yet implemented
        /// </summary>
        public void RebuildPlaylist()
        {
            if (FolderItem != null)
            {
                //future planned improvement
            }
        }
        
        /// <summary>
        /// Replace existing Playlist
        /// </summary>
        /// <param name="_playList">Replacement Playlist</param>
        public void BuildLocalPlaylist(List<FileItem> _playList)
        {
            Playlist = _playList;
        }

        /// <summary>
        /// Given root folder, recursively build playlist
        /// </summary>
        /// <param name="_folderItem">Source folder for adding songs</param>
        /// <param name="_tempPlaylist">Temporary playlist being built</param>
        public void BuildPlaylist(FolderItem _folderItem, List<FileItem> _tempPlaylist = null)
        {
        
            List<FileItem> tempPlaylist;

            if (_tempPlaylist != null)
                tempPlaylist = _tempPlaylist;
            else
                tempPlaylist = new List<FileItem>();

            foreach (UserControl userControl in _folderItem.ChildList.DirectoryPanel.Children)
            {
                if (userControl is FolderItem)
                {
                    FolderItem folderItem = userControl as FolderItem;

                    if (folderItem.Folder_ICB.IsChecked)
                    {
                        if (folderItem.ChildList == null)
                            folderItem.ChildList = new DirectoryList(folderItem.AbsolutePath, folderItem.ParentList.MainPanel, folderItem, folderItem.ParentList.MainWindow, false);

                        BuildPlaylist(folderItem, tempPlaylist);
                    }
                    
                }
                else if (userControl is FileItem)
                {
                    FileItem fileItem = userControl as FileItem;
                    if (fileItem.File_ICB.IsChecked)
                        tempPlaylist.Add(fileItem);
                }
            }

            Playlist = tempPlaylist;
            
        }
        
        /// <summary>
        /// Load and play given song file
        /// </summary>
        /// <param name="_fileItem">Song file</param>
        public void PlaySong(FileItem _fileItem)
        {
            try
            {
                mediaData = null;
               // MediaPlayer.Open(new Uri(_fileItem.AbsolutePath));
               // MediaPlayer.Play();
                mediaData = LoadSong(_fileItem.AbsolutePath, TimeSpan.FromMilliseconds(10000));
                MediaPlayer.Open(new Uri( _fileItem.AbsolutePath));
           
                while (mediaData == null)
                    Thread.Sleep(100);

                if (mediaData.Done)
                {
                    timer = new DispatcherTimer();
                    timer.Interval = TimeSpan.FromSeconds(0.1);
                    timer.Tick += timer_Tick;

                    TrackData.Content = (Playlist.IndexOf(_fileItem) + 1) + " of " + Playlist.Count;
                    Album.Content = _fileItem.Album.Content;
                    SongTitle.Content = _fileItem.SongTitle.Content;

                    if (_fileItem.ParentList.ParentFolder != null)                    
                        AlbumArt.Source = _fileItem.ParentList.ParentFolder.AlbumArt.Source;
                   
                    _fileItem.Background = _fileItem.ParentList.PlayingHighlight;

                    CurrentSong = _fileItem;

                    isPlaying = true;
                    isPaused = false;

                    MediaPlayer.Play();
                    timer.Start();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error loading song into media player. " + e.Message + " Attempting to play next song in Playlist.");

                if (Playlist !=null)
                {

                    int nextTrack = Playlist.IndexOf(_fileItem) + 1;

                    if (nextTrack < Playlist.Count)
                    {
                        MediaPlayer.Stop();
                        PlaySong(Playlist[nextTrack]);
                    }
                }

            }
        }

        /// <summary>
        /// Load song file for playing
        /// </summary>
        /// <param name="_absolutePath">Absolute path to song file</param>
        /// <param name="maxTimeToWait">Time out value for loading file</param>
        /// <returns></returns>
        private MediaData LoadSong(string _absolutePath, TimeSpan maxTimeToWait)
        {
            var mediaData = new MediaData() { MediaUri = new Uri(_absolutePath) };

            var thread = new Thread(GetMediaDataThreadStart);
            DateTime deadline = DateTime.Now.Add(maxTimeToWait);
            thread.Start(mediaData);

            while (!mediaData.Done && ((TimeSpan.Zero == maxTimeToWait) || (DateTime.Now < deadline)))
                Thread.Sleep(100);

            Dispatcher.FromThread(thread).InvokeShutdown();

            if (!mediaData.Done)
                throw new Exception(string.Format("GetMediaDuration timed out after {0}", maxTimeToWait));
            if (mediaData.Failure)
                throw new Exception(string.Format("MediaFailed {0}", _absolutePath));

            return mediaData;
        }

        /// <summary>
        /// Create a non-blocking thread to play the song file
        /// </summary>
        /// <param name="context">MediaData object describing song file</param>
        private void GetMediaDataThreadStart(object context)
        {
            var mediaData = (MediaData)context;
            MediaPlayer threadSafeMediaPlayer= new MediaPlayer();
            threadSafeMediaPlayer.MediaOpened +=
                delegate
                {
                    if (threadSafeMediaPlayer.NaturalDuration.HasTimeSpan)
                        mediaData.Duration = threadSafeMediaPlayer.NaturalDuration.TimeSpan;
                    mediaData.Success = true;
                    threadSafeMediaPlayer.Close();
                };

            threadSafeMediaPlayer.MediaFailed +=
                delegate
                {
                    mediaData.Failure = true;
                    threadSafeMediaPlayer.Close();
                };

            threadSafeMediaPlayer.Open(mediaData.MediaUri);

            Dispatcher.Run();
        }

        /// <summary>
        /// Update timer for song time bar
        /// </summary>        
        private void timer_Tick(object sender, EventArgs e)
        {

            if (mediaData != null)
            {
                ProgressBar.Width = ((double)MediaPlayer.Position.TotalMilliseconds / (double)mediaData.Duration.TotalMilliseconds) * PLAYER_PROGRESS_BAR_MAX_WIDTH;

                if (currentSong != null)                
                    currentSong.ProgressBar.Width = ((double)MediaPlayer.Position.TotalMilliseconds / (double)mediaData.Duration.TotalMilliseconds) * FILEITEM_PROGRESS_BAR_MAX_WIDTH;

                if (MediaPlayer.Position.TotalMilliseconds >= mediaData.Duration.TotalMilliseconds)
                {
                    mediaData = null;
                    timer.Stop();
                    NextTrack();
                }
            }
            else
                ProgressBar.Width = 0;
        }

        /// <summary>
        /// Skip to next file in que
        /// </summary>
        public void NextTrack()
        {
            if (Playlist != null)
            {
                int nextTrack = Playlist.IndexOf(CurrentSong) + 1;

                if (nextTrack < Playlist.Count)
                {
                    MediaPlayer.Stop();                    
                    PlaySong(Playlist[nextTrack]);
                }
            }
        }

        /// <summary>
        /// Go to previous song in que
        /// </summary>
        public void PreviousTrack()
        {
            if (Playlist != null)
            {
                int priorTrack = Playlist.IndexOf(CurrentSong) - 1;

                if (priorTrack >= 0)
                {
                    MediaPlayer.Stop();
                    PlaySong(Playlist[priorTrack]);
                }
            }
        }

        /// <summary>
        /// Go to next folder and play the first song in the que
        /// </summary>
        public void NextAlbum()
        {
            if (Playlist != null && CurrentSong != null)
            {
                int curIndex = Playlist.IndexOf(CurrentSong);

                for (int i = curIndex; i < Playlist.Count; i++)
                {
                    if (CurrentSong.ParentList.ParentFolder != Playlist[i].ParentList.ParentFolder)
                    {
                        MediaPlayer.Stop();
                        CurrentSong.Deselect();
                        PlaySong(Playlist[i]);
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Go to the previous folder and play the first song in the que
        /// </summary>
        public void PreviousAlbum()
        {
            bool albumFound = false;
            FolderItem previousFolder = null;
            FileItem newTrack = null;

            if (Playlist != null && CurrentSong != null)
            {
                int curIndex = Playlist.IndexOf(CurrentSong);

                for (int i = curIndex; i >= 0; i--)
                {
                    if (CurrentSong.ParentList.ParentFolder != Playlist[i].ParentList.ParentFolder && !albumFound)
                    {
                        albumFound = true;
                        previousFolder = Playlist[i].ParentList.ParentFolder;
                    }

                    if (albumFound && Playlist[i].ParentList.ParentFolder == previousFolder)                    
                        newTrack = Playlist[i];

                    if (albumFound && Playlist[i].ParentList.ParentFolder != previousFolder)
                        break;                    
                }

                if (albumFound)
                {
                    MediaPlayer.Stop();
                    CurrentSong.Deselect();
                    PlaySong(newTrack);
                }
            }
        }
        
        private void AlbumBack_But_Click(object sender, RoutedEventArgs e)
        {
            PreviousAlbum();
        }

        private void TrackBack_But_Click(object sender, RoutedEventArgs e)
        {
            PreviousTrack();
        }

        /// <summary>
        /// Play or Pause song based on current state
        /// </summary>        
        private void PlayPause_But_Click(object sender, RoutedEventArgs e)
        {
            if (isPlaying)
            {
                if (isPaused)
                {
                    MediaPlayer.Play();
                    isPaused = false;
                }
                else
                {
                    MediaPlayer.Pause();
                    isPaused = true;
                }
                
            }
            else
            {
                if (Playlist == null)
                {
                    Stopwatch sw = new Stopwatch();
                    sw.Start();
                    
                    BuildPlaylist(folderItem);

                    sw.Stop();
                    Console.WriteLine(String.Format("It took {0}ms to create the playlist", sw.ElapsedMilliseconds));

                    CurrentSong = null;
                }

                if (CurrentSong == null)
                { 
                    if (Playlist.Count > 0)
                        PlaySong(Playlist[0]);
                }
                else
                {
                    MediaPlayer.Play();
                }
            }
        }

        private void Stop_But_Click(object sender, RoutedEventArgs e)
        {
            MediaPlayer.Stop();
            isPlaying = false;
            isPaused = false;
        }

        private void TrackForward_But_Click(object sender, RoutedEventArgs e)
        {
            NextTrack();
        }

        private void AlbumForward_But_Click(object sender, RoutedEventArgs e)
        {
            NextAlbum();
        }

        private void RepeatSingle_But_Click(object sender, RoutedEventArgs e)
        {

        }

        private void RepeatAll_But_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Shuffle_But_Click(object sender, RoutedEventArgs e)
        {

        }


        private void Slider_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Slider.CaptureMouse();
        }

        private void Slider_MouseMove(object sender, MouseEventArgs e)
        {
            
            if (Slider.IsMouseCaptured)
            {
                double x = e.GetPosition(Slider_CV).X;

                if (x < -1)                
                    x = -1;

                if (x > 141)
                    x = 141;

                Canvas.SetLeft(Slider, x);

                MediaPlayer.Volume = (x + 1) / 142;           
            }
            
        }

        private void Slider_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Slider.ReleaseMouseCapture();
        }
        

        private void ProgressBarSlider_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ProgressBarSlider.CaptureMouse();
        }

        private void ProgressBarSlider_MouseMove(object sender, MouseEventArgs e)
        {
            if (ProgressBarSlider.IsMouseCaptured)
            {
                double x = e.GetPosition(ProgressBarSlider).X;

                double duration = MediaPlayer.NaturalDuration.TimeSpan.TotalMilliseconds;

                double position = (x / ProgressBarSlider.ActualWidth);

                if (position < 0)
                    position = 0;
                else if (position > 1)
                    position = 0.9999;

                duration = duration * position;

                MediaPlayer.Position = TimeSpan.FromMilliseconds(duration);
            }
        }

        private void ProgressBarSlider_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (ProgressBarSlider.IsMouseCaptured)
            {
                double x = e.GetPosition(ProgressBarSlider).X;

                double duration = MediaPlayer.NaturalDuration.TimeSpan.TotalMilliseconds;

                double position = (x / ProgressBarSlider.ActualWidth);

                if (position < 0)
                    position = 0;
                else if (position > 1)
                    position = 0.99;

                duration = duration * position;                

                MediaPlayer.Position = TimeSpan.FromMilliseconds(duration);

                ProgressBarSlider.ReleaseMouseCapture();
            }
        }
    }
}

