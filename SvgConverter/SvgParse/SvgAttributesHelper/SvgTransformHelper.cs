using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text.RegularExpressions;

namespace SvgConverter.SvgParse.SvgAttributesHelper
{
    public static class SvgTransformHelper
    {
        public static Matrix3x2 ParseTransform(string transform)
        {
            if (string.IsNullOrWhiteSpace(transform))
                return Matrix3x2.Identity;
            transform = transform.Replace(',', ' ');
            transform = Regex.Replace(transform, @"\s+", " ");
            var transStrList = new List<string>();
            do
            {
                if (char.IsLower(transform[0]))
                {
                    var end = transform.IndexOf(')');
                    transStrList.Add(transform.Substring(0, end + 1));
                    transform = transform.Substring(end + 1);
                }
                else
                {
                    transform = transform.Substring(1);
                }
            } while (transform.Length > 0);

            var result = Matrix3x2.Identity;
            foreach (var trans in transStrList)
            {
                var m = Matrix3x2.Identity;
                var str = trans.Trim();
                if (str.StartsWith("translate"))
                {
                    str = str.Substring(10, str.Length - 11);
                    m = GetTranslate(str);
                }
                else if (str.StartsWith("scale"))
                {
                    str = str.Substring(6, str.Length - 7);
                    m = GetScale(str);
                }
                else if (str.StartsWith("rotate"))
                {
                    str = str.Substring(7, str.Length - 8);
                    m = GetRotate(str);
                }
                else if (str.StartsWith("skewX"))
                {
                    str = str.Substring(6, str.Length - 7);
                    if (float.TryParse(str.Trim(), out var angle))
                        m = Matrix3x2.CreateSkew(angle, 0);
                    else
                        continue;
                }
                else if (str.StartsWith("skewY"))
                {
                    str = str.Substring(6, str.Length - 7);
                    if (float.TryParse(str.Trim(), out var angle))
                        m = Matrix3x2.CreateSkew(0, angle);
                    else
                        continue;
                }
                else if (str.StartsWith("matrix"))
                {
                    str = str.Substring(7, str.Length - 8);
                    m = GetMatrix(str);
                }

                result = m * result;
            }

            return result;
        }

        private static Matrix3x2 GetMatrix(string transform)
        {
            var strs = transform.Split(' ');
            if (strs.Length != 6)
                return Matrix3x2.Identity;
            float.TryParse(strs[0].Trim(), out var a);
            float.TryParse(strs[1].Trim(), out var b);
            float.TryParse(strs[2].Trim(), out var c);
            float.TryParse(strs[3].Trim(), out var d);
            float.TryParse(strs[4].Trim(), out var e);
            float.TryParse(strs[5].Trim(), out var f);
            return new Matrix3x2(a, b, c, d, e, f);
        }

        private static Matrix3x2 GetTranslate(string transform)
        {
            var strs = transform.Split(' ');
            if (!(strs.Length > 0))
                return Matrix3x2.Identity;
            float.TryParse(strs[0].Trim(), out var x);
            if (strs.Length <= 1)
                return Matrix3x2.CreateTranslation(new Vector2(x, 0));
            float.TryParse(strs[1].Trim(), out var y);
            return Matrix3x2.CreateTranslation(new Vector2(x, y));
        }

        private static Matrix3x2 GetScale(string transform)
        {
            var strs = transform.Split(' ');
            if (!(strs.Length > 0))
                return Matrix3x2.Identity;
            float.TryParse(strs[0].Trim(), out var x);
            if (strs.Length <= 1)
                return Matrix3x2.CreateScale(x);
            float.TryParse(strs[1].Trim(), out var y);
            return Matrix3x2.CreateScale(x, y);
        }

        private static Matrix3x2 GetRotate(string transform)
        {
            var strs = transform.Split(' ');
            if (strs.Length == 0)
                return Matrix3x2.Identity;
            float.TryParse(strs[0].Trim(), out var angle);
            angle *= (float) Math.PI / 180;
            if (strs.Length < 3) return Matrix3x2.CreateRotation(angle);

            float.TryParse(strs[1].Trim(), out var x);
            float.TryParse(strs[2].Trim(), out var y);
            return Matrix3x2.CreateRotation(angle, new Vector2(x, y));
        }
    }
}