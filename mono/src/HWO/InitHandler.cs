using System;
using System.Collections.Generic;
using System.Linq;

namespace HWO
{
    public class InitializeSpeed
    {
        private double _limit;
        private double _deceleration;
        private double _deleclerationTick;

        public InitializeSpeed(double limit)
        {
            _limit = limit;
        }

        public double StartSpeed { get; set; }

        public double TargetMaxSpeed { get { return StartSpeed + _limit; } }

        public double TargetLowSpeed { get { return StartSpeed + (_limit / 2); } }

        public double StartTick { get; set; }

        public double AccelerationTick { get; set; }

        public double SlideTick { get; set; }

        public double SlideSpeed { get; set; }

        public double DecelerationTick
        {
            get { return _deleclerationTick; }
            set
            {
                _deleclerationTick = value;
                _deceleration = CountDeceleration();
            }
        }

        public double Deceleration { get { return _deceleration; } }

        private double CountDeceleration()
        {
            var amount = SlideSpeed - TargetLowSpeed;
            var time = DecelerationTick - SlideTick;

            return amount / time;
        }
    }

    public class CrashInfo
    {
        public CrashInfo()
        {
            Infos = new List<Tuple<double, double, double>>();
        }

        public double StartTick { get; set; }

        public double CrashTick { get; set; }

        public List<Tuple<double, double, double>> Infos { get; set; }
    }

    public class InitHandler
    {
        private int _currentThrottleIndex;
        private int _currentLimitIndex;
        private double _initialSpeed = 0;

        private double[] _throttles = new double[] { 1, 0.5 };

        private double[] _limits = new double[] { 0.2, 0.8 };

        public Dictionary<int, List<InitializeSpeed>> Speeds = new Dictionary<int, List<InitializeSpeed>>();

        public InitHandler()
        {
            for (int i = 0; i < _throttles.Count(); i++)
            {
                Speeds.Add(i, new List<InitializeSpeed>());

                foreach (var limit in _limits)
                    Speeds[i].Add(new InitializeSpeed(limit));
            }

            CrashInfo = new CrashInfo();
        }

        public InitializeSpeed Item
        {
            get { return Speeds[_currentThrottleIndex][_currentLimitIndex]; }
        }

        public CrashInfo CrashInfo { get; set; }

        public double GetThrottle()
        {
            return _throttles[_currentThrottleIndex];
        }

        public double GetNextUpTarget()
        {
            return _limits[_currentLimitIndex] + _initialSpeed;
        }

        public double GetNextDownTarget()
        {
            return _limits[_currentLimitIndex] / 2 + _initialSpeed;
        }

        public bool SetNextStage(double initialSpeed)
        {
            _currentThrottleIndex++;
            _currentLimitIndex = 0;
            _initialSpeed = initialSpeed;

            if (_throttles.Count() < _currentThrottleIndex + 1)
                return false;

            Item.StartSpeed = initialSpeed;

            return true;
        }

        public bool SetNextTargets(double initialSpeed)
        {
            _currentLimitIndex++;
            _initialSpeed = initialSpeed;

            if (_limits.Count() < _currentLimitIndex + 1)
                return false;

            Item.StartSpeed = initialSpeed;

            return true;
        }

        public double GetDecelerationPerTick()
        {
            // Return how much speed decreased from slide end to target low
            return Speeds.Values.SelectMany(s => s).Select(s => s.Deceleration).Average();
        }

        public double GetSlowDownNumber()
        {
            // Took 0.2 away so make this little smaller
            return Speeds.Values.SelectMany(s => s).Select(s => s.DecelerationTick).Average() * 0.9;
        }

        public double GetAngleModifier()
        {
            /*
             Speed: [5.11180599642066] Angle: [0] Throttle: [1] Piece: [45]
    Speed: [7.22863936956734] Angle: [0.723889005755153] Throttle: [1] Piece: [45]
    Speed: [7.28406658217599] Angle: [2.12130058231123] Throttle: [1] Piece: [45]
    Speed: [7.33838525053247] Angle: [4.14044786824763] Throttle: [1] Piece: [45]
    Speed: [7.39161754552182] Angle: [6.72859721937124] Throttle: [1] Piece: [45]
    Speed: [7.44378519461138] Angle: [9.83252315623545] Throttle: [1] Piece: [45]
    Speed: [7.49490949071915] Angle: [13.3989416544005] Throttle: [1] Piece: [45]
    Speed: [7.54501130090477] Angle: [17.3749191569781] Throttle: [1] Piece: [45]
    Speed: [7.59411107488668] Angle: [21.7082550416681] Throttle: [1] Piece: [45]
    Speed: [15.4962104873634] Angle: [26.347835634527] Throttle: [1] Piece: [45]
    Speed: [7.68938427632116] Angle: [31.2439582255972] Throttle: [1] Piece: [45]
    Speed: [7.73559659079474] Angle: [36.348623902975] Throttle: [1] Piece: [45]
    Speed: [7.78088465897885] Angle: [41.6157983778668] Throttle: [1] Piece: [45]
    Speed: [7.82526696579927] Angle: [47.0016403199079] Throttle: [1] Piece: [45]
    Speed: [7.86876162648328] Angle: [52.4646970560673] Throttle: [1] Piece: [45]
    Speed: [7.91138639395362] Angle: [57.9660678047321] Throttle: [1] Piece: [45]
             * */

            return AngleMod;
        }

        public double AngleMod { get; set; }

        public double CMax { get; set; }
    }
}