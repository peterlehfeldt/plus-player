using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PlusPlayer.Utility
{
    /// <summary>
    /// Wrapper class for file retrieval
    /// </summary>
    public static class Files
    {
        public static List<string> GetFiles(string _absolutePath)
        {
            try
            {
                List<string> files = new List<string>(Directory.EnumerateFiles(_absolutePath));
                return files;
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
