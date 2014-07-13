using System;
using System.Linq;

namespace HWO
{
    public class ThrottleDefiner
    {
        private double _firstMovementTick;

        private Func<CurrentState, double, double, double, double, double, double> _getThrottle;

        private InitHandler _initHandler;
    
        public ThrottleDefiner()
        {
            _initHandler = new InitHandler();
            _getThrottle = GetInitializeThrottle;
        }

        // TODO: most of the arguments come from CurrentState, so they should be removed.
        public double GetNextThrottle(CurrentState state, double currentPieceTravel, double currentPieceAngle,
            double currentPieceLength, double nextPieceAngle, double distanceToCorner)
        {
            return _getThrottle(state, currentPieceTravel, currentPieceAngle, currentPieceLength, nextPieceAngle, distanceToCorner);
        }

        public double GetModifier(double seed, double currentSpeed, double maxSpeed, double angle, double angleModifier)
        {
            var retVal = (seed + (maxSpeed - currentSpeed)) / ((angle / 90d) * angleModifier);

            return retVal > 1 ? 1 : retVal;
        }

        private double GetInitializeThrottle(CurrentState state, double currentPieceTravel, double currentPieceAngle,
           double currentPieceLength, double nextPieceAngle, double distanceToCorner)
        {
            // TODO: Fix this if hell

            if (state.PreviousTick == 0)
                return _initHandler.GetThrottle();

            if (state.PreviousTick > 0 && state.CurrentSpeed > 0 && _firstMovementTick == 0)
            {
                _initHandler.Item.StartTick = state.PreviousTick;
                _firstMovementTick = state.PreviousTick;
            }

            // Get Acceleration
            if (Math.Round(state.CurrentSpeed, 1) >= _initHandler.GetNextUpTarget() &&
                _initHandler.Item.AccelerationTick == 0)
            {
                _initHandler.Item.AccelerationTick = state.PreviousTick - _initHandler.Item.StartTick;
                return 0;
            }

            // Get Slide
            if (_initHandler.Item.SlideTick == 0 && _initHandler.Item.AccelerationTick > 0 && state.Acceleration <= 0)
            {
                _initHandler.Item.SlideTick = state.PreviousTick - _initHandler.Item.AccelerationTick - _initHandler.Item.StartTick;
                _initHandler.Item.SlideSpeed = state.CurrentSpeed;
                return 0;
            }

            // Get Deceleration
            if (_initHandler.Item.SlideTick > 0 && state.CurrentSpeed < _initHandler.GetNextDownTarget())
            {
                _initHandler.Item.DecelerationTick = state.PreviousTick - _initHandler.Item.AccelerationTick - _initHandler.Item.StartTick;

                if (_initHandler.SetNextTargets(state.CurrentSpeed))
                {
                    _initHandler.Item.StartTick = state.PreviousTick;
                }
                else if (_initHandler.SetNextStage(state.CurrentSpeed))
                {
                    _initHandler.Item.StartTick = state.PreviousTick;
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

        private double GetCrashThrottle(CurrentState state, double currentPieceTravel, double currentPieceAngle,
          double currentPieceLength, double nextPieceAngle, double distanceToCorner)
        {
            if (state.CurrentSpeed == 0)
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
                Console.WriteLine("CMax 45: {0} 22.5: {1}", GetCornerMax(45, 1), GetCornerMax(22.5, 1));

                _getThrottle = GetNormalThrottle;
            }

            if (currentPieceAngle > 0 && state.CurrentAngle > 0)
                _initHandler.CrashInfo.Infos.Add(new Tuple<double, double, double>(state.CurrentSpeed, state.CurrentAngle, currentPieceAngle));

            return 1;
        }

        private double GetWaitThrottle(CurrentState state, double currentPieceTravel, double currentPieceAngle,
         double currentPieceLength, double nextPieceAngle, double distanceToCorner)
        {
            return 0;
        }

        private double GetNormalThrottle(CurrentState state, double currentPieceTravel, double currentPieceAngle,
            double currentPieceLength, double nextCorner, double distanceToCorner)
        {
            var distanceToEnd = currentPieceLength - currentPieceTravel + distanceToCorner;

            if (currentPieceAngle > 0)
            {
                // If over 50% way of the piece, we can increase speed
                // TODO: this should be included (currentPieceAngle < nextCorner), now next piece can be same!
                if (currentPieceTravel > (currentPieceLength / 2))
                {
                    return GetExitSpeed(state, distanceToEnd, nextCorner);
                }

                return GetCorneringSpeed(state, currentPieceAngle);
            }

            return GetStraightSpeed(state, distanceToEnd, nextCorner);
        }

        private double GetStraightSpeed(CurrentState state, double distance, double angle)
        {
            double maxSpeed = GetCornerMax(angle, state.SpeedModifier);

            if (state.CurrentSpeed < maxSpeed)
                return 1;

            var thisMuchTime = distance / state.CurrentSpeed;

            var thisMuchPerTick = _initHandler.GetDecelerationPerTick();

            var timeToSlowDown = distance / thisMuchPerTick;

            if (thisMuchTime > timeToSlowDown)
                return 1;
            else
                return 0;
        }

        private double GetExitSpeed(CurrentState state, double distance, double angle)
        {
            double maxSpeed = GetCornerMax(angle, state.SpeedModifier);

            var modifier = 1d;

            if (state.CurrentSpeed >= (maxSpeed - _initHandler.GetAngleModifier() / 20))
            {
                var t = state.CurrentSpeed - (maxSpeed - _initHandler.GetAngleModifier() / 20);

                modifier = Math.Round((distance / _initHandler.GetSlowDownNumber()) / (t * t * t), 2);
            }

            modifier = GetModifier(1.1, state.CurrentSpeed, maxSpeed, Math.Round(state.CurrentAngle, 0), _initHandler.GetAngleModifier());

            if (modifier > 1)
                modifier = 1;
            else if (modifier < 0)
                modifier = 0;

            return modifier;
        }

        private double GetCorneringSpeed(CurrentState state, double angle)
        {
            double maxSpeed = GetCornerMax(angle, state.SpeedModifier);

            var modifier = 1d;

            if (state.CurrentSpeed >= maxSpeed - _initHandler.GetAngleModifier() / 10)
            {
                return 0;
            }

            modifier = GetModifier(1.1, state.CurrentSpeed, maxSpeed, state.CurrentAngle, _initHandler.GetAngleModifier());

            return modifier;
        }

        private double GetCornerMax(double angle, double speedModifier)
        {
            return _initHandler.CMax * (45d / angle) * speedModifier;
        }

      
    }
}