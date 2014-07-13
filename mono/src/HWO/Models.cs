using System;
using System.Collections.Generic;

namespace HWO
{
    // http://json2csharp.com/

    public interface IRootObject
    { }

    public interface ITickRootObject : IRootObject
    {
        int GameTick { get; }
    }

    public abstract class RootObject<T> : IRootObject
    {
        public string MsgType { get; set; }

        public T Data { get; set; }
    }

    public abstract class TickRootObject<T> : RootObject<T>, ITickRootObject
    {
        public string GameId { get; set; }

        public int GameTick { get; set; }
    }

    #region yourCar

    public class YourCarRoot : RootObject<CarId>
    { }

    #endregion yourCar

    #region gameInit

    public class GameInitRoot : RootObject<RaceData>
    {
        public Guid GameId { get; set; }
    }

    public class RaceData
    {
        public Race Race { get; set; }
    }

    public class Track
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public List<Piece> Pieces { get; set; }

        public List<Lane> Lanes { get; set; }

        public StartingPoint StartingPoint { get; set; }
    }

    public class Piece
    {
        public double Length { get; set; }

        public bool? Switch { get; set; }

        public int? Radius { get; set; }

        public double? Angle { get; set; }
    }

    public class Lane
    {
        public int DistanceFromCenter { get; set; }

        public int Index { get; set; }
    }

    public class StartingPoint
    {
        public Position Position { get; set; }

        public double Angle { get; set; }
    }

    public class Position
    {
        public double X { get; set; }

        public double Y { get; set; }
    }

    public class Car
    {
        public CarId Id { get; set; }

        public Dimensions Dimensions { get; set; }
    }

    public class CarId
    {
        public string Name { get; set; }

        public string Color { get; set; }
    }

    public class Dimensions
    {
        public double Length { get; set; }

        public double Width { get; set; }

        public double GuideFlagPosition { get; set; }
    }

    public class RaceSession
    {
        public int Laps { get; set; }

        public int MaxLapTimeMs { get; set; }

        public bool QuickRace { get; set; }

        public int DurationMs { get; set; }
    }

    public class Race
    {
        public Track Track { get; set; }

        public List<Car> Cars { get; set; }

        public RaceSession RaceSession { get; set; }
    }

    #endregion gameInit

    #region carPositions

    public class CarPositionsRoot : TickRootObject<List<PositionData>>
    { }

    public class PiecePosition
    {
        public int PieceIndex { get; set; }

        public double InPieceDistance { get; set; }

        public Lane Lane { get; set; }

        public int Lap { get; set; }
    }

    public class PositionData
    {
        public CarId Id { get; set; }

        public double Angle { get; set; }

        public PiecePosition PiecePosition { get; set; }
    }

    #endregion carPositions

    #region gameEnd

    public class GameEndRoot : RootObject<EndData>
    { }

    public class RaceInfo
    {
        public int Laps { get; set; }

        public int Ticks { get; set; }

        public int Millis { get; set; }
    }

    public class LapInfo
    {
        public int Lap { get; set; }

        public int Ticks { get; set; }

        public int Millis { get; set; }
    }

    public class RaceResult
    {
        public CarId Car { get; set; }

        public RaceInfo Result { get; set; }
    }

    public class BestLap
    {
        public CarId Car { get; set; }

        public LapInfo Result { get; set; }
    }

    public class EndData
    {
        public List<RaceResult> Results { get; set; }

        public List<BestLap> BestLaps { get; set; }
    }

    #endregion gameEnd

    #region lapFinished

    public class LapFinishedRoot : TickRootObject<LapData>
    { }

    public class LapData
    {
        public CarId Car { get; set; }

        public LapInfo LapTime { get; set; }

        public RaceInfo RaceTime { get; set; }

        public Ranking Ranking { get; set; }
    }

    public class Ranking
    {
        public int Overall { get; set; }

        public int FastestLap { get; set; }
    }

    #endregion lapFinished

    #region dnf

    public class DnfRoot : TickRootObject<DnfData>
    { }

    public class DnfData
    {
        public CarId Car { get; set; }

        public string Reason { get; set; }
    }

    #endregion dnf

    #region turboAvailable

    public class TurboAvailableRoot : RootObject<TurboData>
    { }

    public class TurboData
    {
        public double TurboDurationMilliseconds { get; set; }

        public int TurboDurationTicks { get; set; }

        public double TurboFactor { get; set; }
    }

    #endregion turboAvailable

    #region crash

    public class CrashRoot : TickRootObject<CarId>
    { }

    #endregion crash

    #region spawn

    public class SpawnRoot : TickRootObject<CarId>
    {
    }

    #endregion spawn
}