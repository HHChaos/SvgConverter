using System.Collections.Generic;

namespace SvgConverter.TextParse
{
    public class PathMoveDirection
    {
        public static readonly PathMoveDirection Left = new PathMoveDirection
        {
            _id = -1,
            OffsetPoint = new PathPoint(0, -1),
            _nextMoveDirectionIds = new List<int>
            {
                4,
                3,
                5
            }
        };

        public static readonly PathMoveDirection Right = new PathMoveDirection
        {
            _id = 0,
            OffsetPoint = new PathPoint(0, 1),
            _nextMoveDirectionIds = new List<int>
            {
                7,
                2,
                6
            }
        };

        public static readonly PathMoveDirection Down = new PathMoveDirection
        {
            _id = 1,
            OffsetPoint = new PathPoint(1, 0),
            _nextMoveDirectionIds = new List<int>
            {
                3,
                5,
                6,
                2
            }
        };

        public static readonly PathMoveDirection LowerRight = new PathMoveDirection
        {
            _id = 2,
            OffsetPoint = new PathPoint(1, 1),
            _nextMoveDirectionIds = new List<int>
            {
                1,
                6,
                7,
                0
            }
        };

        public static readonly PathMoveDirection LowerLeft = new PathMoveDirection
        {
            _id = 3,
            OffsetPoint = new PathPoint(1, -1),
            _nextMoveDirectionIds = new List<int>
            {
                -1,
                4,
                5,
                1
            }
        };

        public static readonly PathMoveDirection LowerLeftLeft = new PathMoveDirection
        {
            _id = 4,
            OffsetPoint = new PathPoint(1, -2),
            _nextMoveDirectionIds = new List<int>
            {
                -1,
                3,
                5
            }
        };

        public static readonly PathMoveDirection LowerLeftRight = new PathMoveDirection
        {
            _id = 5,
            OffsetPoint = new PathPoint(2, -1),
            _nextMoveDirectionIds = new List<int>
            {
                4,
                3,
                1,
                6
            }
        };

        public static readonly PathMoveDirection LowerRightLeft = new PathMoveDirection
        {
            _id = 6,
            OffsetPoint = new PathPoint(2, 1),
            _nextMoveDirectionIds = new List<int>
            {
                5,
                1,
                2,
                7
            }
        };

        public static readonly PathMoveDirection LowerRightRight = new PathMoveDirection
        {
            _id = 7,
            OffsetPoint = new PathPoint(1, 2),
            _nextMoveDirectionIds = new List<int>
            {
                6,
                2,
                0
            }
        };

        private int _id;
        private List<int> _nextMoveDirectionIds;
        public PathPoint OffsetPoint { get; private set; }

        public List<PathMoveDirection> GetNextMoveDirections()
        {
            var list = new List<PathMoveDirection>();
            foreach (var id in _nextMoveDirectionIds)
                if (id == Down._id)
                    list.Add(Down);
                else if (id == Right._id)
                    list.Add(Right);
                else if (id == LowerRight._id)
                    list.Add(LowerRight);
                else if (id == LowerLeft._id)
                    list.Add(LowerLeft);
                else if (id == Left._id)
                    list.Add(Left);
                else if (id == LowerRightLeft._id)
                    list.Add(LowerRightLeft);
                else if (id == LowerRightRight._id)
                    list.Add(LowerRightRight);
                else if (id == LowerLeftLeft._id)
                    list.Add(LowerLeftLeft);
                else if (id == LowerLeftRight._id) list.Add(LowerLeftRight);
            return list;
        }
    }
}