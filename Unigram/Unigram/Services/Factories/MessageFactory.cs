﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Td.Api;
using Unigram.Common;
using Unigram.Entities;
using Unigram.Native;
using Unigram.ViewModels;
using Unigram.ViewModels.Delegates;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Media.Effects;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.Storage.FileProperties;
using static Unigram.Services.GenerationService;

namespace Unigram.Services.Factories
{
    public interface IMessageFactory
    {
        MessageViewModel Create(IMessageDelegate delegato, Message message);

        Task<InputMessageFactory> CreatePhotoAsync(StorageFile file, bool asFile, int ttl = 0, BitmapEditState editState = null);
        Task<InputMessageFactory> CreateVideoAsync(StorageFile file, bool animated, bool asFile, int ttl = 0, MediaEncodingProfile profile = null, VideoTransformEffectDefinition transform = null);
        Task<InputMessageFactory> CreateVideoNoteAsync(StorageFile file, MediaEncodingProfile profile = null, VideoTransformEffectDefinition transform = null);
        Task<InputMessageFactory> CreateDocumentAsync(StorageFile file);

        InputMessageContent ToInput(Message message, bool caption);
    }

    public class MessageFactory : IMessageFactory
    {
        private readonly IProtoService _protoService;
        private readonly IPlaybackService _playbackService;
        private readonly IEventAggregator _aggregator;

        public MessageFactory(IProtoService protoService, IPlaybackService playbackService, IEventAggregator aggregator)
        {
            _protoService = protoService;
            _playbackService = playbackService;
            _aggregator = aggregator;
        }

        public MessageViewModel Create(IMessageDelegate delegato, Message message)
        {
            return new MessageViewModel(_protoService, _playbackService, delegato, message);
        }



        public async Task<InputMessageFactory> CreatePhotoAsync(StorageFile file, bool asFile, int ttl = 0, BitmapEditState editState = null)
        {
            using (var stream = await file.OpenReadAsync())
            {
                var decoder = await BitmapDecoder.CreateAsync(stream);
                if (decoder.FrameCount > 1)
                {
                    asFile = true;
                }
            }

            var size = await ImageHelper.GetScaleAsync(file, editState: editState);

            var generated = await file.ToGeneratedAsync(asFile ? ConversionType.Copy : ConversionType.Compress, editState != null ? JsonConvert.SerializeObject(editState) : null);
            var thumbnail = default(InputThumbnail);

            if (asFile)
            {
                return new InputMessageFactory
                {
                    InputFile = generated,
                    Type = new FileTypeDocument(),
                    Delegate = (inputFile, caption) => new InputMessageDocument(inputFile, thumbnail, caption)
                };
            }

            return new InputMessageFactory
            {
                InputFile = generated,
                Type = new FileTypePhoto(),
                Delegate = (inputFile, caption) => new InputMessagePhoto(generated, thumbnail, new int[0], size.Width, size.Height, caption, ttl)
            };
        }

        public async Task<InputMessageFactory> CreateVideoAsync(StorageFile file, bool animated, bool asFile, int ttl = 0, MediaEncodingProfile profile = null, VideoTransformEffectDefinition transform = null)
        {
            var basicProps = await file.GetBasicPropertiesAsync();
            var videoProps = await file.Properties.GetVideoPropertiesAsync();

            //var thumbnail = await ImageHelper.GetVideoThumbnailAsync(file, videoProps, transform);

            var duration = (int)videoProps.Duration.TotalSeconds;
            var videoWidth = (int)videoProps.GetWidth();
            var videoHeight = (int)videoProps.GetHeight();

            if (profile != null)
            {
                videoWidth = videoProps.Orientation == VideoOrientation.Rotate180 || videoProps.Orientation == VideoOrientation.Normal ? (int)profile.Video.Width : (int)profile.Video.Height;
                videoHeight = videoProps.Orientation == VideoOrientation.Rotate180 || videoProps.Orientation == VideoOrientation.Normal ? (int)profile.Video.Height : (int)profile.Video.Width;
            }

            var conversion = new VideoConversion();
            if (profile != null)
            {
                //conversion.Transcode = true;
                conversion.Mute = animated;
                //conversion.Width = profile.Video.Width;
                //conversion.Height = profile.Video.Height;
                //conversion.Bitrate = profile.Video.Bitrate;

                if (transform != null)
                {
                    conversion.Transcode = true;
                    conversion.Transform = true;
                    conversion.Rotation = transform.Rotation;
                    conversion.OutputSize = transform.OutputSize;
                    conversion.Mirror = transform.Mirror;
                    conversion.CropRectangle = transform.CropRectangle;
                }
            }

            var generated = await file.ToGeneratedAsync(ConversionType.Transcode, JsonConvert.SerializeObject(conversion));
            var thumbnail = await file.ToThumbnailAsync(conversion, ConversionType.TranscodeThumbnail, JsonConvert.SerializeObject(conversion));

            if (asFile && ttl == 0)
            {
                return new InputMessageFactory
                {
                    InputFile = generated,
                    Type = new FileTypeDocument(),
                    Delegate = (inputFile, caption) => new InputMessageDocument(inputFile, thumbnail, caption)
                };
            }
            else if (animated && ttl == 0)
            {
                return new InputMessageFactory
                {
                    InputFile = generated,
                    Type = new FileTypeAnimation(),
                    Delegate = (inputFile, caption) => new InputMessageAnimation(inputFile, thumbnail, duration, videoWidth, videoHeight, caption)
                };
            }

            return new InputMessageFactory
            {
                InputFile = generated,
                Type = new FileTypeVideo(),
                Delegate = (inputFile, caption) => new InputMessageVideo(inputFile, thumbnail, new int[0], duration, videoWidth, videoHeight, true, caption, ttl)
            };
        }

        public async Task<InputMessageFactory> CreateVideoNoteAsync(StorageFile file, MediaEncodingProfile profile = null, VideoTransformEffectDefinition transform = null)
        {
            var basicProps = await file.GetBasicPropertiesAsync();
            var videoProps = await file.Properties.GetVideoPropertiesAsync();

            //var thumbnail = await ImageHelper.GetVideoThumbnailAsync(file, videoProps, transform);

            var duration = (int)videoProps.Duration.TotalSeconds;
            var videoWidth = (int)videoProps.GetWidth();
            var videoHeight = (int)videoProps.GetHeight();

            if (profile != null)
            {
                videoWidth = videoProps.Orientation == VideoOrientation.Rotate180 || videoProps.Orientation == VideoOrientation.Normal ? (int)profile.Video.Width : (int)profile.Video.Height;
                videoHeight = videoProps.Orientation == VideoOrientation.Rotate180 || videoProps.Orientation == VideoOrientation.Normal ? (int)profile.Video.Height : (int)profile.Video.Width;
            }

            var conversion = new VideoConversion();
            if (profile != null)
            {
                conversion.Transcode = true;
                conversion.Width = profile.Video.Width;
                conversion.Height = profile.Video.Height;
                conversion.Bitrate = profile.Video.Bitrate;

                if (transform != null)
                {
                    conversion.Transform = true;
                    conversion.Rotation = transform.Rotation;
                    conversion.OutputSize = transform.OutputSize;
                    conversion.Mirror = transform.Mirror;
                    conversion.CropRectangle = transform.CropRectangle;
                }
            }

            var generated = await file.ToGeneratedAsync(ConversionType.Transcode, JsonConvert.SerializeObject(conversion));
            var thumbnail = await file.ToThumbnailAsync(conversion, ConversionType.TranscodeThumbnail, JsonConvert.SerializeObject(conversion));

            return new InputMessageFactory
            {
                InputFile = generated,
                Type = new FileTypeVideoNote(),
                Delegate = (inputFile, caption) => new InputMessageVideoNote(inputFile, thumbnail, duration, Math.Min(videoWidth, videoHeight))
            };
        }

        public async Task<InputMessageFactory> CreateDocumentAsync(StorageFile file)
        {
            var generated = await file.ToGeneratedAsync();
            var thumbnail = new InputThumbnail(await file.ToGeneratedAsync(ConversionType.DocumentThumbnail), 0, 0);

            if (file.FileType.Equals(".webp", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var buffer = await FileIO.ReadBufferAsync(file);
                    var webp = WebPImage.DecodeFromBuffer(buffer);

                    var width = webp.PixelWidth;
                    var height = webp.PixelHeight;

                    return new InputMessageFactory
                    {
                        InputFile = generated,
                        Type = new FileTypeSticker(),
                        Delegate = (inputFile, caption) => new InputMessageSticker(inputFile, null, width, height)
                    };
                }
                catch
                {
                    // Not really a sticker, go on sending as a file
                }
            }
            else if (file.ContentType.StartsWith("audio/", StringComparison.OrdinalIgnoreCase))
            {
                var props = await file.Properties.GetMusicPropertiesAsync();
                var duration = (int)props.Duration.TotalSeconds;

                return new InputMessageFactory
                {
                    InputFile = generated,
                    Type = new FileTypeAudio(),
                    Delegate = (inputFile, caption) => new InputMessageAudio(inputFile, thumbnail, duration, props.Title, props.AlbumArtist, caption)
                };
            }

            return new InputMessageFactory
            {
                InputFile = generated,
                Type = new FileTypeDocument(),
                Delegate = (inputFile, caption) => new InputMessageDocument(inputFile, thumbnail, caption)
            };
        }



        public InputMessageContent ToInput(Message message, bool caption)
        {
            var content = message.Content;
            switch (content)
            {
                case MessageAnimation animation:
                    return new InputMessageAnimation(new InputFileId(animation.Animation.AnimationValue.Id), animation.Animation.Thumbnail.ToInputThumbnail(), animation.Animation.Duration, animation.Animation.Width, animation.Animation.Height, caption ? animation.Caption : null);
                case MessageAudio audio:
                    return new InputMessageAudio(new InputFileId(audio.Audio.AudioValue.Id), audio.Audio.AlbumCoverThumbnail.ToInputThumbnail(), audio.Audio.Duration, audio.Audio.Title, audio.Audio.Performer, caption ? audio.Caption : null);
                case MessageContact contact:
                    return new InputMessageContact(contact.Contact);
                case MessageDocument document:
                    return new InputMessageDocument(new InputFileId(document.Document.DocumentValue.Id), document.Document.Thumbnail.ToInputThumbnail(), caption ? document.Caption : null);
                case MessageGame game:
                    return new InputMessageGame(message.ViaBotUserId != 0 ? message.ViaBotUserId : message.SenderUserId, game.Game.ShortName);
                case MessageLocation location:
                    return new InputMessageLocation(location.Location, 0);
                case MessagePhoto photo:
                    var big = photo.Photo.GetBig();
                    var small = photo.Photo.GetSmall();
                    return new InputMessagePhoto(new InputFileId(big.Photo.Id), small.ToInputThumbnail(), new int[0], big.Width, big.Height, caption ? photo.Caption : null, 0);
                //case MessagePoll poll:
                //    return new InputMessagePoll()
                case MessageSticker sticker:
                    return new InputMessageSticker(new InputFileId(sticker.Sticker.StickerValue.Id), sticker.Sticker.Thumbnail.ToInputThumbnail(), sticker.Sticker.Width, sticker.Sticker.Height);
                case MessageText text:
                    return new InputMessageText(text.Text, false, false);
                case MessageVenue venue:
                    return new InputMessageVenue(venue.Venue);
                case MessageVideo video:
                    return new InputMessageVideo(new InputFileId(video.Video.VideoValue.Id), video.Video.Thumbnail.ToInputThumbnail(), new int[0], video.Video.Duration, video.Video.Width, video.Video.Height, video.Video.SupportsStreaming, caption ? video.Caption : null, 0);
                case MessageVideoNote videoNote:
                    return new InputMessageVideoNote(new InputFileId(videoNote.VideoNote.Video.Id), videoNote.VideoNote.Thumbnail.ToInputThumbnail(), videoNote.VideoNote.Duration, videoNote.VideoNote.Length);
                case MessageVoiceNote voiceNote:
                    return new InputMessageVoiceNote(new InputFileId(voiceNote.VoiceNote.Voice.Id), voiceNote.VoiceNote.Duration, voiceNote.VoiceNote.Waveform, caption ? voiceNote.Caption : null);

                default:
                    return null;
            }
        }
    }

    public class InputMessageFactory
    {
        public InputFile InputFile { get; set; }
        public FileType Type { get; set; }
        public Func<InputFile, FormattedText, InputMessageContent> Delegate { get; set; }
    }
}
