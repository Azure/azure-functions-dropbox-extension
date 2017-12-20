// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.Azure.WebJobs.Description;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;

namespace WebJobs.DropboxExtension
{
    /// <summary>
    /// Binding attribute for Dropbox.com
    /// </summary>
    /// <remarks>
    /// See Dropbox.NET client. https://github.com/dropbox/dropbox-sdk-dotnet/
    /// Nuget: Dropbox.Api 4.6.0
    /// See https://www.dropbox.com/developers/documentation/dotnet#tutorial 
    /// </remarks>
    [Binding]
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    public class DropboxAttribute : Attribute
    {
        public DropboxAttribute()
        {
        }

        public DropboxAttribute(string path)
        {
            this.Path = path;
        }

        public DropboxAttribute(string path, FileAccess access)
        {
            this.Path = path;
            this.Access = access; // access is nullable, so need a ctor to set it.
        }

        /// <summary>
        /// Dropbox path to the file. 
        /// Should start with '/'. 
        /// </summary>
        /// <remarks>
        /// See validation at: https://github.com/dropbox/dropbox-sdk-dotnet/blob/452ea467b39c7708a30735a5293427ef34cf4aa1/dropbox-sdk-dotnet/Dropbox.Api/Generated/Files/CommitInfo.cs#L61
        /// </remarks>
        [AutoResolve]
        [RegularExpression(@"\A(?:(/(.|[\r\n])*)|(ns:[0-9]+(/.*)?)|(id:.*))\z")]
        public string Path { get; set; }

        /// <summary>
        /// Direction, Required for binding to Stream. Else we infer direction from the parameter type.
        /// Write will default to Overwrite <see cref="Dropbox.Api.Files.WriteMode.Overwrite"/>.
        /// </summary>
        public FileAccess? Access { get; set; }

        // Optional. If missing, use from Extension
        /// <summary>
        /// Gets or sets the appsetting name for the dropbox connection. 
        /// If missing, infer from extension configuration.
        /// </summary>
        [AppSetting]
        public string Connection { get; set; }
    }
}