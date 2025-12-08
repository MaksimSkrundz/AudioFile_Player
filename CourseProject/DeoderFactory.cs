using System;
using System.Collections.Generic;
using System.Linq;

namespace CourseProject
{
    public static class DecoderFactory
    {
        private static List<IAudioDecoder> decoders = new List<IAudioDecoder>()
        {
            new Mp3Decoder()
        };

        public static IAudioDecoder GetDecoderForFile(string path)
        {
            var d = decoders.FirstOrDefault(x => x.CanOpen(path));
            if (d == null) throw new NotSupportedException("No decoder for this file type: " + path);
            return d;
        }
    }
}
