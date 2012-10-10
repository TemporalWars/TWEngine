#region File Description

//-----------------------------------------------------------------------------
// Deflater.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------

#endregion

namespace ImageNexus.BenScharbach.TWEngine.Utilities.Compression
{
    internal class Deflater
    {
        // Fields
        private const double BadCompressionThreshold = 1.0;
        private const int CleanCopySize = 0xf88;
        private const int MaxHeaderFooterGoo = 120;
        private const int MinBlockSize = 0x100;
        private readonly CopyEncoder _copyEncoder = new CopyEncoder();
        private readonly FastEncoder _deflateEncoder = new FastEncoder();
        private readonly DeflateInput _input = new DeflateInput();
        private readonly OutputBuffer _output = new OutputBuffer();
        private DeflateInput _inputFromHistory;
        private DeflaterState _processingState = DeflaterState.NotStarted;

        // Methods
        public int Finish(byte[] outputBuffer)
        {
            if (_processingState == DeflaterState.NotStarted)
            {
                return 0;
            }
            _output.UpdateBuffer(outputBuffer);
            if (((_processingState == DeflaterState.CompressThenCheck) ||
                 (_processingState == DeflaterState.HandlingSmallData)) ||
                (_processingState == DeflaterState.SlowDownForIncompressible1))
            {
                _deflateEncoder.GetBlockFooter(_output);
            }
            WriteFinal();
            return _output.BytesWritten;
        }

        private void FlushInputWindows()
        {
            _deflateEncoder.FlushInput();
        }

        public int GetDeflateOutput(byte[] outputBuffer)
        {
            _output.UpdateBuffer(outputBuffer);
            switch (_processingState)
            {
                case DeflaterState.NotStarted:
                    {
                        var state = _input.DumpState();
                        var state2 = _output.DumpState();
                        _deflateEncoder.GetBlockHeader(_output);
                        _deflateEncoder.GetCompressedData(_input, _output);
                        if (UseCompressed(_deflateEncoder.LastCompressionRatio))
                        {
                            _processingState = DeflaterState.CompressThenCheck;
                        }
                        else
                        {
                            _input.RestoreState(state);
                            _output.RestoreState(state2);
                            _copyEncoder.GetBlock(_input, _output, false);
                            FlushInputWindows();
                            _processingState = DeflaterState.CheckingForIncompressible;
                        }
                        goto Label_023A;
                    }
                case DeflaterState.SlowDownForIncompressible1:
                    _deflateEncoder.GetBlockFooter(_output);
                    _processingState = DeflaterState.SlowDownForIncompressible2;
                    break;

                case DeflaterState.SlowDownForIncompressible2:
                    break;

                case DeflaterState.StartingSmallData:
                    _deflateEncoder.GetBlockHeader(_output);
                    _processingState = DeflaterState.HandlingSmallData;
                    goto Label_0223;

                case DeflaterState.CompressThenCheck:
                    _deflateEncoder.GetCompressedData(_input, _output);
                    if (!UseCompressed(_deflateEncoder.LastCompressionRatio))
                    {
                        _processingState = DeflaterState.SlowDownForIncompressible1;
                        _inputFromHistory = _deflateEncoder.UnprocessedInput;
                    }
                    goto Label_023A;

                case DeflaterState.CheckingForIncompressible:
                    {
                        DeflateInput.InputState state3 = _input.DumpState();
                        OutputBuffer.BufferState state4 = _output.DumpState();
                        _deflateEncoder.GetBlock(_input, _output, 0xf88);
                        if (!UseCompressed(_deflateEncoder.LastCompressionRatio))
                        {
                            _input.RestoreState(state3);
                            _output.RestoreState(state4);
                            _copyEncoder.GetBlock(_input, _output, false);
                            FlushInputWindows();
                        }
                        goto Label_023A;
                    }
                case DeflaterState.HandlingSmallData:
                    goto Label_0223;

                default:
                    goto Label_023A;
            }
            if (_inputFromHistory.Count > 0)
            {
                _copyEncoder.GetBlock(_inputFromHistory, _output, false);
            }
            if (_inputFromHistory.Count == 0)
            {
                _deflateEncoder.FlushInput();
                _processingState = DeflaterState.CheckingForIncompressible;
            }
            goto Label_023A;
            Label_0223:
            _deflateEncoder.GetCompressedData(_input, _output);
            Label_023A:
            return _output.BytesWritten;
        }

        public bool NeedsInput()
        {
            return ((_input.Count == 0) && (_deflateEncoder.BytesInHistory == 0));
        }

        public void SetInput(byte[] inputBuffer, int startIndex, int count)
        {
            _input.Buffer = inputBuffer;
            _input.Count = count;
            _input.StartIndex = startIndex;
            if ((count > 0) && (count < 0x100))
            {
                switch (_processingState)
                {
                    case DeflaterState.CompressThenCheck:
                        _processingState = DeflaterState.HandlingSmallData;
                        return;

                    case DeflaterState.CheckingForIncompressible:
                    case DeflaterState.NotStarted:
                        _processingState = DeflaterState.StartingSmallData;
                        return;

                    default:
                        return;
                }
            }
        }

        private bool UseCompressed(double ratio)
        {
            return (ratio <= 1.0);
        }

        private void WriteFinal()
        {
            _copyEncoder.GetBlock(null, _output, true);
        }

        // Nested Types

        #region Nested type: DeflaterState

        internal enum DeflaterState
        {
            NotStarted,
            SlowDownForIncompressible1,
            SlowDownForIncompressible2,
            StartingSmallData,
            CompressThenCheck,
            CheckingForIncompressible,
            HandlingSmallData
        }

        #endregion
    }
}