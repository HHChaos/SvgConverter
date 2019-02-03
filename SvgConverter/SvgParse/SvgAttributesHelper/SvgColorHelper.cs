using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Windows.UI;

namespace SvgConverter.SvgParse.SvgAttributesHelper
{
    public static class SvgColorHelper
    {
        private static readonly Dictionary<string, Color> Colors = new Dictionary<string, Color>
        {
            ["aliceblue"] = Color.FromArgb(255, 240, 248, 255),
            ["antiquewhite"] = Color.FromArgb(255, 250, 235, 215),
            ["aqua"] = Color.FromArgb(255, 0, 255, 255),
            ["aquamarine"] = Color.FromArgb(255, 127, 255, 212),
            ["azure"] = Color.FromArgb(255, 240, 255, 255),
            ["beige"] = Color.FromArgb(255, 245, 245, 220),
            ["bisque"] = Color.FromArgb(255, 255, 228, 196),
            ["black"] = Color.FromArgb(255, 0, 0, 0),
            ["blanchedalmond"] = Color.FromArgb(255, 255, 235, 205),
            ["blue"] = Color.FromArgb(255, 0, 0, 255),
            ["blueviolet"] = Color.FromArgb(255, 138, 43, 226),
            ["brown"] = Color.FromArgb(255, 165, 42, 42),
            ["burlywood"] = Color.FromArgb(255, 222, 184, 135),
            ["cadetblue"] = Color.FromArgb(255, 95, 158, 160),
            ["chartreuse"] = Color.FromArgb(255, 127, 255, 0),
            ["chocolate"] = Color.FromArgb(255, 210, 105, 30),
            ["coral"] = Color.FromArgb(255, 255, 127, 80),
            ["cornflowerblue"] = Color.FromArgb(255, 100, 149, 237),
            ["cornsilk"] = Color.FromArgb(255, 255, 248, 220),
            ["crimson"] = Color.FromArgb(255, 220, 20, 60),
            ["cyan"] = Color.FromArgb(255, 0, 255, 255),
            ["darkblue"] = Color.FromArgb(255, 0, 0, 139),
            ["darkcyan"] = Color.FromArgb(255, 0, 139, 139),
            ["darkgoldenrod"] = Color.FromArgb(255, 184, 134, 11),
            ["darkgray"] = Color.FromArgb(255, 169, 169, 169),
            ["darkgreen"] = Color.FromArgb(255, 0, 100, 0),
            ["darkgrey"] = Color.FromArgb(255, 169, 169, 169),
            ["darkkhaki"] = Color.FromArgb(255, 189, 183, 107),
            ["darkmagenta"] = Color.FromArgb(255, 139, 0, 139),
            ["darkolivegreen"] = Color.FromArgb(255, 85, 107, 47),
            ["darkorange"] = Color.FromArgb(255, 255, 140, 0),
            ["darkorchild"] = Color.FromArgb(255, 255, 140, 0),
            ["darkorchid"] = Color.FromArgb(255, 153, 50, 204),
            ["darkred"] = Color.FromArgb(255, 139, 0, 0),
            ["darksalmon"] = Color.FromArgb(255, 233, 150, 122),
            ["darkseagreen"] = Color.FromArgb(255, 143, 188, 143),
            ["darkslateblue"] = Color.FromArgb(255, 72, 61, 139),
            ["darkslategray"] = Color.FromArgb(255, 47, 79, 79),
            ["darkslategrey"] = Color.FromArgb(255, 47, 79, 79),
            ["darkturquoise"] = Color.FromArgb(255, 0, 206, 209),
            ["darkviolet"] = Color.FromArgb(255, 148, 0, 211),
            ["deeppink"] = Color.FromArgb(255, 255, 20, 147),
            ["deepskyblue"] = Color.FromArgb(255, 0, 191, 255),
            ["dimgray"] = Color.FromArgb(255, 105, 105, 105),
            ["dimgrey"] = Color.FromArgb(255, 105, 105, 105),
            ["dodgerblue"] = Color.FromArgb(255, 30, 144, 255),
            ["firebrick"] = Color.FromArgb(255, 178, 34, 34),
            ["floralwhite"] = Color.FromArgb(255, 255, 250, 240),
            ["forestgreen"] = Color.FromArgb(255, 34, 139, 34),
            ["fuchsia"] = Color.FromArgb(255, 255, 0, 255),
            ["gainsboro"] = Color.FromArgb(255, 220, 220, 220),
            ["ghostwhite"] = Color.FromArgb(255, 248, 248, 255),
            ["gold"] = Color.FromArgb(255, 255, 215, 0),
            ["goldenrod"] = Color.FromArgb(255, 218, 165, 32),
            ["gray"] = Color.FromArgb(255, 128, 128, 128),
            ["grey"] = Color.FromArgb(255, 128, 128, 128),
            ["green"] = Color.FromArgb(255, 0, 128, 0),
            ["greenyellow"] = Color.FromArgb(255, 173, 255, 47),
            ["honeydew"] = Color.FromArgb(255, 240, 255, 240),
            ["hotpink"] = Color.FromArgb(255, 255, 105, 180),
            ["indianred"] = Color.FromArgb(255, 205, 92, 92),
            ["indigo"] = Color.FromArgb(255, 75, 0, 130),
            ["ivory"] = Color.FromArgb(255, 255, 255, 240),
            ["khaki"] = Color.FromArgb(255, 240, 230, 140),
            ["lavender"] = Color.FromArgb(255, 230, 230, 250),
            ["lavenderblush"] = Color.FromArgb(255, 255, 240, 245),
            ["lawngreen"] = Color.FromArgb(255, 124, 252, 0),
            ["lemonchiffon"] = Color.FromArgb(255, 255, 250, 205),
            ["lightblue"] = Color.FromArgb(255, 173, 216, 230),
            ["lightcoral"] = Color.FromArgb(255, 240, 128, 128),
            ["lightcyan"] = Color.FromArgb(255, 224, 255, 255),
            ["lightgoldenrodyellow"] = Color.FromArgb(255, 250, 250, 210),
            ["lightgray"] = Color.FromArgb(255, 211, 211, 211),
            ["lightgreen"] = Color.FromArgb(255, 144, 238, 144),
            ["lightgrey"] = Color.FromArgb(255, 211, 211, 211),
            ["lightpink"] = Color.FromArgb(255, 255, 182, 193),
            ["lightsalmon"] = Color.FromArgb(255, 255, 160, 122),
            ["lightseagreen"] = Color.FromArgb(255, 32, 178, 170),
            ["lightskyblue"] = Color.FromArgb(255, 135, 206, 250),
            ["lightslategray"] = Color.FromArgb(255, 119, 136, 153),
            ["lightslategrey"] = Color.FromArgb(255, 119, 136, 153),
            ["lightsteelblue"] = Color.FromArgb(255, 176, 196, 222),
            ["lightyellow"] = Color.FromArgb(255, 255, 255, 224),
            ["lime"] = Color.FromArgb(255, 0, 255, 0),
            ["limegreen"] = Color.FromArgb(255, 50, 205, 50),
            ["linen"] = Color.FromArgb(255, 250, 240, 230),
            ["magenta"] = Color.FromArgb(255, 255, 0, 255),
            ["maroon"] = Color.FromArgb(255, 128, 0, 0),
            ["mediumaquamarine"] = Color.FromArgb(255, 102, 205, 170),
            ["mediumblue"] = Color.FromArgb(255, 0, 0, 205),
            ["mediumorchid"] = Color.FromArgb(255, 186, 85, 211),
            ["mediumpurple"] = Color.FromArgb(255, 147, 112, 219),
            ["mediumseagreen"] = Color.FromArgb(255, 60, 179, 113),
            ["mediumslateblue"] = Color.FromArgb(255, 13, 104, 238),
            ["mediumspringgreen"] = Color.FromArgb(255, 0, 250, 154),
            ["mediumturquoise"] = Color.FromArgb(255, 72, 209, 204),
            ["mediumvioletred"] = Color.FromArgb(255, 199, 21, 133),
            ["midnightblue"] = Color.FromArgb(255, 25, 25, 112),
            ["mintcream"] = Color.FromArgb(255, 245, 255, 250),
            ["mistyrose"] = Color.FromArgb(255, 255, 228, 225),
            ["moccasin"] = Color.FromArgb(255, 255, 228, 181),
            ["navajowhite"] = Color.FromArgb(255, 255, 222, 173),
            ["navy"] = Color.FromArgb(255, 0, 0, 128),
            ["oldlace"] = Color.FromArgb(255, 253, 245, 230),
            ["olive"] = Color.FromArgb(255, 128, 128, 0),
            ["olivedrab"] = Color.FromArgb(255, 107, 142, 35),
            ["orange"] = Color.FromArgb(255, 255, 165, 0),
            ["orangered"] = Color.FromArgb(255, 255, 69, 0),
            ["orchid"] = Color.FromArgb(255, 128, 112, 214),
            ["palegoldenrod"] = Color.FromArgb(255, 238, 232, 170),
            ["palegreen"] = Color.FromArgb(255, 152, 251, 152),
            ["paleturquoise"] = Color.FromArgb(255, 175, 238, 238),
            ["palevioletred"] = Color.FromArgb(255, 219, 112, 147),
            ["papayawhip"] = Color.FromArgb(255, 255, 239, 213),
            ["peachpuff"] = Color.FromArgb(255, 255, 218, 185),
            ["peru"] = Color.FromArgb(255, 205, 133, 63),
            ["pink"] = Color.FromArgb(255, 255, 192, 203),
            ["plum"] = Color.FromArgb(255, 221, 160, 221),
            ["powderblue"] = Color.FromArgb(255, 176, 224, 230),
            ["purple"] = Color.FromArgb(255, 128, 0, 128),
            ["red"] = Color.FromArgb(255, 255, 0, 0),
            ["rosybrown"] = Color.FromArgb(255, 188, 143, 143),
            ["royalblue"] = Color.FromArgb(255, 65, 105, 225),
            ["saddlebrown"] = Color.FromArgb(255, 139, 69, 19),
            ["salmon"] = Color.FromArgb(255, 250, 128, 114),
            ["sandybrown"] = Color.FromArgb(255, 244, 164, 96),
            ["seagreen"] = Color.FromArgb(255, 46, 139, 87),
            ["seashell"] = Color.FromArgb(255, 255, 245, 238),
            ["sienna"] = Color.FromArgb(255, 160, 82, 45),
            ["silver"] = Color.FromArgb(255, 192, 192, 192),
            ["skyblue"] = Color.FromArgb(255, 135, 206, 235),
            ["slateblue"] = Color.FromArgb(255, 106, 90, 205),
            ["slategray"] = Color.FromArgb(255, 112, 128, 144),
            ["slategrey"] = Color.FromArgb(255, 112, 128, 144),
            ["snow"] = Color.FromArgb(255, 255, 250, 250),
            ["springgreen"] = Color.FromArgb(255, 0, 255, 127),
            ["steelblue"] = Color.FromArgb(255, 70, 130, 180),
            ["tan"] = Color.FromArgb(255, 210, 180, 140),
            ["teal"] = Color.FromArgb(255, 0, 128, 128),
            ["thistle"] = Color.FromArgb(255, 216, 191, 216),
            ["tomato"] = Color.FromArgb(255, 255, 99, 71),
            ["turquoise"] = Color.FromArgb(255, 64, 224, 208),
            ["violet"] = Color.FromArgb(255, 238, 130, 238),
            ["wheat"] = Color.FromArgb(255, 245, 222, 179),
            ["white"] = Color.FromArgb(255, 255, 255, 255),
            ["whitesmoke"] = Color.FromArgb(255, 245, 245, 245),
            ["yellow"] = Color.FromArgb(255, 255, 255, 0),
            ["yellowgreen"] = Color.FromArgb(255, 154, 205, 50)
        };

        public static Color ParseColor(string color)
        {
            color = color.Replace(" ", string.Empty);
            if (string.IsNullOrWhiteSpace(color) || color.ToLower().Equals("none"))
                return Windows.UI.Colors.Transparent;
            if (color.StartsWith("#"))
                return ParseColorFromHexString(color);
            if (color.StartsWith("rgb"))
                return ParseColorFromRgbString(color);
            return ParseColorFromName(color);
        }

        private static Color ParseColorFromHexString(string color)
        {
            color = color.Replace("#", string.Empty);
            if (color.Length == 3)
            {
                var str = string.Empty;
                foreach (var c in color)
                {
                    str += c;
                    str += c;
                }

                color = str;
            }

            byte alpha = 255;
            if (color.Length == 8)
            {
                alpha = byte.Parse(color.Substring(0, 2), NumberStyles.HexNumber);
                color = color.Substring(2);
            }

            var v = int.Parse(color, NumberStyles.HexNumber);
            return new Color
            {
                A = alpha,
                R = Convert.ToByte((v >> 16) & 255),
                G = Convert.ToByte((v >> 8) & 255),
                B = Convert.ToByte((v >> 0) & 255)
            };
        }

        private static Color ParseColorFromRgbString(string color)
        {
            var hadAlpha = color.StartsWith("rgba");
            color = color.Substring(hadAlpha ? "rgba(".Length : "rgb(".Length)?.Replace(")", "");
            byte alpha = 255;
            var strs = color?.Split(',').ToList();
            if (hadAlpha && strs?.Count == 4)
            {
                var alphaStr = strs[3];
                strs.Remove(alphaStr);
                double.TryParse(alphaStr, out var alphaValue);
                alpha = (byte) (255 * alphaValue);
            }

            if (strs?.Count == 3)
            {
                var vals = new List<int>();
                foreach (var str in strs)
                {
                    var item = str.Trim();
                    int value;
                    if (item.Contains('%'))
                    {
                        item = item.Replace('%', ' ');
                        item = item.Trim();
                        value = int.Parse(item);
                        value = (int) (2.55 * value);
                    }
                    else
                    {
                        value = int.Parse(item);
                    }

                    vals.Add(value);
                }

                return Color.FromArgb(alpha, (byte) vals[0], (byte) vals[1], (byte) vals[2]);
            }

            return Windows.UI.Colors.Transparent;
        }

        private static Color ParseColorFromName(string colorName)
        {
            if (Colors.ContainsKey(colorName.ToLower()))
                return Colors[colorName.ToLower()];
            return Windows.UI.Colors.Transparent;
        }
    }
}