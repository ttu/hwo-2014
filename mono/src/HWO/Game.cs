using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HWO
{
    public class Game
    {
        private Guid _gameId;

        private CarId _car;
        private Race _race;

        private ThrottleDefiner _throttleDefiner;
        private CurrentState _state;

        public ConcurrentQueue<IRootObject> _updates = new ConcurrentQueue<IRootObject>();

        private double _longestStraight;

        private Task _updateTask;
        private CancellationTokenSource _cls;
        private AutoResetEvent _tick;

        public Game()
        {
            _cls = new CancellationTokenSource();
            _tick = new AutoResetEvent(false);
        }

        public CarId MyCar { get { return _car; } }

        public Race Race
        {
            get { return _race; }
            private set
            {
                _race = value;
                _longestStraight = _race.Track.GetLongestStraigthLength();
            }
        }

        public event EventHandler<BotMsg> NewMessage;

        public void SetCar(CarId car)
        {
            _car = car;
        }

        public void Initialize(Guid gameId, Race race)
        {
            if (_gameId != gameId)
            {
                _gameId = gameId;
                _throttleDefiner = new ThrottleDefiner();
                _state = new CurrentState();
                Race = race;
            }

            _updates = new ConcurrentQueue<IRootObject>();
            _cls = new CancellationTokenSource();
        }

        public void Start()
        {
            _updateTask = Task.Factory.StartNew(UpdateProcess, _cls.Token);
            _tick.Set();
        }

        public void End()
        {
            _updates = new ConcurrentQueue<IRootObject>();
            _cls.Cancel();
            _tick.Set();
        }

        public void AddUpdate(IRootObject root)
        {
            _updates.Enqueue(root);
        }

        private void UpdateProcess(object t)
        {
            var token = (CancellationToken)t;

            while (token.IsCancellationRequested == false)
            {
                _tick.WaitOne();

                if (token.IsCancellationRequested)
                    break;

                var toProcess = new List<IRootObject>();

                IRootObject rObject;
                while (_updates.TryDequeue(out rObject))
                    toProcess.Add(rObject);

                foreach (var root in toProcess)
                {
                    Handle(root as dynamic);
                }

                SendNextMessage();
            }
        }

        public void ReceivedCarPositions()
        {
            _tick.Set();
        }

        public void Handle(CarPositionsRoot pos)
        {
            UpdateGameState(pos);
        }

        public void Handle(SpawnRoot spawn)
        {
            if (spawn.Data.Color == _car.Color)
                _state.Spawn();
        }

        public void Handle(CrashRoot crash)
        {
            if (crash.Data.Color == _car.Color)
                _state.Crash(_state.CurrentPiece.GetAngle());
        }

        public void Handle(TurboAvailableRoot turbo)
        {
            _state.TurboAvailable(turbo.Data);
        }

        public void Handle(object unknown)
        {
            throw new Exception("Unknown IRootObject");
        }

        /// <summary>
        /// </summary>
        /// <param name="myPos"></param>
        /// <returns>
        /// 0  : Do not switch
        /// -1 : Switch to left
        /// 1  : Switch to right
        /// </returns>
        public int GetSwitchLane()
        {
            var nextCornerIndex = _race.Track.GetNextCornerIndex(_state.CurrentPosition.PiecePosition.PieceIndex);
            var nextSwitchIndex = _race.Track.GetNextSwitchIndex(_state.CurrentPosition.PiecePosition.PieceIndex);

            if (nextSwitchIndex > nextCornerIndex || nextSwitchIndex == _state.SwitchingOnIndex)
                return 0;

            if (nextSwitchIndex == nextCornerIndex)
            {
                // Switch is on the same index as corner so ignore it
                nextCornerIndex = _race.Track.GetNextCornerIndex(nextSwitchIndex);
            }

            var moreCornersTo = _race.Track.CalculateCorners(nextSwitchIndex, nextCornerIndex);

            var laneDistance = _state.CurrentPosition.PiecePosition.Lane.DistanceFromCenter;

            if (moreCornersTo > 0) // Turns to right, so should be in the right lane
            {
                if (laneDistance != _race.Track.Lanes.Max(l => l.DistanceFromCenter))
                {
                    _state.SwitchingOnIndex = nextSwitchIndex;
                    return 1;
                }
            }
            else if (moreCornersTo < 0)
            {
                if (laneDistance != _race.Track.Lanes.Min(l => l.DistanceFromCenter))
                {
                    _state.SwitchingOnIndex = nextSwitchIndex;
                    return -1;
                }
            }

            return 0;
        }

        private double GetNextThrottle()
        {
            var nextCornerIndex = _race.Track.GetNextCornerIndex(_state.CurrentPosition.PiecePosition.PieceIndex);
            var nextCorner = _race.Track.Pieces[nextCornerIndex];

            var speed = _throttleDefiner.GetNextThrottle(
                _state,
                _state.CurrentPosition.PiecePosition.InPieceDistance,
                _state.CurrentPiece.GetAngle(),
                _state.CurrentPiece.GetLength(_state.CurrentPosition.PiecePosition.Lane.DistanceFromCenter),
                nextCorner.GetAngle(),
                _race.Track.DistanceToPiece(_state.CurrentPosition.PiecePosition.PieceIndex, nextCornerIndex));

            return speed;
        }

        private bool GetUseTurbo()
        {
            if (_state.Turbo == null)
                return false;

            int startIndex = 0;
            double length = 0;
            int nextCorner = 0;

            _race.Track.GetNextStraigth(_state.CurrentPosition.PiecePosition.PieceIndex, out startIndex, out length, out nextCorner);

            // TODO: Maybe should check current position if in the end of the previous
            if (_state.CurrentPosition.PiecePosition.PieceIndex != startIndex)
                return false;

            if (length >= _longestStraight)
            {
                _state.TurboAvailable(null);
                return true;
            }

            return false;
        }

        private PositionData GetOwnPositionData(List<PositionData> list)
        {
            foreach (PositionData pos in list)
            {
                if (pos.Id.Color == _car.Color)
                    return pos;
            }

            return null;
        }

        private void UpdateGameState(CarPositionsRoot pos)
        {
            var myPos = GetOwnPositionData(pos.Data);

            if (myPos == null)
                throw new Exception("Position missing");

            var currentPiece = _race.Track.Pieces[myPos.PiecePosition.PieceIndex];
            var nextPiece = _race.Track.GetNextPiece(myPos.PiecePosition.PieceIndex);

            _state.Update(pos.GameTick, myPos, currentPiece, nextPiece);
        }

        private void SendNextMessage()
        {
            BotMsg message = null;

            var sw = GetSwitchLane();

            if (sw == 0)
            {
                var turbo = GetUseTurbo();

                if (turbo == false)
                {
                    // Always send at least throttle
                    var throttle = GetNextThrottle();
                    message = new Throttle(throttle, _state.CurrentTick);

                    Debug.WriteLine("Speed: [{0}] Angle: [{1}] Throttle: [{2}] Piece: [{3}]",
                              _state.CurrentSpeed,
                              _state.CurrentAngle,
                              throttle,
                              _state.CurrentPiece.Angle);
                }
                else
                {
                    message = new Turbo("WAWAWAW!", _state.CurrentTick);
                    Console.WriteLine("Turbo!");
                }
            }
            else
            {
                message = new SwitchLane(sw, _state.CurrentTick);
            }

            var handler = NewMessage;

            if (handler != null)
            {
                NewMessage(this, message);
            }
        }
    }
}