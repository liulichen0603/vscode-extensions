﻿// Copyright 2013 The Chromium Authors. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.

using System.Reflection;
using System.Runtime.InteropServices;
using VsChromium.Core;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.

[assembly: AssemblyTitle("VsChromium.Server")]
[assembly: AssemblyDescription("Server process for VsChromium.")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("The Chromium Authors")]
[assembly: AssemblyProduct("VsChromium.Server")]
[assembly: AssemblyCopyright(VsChromiumVersion.Copyright)]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.

[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM

[assembly: Guid("7d45902d-181c-4849-bfdc-191da12512b8")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]

[assembly: AssemblyVersion(VsChromiumVersion.File)]
[assembly: AssemblyFileVersion(VsChromiumVersion.File)]
