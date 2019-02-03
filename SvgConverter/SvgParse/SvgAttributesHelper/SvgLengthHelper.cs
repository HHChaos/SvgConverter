namespace SvgConverter.SvgParse.SvgAttributesHelper
{
    public static class SvgLengthHelper
    {
        /// <summary>
        ///     转换svg中的长度单位（dpi=96,1em=16,1em=1ex）
        /// </summary>
        /// <param name="length"></param>
        /// <param name="refLength"></param>
        /// <returns></returns>
        public static double ParseLength(string length, double refLength)
        {
            if (string.IsNullOrWhiteSpace(length))
                return 0;
            if (double.TryParse(length, out var result))
                return result;
            length = length.Replace(" ", string.Empty).ToLower();
            if (length.Contains("px"))
                return ParseLength(length.Replace("px", string.Empty), refLength);
            if (length.Contains("%"))
                return ParseLength(length.Replace("%", string.Empty), refLength) * refLength / 100;
            if (length.Contains("pt"))
                return ParseLength(length.Replace("pt", string.Empty), refLength) * 12 / 9;
            if (length.Contains("pc"))
                return ParseLength(length.Replace("pc", string.Empty), refLength) * 16;
            if (length.Contains("em"))
                return ParseLength(length.Replace("em", string.Empty), refLength) * 16;
            if (length.Contains("ex"))
                return ParseLength(length.Replace("ex", string.Empty), refLength) * 16;
            if (length.Contains("cm"))
                return ParseLength(length.Replace("cm", string.Empty), refLength) * 96 / 0.254;
            if (length.Contains("mm"))
                return ParseLength(length.Replace("mm", string.Empty), refLength) * 96 / 25.4;
            if (length.Contains("in")) return ParseLength(length.Replace("in", string.Empty), refLength) * 96;
            return 0;
        }
    }
}