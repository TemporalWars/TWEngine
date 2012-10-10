#region File Description

//-----------------------------------------------------------------------------
// DeflateStreamAsyncResult.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------

#endregion

using System;
using System.Threading;

namespace ImageNexus.BenScharbach.TWEngine.Utilities.Compression
{
    internal class DeflateStreamAsyncResult : IAsyncResult
    {
        // Fields
        private readonly AsyncCallback _mAsyncCallback;
        private readonly object _mAsyncState;
        public byte[] Buffer;
        public int Count;
        public bool IsWrite;
        internal bool MCompletedSynchronously;
        public int Offset;
        private object _mAsyncObject;
        private int _mCompleted;
        private object _mEvent;
        private int _mInvokedCallback;
        private object _mResult;

        // Methods
        public DeflateStreamAsyncResult(object asyncObject, object asyncState, AsyncCallback asyncCallback,
                                        byte[] buffer, int offset, int count)
        {
            Buffer = buffer;
            Offset = offset;
            Count = count;
            MCompletedSynchronously = true;
            _mAsyncObject = asyncObject;
            _mAsyncState = asyncState;
            _mAsyncCallback = asyncCallback;
        }

        internal object Result
        {
            get { return _mResult; }
        }

        #region IAsyncResult Members

        public object AsyncState
        {
            get { return _mAsyncState; }
        }

        public WaitHandle AsyncWaitHandle
        {
            get
            {
                int completed = _mCompleted;
                if (_mEvent == null)
                {
                    Interlocked.CompareExchange(ref _mEvent, new ManualResetEvent(completed != 0), null);
                }
                var event2 = (ManualResetEvent) _mEvent;
                if ((completed == 0) && (_mCompleted != 0))
                {
                    event2.Set();
                }
                return event2;
            }
        }

        public bool CompletedSynchronously
        {
            get { return MCompletedSynchronously; }
        }

        public bool IsCompleted
        {
            get { return (_mCompleted != 0); }
        }

        #endregion

        internal void Close()
        {
            if (_mEvent != null)
            {
                ((ManualResetEvent) _mEvent).Close();
            }
        }

        private void Complete(object result)
        {
            _mResult = result;
            Interlocked.Increment(ref _mCompleted);
            if (_mEvent != null)
            {
                ((ManualResetEvent) _mEvent).Set();
            }
            if ((Interlocked.Increment(ref _mInvokedCallback) == 1) && (_mAsyncCallback != null))
            {
                _mAsyncCallback(this);
            }
        }

        private void Complete(bool completedSynchronously, object result)
        {
            MCompletedSynchronously = completedSynchronously;
            Complete(result);
        }

        internal void InvokeCallback(object result)
        {
            Complete(result);
        }

        internal void InvokeCallback(bool completedSynchronously, object result)
        {
            Complete(completedSynchronously, result);
        }

        // Properties
    }
}