﻿// Copyright 2013 The Chromium Authors. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.

using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using VsChromium.Core.Files;

namespace VsChromium.Server.Projects {
  [Export(typeof(IProjectDiscovery))]
  public class ProjectDiscovery : IProjectDiscovery {
    private readonly IProjectDiscoveryProvider[] _providers;

    [ImportingConstructor]
    public ProjectDiscovery([ImportMany] IEnumerable<IProjectDiscoveryProvider> providers) {
      _providers = providers.OrderByDescending(x => x.Priority).ToArray();
    }

    public IProject GetProjectFromAnyPath(FullPath path) {
      return _providers
        .Select(t => t.GetProjectFromAnyPath(path))
        .Where(project => project != null)
        .OrderByDescending(p => p.RootPath)
        .FirstOrDefault();
    }

    public IProject GetProjectFromRootPath(FullPath projectRootPath) {
      return _providers
        .Select(t => t.GetProjectFromRootPath(projectRootPath))
        .FirstOrDefault(project => project != null);
    }

    public void ValidateCache() {
      foreach (var provider in _providers) {
        provider.ValidateCache();
      }
    }
  }
}
