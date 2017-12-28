using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cql.Common.Logging;
using Cql.ProcessScheduler.Library;
using System.IO.Compression;
using Renci.SshNet;

namespace EnsightenGTMConverter.Core
{
    public class FileManager
    {
        private static readonly ILogger Logger = LogContextManager.GetLogger<FileManager>();
        
        /// <summary>
        /// Replicates files into multiple destination folders.
        /// </summary>
        /// <param name="sourceFile">The source file to copy.</param>
        /// <param name="destinationFiles">Where the file should be copied.</param>
        /// <param name="overwrite">Whether the destination should be overwritten if it already exists.</param>
        /// <param name="compress">Whether to zip the file up as part of the copy operation.</param>
        /// <returns></returns>
        /// <remarks>Swallows exceptions, but does log them as errors.</remarks>
        public static CodedResult<FileReplicatorResult> ReplicateFile(string sourceFile, IEnumerable<string> destinationFiles, bool overwrite = false, bool compress = false)
        {
            if (destinationFiles == null)
                return new CodedResult<FileReplicatorResult>(true, new FileReplicatorResult() { FailedCopies = new string[0], SuccessfulCopies = new string[0], });
            var successfulCopies = new List<string>();
            var failedCopies = new List<string>();
            foreach (var destination in destinationFiles)
            {
                try
                {
                    var destinationFolder = Path.GetDirectoryName(destination);
                    if (destinationFolder != null && !Directory.Exists(destinationFolder))
                    Directory.CreateDirectory(destinationFolder);
                    File.Copy(sourceFile, destination, overwrite);
                    successfulCopies.Add(destination);
                }
                catch (Exception ex)
                {
                    Logger.Error(string.Format("Exception occurred while copying {0} to {1}.", sourceFile, destination), ex);
                    failedCopies.Add(destination);
                }
            }
            var result = new FileReplicatorResult() { FailedCopies = failedCopies.ToArray(), SuccessfulCopies = successfulCopies.ToArray(), };
            return new CodedResult<FileReplicatorResult>(failedCopies.Count == 0, result);
        }

        /// <summary>
        /// Deletes files older than a specific number of days within a folder.
        /// </summary>
        /// <param name="folder">The folder to scan.</param>
        /// <param name="filenamePattern">The file naming pattern (default is "*.*").</param>
        /// <param name="daysToKeep">The number of days history to maintain.  Note that -5 and 5 will BOTH be treated as 5 days history to keep.</param>
        /// <remarks>Swallows exceptions, but does log them as errors.</remarks>
        public static CodedResult<FileCleanerResult> CleanFiles(string folder, string filenamePattern, int daysToKeep)
        {
            if (daysToKeep > 0)
                daysToKeep = daysToKeep * -1;
            var currentFiles = Directory.GetFiles(folder, filenamePattern);
            var oldestFileToKeep = DateTime.UtcNow.AddDays(daysToKeep);
            var deletedFiles = new List<string>();
            var errorFiles = new List<string>();
            foreach (var f in currentFiles)
            {
                try
                {
                    var utcCreatedDate = File.GetCreationTimeUtc(f);
                    if (utcCreatedDate < oldestFileToKeep)
                    {
                        File.Delete(f);
                        deletedFiles.Add(f);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(string.Format("Error while cleaning up folder {0}, accessing {1}.", folder, f), ex);
                    errorFiles.Add(f);
                }
            }

            var result = new FileCleanerResult() { DeletedFiles = deletedFiles.ToArray(), ErrorFiles = errorFiles.ToArray(), FilesLeftInPlace = currentFiles.Length - deletedFiles.Count, };

            return new CodedResult<FileCleanerResult>(result.ErrorFiles.Length == 0, result);
        }
        
    }

    public class FileReplicatorResult
    {
        public string[] SuccessfulCopies { get; set; }
        public string[] FailedCopies { get; set; }
    }

    public class FileCleanerResult
    {
        public string[] DeletedFiles { get; set; }
        public string[] ErrorFiles { get; set; }
        public int FilesLeftInPlace { get; set; }
    }
}
