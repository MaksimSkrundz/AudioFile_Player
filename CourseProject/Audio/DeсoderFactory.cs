using System;
using System.Collections.Generic;
using System.Linq;

namespace CourseProject
{
    public static class DecoderFactory
    {
        private static readonly List<IAudioDecoder> decoders = new List<IAudioDecoder>()
        {
            new Mp3Decoder(),
            new WavDecoder()
            // можно добавить FLAC/other декодеры
        };

        public static IAudioDecoder GetDecoderForFile(string path)
        {
            var d = decoders.FirstOrDefault(x => x.CanDecode(path));
            if (d == null) throw new NotSupportedException("Неизвестный формат: " + path);
            return d;
        }
    }
}