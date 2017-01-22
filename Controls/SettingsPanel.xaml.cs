using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
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
    /// Interaction logic for SettingsPanel.xaml
    /// </summary>
    public partial class SettingsPanel : UserControl
    {
        private MainWindow mainWindow;
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="_mainWindow"></param>
        public SettingsPanel(MainWindow _mainWindow)
        {
            InitializeComponent();

            mainWindow = _mainWindow;
        }

        private void Close_But_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.SpinIcon(false);

            List<string> newPaths = new List<string>();

            for (int i = MusicFolders_SP.Children.Count-1; i >= 0; i--)            
                newPaths.Add((MusicFolders_SP.Children[i] as PathItem).Path);
            

            if (newPaths.Count != mainWindow.MusicPaths.Count)
            {
                mainWindow.MusicPaths = newPaths;
                WriteSettings(newPaths);
                mainWindow.BuildRootList();
            }
            else
            {
                newPaths.Sort();
                mainWindow.MusicPaths.Sort();

                for (int i = 0; i < newPaths.Count; i++)
                {
                    if (newPaths[i] != mainWindow.MusicPaths[i])
                    {
                        mainWindow.MusicPaths = newPaths;
                        WriteSettings(newPaths);
                        mainWindow.BuildRootList();
                        break;
                    }
                }
            }

            DoubleAnimation doubleAnimation = new DoubleAnimation(320, 0, new Duration(TimeSpan.FromSeconds(0.2)));
            doubleAnimation.Completed += RemovePanel;
            this.BeginAnimation(UserControl.WidthProperty, doubleAnimation);
            
        }

        private void WriteSettings(List<string> _paths)
        {            
            File.WriteAllLines(AppDomain.CurrentDomain.BaseDirectory + MainWindow.CONFIGFILENAME, _paths);            
        }

        private void RemovePanel(object sender, EventArgs e)
        {
            mainWindow.MenuPanel.Children.Remove(this);
        }

        private void AddMusicPath_But_Click(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog folderExplorer = new VistaFolderBrowserDialog();
            
            if (folderExplorer.ShowDialog() == true)
            {
                string path = folderExplorer.SelectedPath;

                foreach (PathItem _pathItem in MusicFolders_SP.Children)                
                    if (_pathItem.Path == path)
                        return;
                
                PathItem pathItem = new PathItem(path, this);

                MusicFolders_SP.Children.Add(pathItem);
            }                
        }
    }
}
