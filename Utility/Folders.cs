using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PlusPlayer.Utility
{
    /// <summary>
    /// Wrapper class for folder structure retrieval
    /// </summary>
    public static class Folders
    {        
        public static List<string> GetSubFolders(string _absolutePath)
        {
            try
            {              
                List<string> dirs = new List<string>(Directory.EnumerateDirectories(_absolutePath));
                return dirs;             
            }
            catch (UnauthorizedAccessException UAEx)
            {
                Console.WriteLine(UAEx.Message);
            }
            catch (PathTooLongException PathEx)
            {
                Console.WriteLine(PathEx.Message);
            }            
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            
            return null;
        }
    }
}





