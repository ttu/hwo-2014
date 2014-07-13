using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace HWO.Test
{
    [TestClass]
    public class GameTests
    {
        [TestMethod]
        public void KeimolaTest()
        {
            var car = JsonConvert.DeserializeObject<YourCarRoot>(SampleDatas.YourCarOwn);

            var keimola = JsonConvert.DeserializeObject<GameInitRoot>(SampleDatas.GameInitKeimola);

            var game = new Game();
            game.SetCar(car.Data);
            game.Initialize(keimola.GameId, keimola.Data.Race);

            var p1 = new PositionData { PiecePosition = new PiecePosition { InPieceDistance = 0, PieceIndex = 0, Lane = new Lane() }, Id = car.Data };
            var pr1 = new CarPositionsRoot { Data = new List<PositionData> { p1 }, GameTick = 0 };

            game.Handle(pr1);

            var p2 = new PositionData { PiecePosition = new PiecePosition { InPieceDistance = 10, PieceIndex = 1, Lane = new Lane() }, Id = car.Data };
            var pr2 = new CarPositionsRoot { Data = new List<PositionData> { p2 }, GameTick = 3 };

            game.Handle(pr2);
        }

        [TestMethod]
        public void Keimola_CornerTest()
        {
            var car = JsonConvert.DeserializeObject<YourCarRoot>(SampleDatas.YourCarOwn);

            var keimola = JsonConvert.DeserializeObject<GameInitRoot>(SampleDatas.GameInitKeimola);

            var game = new Game();
            game.SetCar(car.Data);
            game.Initialize(keimola.GameId, keimola.Data.Race);

            var p1 = new PositionData
            {
                PiecePosition = new PiecePosition
                {
                    InPieceDistance = 0,
                    PieceIndex = 0,
                    Lane = new Lane { DistanceFromCenter = -10 }
                },
                Id = car.Data
            };

            var pr1 = new CarPositionsRoot { Data = new List<PositionData> { p1 }, GameTick = 0 };

            game.AddUpdate(pr1);
            game.ReceivedCarPositions();

            var lane = game.GetSwitchLane();

            Assert.AreEqual(1, lane);

            var p2 = new PositionData
            {
                PiecePosition = new PiecePosition
                {
                    InPieceDistance = 0,
                    PieceIndex = 8,
                    Lane = new Lane { DistanceFromCenter = 10 }
                },
                Id = car.Data
            };

            var pr2 = new CarPositionsRoot { Data = new List<PositionData> { p2 }, GameTick = 0 };

            game.AddUpdate(pr2);
            game.ReceivedCarPositions();

            lane = game.GetSwitchLane();

            Assert.AreEqual(0, lane);
        }

        [TestMethod]
        public void Keimola_HasStraight()
        {
            var car = JsonConvert.DeserializeObject<YourCarRoot>(SampleDatas.YourCarOwn);

            var keimola = JsonConvert.DeserializeObject<GameInitRoot>(SampleDatas.GameInitKeimola);

            var game = new Game();
            game.SetCar(car.Data);
            game.Initialize(keimola.GameId, keimola.Data.Race);

            var hasStraights = keimola.Data.Race.Track.HasStraights();

            Assert.IsTrue(hasStraights);
        }

        [TestMethod]
        public void Keimola_NextStraight()
        {
            var car = JsonConvert.DeserializeObject<YourCarRoot>(SampleDatas.YourCarOwn);

            var keimola = JsonConvert.DeserializeObject<GameInitRoot>(SampleDatas.GameInitKeimola);

            var game = new Game();
            game.SetCar(car.Data);
            game.Initialize(keimola.GameId, keimola.Data.Race);

            int index = 0;
            double length = 0;
            int firstCornerIndex = 0;

            keimola.Data.Race.Track.GetNextStraigth(0, out index, out length, out firstCornerIndex);

            Assert.AreEqual(0, index);
            Assert.AreEqual(400, length);

            keimola.Data.Race.Track.GetNextStraigth(34, out index, out length, out firstCornerIndex);

            Assert.AreEqual(35, index);
            Assert.AreEqual(890, length);
            Assert.AreEqual(4, firstCornerIndex);
        }

        [TestMethod]
        public void Keimola_FindLongestStraight()
        {
            var car = JsonConvert.DeserializeObject<YourCarRoot>(SampleDatas.YourCarOwn);

            var keimola = JsonConvert.DeserializeObject<GameInitRoot>(SampleDatas.GameInitKeimola);

            var game = new Game();
            game.SetCar(car.Data);
            game.Initialize(keimola.GameId, keimola.Data.Race);

            int index = 0;
            double length = 0;
            int firstCornerIndex = 0;

            keimola.Data.Race.Track.GetLongestStraigth(out index, out length, out firstCornerIndex);

            Assert.AreEqual(35, index);
            Assert.AreEqual(890, length);
            Assert.AreEqual(4, firstCornerIndex);
        }

        [TestMethod]
        public void Keimola_Corner_LastIndex_SwitchFromLeftToRight()
        {
            var car = JsonConvert.DeserializeObject<YourCarRoot>(SampleDatas.YourCarOwn);

            var keimola = JsonConvert.DeserializeObject<GameInitRoot>(SampleDatas.GameInitKeimola);

            var game = new Game();
            game.SetCar(car.Data);
            game.Initialize(keimola.GameId, keimola.Data.Race);

            var p1 = new PositionData
            {
                PiecePosition = new PiecePosition
                {
                    InPieceDistance = 0,
                    PieceIndex = game.Race.Track.Pieces.Count - 1,
                    Lane = new Lane { DistanceFromCenter = -10 }
                },
                Id = car.Data
            };

            var pr1 = new CarPositionsRoot { Data = new List<PositionData> { p1 }, GameTick = 0 };

            game.AddUpdate(pr1);
            game.ReceivedCarPositions();

            var lane = game.GetSwitchLane();

            Assert.AreEqual(1, lane);
        }

        [TestMethod]
        public void Keimola_CornerAndSwitch_SwitchFromRightToLeft()
        {
            var car = JsonConvert.DeserializeObject<YourCarRoot>(SampleDatas.YourCarOwn);

            var keimola = JsonConvert.DeserializeObject<GameInitRoot>(SampleDatas.GameInitKeimola);

            var game = new Game();
            game.SetCar(car.Data);
            game.Initialize(keimola.GameId, keimola.Data.Race);

            var p1 = new PositionData
            {
                PiecePosition = new PiecePosition
                {
                    InPieceDistance = 0,
                    PieceIndex = 7,
                    Lane = new Lane { DistanceFromCenter = 10 } // Right lane
                },
                Id = car.Data
            };

            var pr1 = new CarPositionsRoot { Data = new List<PositionData> { p1 }, GameTick = 0 };

            game.Handle(pr1);

            var lane = game.GetSwitchLane();

            Assert.AreEqual(-1, lane);
        }

        [TestMethod]
        public void Keimola_Swith_MultipleCorners()
        {
            var car = JsonConvert.DeserializeObject<YourCarRoot>(SampleDatas.YourCarOwn);

            var keimola = JsonConvert.DeserializeObject<GameInitRoot>(SampleDatas.GameInitKeimola);

            var game = new Game();
            game.SetCar(car.Data);
            game.Initialize(keimola.GameId, keimola.Data.Race);

            var p1 = new PositionData
            {
                PiecePosition = new PiecePosition
                {
                    InPieceDistance = 0,
                    PieceIndex = 28,
                    Lane = new Lane { DistanceFromCenter = -10 } // Left lane
                },
                Id = car.Data
            };

            var pr1 = new CarPositionsRoot { Data = new List<PositionData> { p1 }, GameTick = 0 };

            game.Handle(pr1);

            var lane = game.GetSwitchLane();

            Assert.AreEqual(1, lane);
        }

        [TestMethod]
        public void GetCornerPieceLength()
        {
            var arcLength = MathHelper.GetCornerPieceLength(45, 10);
            Assert.AreEqual(8, Math.Round(arcLength));
        }

        [TestMethod]
        public void CarPhysics_UpdateValues()
        {
            var speedDefiner = new CurrentState();

            speedDefiner.UpdateSpeedAndAngle(0, 0, 0);

            speedDefiner.UpdateSpeedAndAngle(100, 10, 2);

            Assert.AreEqual(50, speedDefiner.CurrentSpeed);
            Assert.AreEqual(50, speedDefiner.Acceleration);

            Assert.AreEqual(10, speedDefiner.CurrentAngle);
            Assert.AreEqual(5, speedDefiner.AngleAcceleration);

            speedDefiner.UpdateSpeedAndAngle(40, 40, 3);

            Assert.AreEqual(40, speedDefiner.CurrentSpeed);
            Assert.AreEqual(-10, speedDefiner.Acceleration);

            Assert.AreEqual(40, speedDefiner.CurrentAngle);
            Assert.AreEqual(30, speedDefiner.AngleAcceleration);
        }

        [TestMethod]
        public void CarSpeeds_AngleThrottle()
        {
            var game = new ThrottleDefiner();
            var mod = game.GetModifier(1, 6, 7, 10, 16);

            var mod2 = game.GetModifier(1, 6, 7, 40, 16);
        }

        [TestMethod]
        public void Keimola_Pieces()
        {
            var car = JsonConvert.DeserializeObject<YourCarRoot>(SampleDatas.YourCarOwn);

            var keimola = JsonConvert.DeserializeObject<GameInitRoot>(SampleDatas.GameInitKeimola);

            var game = new Game();
            game.SetCar(car.Data);
            game.Initialize(keimola.GameId, keimola.Data.Race);

            var pieces = keimola.Data.Race.Track.GetPieces(12, 10);

            Assert.IsNotNull(pieces);
        }

        [TestMethod]
        public void Keimola_DistanceToPiece()
        {
            var car = JsonConvert.DeserializeObject<YourCarRoot>(SampleDatas.YourCarOwn);

            var keimola = JsonConvert.DeserializeObject<GameInitRoot>(SampleDatas.GameInitKeimola);

            var game = new Game();
            game.SetCar(car.Data);
            game.Initialize(keimola.GameId, keimola.Data.Race);

            var distance = keimola.Data.Race.Track.DistanceToPiece(12, 14);

            Assert.AreEqual(100, distance);

            var distance2 = keimola.Data.Race.Track.DistanceToPiece(39, 2);

            // piece 0 = 100;
            // piece 1 = 100;
            Assert.AreEqual(200, distance2);
        }

        [TestMethod]
        [Ignore]
        public void Game_AsyncTest()
        {
            var game = new Game();
            game.NewMessage += (s, e) => { Debug.WriteLine(e); };

            var car = JsonConvert.DeserializeObject<YourCarRoot>(SampleDatas.YourCarOwn);
            var keimola = JsonConvert.DeserializeObject<GameInitRoot>(SampleDatas.GameInitKeimola);

            game.SetCar(car.Data);
            game.Initialize(keimola.GameId, keimola.Data.Race);

            var pos = JsonConvert.DeserializeObject<CarPositionsRoot>(SampleDatas.MyCarPositions);
            game.AddUpdate(pos);
            game.ReceivedCarPositions();

            game.Start();

            Thread.Sleep(1000);

            //var sideJob = Task.Factory.StartNew(() =>
            //{
            //    game.AddUpdate(new SpawnRoot() { Data = new CarId() { Color = "red" } });
            //});

            game.ReceivedCarPositions();
            game.End();

            game.Initialize(keimola.GameId, keimola.Data.Race);

            game.Start();

            Thread.Sleep(100);

            game.ReceivedCarPositions();

            while (true)
                Thread.Sleep(4000);
        }
    }
}