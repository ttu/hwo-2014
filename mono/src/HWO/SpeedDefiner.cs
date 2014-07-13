using System;
using System.Linq;

namespace HWO
{
    public class SpeedDefiner
    {
        //private Dictionary<double, double> _maxSpeeds = new Dictionary<double, double>();

        private double _firstMovementTick;

        private double _prevAngle;
        private double _prevTick;

        private Func<double, double, double, double, double, double> _getThrottle;

        private InitHandler _initHandler;
        private TurboData _turbo;
        private bool _isCashed;

        public SpeedDefiner()
        {
            _initHandler = new InitHandler();
            _getThrottle = GetInitializeThrottle;
            SpeedModifier = 1.1;
            //_getThrottle = GetNormalThrottle;

            // For Keimola debugging purposes
            //_maxSpeeds.Add(45, 8.115545636029978);
        }

        public double CurrentSpeed { get; private set; }

        public double Acceleration { get; private set; }

        public double CurrentAngle { get; private set; }

        public double AngleAcceleration { get; private set; }

        public double SpeedModifier { get; private set; }

        public TurboData Turbo { get { return _turbo; } }

        public void UpdateSpeedAndAngle(double distance, double currentAngle, int currentTick)
        {
            if (currentTick == 0) // TODO: Should there be some init function for the first tick?
            {
                _prevAngle = CurrentAngle;
                return;
            }

            var ticks = currentTick - _prevTick;
            _prevTick = currentTick;

            var newSpeed = distance / ticks;
            this.Acceleration = newSpeed - this.CurrentSpeed;
            this.CurrentSpeed = newSpeed;

            this.CurrentAngle = Math.Abs(currentAngle);
            this.AngleAcceleration = (currentAngle - _prevAngle) / ticks;
            _prevAngle = this.CurrentAngle;
        }

        public void Crash(double currentPieceAngle)
        {
            _isCashed = true;
            //_getThrottle = GetWaitThrottle;

            this.SpeedModifier -= 0.1;

            if (this.CurrentSpeed < 0)
                return;

            Console.WriteLine("Crash: a:{0} s:{1}", currentPieceAngle, this.CurrentSpeed);
        }

        public void Spawn()
        {
            _isCashed = false;
            //_getThrottle = GetNormalThrottle;
        }

        public double GetNextThrottle(double currentPieceTravel, double currentPieceAngle,
            double currentPieceLength, double nextPieceAngle, double distanceToCorner)
        {
            return _getThrottle(currentPieceTravel, currentPieceAngle, currentPieceLength, nextPieceAngle, distanceToCorner);
        }

        private double GetInitializeThrottle(double currentPieceTravel, double currentPieceAngle,
           double currentPieceLength, double nextPieceAngle, double distanceToCorner)
        {
            // TODO: Fix this if hell

            if (_prevTick == 0)
                return _initHandler.GetThrottle();

            if (_prevTick > 0 && this.CurrentSpeed > 0 && _firstMovementTick == 0)
            {
                _initHandler.Item.StartTick = _prevTick;
                _firstMovementTick = _prevTick;
            }

            // Get Acceleration
            if (Math.Round(this.CurrentSpeed, 1) >= _initHandler.GetNextUpTarget() &&
                _initHandler.Item.AccelerationTick == 0)
            {
                _initHandler.Item.AccelerationTick = _prevTick - _initHandler.Item.StartTick;
                return 0;
            }

            // Get Slide
            if (_initHandler.Item.SlideTick == 0 && _initHandler.Item.AccelerationTick > 0 && this.Acceleration <= 0)
            {
                _initHandler.Item.SlideTick = _prevTick - _initHandler.Item.AccelerationTick - _initHandler.Item.StartTick;
                _initHandler.Item.SlideSpeed = this.CurrentSpeed;
                return 0;
            }

            // Get Deceleration
            if (_initHandler.Item.SlideTick > 0 && this.CurrentSpeed < _initHandler.GetNextDownTarget())
            {
                _initHandler.Item.DecelerationTick = _prevTick - _initHandler.Item.AccelerationTick - _initHandler.Item.StartTick;

                if (_initHandler.SetNextTargets(this.CurrentSpeed))
                {
                    _initHandler.Item.StartTick = _prevTick;
                }
                else if (_initHandler.SetNextStage(this.CurrentSpeed))
                {
                    _initHandler.Item.StartTick = _prevTick;
                }
                else
                {
                    Console.WriteLine("Slowdown: {0}", _initHandler.GetSlowDownNumber());
                    Console.WriteLine("Deceleration: {0}", _initHandler.GetDecelerationPerTick());

                    _getThrottle = GetCrashThrottle;
                    return 1;
                }
            }

            if (_initHandler.Item.AccelerationTick != 0 && _initHandler.Item.DecelerationTick == 0)
                return 0;

            return _initHandler.GetThrottle();
        }

        private double GetCrashThrottle(double currentPieceTravel, double currentPieceAngle,
          double currentPieceLength, double nextPieceAngle, double distanceToCorner)
        {
            if (this.CurrentSpeed == 0)
            {
                var items = _initHandler.CrashInfo.Infos;
                var slipperiness = items.Select(t => t.Item2).Average();

                // * (slipperiness / items.Count())
                _initHandler.AngleMod = slipperiness / (items.Select(t => t.Item3).Average() / 45d); ;
                // Just in case skip largest
                var last5 = items.Select(t => t.Item1).Reverse().Take(5);
                var ordered = last5.OrderByDescending(k => k);
                var no2 = ordered.Skip(2);
                _initHandler.CMax = no2.Max() * (items.Select(t => t.Item3).Average() / 45d);

                Console.WriteLine("CMax: {0} AMod: {1}", _initHandler.CMax, _initHandler.AngleMod);
                Console.WriteLine("CMax 45: {0} 22.5: {1}", GetCornerMax(45), GetCornerMax(22.5));

                _getThrottle = GetNormalThrottle;
            }

            if (currentPieceAngle > 0 && CurrentAngle > 0)
                _initHandler.CrashInfo.Infos.Add(new Tuple<double, double, double>(this.CurrentSpeed, this.CurrentAngle, currentPieceAngle));

            return 1;
        }

        private double GetWaitThrottle(double currentPieceTravel, double currentPieceAngle,
         double currentPieceLength, double nextPieceAngle, double distanceToCorner)
        {
            return 0;
        }

        private double GetNormalThrottle(double currentPieceTravel, double currentPieceAngle,
            double currentPieceLength, double nextCorner, double distanceToCorner)
        {
            var distanceToEnd = currentPieceLength - currentPieceTravel + distanceToCorner;

            if (currentPieceAngle > 0)
            {
                // If over 50% way of the piece, we can increase speed
                // TODO: this should be included (currentPieceAngle < nextCorner), now next piece can be same!
                if (currentPieceTravel > (currentPieceLength / 2))
                {
                    return GetExitSpeed(this, distanceToEnd, nextCorner);
                }

                return GetCorneringSpeed(this, currentPieceAngle);
            }

            return GetStraightSpeed(this, distanceToEnd, nextCorner);
        }

        public double GetStraightSpeed(SpeedDefiner speeds, double distance, double angle)
        {
            double maxSpeed = GetCornerMax(angle);

            if (speeds.CurrentSpeed < maxSpeed)
                return 1;

            var thisToReduce = speeds.CurrentSpeed - maxSpeed;

            var thisMuchTime = distance / speeds.CurrentSpeed;

            var thisMuchPerTick = _initHandler.GetDecelerationPerTick();

            var timeToSlowDown = distance / thisMuchPerTick;

            if (thisMuchTime > timeToSlowDown)
                return 1;
            else
                return 0;
        }

        public double GetExitSpeed(SpeedDefiner speeds, double distance, double angle)
        {
            double maxSpeed = GetCornerMax(angle);

            var modifier = 1d;

            if (speeds.CurrentSpeed >= (maxSpeed - _initHandler.GetAngleModifier() / 20))
            {
                var t = speeds.CurrentSpeed - (maxSpeed - _initHandler.GetAngleModifier() / 20);

                modifier = Math.Round((distance / _initHandler.GetSlowDownNumber()) / (t * t * t), 2);
            }

            modifier = GetModifier(1.1, speeds.CurrentSpeed, maxSpeed, Math.Round(speeds.CurrentAngle, 0), _initHandler.GetAngleModifier());

            if (modifier > 1)
                modifier = 1;
            else if (modifier < 0)
                modifier = 0;

            return modifier;
        }

        public double GetCorneringSpeed(SpeedDefiner speeds, double angle)
        {
            double maxSpeed = GetCornerMax(angle);

            var modifier = 1d;

            if (speeds.CurrentSpeed >= maxSpeed - _initHandler.GetAngleModifier() / 10)
            {
                return 0;
            }

            modifier = GetModifier(1.1, speeds.CurrentSpeed, maxSpeed, speeds.CurrentAngle, _initHandler.GetAngleModifier());

            return modifier;
        }

        public double GetModifier(double seed, double currentSpeed, double maxSpeed, double angle, double angleModifier)
        {
            var retVal = (seed + (maxSpeed - currentSpeed)) / ((angle / 90d) * angleModifier);

            return retVal > 1 ? 1 : retVal;
        }

        private double GetCornerMax(double angle)
        {
            return _initHandler.CMax * (45d / angle) * SpeedModifier;
        }

        public void TurboAvailable(TurboData turbo)
        {
            if (turbo != null && _isCashed)
                return;

            _turbo = turbo;
        }
    }
}