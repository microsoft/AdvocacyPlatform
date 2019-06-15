// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Clients
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.AdvocacyPlatform.Contracts;
    using Microsoft.CognitiveServices.Speech;
    using Microsoft.CognitiveServices.Speech.Audio;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Wrapper for the Azure speech to text service.
    /// See
    /// https://docs.microsoft.com/en-us/azure/cognitive-services/speech-service/quickstart-csharp-dotnetcore-windows
    /// and
    /// https://github.com/Azure-Samples/cognitive-services-speech-sdk/blob/master/samples/csharp/sharedcontent/console/speech_recognition_samples.cs.
    /// </summary>
    public class AzureTranscriber : ITranscriber
    {
        /// <summary>
        /// Speech recognizer.
        /// </summary>
        private SpeechRecognizer _speechRecognizer;

        /// <summary>
        /// StringBuilder for continuously building transcription text.
        /// </summary>
        private StringBuilder _transcriptBuilder;

        /// <summary>
        /// Describes cancellation details if operation was canceled.
        /// </summary>
        private string _cancellationDetails;

        /// <summary>
        /// Trace logging instance.
        /// </summary>
        private ILogger _log;

        /// <summary>
        /// Task to wait on.
        /// </summary>
        private TaskCompletionSource<int> _stopRecognition;

        /// <summary>
        /// Common routine for transcribing an audio file.
        /// </summary>
        /// <param name="apiKey">The subscription key.</param>
        /// <param name="region">The region of the resource.</param>
        /// <param name="audioFileUri">The public URI of the audio file to transcribe.</param>
        /// <param name="storageClient">The implementation of IStorageClient to use when reading the audio file from a source data store.</param>
        /// <param name="storageConnectionString">The connection string to use when connecting the source data store.</param>
        /// <param name="storageContainerName">The container containing the audio file.</param>
        /// <param name="log">Trace logger instance.</param>
        /// <returns>A Task returning the transcribed speech.</returns>
        public async Task<string> TranscribeAudioFileUriAsync(Secret apiKey, string region, string audioFileUri, IStorageClient storageClient, Secret storageConnectionString, string storageContainerName, ILogger log)
        {
            return await TranscribeAudioStorageUriCommonAsync(apiKey, region, audioFileUri, storageClient, storageConnectionString, storageContainerName, log);
        }

        /// <summary>
        /// Common routine for transcribing an audio file.
        /// </summary>
        /// <param name="apiKey">The subscription key.</param>
        /// <param name="region">The region of the resource.</param>
        /// <param name="audioFileUri">The public URI of the audio file to transcribe.</param>
        /// <param name="httpClient">The implementation of IHttpClientWrapper to use when making transcription requests.</param>
        /// <param name="log">Trace logger instance.</param>
        /// <returns>A Task returning the transcribed speech.</returns>
        public async Task<string> TranscribeAudioFileUriAsync(Secret apiKey, string region, string audioFileUri, IHttpClientWrapper httpClient, ILogger log)
        {
            return await TranscribeAudioPublicUriCommonAsync(apiKey, region, audioFileUri, true, httpClient, log);
        }

        /// <summary>
        /// Common routine for transcribing an audio file.
        /// </summary>
        /// <param name="apiKey">The subscription key.</param>
        /// <param name="region">The region of the resource.</param>
        /// <param name="audioFilePath">The local file path of the audio file to transcribe.</param>
        /// <param name="log">Trace logger instance.</param>
        /// <returns>A Task returning the transcribed speech.</returns>
        public async Task<string> TranscribeAudioFilePathAsync(Secret apiKey, string region, string audioFilePath, ILogger log)
        {
            return await TranscribeAudioPublicUriCommonAsync(apiKey, region, audioFilePath, false, null, log);
        }

        /// <summary>
        /// From https://github.com/Azure-Samples/cognitive-services-speech-sdk/blob/master/samples/csharp/sharedcontent/console/helper.cs .
        /// </summary>
        /// <param name="reader">A BinaryReader wrapping the audio stream to read.</param>
        /// <returns>The audio format of the stream.</returns>
        private static AudioStreamFormat ReadWaveHeader(BinaryReader reader)
        {
            // Tag "RIFF"
            char[] data = new char[4];
            reader.Read(data, 0, 4);

            // Trace.Assert((data[0] == 'R') && (data[1] == 'I') && (data[2] == 'F') && (data[3] == 'F'), "Wrong wav header");

            // Chunk size
            long fileSize = reader.ReadInt32();

            // Subchunk, Wave Header
            // Subchunk, Format
            // Tag: "WAVE"
            reader.Read(data, 0, 4);

            // Trace.Assert((data[0] == 'W') && (data[1] == 'A') && (data[2] == 'V') && (data[3] == 'E'), "Wrong wav tag in wav header");

            // Tag: "fmt"
            reader.Read(data, 0, 4);

            // Trace.Assert((data[0] == 'f') && (data[1] == 'm') && (data[2] == 't') && (data[3] == ' '), "Wrong format tag in wav header");

            // chunk format size
            var formatSize = reader.ReadInt32();
            var formatTag = reader.ReadUInt16();
            var channels = reader.ReadUInt16();
            var samplesPerSecond = reader.ReadUInt32();
            var avgBytesPerSec = reader.ReadUInt32();
            var blockAlign = reader.ReadUInt16();
            var bitsPerSample = reader.ReadUInt16();

            // Until now we have read 16 bytes in format, the rest is cbSize and is ignored for now.
            if (formatSize > 16)
            {
                reader.ReadBytes((int)(formatSize - 16));
            }

            // Second Chunk, data
            // tag: data.
            reader.Read(data, 0, 4);

            // Trace.Assert((data[0] == 'd') && (data[1] == 'a') && (data[2] == 't') && (data[3] == 'a'), "Wrong data tag in wav");
            // data chunk size
            int dataSize = reader.ReadInt32();

            // now, we have the format in the format parameter and the
            // reader set to the start of the body, i.e., the raw sample data
            return AudioStreamFormat.GetWaveFormatPCM(samplesPerSecond, (byte)bitsPerSample, (byte)channels);
        }

        /// <summary>
        /// Callback that stops continuous recognition upon receiving an event.
        /// </summary>
        /// <param name="sender">The object sending the event.</param>
        /// <param name="ev">Session event arguments.</param>
        private void SessionStopped(object sender, SessionEventArgs ev)
        {
            _stopRecognition.TrySetResult(0);
        }

        /// <summary>
        /// As recognition is continuous, every sentence gets recognized separately.
        /// Therefore, we need to concatenate all the sentences and return the full transcript.
        /// </summary>
        /// <param name="sender">The object sending the event.</param>
        /// <param name="ev">Speech recognition event arguments.</param>
        private void Recognized(object sender, SpeechRecognitionEventArgs ev)
        {
            _transcriptBuilder.Append($" {ev.Result.Text}");
        }

        /// <summary>
        /// Sets cancellation details after the operation is canceled.
        /// </summary>
        /// <param name="sender">The object sending the event.</param>
        /// <param name="ev">Speech recognition cancellation event arguments.</param>
        private void Canceled(object sender, SpeechRecognitionCanceledEventArgs ev)
        {
            _cancellationDetails = ev.ErrorDetails;
            _stopRecognition.TrySetResult(0);
        }

        /// <summary>
        /// Common routine for transcribing an audio file.
        /// </summary>
        /// <param name="apiKey">The subscription key.</param>
        /// <param name="region">The region of the resource.</param>
        /// <param name="reader">BinaryReader instance for reading the input stream.</param>
        /// <returns>A Task returning the transcribed speech.</returns>
        private async Task<string> TranscribeAudioCommonAsync(Secret apiKey, string region, BinaryReader reader)
        {
            string transcript = null;

            using (BinaryAudioStreamReader streamReader = new BinaryAudioStreamReader(reader))
            {
                AudioStreamFormat audioStreamFormat = ReadWaveHeader(reader);
                AudioConfig audioConfig = AudioConfig.FromStreamInput(streamReader, audioStreamFormat);
                SpeechConfig speechConfig = SpeechConfig.FromSubscription(apiKey.Value, region);

                _speechRecognizer = new SpeechRecognizer(speechConfig, audioConfig);

                _speechRecognizer.Recognized += Recognized;
                _speechRecognizer.Canceled += Canceled;
                _speechRecognizer.SessionStopped += SessionStopped;
                _speechRecognizer.Canceled += SessionStopped;

                await _speechRecognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);

                Task.WaitAny(new[] { _stopRecognition.Task });

                await _speechRecognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);

                if (!string.IsNullOrWhiteSpace(_cancellationDetails))
                {
                    throw new TranscriberCanceledException($"Azure Speech cancellation error: {_cancellationDetails}");
                }

                transcript = _transcriptBuilder.ToString();

                if (string.IsNullOrWhiteSpace(transcript))
                {
                    throw new TranscriberEmptyTranscriptException("Azure Speech returned blank transcript!");
                }
            }

            return transcript;
        }

        /// <summary>
        /// Common routine for transcribing an audio file.
        /// </summary>
        /// <param name="apiKey">The subscription key.</param>
        /// <param name="region">The region of the resource.</param>
        /// <param name="audioFilePath">The public URI of the audio file to transcribe.</param>
        /// <param name="isUri">Specifies if the <paramref name="audioFilePath"/> is a URI (true) or local file path (false).</param>
        /// <param name="httpClient">The implementation of IHttpClientWrapper to use when making transcription requests.</param>
        /// <param name="log">Trace logger instance.</param>
        /// <returns>A Task returning the transcribed speech.</returns>
        private async Task<string> TranscribeAudioPublicUriCommonAsync(Secret apiKey, string region, string audioFilePath, bool isUri, IHttpClientWrapper httpClient, ILogger log)
        {
            _log = log;
            _transcriptBuilder = new StringBuilder();
            _stopRecognition = new TaskCompletionSource<int>();

            using (BinaryReader binaryReader = BinaryReaderFactory.GetBinaryReader(audioFilePath, isUri, httpClient, log))
            {
                return await TranscribeAudioCommonAsync(apiKey, region, binaryReader);
            }
        }

        /// <summary>
        /// Common routine for transcribing an audio file.
        /// </summary>
        /// <param name="apiKey">The subscription key.</param>
        /// <param name="region">The region of the resource.</param>
        /// <param name="audioFileUri">The public URI of the audio file to transcribe.</param>
        /// <param name="storageClient">The implementation of IStorageClient to use when reading the audio file from a source data store.</param>
        /// <param name="storageConnectionString">The connection string to use when connecting the source data store.</param>
        /// <param name="storageContainerName">The container containing the audio file.</param>
        /// <param name="log">Trace logger instance.</param>
        /// <returns>A Task returning the transcribed speech.</returns>
        private async Task<string> TranscribeAudioStorageUriCommonAsync(Secret apiKey, string region, string audioFileUri, IStorageClient storageClient, Secret storageConnectionString, string storageContainerName, ILogger log)
        {
            _log = log;
            _transcriptBuilder = new StringBuilder();
            _stopRecognition = new TaskCompletionSource<int>();

            using (MemoryStream outputStream = new MemoryStream())
            {
                using (BinaryReader binaryReader = BinaryReaderFactory.GetBinaryReader(audioFileUri, outputStream, storageClient, storageConnectionString, storageContainerName, log))
                {
                    return await TranscribeAudioCommonAsync(apiKey, region, binaryReader);
                }
            }
        }
    }
}
