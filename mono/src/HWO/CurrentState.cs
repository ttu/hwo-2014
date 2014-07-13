using System;

namespace HWO
{
    // Fix what should be passed as arguments and when to use private variables

    public class CurrentState
    {
        private double _prevAngle;

        public CurrentState()
        {
            this.SpeedModifier = 1.1d;

        }
        public int PreviousTick { get; set; }

        public int CurrentTick { get; set; }

        public PositionData CurrentPosition { get; set; }

        public PositionData PreviousPosition { get; set; }

        public Piece PreviousPiece { get; set; }

        public Piece CurrentPiece { get; set; }

        public Piece NextPiece { get; set; }

        public double CurrentSpeed { get; private set; }

        public double Acceleration { get; private set; }

        public double PreviousAngle { get; private set; }

        public double CurrentAngle { get; private set; }

        public double AngleAcceleration { get; private set; }

        public TurboData Turbo { get; set; }

        public bool IsCrashed { get; set; }

        public double SpeedModifier { get; private set; }

        // If only 1 switch on track, this won't work
        // This should be set to -1 after switch has been done
        public int SwitchingOnIndex { get; set; }

        public void Crash(double currentPieceAngle)
        {
            this.Turbo = null;

            this.IsCrashed = true;
            this.SpeedModifier -= 0.1;

            Console.WriteLine("Crash: a:{0} s:{1}", currentPieceAngle, this.CurrentSpeed);
        }

        public void Spawn()
        {
            this.IsCrashed = false;
        }

        public void TurboAvailable(TurboData turbo)
        {
            if (turbo != null && this.IsCrashed)
                return;

            this.Turbo = turbo;
        }

        public void Update(int currentTick, PositionData myPos, Piece currentPiece, Piece nextPiece)
        {
            this.PreviousTick = this.CurrentTick;
            this.CurrentTick = currentTick;

            this.CurrentPosition = myPos;

            this.CurrentPiece = currentPiece;
            this.NextPiece = nextPiece;

            var distance = GetTraveledDistance();

            UpdateSpeedAndAngle(distance, myPos.Angle, this.CurrentTick);

            this.PreviousPiece = this.CurrentPiece;
            this.PreviousPosition = this.CurrentPosition;
        }

        public void UpdateSpeedAndAngle(double distance, double currentAngle, int currentTick)
        {
            if (currentTick == 0)
            {
                _prevAngle = this.CurrentAngle;
                return;
            }

            var ticks = currentTick - PreviousTick;
            PreviousTick = currentTick;

            var newSpeed = distance / ticks;
            this.Acceleration = newSpeed - this.CurrentSpeed;
            this.CurrentSpeed = newSpeed;

            this.CurrentAngle = Math.Abs(currentAngle);
            this.AngleAcceleration = (currentAngle - _prevAngle) / ticks;
            _prevAngle = this.CurrentAngle;
        }

        public double GetTraveledDistance()
        {
            var distance = this.CurrentPosition.PiecePosition.InPieceDistance;

            if (this.PreviousPosition != null)
            {
                if (this.CurrentPosition.PiecePosition.PieceIndex != this.PreviousPosition.PiecePosition.PieceIndex)
                {
                    var endOfPreviousPiece = this.PreviousPiece.GetLength(this.CurrentPosition.PiecePosition.Lane.DistanceFromCenter) -
                        this.PreviousPosition.PiecePosition.InPieceDistance;

                    distance += endOfPreviousPiece;
                }
                else
                {
                    distance -= this.PreviousPosition.PiecePosition.InPieceDistance;
                }
            }

            return distance;
        }
    }
}