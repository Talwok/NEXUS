using NEXUS.Parsers.Mdt.Models.Frames.MDA;
using NEXUS.Parsers.Mdt.Models.Frames.Scanned;

namespace NEXUS.Parsers.Mdt.Helpers;

public static class FramesPipeline
{
    public static MdaFrameImageProcessor? CreateFromMdaFrame(this MdaFrame frame)
    {
        if (frame.Dimensions.Count() < 2 || frame.ImageBuffer.Length == 0)
            return null;

        return new MdaFrameImageProcessor(frame);
    }

    public static ScannedFrameImageProcessor? CreateFromScannedFrame(this ScannedFrame frame)
    {
        if (frame.FrameXRes == 0 || frame.FrameYRes == 0 || frame.ImageBuffer.Length == 0)
            return null;

        return new ScannedFrameImageProcessor(frame);
    }
}
