using System.Numerics;

namespace SvgConverter.TextParse
{
    public struct PathPoint
    {
        public PathPoint(int row, int col)
        {
            Row = row;
            Col = col;
        }

        public int Row { get; }
        public int Col { get; }

        public Vector2 ToVector2(Vector2 refSplitVector2)
        {
            return new Vector2
            {
                X = refSplitVector2.X * (Col + 0.5f),
                Y = refSplitVector2.Y * (Row + 0.5f)
            };
        }

        public override bool Equals(object obj)
        {
            if (!(obj is PathPoint)) return false;

            var point = (PathPoint) obj;
            return Row == point.Row &&
                   Col == point.Col;
        }

        public override int GetHashCode()
        {
            var hashCode = 1084646500;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + Row.GetHashCode();
            hashCode = hashCode * -1521134295 + Col.GetHashCode();
            return hashCode;
        }

        public static PathPoint operator +(PathPoint point1, PathPoint point2)
        {
            return new PathPoint(point1.Row + point2.Row, point1.Col + point2.Col);
        }

        public static bool operator ==(PathPoint point1, PathPoint point2)
        {
            return point1.Row == point2.Row && point1.Col == point2.Col;
        }

        public static bool operator !=(PathPoint point1, PathPoint point2)
        {
            return point1.Row != point2.Row || point1.Col != point2.Col;
        }
    }
}