
namespace IsoParser.Lib.Concretes
{
    public enum ChannelLabel {
        Unknown = 0x7FFFFFFF,  // unknown role or unspecified other use for channel
        Unused = 0,            // channel is present, but has no intended role or destination
        UseCoordinates = 100,  // channel is described solely by the mCoordinates fields

        Left = 1,
        Right = 2,
        Center = 3,
        LFEScreen = 4,
        LeftSurround = 5,         // WAVE (.wav files): “Back Left”
        RightSurround = 6,         // WAVE: "Back Right"
        LeftCenter = 7,
        RightCenter = 8,
        CenterSurround = 9,        // WAVE: "Back Center or plain "Rear Surround"
        LeftSurroundDirect = 10,   // WAVE: "Side Left"
        RightSurroundDirect = 11,  // WAVE: "Side Right"
        TopCenterSurround = 12,
        VerticalHeightLeft = 13,   // WAVE: "Top Front Left”
        VerticalHeightCenter = 14, // WAVE: "Top Front Center”
        VerticalHeightRight = 15,  // WAVE: "Top Front Right”
        TopBackLeft = 16,
        TopBackCenter = 17,
        TopBackRight = 18,
        RearSurroundLeft = 33,
        RearSurroundRight = 34,
        LeftWide = 35,
        RightWide = 36,
        LFE2 = 37,
        LeftTotal = 38,   // matrix encoded 4 channels
        RightTotal = 39,   // matrix encoded 4 channels
        HearingImpaired = 40,
        Narration = 41,
        Mono = 42,
        DialogCentricMix = 43,
        CenterSurroundDirect = 44,   // back center, non diffuse

        // first order ambisonic channels
        Ambisonic_W = 200,
        Ambisonic_X = 201,
        Ambisonic_Y = 202,
        Ambisonic_Z = 203,

        // Mid/Side Recording
        MS_Mid = 204,
        MS_Side = 205,

        // X-Y Recording
        XY_X = 206,
        XY_Y = 207,

        // other
        HeadphonesLeft = 301,
        HeadphonesRight = 302,
        ClickTrack = 304,
        ForeignLanguage = 305
    };
}