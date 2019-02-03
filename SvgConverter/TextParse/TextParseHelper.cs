using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Windows.Foundation;
using Windows.UI.Text;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.Text;

namespace SvgConverter.TextParse
{
    public static class TextParseHelper
    {
        public static CharGeometry GetGeometrys(int rowCount, int colCount, int pathWidthLimit, char text,
            string fontFamily, float fontSize)
        {
            var textLayout = new CanvasTextLayout(CanvasDevice.GetSharedDevice(), new string(new[] {text}),
                new CanvasTextFormat
                {
                    FontWeight = new FontWeight
                    {
                        Weight = 1
                    },
                    FontFamily = fontFamily,
                    FontSize = fontSize,
                    WordWrapping = CanvasWordWrapping.NoWrap
                }, 0, 0);
            var textDotArray = GetDotArray(textLayout, rowCount, colCount);
            var drawBounds = textLayout.DrawBounds;
            textLayout.Dispose();
            var directs = new List<PathMoveDirection>
            {
                PathMoveDirection.Right,
                PathMoveDirection.LowerRightRight,
                PathMoveDirection.LowerRight,
                PathMoveDirection.LowerRightLeft,
                PathMoveDirection.Down,
                PathMoveDirection.LowerLeftLeft,
                PathMoveDirection.LowerLeft,
                PathMoveDirection.LowerLeftRight
            };
            var pathList = new List<List<PathPoint>>();
            for (var i = 0; i < rowCount; i++)
            for (var j = 0; j < colCount; j++)
            {
                if (!textDotArray[i, j]) continue;
                var list = GetMaxPath(textDotArray, new PathPoint(i, j), directs);
                textDotArray[i, j] = false;
                if (list == null) continue;
                list.Insert(0, new PathPoint(i, j));
                pathList.Add(list);
                foreach (var p in list)
                    for (var k = -pathWidthLimit / 2; k < pathWidthLimit / 2 + 1; k++)
                    for (var l = -pathWidthLimit / 2; l < pathWidthLimit / 2 + 1; l++)
                        InvalidPoint(textDotArray, p, new PathPoint(k, l));
            }

            return new CharGeometry
            {
                Char = text,
                FontFamily = fontFamily,
                FontSize = fontSize,
                PathList = GetGeometrys(pathList, new Size(drawBounds.Width, drawBounds.Height),
                    rowCount, colCount)
            };
        }

        private static List<Vector2[]> GetGeometrys(List<List<PathPoint>> pathList, Size sise, int rowCount,
            int colCount)
        {
            if (pathList == null || pathList.Count <= 0) return new List<Vector2[]>();
            var splitPoint = new Point(sise.Width / colCount, sise.Height / rowCount).ToVector2();
            return (from pathPoints in pathList
                where pathPoints.Count >= 2
                select (from point in pathPoints select point.ToVector2(splitPoint))
                into points
                select points.ToArray()).ToList();
        }

        private static void InvalidPoint(bool[,] dotArrays, PathPoint sourcePoint, PathPoint offsetPoint)
        {
            var point = sourcePoint + offsetPoint;
            var i = dotArrays.GetLength(0);
            var j = dotArrays.GetLength(1);
            if (0 <= point.Row && point.Row < i && 0 <= point.Col && point.Col < j)
                dotArrays[point.Row, point.Col] = false;
        }

        private static List<PathPoint> GetMaxPath(bool[,] dotArrays, PathPoint startPoint,
            IEnumerable<PathMoveDirection> canMoveDirections)
        {
            var maxLengthList = new List<PathPoint>();
            foreach (var direct in canMoveDirections)
            {
                if (!CanMove(dotArrays, startPoint, direct))
                    continue;
                var dList = GetPath(dotArrays, startPoint, direct);
                if (dList != null && dList.Count > maxLengthList.Count) maxLengthList = dList;
            }

            return maxLengthList;
        }

        private static List<PathPoint> GetPath(bool[,] dotArrays, PathPoint startPoint,
            PathMoveDirection moveDirection)
        {
            var list = new List<PathPoint>();
            if (!CanMove(dotArrays, startPoint, moveDirection))
                return GetMaxPath(dotArrays, startPoint, moveDirection.GetNextMoveDirections());
            var point = startPoint + moveDirection.OffsetPoint;
            list.Add(point);
            var next = GetPath(dotArrays, point, moveDirection);
            if (next != null)
                list.AddRange(next);
            return list;
        }

        private static bool CanMove(bool[,] dotArrays, PathPoint startPoint, PathMoveDirection moveDirection)
        {
            var nextPoint = startPoint + moveDirection.OffsetPoint;
            var next2Point = nextPoint + moveDirection.OffsetPoint;
            if (0 <= nextPoint.Row
                && nextPoint.Row < dotArrays.GetLength(0)
                && 0 <= nextPoint.Col
                && nextPoint.Col < dotArrays.GetLength(1)
                && 0 <= next2Point.Row
                && next2Point.Row < dotArrays.GetLength(0)
                && 0 <= next2Point.Col
                && next2Point.Col < dotArrays.GetLength(1))
                return dotArrays[nextPoint.Row, nextPoint.Col] &&
                       dotArrays[next2Point.Row, next2Point.Col];
            return false;
        }

        private static bool[,] GetDotArray(CanvasTextLayout text, int rowCount, int colCount)
        {
            var path = new bool[rowCount, colCount];
            var startVector2 = new Vector2((float) text.DrawBounds.X, (float) text.DrawBounds.Y);
            var rectSize = new Size(text.DrawBounds.Width / colCount, text.DrawBounds.Height / rowCount);
            var textGeometry = CanvasGeometry.CreateText(text);
            for (var i = 0; i < rowCount; i++)
            {
                startVector2.X = (float) text.DrawBounds.X;
                for (var j = 0; j < colCount; j++)
                {
                    path[i, j] = textGeometry.FillContainsPoint(
                        new Point(startVector2.X + rectSize.Height / 2, startVector2.Y + rectSize.Width / 2)
                            .ToVector2());
                    startVector2.X += (float) rectSize.Width;
                }

                startVector2.Y += (float) rectSize.Height;
            }

            textGeometry.Dispose();
            return path;
        }
    }
}