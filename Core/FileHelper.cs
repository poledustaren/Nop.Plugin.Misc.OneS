using System.Collections.Generic;
using System.IO;

namespace Nop.Plugin.Misc.OneS.Core
{
    public static class FileHelper
    {
        public static void MoveToProcessed(string pathToFile, string pathToProcessed)
        {
            pathToProcessed = pathToProcessed + @"\Exchange\Processed";
            if (!Directory.Exists(pathToProcessed))
                Directory.CreateDirectory(pathToProcessed);
            var destinationFilename = Path.Combine(pathToProcessed, Path.GetFileName(pathToFile));
            MoveFile(pathToFile, destinationFilename);
        }

        public static void MoveToError(string pathToFile, string pathToError)
        {
            pathToError = pathToError + @"\Exchange\Error";
            if (!Directory.Exists(pathToError))
                Directory.CreateDirectory(pathToError);

            var destinationFilename = Path.Combine(pathToError, Path.GetFileName(pathToFile));

            MoveFile(pathToFile, destinationFilename);
            
        }

        private static void MoveFile(string pathToFile, string destinationFilename)
        {
            if (!File.Exists(destinationFilename))
            {
                File.Move(pathToFile, destinationFilename);
            }
            else
            {
                File.Delete(destinationFilename);
                File.Move(pathToFile, destinationFilename);
            }
        }
    }
}