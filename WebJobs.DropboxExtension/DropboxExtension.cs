// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;
using Dropbox.Api;
using Microsoft.Azure.WebJobs.Description;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Config;

namespace WebJobs.DropboxExtension
{
    // References nuget: Dropbox.API 4.6.0 
    public class DropboxExtension : IExtensionConfigProvider
    {
        public string Connection { get; set; }

        public void Initialize(ExtensionConfigContext context)
        {
            var rule = context.AddBindingRule<DropboxAttribute>();

            rule.WhenIsNull(nameof(DropboxAttribute.Path)).
                WhenIsNull(nameof(DropboxAttribute.Access)).
                BindToInput<DropboxClient>(attr => GetClient(attr));

            rule.WhenIsNotNull(nameof(DropboxAttribute.Path)).
                BindToStream(ToStream, FileAccess.ReadWrite);
        }

        private async Task<Stream> ToStream(DropboxAttribute attribute, ValueBindingContext arg2)
        {
            var client = this.GetClient(attribute);
            if (attribute.Access == FileAccess.Read)
            {
                try
                {
                    var response = await client.Files.DownloadAsync(attribute.Path);
                    var res = await response.GetContentAsStreamAsync();
                    return res;
                }
                catch (DropboxException e)
                {
                    if (IsFileNotFoundException(e))
                    {
                        return null;
                    }
                    throw e;
                }
            }
            else if (attribute.Access == FileAccess.Write)
            {
                Stream stream = new ChunkUploadStream(client, attribute.Path);
                stream = new BufferedStream(stream);
                return stream;
            }

            throw new InvalidOperationException("Cannot bind using FileAccess.ReadWrite: must be either read or write.");
        }

        public DropboxClient GetClient(DropboxAttribute attribute)
        {
            var cx = attribute.Connection ?? this.Connection;
            return new DropboxClient(cx);
        }

        // We could call GetMetadataASync() to check for file-not-found, but that'd be an extra network call. 
        private static bool IsFileNotFoundException(DropboxException e)
        {
            return e.Message == "path/not_found/";
        }
    }
}
