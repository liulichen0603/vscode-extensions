using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VsChromium.Core.Ipc;
using VsChromium.Core.Ipc.TypedMessages;
using VsChromium.Core.Threads;
using VsChromium.ServerProxy;
using VsChromium.Threads;

namespace FHL
{
    internal class CodeSearchController
    {
        private static class OperationsIds
        {
            public const string FileSystemScanning = "file-system-scanning";
            public const string FilesLoading = "files-loading";
            public const string SearchCode = "files-contents-search";
            public const string SearchFilePaths = "file-names-search";
        }

        public CodeSearchController(string workspace) {
            _fileSystemTreeSource = new FileSystemTreeSource(_dispatchThreadServerRequestExecutor._typedRequestProcessProxy, _dispatchThreadServerRequestExecutor._delayedOperationExecutor);
            _fileRegistrationRequestService = new FileRegistrationRequestService(_dispatchThreadServerRequestExecutor);
            _fileSystemTreeSource.Fetch();
            _fileRegistrationRequestService.RegisterTextDocument(workspace);
        }

        private int _operationSequenceId;
        private readonly DispatchThreadServerRequestExecutor _dispatchThreadServerRequestExecutor = new DispatchThreadServerRequestExecutor();
        private readonly FileSystemTreeSource _fileSystemTreeSource;
        private readonly FileRegistrationRequestService _fileRegistrationRequestService;


        private class SearchWorkerParams
        {
            /// <summary>
            /// Simple short name of the operation (for debugging only).
            /// </summary>
            public string OperationName { get; set; }
            /// <summary>
            /// Short description of the operation (for display in status bar
            /// progress)
            /// </summary>
            public string HintText { get; set; }
            /// <summary>
            /// The request to sent to the server
            /// </summary>
            public TypedRequest TypedRequest { get; set; }
            /// <summary>
            /// Amount of time to wait before sending the request to the server.
            /// </summary>
            public TimeSpan Delay { get; set; }
            /// <summary>
            /// Lambda invoked when the response to the request has been successfully
            /// received from the server.
            /// </summary>
            public Action<TypedResponse, Stopwatch> ProcessResponse { get; set; }
            /// <summary>
            /// Lambda invoked when the request resulted in an error from the server.
            /// </summary>
            public Action<ErrorResponse, Stopwatch> ProcessError { get; set; }
        }
        private SearchCodeRequest CreateSearchCodeRequest(string searchPattern, string filePathPattern, int maxResults)
        {
            return new SearchCodeRequest
            {
                SearchParams = new SearchParams
                {
                    SearchString = searchPattern,
                    FilePathPattern = filePathPattern,
                    MaxResults = maxResults,
                    MatchCase = true,
                    MatchWholeWord = false,
                    IncludeSymLinks = false,
                    UseRe2Engine = true,
                    Regex = false
                }
            };
        }

        private void SearchWorker(SearchWorkerParams workerParams)
        {
            var id = Interlocked.Increment(ref _operationSequenceId);
            var progressId = string.Format("{0}-{1}", workerParams.OperationName, id);
            var sw = new Stopwatch();
            var request = new DispatchThreadServerRequest
            {
                // Note: Having a single ID for all searches ensures previous search
                // requests are superseeded.
                Id = "MetaSearch",
                Request = workerParams.TypedRequest,
                Delay = workerParams.Delay,
                OnThreadPoolSend = () => {
                    sw.Start();
                },
                OnThreadPoolReceive = () => {
                    sw.Stop();
                },
                OnDispatchThreadSuccess = typedResponse => {
                    workerParams.ProcessResponse(typedResponse, sw);
                },
                OnDispatchThreadError = errorResponse => {
                    workerParams.ProcessError(errorResponse, sw);
                }
            };

            _dispatchThreadServerRequestExecutor.Post(request);
        }

        private void SearchCode(string searchPattern, string filePathPattern, bool immediate)
        {
            var maxResults = 10000;
            var request = CreateSearchCodeRequest(searchPattern, filePathPattern, maxResults);
            SearchWorker(new SearchWorkerParams
            {
                OperationName = OperationsIds.SearchCode,
                HintText = "Searching for matching text in files...",
                Delay = TimeSpan.FromMilliseconds(0),
                TypedRequest = request,
                ProcessError = (errorResponse, stopwatch) => {
                    
                },
                ProcessResponse = (typedResponse, stopwatch) => {
                    var response = ((SearchCodeResponse)typedResponse);
                    var msg = string.Format("Found {0:n0} results among {1:n0} files ({2:0.00} seconds) matching text \"{3}\"",
                      response.HitCount,
                      response.SearchedFileCount,
                      stopwatch.Elapsed.TotalSeconds,
                      searchPattern);
                    if (!string.IsNullOrEmpty(filePathPattern))
                    {
                        msg += string.Format(", File Paths: \"{0}\"", filePathPattern);
                    }
                    Console.WriteLine(msg);
                    if (response.SearchResults.Entries.Count > 0)
                    {
                        GlobalClass.OneResponse = (DirectoryEntry)response.SearchResults.Entries[0];
                    }
                    else {
                        GlobalClass.OneResponse = new DirectoryEntry();
                    }
                    GlobalClass.WaitEnd();
                }
            });
        }

        public void PerformSearch(string searchString, string searchPath)
        {
            var searchCodeText = searchString;
            var searchFilePathsText = searchPath;

            if (string.IsNullOrWhiteSpace(searchCodeText) &&
                string.IsNullOrWhiteSpace(searchFilePathsText))
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(searchCodeText))
            {
                // SearchFilesPaths(searchFilePathsText, immediate);
                return;
            }

            SearchCode(searchCodeText, searchFilePathsText, true);
        }
    }
}


