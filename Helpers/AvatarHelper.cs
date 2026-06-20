namespace ksp.care.Helpers
{
    /// <summary>
    /// Returns gender-specific accent colors, gradient, icon, and label.
    /// Used for profile banners, patient list rows, and dashboard greeting.
    /// </summary>
    public static class AvatarHelper
    {
        public static AvatarInfo GetAvatar(string? gender, int age)
        {
            var g = (gender ?? "").Trim().ToLowerInvariant();

            if (g == "female")
            {
                if (age < 18)
                    return new AvatarInfo {
                        BgColor     = "rgba(232,130,180,0.12)",
                        BorderColor = "rgba(232,130,180,0.25)",
                        GlowColor   = "rgba(232,130,180,0.15)",
                        AccentHex   = "#E882B4",
                        GradientCss = "linear-gradient(135deg, #F472B6, #EC4899, #DB2777)",
                        Icon        = "bi-gender-female",
                        Label       = "Girl"
                    };
                else
                    return new AvatarInfo {
                        BgColor     = "rgba(244,132,95,0.12)",
                        BorderColor = "rgba(244,132,95,0.25)",
                        GlowColor   = "rgba(244,132,95,0.15)",
                        AccentHex   = "#F4845F",
                        GradientCss = "linear-gradient(135deg, #FB923C, #F472B6, #E879A0)",
                        Icon        = "bi-gender-female",
                        Label       = "Woman"
                    };
            }

            if (g == "male")
            {
                if (age < 18)
                    return new AvatarInfo {
                        BgColor     = "rgba(110,181,255,0.12)",
                        BorderColor = "rgba(110,181,255,0.25)",
                        GlowColor   = "rgba(110,181,255,0.15)",
                        AccentHex   = "#6EB5FF",
                        GradientCss = "linear-gradient(135deg, #60A5FA, #3B82F6, #2563EB)",
                        Icon        = "bi-gender-male",
                        Label       = "Boy"
                    };
                else
                    return new AvatarInfo {
                        BgColor     = "rgba(107,191,122,0.12)",
                        BorderColor = "rgba(107,191,122,0.25)",
                        GlowColor   = "rgba(107,191,122,0.15)",
                        AccentHex   = "#6BBF7A",
                        GradientCss = "linear-gradient(135deg, #34D399, #10B981, #059669)",
                        Icon        = "bi-gender-male",
                        Label       = "Man"
                    };
            }

            // Other / Unknown
            return new AvatarInfo {
                BgColor     = "rgba(139,92,246,0.12)",
                BorderColor = "rgba(139,92,246,0.25)",
                GlowColor   = "rgba(139,92,246,0.15)",
                AccentHex   = "#8B5CF6",
                GradientCss = "linear-gradient(135deg, #A78BFA, #8B5CF6, #7C3AED)",
                Icon        = "bi-person",
                Label       = "Patient"
            };
        }
    }

    public class AvatarInfo
    {
        public string BgColor     { get; set; } = "";
        public string BorderColor { get; set; } = "";
        public string GlowColor   { get; set; } = "";
        public string AccentHex   { get; set; } = "";
        public string GradientCss { get; set; } = "";
        public string Icon        { get; set; } = "";
        public string Label       { get; set; } = "";
    }
}
