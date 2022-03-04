/**************************************************************
  This file was generated by FileConcatenator.
  It combined all classes in the project to work in Codingame.
  This hasn't been put in a namespace to allow for class 
  name duplicates.
***************************************************************/
using System.Collections.Generic;
using System.Drawing;
using System;
using System.Linq;
using System.Net;
using System.Diagnostics;
using System.Threading;
using System.Runtime.CompilerServices;

    internal sealed class Ball
    {
        private Stack<Direction> _moveDirections;

        public Point Position { get; set; }

        public int NumberOfHits { get; set; }

        public Ball(Point position, int numberOfHits)
        {
            Position = position;
            NumberOfHits = numberOfHits;

            _moveDirections = new Stack<Direction>();
            _moveDirections.Push(Direction.Vertical);
        }
        public void AddDirection(Direction direction)
        {
            _moveDirections.Push(direction);
        }
        public Direction PeekMoveDirection()
        {
            return _moveDirections.Peek();
        }

        public Direction PopMoveDirection()
        {
            return _moveDirections.Pop();
        }
    }

    internal sealed class Course
    {
        internal CourseContent[,] Contents { get; }

        private List<Ball> _balls = new List<Ball>();

        internal Course(int x, int y)
        {
            Contents = new CourseContent[x,y];
        }

        internal void AddBall(int x, int y, int numberOfHits)
        {
            _balls.Add(new Ball(new Point(x, y), numberOfHits));
        }

        internal void AddContent(int x, int y, CourseContent content)
        {
            Contents[x, y] = content;
        }

        internal List<Ball> GetBalls()
        {
            return _balls.ConvertAll(b => (new Ball(b.Position, b.NumberOfHits)));
        }

        internal int GetNumberOfHits(int x, int y)
        {
            return _balls.Single(b => b.Position.X == x && b.Position.Y == y).NumberOfHits;
        }

        private List<int> _movedIndexes = new List<int>();

        public void MoveBall(Point startPoint, Point endPoint)
        {
            //DebugDisplayer.DisplayBallLocations(Contents.GetLength(0), Contents.GetLength(1), _balls);

            //Console.Error.WriteLine($"Moving ball from {startPoint.X},{startPoint.Y} to {endPoint.X},{endPoint.Y}");

            _movedIndexes.Add(_balls.IndexOf(_balls.Single(b => b.Position.X == startPoint.X && b.Position.Y == startPoint.Y)));

            //DebugDisplayer.DisplayMoveIndexes(_movedIndexes);

            var movedBall = _balls[_movedIndexes[^1]];

            movedBall.Position = new Point(endPoint.X, endPoint.Y);
            movedBall.NumberOfHits--;

            if (startPoint.X - endPoint.X != 0)
            {
                movedBall.AddDirection(Direction.Horizontal);
                //movedBall.MoveDirections.Push(Direction.Horizontal);
                //Console.Error.WriteLine("Direction.Horizontal");
            }
            else
            {
                //movedBall.MoveDirections.Push(Direction.Vertical);
                movedBall.AddDirection(Direction.Vertical);
                //Console.Error.WriteLine("Direction.Vertical");
            }

            //DebugDisplayer.DisplayBallLocations(Contents.GetLength(0), Contents.GetLength(1), _balls);
        }

        public void UnMoveBall(Point startPoint, Point endPoint)
        {
            //DebugDisplayer.DisplayBallLocations(Contents.GetLength(0), Contents.GetLength(1), _balls);

            var lastIndex = _movedIndexes[^1];
            var movedBall = _balls[lastIndex];

            movedBall.Position = new Point(startPoint.X, startPoint.Y);
            movedBall.NumberOfHits++;

            _movedIndexes.RemoveAt(_movedIndexes.Count-1);

            movedBall.PopMoveDirection();

            //DebugDisplayer.DisplayBallLocations(Contents.GetLength(0), Contents.GetLength(1), _balls);
        }
        public void OrderBalls()
        {
            _balls = _balls.OrderBy(b => b.NumberOfHits).ToList();
        }
    }

    public enum CourseContent
    {
        Empty,
        Ball,
        Hole,
        Water
    }

    internal sealed class CourseConverter
    {
        internal static Course TextToCourse(char[,] courseText)
        {
            var course = new Course(courseText.GetLength(0), courseText.GetLength(1));

            for (var y = 0; y < courseText.GetLength(1); y++)
            {
                for (var x = 0; x < courseText.GetLength(0); x++)
                {
                    var character = courseText[x,y];

                    var result = 0;

                    if(int.TryParse(character.ToString(), out result))
                    {
                        //Console.Error.WriteLine($"Adding ball to {x},{y}");
                        course.AddBall(x, y, result);
                    }
                    else
                    {
                        var courseContent = character switch
                        {
                            '.' => CourseContent.Empty,
                            'X' => CourseContent.Water,
                            'H' => CourseContent.Hole,
                            _ => throw new ArgumentOutOfRangeException()
                        };

                        //Console.Error.WriteLine($"Adding {courseContent} to {x},{y}");

                        course.AddContent(x, y, courseContent);
                    }
                }
            }

            course.OrderBalls();

            return course;
        }

        internal static char[,] CourseToText(Course course)
        {
            var contents = course.Contents;

            var courseText = new char[contents.GetLength(0), contents.GetLength(1)];

            for (var y = 0; y < contents.GetLength(1); y++)
            {
                for (var x = 0; x < contents.GetLength(0); x++)
                {
                    var content = contents[x,y];

                    var result = 0;

                    var character = contents[x,y] switch
                    {
                        CourseContent.Empty => '.',
                        CourseContent.Water => 'X',
                        CourseContent.Hole => 'H',
                        _ => throw new ArgumentOutOfRangeException()
                    };

                    courseText[x, y] = character;
                }
            }

            return courseText;
        }

        internal static char[,] CreateMoveBoard(int width, int height, List<(Point, Point)> verifiedMoves)
        {
            var moveChars = new char[width, height];

            for (var y = 0; y < moveChars.GetLength(1); y++)
            {
                for (var x = 0; x < moveChars.GetLength(0); x++)
                {
                    moveChars[x, y] = '.';
                }
            }

            //TODO: Add arrows for the whole move, not just the start

            foreach (var move in verifiedMoves)
            {
                char arrowDirection;

                if (move.Item2.X > move.Item1.X)
                {
                    for (var x = move.Item1.X; x < move.Item2.X; x++)
                    {
                        moveChars[x, move.Item1.Y] = '>';
                    }
                }
                else if (move.Item2.X < move.Item1.X)
                {
                    for (var x = move.Item1.X; x > move.Item2.X; x--)
                    {
                        moveChars[x, move.Item1.Y] = '<';
                    }
                }
                else if(move.Item2.Y < move.Item1.Y)
                {
                    for (var y = move.Item1.Y; y > move.Item2.Y; y--)
                    {
                        moveChars[move.Item1.X, y] = '^';
                    }

                }
                else if(move.Item2.Y > move.Item1.Y)
                {
                    for (var y = move.Item1.Y; y < move.Item2.Y; y++)
                    {
                        moveChars[move.Item1.X, y] = 'v';
                    }
                }
            }

            return moveChars;
        }

        internal static string ConvertMoveBoardToString(char[,] moveBoard)
        {
            var answer = string.Empty;

            for (var y = 0; y < moveBoard.GetLength(1); y++)
            {
                for (var x = 0; x < moveBoard.GetLength(0); x++)
                {
                    answer += moveBoard[x,y];
                }

                if (y < moveBoard.GetLength(1) - 1)
                {
                    answer += "\n";
                }
            }

            return answer;
        }
    }

    internal static class DebugDisplayer
    {
        internal static void DisplayCourse(Course course)
        {
            var display = string.Empty;

            var courseContents = course.Contents;

            for (var y = 0; y < courseContents.GetLength(1); y++)
            {
                for (var x = 0; x < courseContents.GetLength(0); x++)
                {
                    var character = courseContents[x,y] switch
                    {
                        CourseContent.Empty => '.',
                        CourseContent.Water => 'X',
                        CourseContent.Hole => 'H',
                        _ => '.'
                    };

                    Console.Error.WriteLine($"{x}, {y}, {character}");

                    display += character;
                }

                display += "\n";
            }

            Console.Error.WriteLine(display);
        }

        internal static void DisplayCourseText(char[,] course)
        {
            var display = string.Empty;

            for (var y = 0; y < course.GetLength(1); y++)
            {
                for (var x = 0; x < course.GetLength(0); x++)
                {
                    display += course[x,y];
                }

                display += "\n";
            }

            Console.Error.WriteLine(display);
        }

        internal static void DisplayMoves(int width, int height, List<(Point, Point)> moves)
        {
            var board = new string[width, height];

            for (var y = 0; y < board.GetLength(1); y++)
            {
                for (var x = 0; x < board.GetLength(0); x++)
                {
                    board[x, y] = "  ";
                }
            }

            for (var i = 0; i < moves.Count; i++)
            {
                var move = moves[i];
                board[move.Item1.X, move.Item1.Y] = i + "a";
                board[move.Item2.X, move.Item2.Y] = i + "b";
            }

            var display = string.Empty;

            for (var i = 0; i < width; i++)
            {
                display += "---";
            }

            display += "\n";

            for (var y = 0; y < board.GetLength(1); y++)
            {
                display += "|";
                for (var x = 0; x < board.GetLength(0); x++)
                {
                    display += board[x,y];
                    display += "|";
                }

                display += "\n";
            }

            for (var i = 0; i < width; i++)
            {
                display += "---";
            }

            Console.Error.WriteLine(display);
        }
        public static void DisplayBallLocations(int width, int height, List<Ball> balls)
        {
        var board = new char[width, height];

            for (var y = 0; y < board.GetLength(1); y++)
            {
                for (var x = 0; x < board.GetLength(0); x++)
                {
                    board[x, y] = ' ';
                }
            }

            foreach (var ball in balls)
            {
                board[ball.Position.X, ball.Position.Y] = (char)('0' + ball.NumberOfHits);
                Console.Error.WriteLine($"{ball.Position.X},{ball.Position.Y}, {ball.NumberOfHits}");
            }

            var display = string.Empty;

            for (var i = 0; i < width; i++)
            {
                display += "---";
            }

            display += "\n";

            for (var y = 0; y < board.GetLength(1); y++)
            {
                display += "|";
                for (var x = 0; x < board.GetLength(0); x++)
                {
                    display += board[x,y];
                    display += "|";
                }

                display += "\n";
            }

            for (var i = 0; i < width; i++)
            {
                display += "---";
            }

            Console.Error.WriteLine(display);
        }

        internal static void DisplayMoveIndexes(IEnumerable<int> moveIndexes)
        {
            Console.Error.Write("MoveIndexes: ");

            foreach (var moveIndex in moveIndexes)
            {
                Console.Error.Write(moveIndex + " ");
            }

            Console.Error.WriteLine();
        }
    }

    public enum Direction
    {
        None,
        Vertical,
        Horizontal
    }

    internal sealed class MoveCalculator
    {
        private Stopwatch _totalTimeStopwatch = new Stopwatch();

        internal string CalculateMoves(Course course)
        {
            _totalTimeStopwatch.Start();

            var verifiedMoves = new List<(Point, Point)>();
            var possibleMoves = new List<(Point, Point)>();

            var courseContents = course.Contents;
            var moveBoard = CourseConverter.CreateMoveBoard(courseContents.GetLength(0), courseContents.GetLength(1), verifiedMoves);

            foreach (var ball in course.GetBalls())
            {
                possibleMoves.AddRange(CalculateMovesForBall(courseContents, moveBoard, ball));
            }

            //Console.Error.WriteLine($"Base calculate move. {possibleMoves.Count} possible moves found");

            foreach (var possibleMove in possibleMoves)
            {
                //Console.Error.WriteLine($"Attempting move {possibleMove.Item1.X},{possibleMove.Item1.Y} to {possibleMove.Item2.X},{possibleMove.Item2.Y}");

                // Make move
                course.MoveBall(possibleMove.Item1, possibleMove.Item2);
                verifiedMoves.Add(possibleMove);

                var works = CalculateMoves(verifiedMoves, course);

                // Unmake move
                course.UnMoveBall(possibleMove.Item1, possibleMove.Item2);

                if (works)
                {
                    // convert verified moves to output board

                    //Console.Error.WriteLine($"VerifiedMove count: {verifiedMoves.Count}");

                    _totalTimeStopwatch.Stop();

                    var timeSpan = _totalTimeStopwatch.Elapsed;
                    Console.Error.WriteLine($"Total time: {timeSpan}");

                    return CourseConverter.ConvertMoveBoardToString(CourseConverter.CreateMoveBoard(courseContents.GetLength(0), courseContents.GetLength(1), verifiedMoves));
                }
                else
                {
                    verifiedMoves.RemoveAt(verifiedMoves.Count-1);
                }
            }

            return string.Empty;
        }

        private static bool CalculateMoves(List<(Point, Point)> verifiedMoves, Course course)
        {
            // If a ball has 0 hits left and isn't in a hole don't bother
            if (AreAnyDeadBalls(course))
            {
                return false;
            }

            // If any balls are in the same grid return
            if (AreAnyBallsInSameSpot(course.GetBalls()))
            {
                return false;
            }

            if (AreAllBallsInSeparateHoles(course))
            {
                //Console.Error.WriteLine("All balls in holes. Returning true");
                return true;
            }

            var possibleMoves = new List<(Point, Point)>();

            var courseContents = course.Contents;

            var moveBoard = CourseConverter.CreateMoveBoard(courseContents.GetLength(0), courseContents.GetLength(1), verifiedMoves);

            foreach (var ball in course.GetBalls())
            {
                if (ball.NumberOfHits > 0)
                {
                    possibleMoves.AddRange(CalculateMovesForBall(courseContents, moveBoard, ball));
                }
            }

            //Console.Error.WriteLine($"Calculate move. {possibleMoves.Count} possible moves found");

            if (possibleMoves.Count == 0)
            {
                //Console.Error.WriteLine("returning false");
                return false;
            }

            //Console.Error.WriteLine("=======================================");
            //Console.Error.WriteLine("Before move");
            //DebugDisplayer.DisplayMoves(courseContents.GetLength(0), courseContents.GetLength(1), verifiedMoves);
            //DebugDisplayer.DisplayBallLocations(course.Contents.GetLength(0), course.Contents.GetLength(1), course.GetBalls());


            foreach (var possibleMove in possibleMoves)
            {
                //Console.Error.WriteLine("=======================================");
                //Console.Error.WriteLine("Before make move");
                //Console.Error.WriteLine($"Attempting move {possibleMove.Item1.X},{possibleMove.Item1.Y} to {possibleMove.Item2.X},{possibleMove.Item2.Y}");

                // make move
                course.MoveBall(possibleMove.Item1, possibleMove.Item2);

                verifiedMoves.Add(possibleMove);

                //Console.Error.WriteLine("=======================================");
                //Console.Error.WriteLine("After make move");
                //DebugDisplayer.DisplayMoves(courseContents.GetLength(0), courseContents.GetLength(1), verifiedMoves);
                //DebugDisplayer.DisplayBallLocations(course.Contents.GetLength(0), course.Contents.GetLength(1), course.GetBalls());

                var works = CalculateMoves(verifiedMoves, course);

                if (works)
                {
                    //Console.Error.WriteLine("returning true");
                    return true;
                }
                else
                {
                    verifiedMoves.RemoveAt(verifiedMoves.Count-1);

                    //Console.Error.WriteLine("=======================================");
                    //Console.Error.WriteLine("After unmake move");
                    //DebugDisplayer.DisplayMoves(courseContents.GetLength(0), courseContents.GetLength(1), verifiedMoves);
                    //DebugDisplayer.DisplayBallLocations(course.Contents.GetLength(0), course.Contents.GetLength(1), course.GetBalls());
                }

                course.UnMoveBall(possibleMove.Item1, possibleMove.Item2);
            }

            //Console.Error.WriteLine("returning false");
            return false;
        }

        private static bool AreAnyBallsInSameSpot(List<Ball> balls)
        {
            var duplicates = balls.GroupBy(b => new { b.Position.X, b.Position.Y }).Where(x => x.Skip(1).Any()).Any();

            if (duplicates)
            {
                //Console.Error.WriteLine($"Duplicates found");
                return true;
            }

            return false;
        }

        private static bool AreAnyDeadBalls(Course course)
        {
            return course.GetBalls().Any(b => b.NumberOfHits == 0 && course.Contents[b.Position.X, b.Position.Y] != CourseContent.Hole);
        }

        private static IEnumerable<(Point, Point)> CalculateMovesForBall(CourseContent[,] courseContent, char[,] moveBoard, Ball ball)
        {
            var xStart = ball.Position.X;
            var yStart = ball.Position.Y;
            var numberOfHits = ball.NumberOfHits;
            var direction = ball.PeekMoveDirection();

            var allowedMoves = new List<(Point, Point)>();

            var startPoint = new Point(xStart, yStart);

            int xPosition, yPosition;

            if (direction != Direction.Horizontal)
            {
                // check left
                xPosition = startPoint.X - numberOfHits;
                yPosition = startPoint.Y;

                if (xPosition >= 0)
                {
                    var blocked = false;

                    for (var x = startPoint.X - 1; x >= startPoint.X - numberOfHits; x--)
                    {
                        blocked = IsBlocked(moveBoard, x, yPosition);
                    }

                    if (!blocked)
                    {
                        var gridContent = courseContent[xPosition, yPosition];

                        if (gridContent == CourseContent.Hole)
                        {
                            // verify there's no other ball here

                            allowedMoves.Add((startPoint, new Point(xPosition, yPosition)));

                        }
                        else if (gridContent == CourseContent.Empty)
                        {
                            allowedMoves.Add((startPoint, new Point(xPosition, yPosition)));
                        }
                    }
                }

                // check right
                xPosition = startPoint.X + numberOfHits;
                yPosition = startPoint.Y;

                if (xPosition < moveBoard.GetLength(0))
                {
                    var blocked = false;

                    for (var x = startPoint.X + 1; x <= startPoint.X + numberOfHits; x++)
                    {
                        blocked = IsBlocked(moveBoard, x, yPosition);
                    }

                    if (!blocked)
                    {
                        var gridContent = courseContent[xPosition, yPosition];

                        if (gridContent == CourseContent.Hole)
                        {
                            // verify there's no other ball here

                            allowedMoves.Add((startPoint, new Point(xPosition, yPosition)));

                        }
                        else if (gridContent == CourseContent.Empty)
                        {
                            allowedMoves.Add((startPoint, new Point(xPosition, yPosition)));
                        }
                    }
                }
            }

            if (direction != Direction.Vertical)
            {

                // check up
                xPosition = startPoint.X;
                yPosition = startPoint.Y - numberOfHits;

                if (yPosition >= 0)
                {
                    var blocked = false;

                    for (var y = startPoint.Y - 1; y >= startPoint.Y - numberOfHits; y--)
                    {
                        blocked = IsBlocked(moveBoard, xPosition, y);
                    }

                    if (!blocked)
                    {
                        var gridContent = courseContent[xPosition, yPosition];

                        if (gridContent == CourseContent.Hole)
                        {
                            // verify there's no other ball here

                            allowedMoves.Add((startPoint, new Point(xPosition, yPosition)));

                        }
                        else if (gridContent == CourseContent.Empty)
                        {
                            allowedMoves.Add((startPoint, new Point(xPosition, yPosition)));
                        }
                    }
                }

                //check down
                xPosition = startPoint.X;
                yPosition = startPoint.Y + numberOfHits;

                if (yPosition < moveBoard.GetLength(1))
                {
                    var blocked = false;

                    for (var y = startPoint.Y + 1; y <= startPoint.Y + numberOfHits; y++)
                    {
                        blocked = IsBlocked(moveBoard, xPosition, y);
                    }

                    if (!blocked)
                    {
                        var gridContent = courseContent[xPosition, yPosition];

                        if (gridContent == CourseContent.Hole)
                        {
                            // verify there's no other ball here

                            allowedMoves.Add((startPoint, new Point(xPosition, yPosition)));

                        }
                        else if (gridContent == CourseContent.Empty)
                        {
                            allowedMoves.Add((startPoint, new Point(xPosition, yPosition)));
                        }
                    }
                }
            }

            // = allowedMoves.OrderBy(m => GetDistance(m.Item1, m.Item2)).ToList();

            //allowedMoves = allowedMoves.OrderByDescending(m => GetDistance(m.Item1, m.Item2)).ToList();

            return allowedMoves;
        }

        private static int GetDistance(Point point1, Point point2)
        {
            return Math.Abs(point1.X - point2.X) + Math.Abs(point1.Y - point2.Y);
            //return Math.Sqrt(Math.Pow(point1.X - point2.X, 2) - Math.Pow(point1.Y - point2.Y, 2));
        }
        private static bool IsBlocked(char[,] moveBoard, int x, int yPosition)
        {
            return moveBoard[x, yPosition] != '.';
        }

        private static bool AreAllBallsInSeparateHoles(Course course)
        {
            var balls = course.GetBalls();
            var courseContents = course.Contents;

            foreach (var ball in balls)
            {
                //Console.Error.WriteLine($"Checking {ball.Item1.X},{ball.Item1.Y} - courseContents[ball.Item1.X, ball.Item1.Y]");
                if (courseContents[ball.Position.X, ball.Position.Y] != CourseContent.Hole)
                {
                    //Console.Error.WriteLine($"A ball is not in a hole");
                    return false;
                }
            }

            return true;
        }
    }

    internal sealed class Solution
    {
        static void Main(string[] args)
        {
            var inputs = Console.ReadLine().Split(' ');
            var width = int.Parse(inputs[0]);
            var height = int.Parse(inputs[1]);

            var courseText = new char[width, height];

            for (var y = 0; y < height; y++)
            {
                var row = Console.ReadLine();

                var cols = row.ToCharArray();

                for (var x = 0; x < width; x++)
                {
                    courseText[x,y] = cols[x];
                }

                //Console.Error.WriteLine(row);
            }

            //DebugDisplayer.DisplayCourseText(courseText);
            
            // Convert to Course
            var course = CourseConverter.TextToCourse(courseText);

            //DebugDisplayer.DisplayCourse(course);
            //DebugDisplayer.DisplayBallLocations(course.Contents.GetLength(0), course.Contents.GetLength(1), course.GetBalls());

            var moveCalculator = new MoveCalculator();

            var moves = moveCalculator.CalculateMoves(course);

            var results = moves.Split("\n");

            foreach (var result in results)
            {
                //Console.Error.WriteLine($"result - {result}");
                Console.WriteLine(result);
            }
        }
    }
