#region File Description

//-----------------------------------------------------------------------------
// DeflateStream.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------

#endregion

using System;
using System.IO;
using System.Threading;
using ImageNexus.BenScharbach.TWEngine.Utilities.Compression.Enums;

namespace ImageNexus.BenScharbach.TWEngine.Utilities.Compression
{
    public class DeflateStream : Stream
    {
        // Fields
        private const int BufferSize = 0x1000;
        internal const int DefaultBufferSize = 0x1000;
        private readonly bool _leaveOpen;
        private readonly CompressionMode _mode;
        private readonly byte[] _buffer;
        private readonly Deflater _deflater;
        private readonly Inflater _inflater;
        private readonly AsyncWriteDelegate _mAsyncWriterDelegate;
        private readonly AsyncCallback _mCallBack;
        private Stream _stream;
        private int _asyncOperations;
        private IFileFormatWriter _formatWriter;
        private bool _wroteHeader;

        // Methods
        //[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public DeflateStream(Stream stream, CompressionMode mode)
            : this(stream, mode, false)
        {
        }

        public DeflateStream(Stream stream, CompressionMode mode, bool leaveOpen)
        {
            _stream = stream;
            _mode = mode;
            _leaveOpen = leaveOpen;
            if (_stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            switch (_mode)
            {
                case CompressionMode.Decompress:
                    if (!_stream.CanRead)
                    {
                        throw new ArgumentException("NotReadableStream", "stream");
                    }
                    _inflater = new Inflater();
                    _mCallBack = ReadCallback;
                    break;

                case CompressionMode.Compress:
                    if (!_stream.CanWrite)
                    {
                        throw new ArgumentException("NotWriteableStream", "stream");
                    }
                    _deflater = new Deflater();
                    _mAsyncWriterDelegate = InternalWrite;
                    _mCallBack = WriteCallback;
                    break;

                default:
                    throw new ArgumentException("ArgumentOutOfRange_Enum", "mode");
            }
            _buffer = new byte[0x1000];
        }

        public Stream BaseStream
        {
            //[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get { return _stream; }
        }

        public override bool CanRead
        {
            get
            {
                if (_stream == null)
                {
                    return false;
                }
                return ((_mode == CompressionMode.Decompress) && _stream.CanRead);
            }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get
            {
                if (_stream == null)
                {
                    return false;
                }
                return ((_mode == CompressionMode.Compress) && _stream.CanWrite);
            }
        }

        public override long Length
        {
            get { throw new NotSupportedException("NotSupported"); }
        }

        public override long Position
        {
            get { throw new NotSupportedException("NotSupported"); }
            set { throw new NotSupportedException("NotSupported"); }
        }

        //[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
        public override IAsyncResult BeginRead(byte[] array, int offset, int count, AsyncCallback asyncCallback,
                                               object asyncState)
        {
            IAsyncResult result2;
            EnsureDecompressionMode();
            if (_asyncOperations != 0)
            {
                throw new InvalidOperationException("InvalidBeginCall");
            }
            Interlocked.Increment(ref _asyncOperations);
            try
            {
                ValidateParameters(array, offset, count);
                var state = new DeflateStreamAsyncResult(this, asyncState, asyncCallback, array, offset, count)
                                {IsWrite = false};
                int result = _inflater.Inflate(array, offset, count);
                if (result != 0)
                {
                    state.InvokeCallback(true, result);
                    return state;
                }
                if (_inflater.Finished())
                {
                    state.InvokeCallback(true, 0);
                    return state;
                }
                _stream.BeginRead(_buffer, 0, _buffer.Length, _mCallBack, state);
                state.MCompletedSynchronously &= state.IsCompleted;
                result2 = state;
            }
            catch
            {
                Interlocked.Decrement(ref _asyncOperations);
                throw;
            }
            return result2;
        }

        //[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
        public override IAsyncResult BeginWrite(byte[] array, int offset, int count, AsyncCallback asyncCallback,
                                                object asyncState)
        {
            IAsyncResult result2;
            EnsureCompressionMode();
            if (_asyncOperations != 0)
            {
                throw new InvalidOperationException("InvalidBeginCall");
            }
            Interlocked.Increment(ref _asyncOperations);
            try
            {
                ValidateParameters(array, offset, count);
                var result = new DeflateStreamAsyncResult(this, asyncState, asyncCallback, array, offset, count)
                                 {IsWrite = true};
                _mAsyncWriterDelegate.BeginInvoke(array, offset, count, true, _mCallBack, result);
                result.MCompletedSynchronously &= result.IsCompleted;
                result2 = result;
            }
            catch
            {
                Interlocked.Decrement(ref _asyncOperations);
                throw;
            }
            return result2;
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing && (_stream != null))
                {
                    Flush();
                    if ((_mode == CompressionMode.Compress) && (_stream != null))
                    {
                        int deflateOutput;
                        while (!_deflater.NeedsInput())
                        {
                            deflateOutput = _deflater.GetDeflateOutput(_buffer);
                            if (deflateOutput != 0)
                            {
                                _stream.Write(_buffer, 0, deflateOutput);
                            }
                        }
                        deflateOutput = _deflater.Finish(_buffer);
                        if (deflateOutput > 0)
                        {
                            DoWrite(_buffer, 0, deflateOutput, false);
                        }
                        if ((_formatWriter != null) && _wroteHeader)
                        {
                            byte[] footer = _formatWriter.GetFooter();
                            _stream.Write(footer, 0, footer.Length);
                        }
                    }
                }
            }
            finally
            {
                try
                {
                    if ((disposing && !_leaveOpen) && (_stream != null))
                    {
                        _stream.Close();
                    }
                }
                finally
                {
                    _stream = null;
                    base.Dispose(disposing);
                }
            }
        }

        private void DoMaintenance(byte[] array, int offset, int count)
        {
            if (_formatWriter != null)
            {
                if (!_wroteHeader && (count > 0))
                {
                    byte[] header = _formatWriter.GetHeader();
                    _stream.Write(header, 0, header.Length);
                    _wroteHeader = true;
                }
                if (count > 0)
                {
                    _formatWriter.UpdateWithBytesRead(array, offset, count);
                }
            }
        }

        private void DoWrite(byte[] array, int offset, int count, bool isAsync)
        {
            if (isAsync)
            {
                IAsyncResult asyncResult = _stream.BeginWrite(array, offset, count, null, null);
                _stream.EndWrite(asyncResult);
            }
            else
            {
                _stream.Write(array, offset, count);
            }
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            EnsureDecompressionMode();
            if (_asyncOperations != 1)
            {
                throw new InvalidOperationException("InvalidEndCall");
            }
            if (asyncResult == null)
            {
                throw new ArgumentNullException("asyncResult");
            }
            if (_stream == null)
            {
                throw new InvalidOperationException("ObjectDisposed_StreamClosed");
            }
            var result = asyncResult as DeflateStreamAsyncResult;
            if (result == null)
            {
                throw new ArgumentNullException("asyncResult");
            }
            try
            {
                if (!result.IsCompleted)
                {
                    result.AsyncWaitHandle.WaitOne();
                }
            }
            finally
            {
                Interlocked.Decrement(ref _asyncOperations);
                result.Close();
            }
            if (result.Result is Exception)
            {
                throw ((Exception) result.Result);
            }
            return (int) result.Result;
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            EnsureCompressionMode();
            if (_asyncOperations != 1)
            {
                throw new InvalidOperationException("InvalidEndCall");
            }
            if (asyncResult == null)
            {
                throw new ArgumentNullException("asyncResult");
            }
            if (_stream == null)
            {
                throw new InvalidOperationException("ObjectDisposed_StreamClosed");
            }
            var result = asyncResult as DeflateStreamAsyncResult;
            if (result == null)
            {
                throw new ArgumentNullException("asyncResult");
            }
            try
            {
                if (!result.IsCompleted)
                {
                    result.AsyncWaitHandle.WaitOne();
                }
            }
            finally
            {
                Interlocked.Decrement(ref _asyncOperations);
                result.Close();
            }
            if (result.Result is Exception)
            {
                throw ((Exception) result.Result);
            }
        }

        private void EnsureCompressionMode()
        {
            if (_mode != CompressionMode.Compress)
            {
                throw new InvalidOperationException("CannotWriteToDeflateStream");
            }
        }

        private void EnsureDecompressionMode()
        {
            if (_mode != CompressionMode.Decompress)
            {
                throw new InvalidOperationException("CannotReadFromDeflateStream");
            }
        }

        public override void Flush()
        {
            if (_stream == null)
            {
                throw new ObjectDisposedException(null, "ObjectDisposed_StreamClosed");
            }
        }

        internal void InternalWrite(byte[] array, int offset, int count, bool isAsync)
        {
            int deflateOutput;
            DoMaintenance(array, offset, count);
            while (!_deflater.NeedsInput())
            {
                deflateOutput = _deflater.GetDeflateOutput(_buffer);
                if (deflateOutput != 0)
                {
                    DoWrite(_buffer, 0, deflateOutput, isAsync);
                }
            }
            _deflater.SetInput(array, offset, count);
            while (!_deflater.NeedsInput())
            {
                deflateOutput = _deflater.GetDeflateOutput(_buffer);
                if (deflateOutput != 0)
                {
                    DoWrite(_buffer, 0, deflateOutput, isAsync);
                }
            }
        }

        public override int Read(byte[] array, int offset, int count)
        {
            EnsureDecompressionMode();
            ValidateParameters(array, offset, count);
            int num2 = offset;
            int length = count;
            while (true)
            {
                int num = _inflater.Inflate(array, num2, length);
                num2 += num;
                length -= num;
                if ((length == 0) || _inflater.Finished())
                {
                    break;
                }
                int num4 = _stream.Read(_buffer, 0, _buffer.Length);
                if (num4 == 0)
                {
                    break;
                }
                _inflater.SetInput(_buffer, 0, num4);
            }
            return (count - length);
        }

        private void ReadCallback(IAsyncResult baseStreamResult)
        {
            var asyncState = (DeflateStreamAsyncResult) baseStreamResult.AsyncState;
            asyncState.MCompletedSynchronously &= baseStreamResult.CompletedSynchronously;
            try
            {
                int length = _stream.EndRead(baseStreamResult);
                if (length <= 0)
                {
                    asyncState.InvokeCallback(0);
                }
                else
                {
                    _inflater.SetInput(_buffer, 0, length);
                    length = _inflater.Inflate(asyncState.Buffer, asyncState.Offset, asyncState.Count);
                    if ((length == 0) && !_inflater.Finished())
                    {
                        _stream.BeginRead(_buffer, 0, _buffer.Length, _mCallBack, asyncState);
                    }
                    else
                    {
                        asyncState.InvokeCallback(length);
                    }
                }
            }
            catch (Exception exception)
            {
                asyncState.InvokeCallback(exception);
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException("NotSupported");
        }

        internal void SetFileFormatReader(IFileFormatReader reader)
        {
            if (reader != null)
            {
                _inflater.SetFileFormatReader(reader);
            }
        }

        internal void SetFileFormatWriter(IFileFormatWriter writer)
        {
            if (writer != null)
            {
                _formatWriter = writer;
            }
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException("NotSupported");
        }

        private void ValidateParameters(byte[] array, int offset, int count)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count");
            }
            if ((array.Length - offset) < count)
            {
                throw new ArgumentException("InvalidArgumentOffsetCount");
            }
            if (_stream == null)
            {
                throw new ObjectDisposedException(null, "ObjectDisposed_StreamClosed");
            }
        }

        public override void Write(byte[] array, int offset, int count)
        {
            EnsureCompressionMode();
            ValidateParameters(array, offset, count);
            InternalWrite(array, offset, count, false);
        }

        private void WriteCallback(IAsyncResult asyncResult)
        {
            var asyncState = (DeflateStreamAsyncResult) asyncResult.AsyncState;
            asyncState.MCompletedSynchronously &= asyncResult.CompletedSynchronously;
            try
            {
                _mAsyncWriterDelegate.EndInvoke(asyncResult);
            }
            catch (Exception exception)
            {
                asyncState.InvokeCallback(exception);
                return;
            }
            asyncState.InvokeCallback(null);
        }

        // Properties

        // Nested Types

        #region Nested type: AsyncWriteDelegate

        internal delegate void AsyncWriteDelegate(byte[] array, int offset, int count, bool isAsync);

        #endregion
    }
}