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
        };

        public static IAudioDecoder GetDecoderForFile(string path)
        {
            var decoder = decoders.FirstOrDefault(d => d.CanDecode(path));

            if (decoder == null)
                throw new NotSupportedException("Неизвестный формат: " + path);

            return decoder;
        }
    }
}
