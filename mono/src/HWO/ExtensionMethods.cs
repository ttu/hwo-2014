using System;
using System.Collections.Generic;
using System.Linq;

namespace HWO
{
    public static class ExtensionMethods
    {
        public static bool IsCorner(this Piece piece)
        {
            if (piece.Angle.HasValue)
            {
                return piece.Angle.Value != 0;
            }

            return false;
        }

        public static double GetAngle(this Piece piece)
        {
            return piece.Angle.HasValue ? Math.Abs(piece.Angle.Value) : 0;
        }

        public static double GetLength(this Piece piece, int distanceFromCenter)
        {
            if (piece.Length != 0)
                return piece.Length;

            return MathHelper.GetCornerPieceLength(piece.Angle.Value, piece.Radius.Value + distanceFromCenter);
        }

        public static int GetLastPieceIndex(this Track track)
        {
            return track.Pieces.Count - 1;
        }

        public static Piece GetNextPiece(this Track track, int currentIndex)
        {
            var nextIndex = currentIndex == track.GetLastPieceIndex() ? 0 : currentIndex + 1;
            return track.Pieces[nextIndex];
        }

        public static int GetNextCornerIndex(this Track track, int currentIndex)
        {
            var piece = track.GetNextPiece(currentIndex);
            currentIndex = currentIndex == track.GetLastPieceIndex() ? 0 : currentIndex + 1;

            while (piece.IsCorner() == false)
            {
                piece = track.GetNextPiece(currentIndex);
                currentIndex = currentIndex == track.GetLastPieceIndex() ? 0 : currentIndex + 1;
            }

            return track.Pieces.IndexOf(piece);
        }

        public static int GetNextSwitchIndex(this Track track, int currentIndex)
        {
            var piece = track.GetNextPiece(currentIndex);
            currentIndex = currentIndex == track.GetLastPieceIndex() ? 0 : currentIndex + 1;

            while (piece.Switch == null || piece.Switch == false)
            {
                piece = track.GetNextPiece(currentIndex);
                currentIndex = currentIndex == track.GetLastPieceIndex() ? 0 : currentIndex + 1;
            }

            return track.Pieces.IndexOf(piece);
        }

        public static bool HasStraights(this Track track)
        {
            return track.Pieces.Any(p => p.IsCorner() == false);
        }

        public static IEnumerable<Piece> GetPieces(this Track track, int startIndex, int endIndex)
        {
            if (endIndex > startIndex)
            {
                return track.Pieces.Skip(startIndex).Take(endIndex - startIndex);
            }
            else
            {
                var end = track.Pieces.Skip(startIndex).Take(track.Pieces.Count - startIndex);
                var beginning = track.Pieces.Take(endIndex);

                return end.Concat(beginning);
            }
        }

        public static void GetNextStraigth(this Track track, int currentIndex, out int startIndex, out double length, out int firstCornerIndex)
        {
            if (track.HasStraights() == false)
            {
                startIndex = -1;
                length = -1;
                firstCornerIndex = -1;
            }

            while (track.Pieces[currentIndex].IsCorner())
            {
                currentIndex++;
            }

            var nextCorner = track.GetNextCornerIndex(currentIndex);

            var pieces = track.GetPieces(currentIndex, nextCorner);

            length = pieces.Select(p => p.Length).Sum();
            startIndex = currentIndex;
            firstCornerIndex = nextCorner;
        }

        public static void GetLongestStraigth(this Track track, out int index, out double length, out int firstCornerIndex)
        {
            if (track.HasStraights() == false)
            {
                index = -1;
                length = -1;
                firstCornerIndex = -1;
            }

            index = 0;
            firstCornerIndex = 0;
            length = 0;

            // TODO: This could just take start and next corner. No point checking each piece.
            for (int i = 0; i < track.Pieces.Count(); i++)
            {
                if (track.Pieces[i].IsCorner())
                    continue;

                var nextCorner = track.GetNextCornerIndex(i);

                var pieces = track.GetPieces(i, nextCorner);

                var straightLength = pieces.Select(p => p.Length).Sum();

                if (straightLength > length)
                {
                    length = straightLength;
                    index = i;
                    firstCornerIndex = nextCorner;
                }
            }

            return;
        }

        public static double GetLongestStraigthLength(this Track track)
        {
            int index;
            double length;
            int firstCorner;

            track.GetLongestStraigth(out index, out length, out firstCorner);
            return length;
        }

        public static double DistanceToPiece(this Track track, int currentIndex, int targetIndex)
        {
            if (currentIndex + 1 == targetIndex)
                return 0;

            var pieces = track.GetPieces(currentIndex +1, targetIndex);

            return pieces.Select(p => p.GetLength(0)).Sum();
        }

        /// <summary>
        /// Calculates which direction has more corners between this and next switch
        /// </summary>
        /// <param name="startingSwitchIndex"></param>
        /// <param name="startingCornerIndex"></param>
        /// <returns></returns>
        public static double CalculateCorners(this Track track, int startingSwitchIndex, int startingCornerIndex)
        {
            var nextSwitchIndex = track.GetNextSwitchIndex(startingSwitchIndex++);

            List<Piece> allCorners = new List<Piece>();
            startingCornerIndex--;

            while (startingCornerIndex != nextSwitchIndex)
            {
                var piece = track.GetNextPiece(startingCornerIndex++);
                if (piece.IsCorner())
                    allCorners.Add(piece);

                startingCornerIndex = startingCornerIndex == track.GetLastPieceIndex() ? 0 : startingCornerIndex;
            }

            var leftTurns = allCorners.Where(c => c.Angle < 0).Sum(c => c.GetLength(0));
            var rightTurns = allCorners.Where(c => c.Angle > 0).Sum(c => c.GetLength(0));

            if (leftTurns > rightTurns)
                return -1;
            else
                return 1;
        }

        public static bool IsQualifying(this RaceSession race)
        {
            return race.DurationMs > 0;
        }
    }
}