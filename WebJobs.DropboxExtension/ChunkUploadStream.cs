// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Dropbox.Api;
using Dropbox.Api.Files;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace WebJobs.DropboxExtension
{
    // Provide a Stream abstraction over Dropbox's chunking API. 
    // See https://github.com/dropbox/dropbox-sdk-dotnet/blob/9803a40b3169acf66a2d14e29af8750ebe6e3e17/dropbox-sdk-dotnet/Examples/SimpleTest/Program.cs#L340
    // This doesn't buffer, but a caller can wrap this in a BufferedStream. 
    // This should really live in the Dropbox.NET SDK. 
    public class ChunkUploadStream : Stream
    {
        private DropboxClient _client;
        private CommitInfo _commitInfo; // Specifies final destination file to write to
        private bool _open = true;

        private int _idx = 0; // Chunk number we're on. 
        private string _sessionId;
        private ulong _uploadedByteCount = 0; 

        public ChunkUploadStream(DropboxClient client, string path)
            : this(client, new CommitInfo(path))
        {
        }

        public ChunkUploadStream(DropboxClient client, CommitInfo commitInfo)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _commitInfo = commitInfo;
        }

        public override void Close()
        {
            if (!_open)
            {
                return; // already done
            }
  
            Task.Run(() => this.HandleFinishAsync()).GetAwaiter().GetResult();

            _open = false;
        }

        private Exception WriteOnlyStream()
        {
            return new InvalidOperationException("Write-only stream");
        }

        #region Abstract Methods

        public override bool CanRead => false;

        public override bool CanSeek => false;

        public override bool CanWrite => _open;

        public override long Length => throw new InvalidOperationException();

        public override long Position
        {
            get => (long) this._uploadedByteCount;
            set { throw WriteOnlyStream(); }
        }

        public override void Flush()
        {
            // Nop. 
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw WriteOnlyStream();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw WriteOnlyStream();
        }

        public override void SetLength(long value)
        {
            throw WriteOnlyStream();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            Task.Run(() => this.HandleWriteAsync(buffer, offset, count)).GetAwaiter().GetResult();
        }
        #endregion

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return this.HandleWriteAsync(buffer, offset, count);
        }

        private async Task HandleWriteAsync(byte[] buffer, int offset, int count)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
            if (count == 0)
            {
                return;
            }
            using (MemoryStream memStream = new MemoryStream(buffer, offset, count))
            {
                if (_idx == 0)
                {
                    var result = await _client.Files.UploadSessionStartAsync(body: memStream);
                    _sessionId = result.SessionId;
                }
                else
                {
                    UploadSessionCursor cursor = this.GetCursor();
                    await _client.Files.UploadSessionAppendV2Async(cursor, body: memStream);
                }
                _uploadedByteCount += (ulong)count;
                _idx++;
            }
        }
        private async Task HandleFinishAsync()
        {
            UploadSessionCursor cursor = this.GetCursor();
            using (MemoryStream memStream = new MemoryStream(new byte[0], 0, 0))
            {
                await _client.Files.UploadSessionFinishAsync(cursor, _commitInfo, memStream);
            }
        }

        private UploadSessionCursor GetCursor()
        {
            UploadSessionCursor cursor = new UploadSessionCursor(_sessionId, _uploadedByteCount);
            return cursor;
        }
    }
}