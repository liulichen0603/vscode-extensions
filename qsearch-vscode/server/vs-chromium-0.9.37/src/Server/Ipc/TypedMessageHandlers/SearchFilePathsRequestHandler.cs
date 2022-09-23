﻿// Copyright 2013 The Chromium Authors. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.

using System.ComponentModel.Composition;
using VsChromium.Core.Ipc.TypedMessages;
using VsChromium.Server.FileSystem;
using VsChromium.Server.FileSystemNames;
using VsChromium.Server.Search;

namespace VsChromium.Server.Ipc.TypedMessageHandlers {
  [Export(typeof(ITypedMessageRequestHandler))]
  public class SearchFilePathsRequestHandler : TypedMessageRequestHandler {
    private readonly IFileSystemNameFactory _fileSystemNameFactory;
    private readonly ISearchEngine _searchEngine;

    [ImportingConstructor]
    public SearchFilePathsRequestHandler(ISearchEngine searchEngine, IFileSystemNameFactory fileSystemNameFactory) {
      _searchEngine = searchEngine;
      _fileSystemNameFactory = fileSystemNameFactory;
    }

    public override TypedResponse Process(TypedRequest typedRequest) {
      var request = (SearchFilePathsRequest)typedRequest;
      var result = _searchEngine.SearchFilePaths(request.SearchParams);
      return new SearchFilePathsResponse {
        SearchResult = FileSystemNameFactoryExtensions.ToFlatSearchResult(_fileSystemNameFactory, result.FileNames),
        HitCount = result.FileNames.Count,
        TotalCount = result.TotalCount
      };
    }
  }
}
