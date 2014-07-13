using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace HWO.Test
{
    [TestClass]
    public class ModelsTests
    {
        [TestMethod]
        public void YourCarMsg_CorrectData()
        {
            var data = @"  {'msgType': 'yourCar', 'data': {
                              'name': 'Schumacher',
                              'color': 'red'
                            }}";

            var ycMsg = JsonConvert.DeserializeObject<YourCarRoot>(data);

            Assert.AreEqual("red", ycMsg.Data.Color);
            Assert.AreEqual("Schumacher", ycMsg.Data.Name);
        }

        [TestMethod]
        public void GameInit_CorrectData()
        {
            var sample = @"{'msgType': 'gameInit', 'data': {
                              'race': {
                                'track': {
                                  'id': 'indianapolis',
                                  'name': 'Indianapolis',
                                  'pieces': [
                                    {
                                      'length': 100.0
                                    },
                                    {
                                      'length': 100.0,
                                      'switch': true
                                    },
                                    {
                                      'radius': 200,
                                      'angle': 22.5
                                    }
                                  ],
                                  'lanes': [
                                    {
                                      'distanceFromCenter': -20,
                                      'index': 0
                                    },
                                    {
                                      'distanceFromCenter': 0,
                                      'index': 1
                                    },
                                    {
                                      'distanceFromCenter': 20,
                                      'index': 2
                                    }
                                  ],
                                  'startingPoint': {
                                    'position': {
                                      'x': -340.0,
                                      'y': -96.0
                                    },
                                    'angle': 90.0
                                  }
                                },
                                'cars': [
                                  {
                                    'id': {
                                      'name': 'Schumacher',
                                      'color': 'red'
                                    },
                                    'dimensions': {
                                      'length': 40.0,
                                      'width': 20.0,
                                      'guideFlagPosition': 10.0
                                    }
                                  },
                                  {
                                    'id': {
                                      'name': 'Rosberg',
                                      'color': 'blue'
                                    },
                                    'dimensions': {
                                      'length': 40.0,
                                      'width': 20.0,
                                      'guideFlagPosition': 10.0
                                    }
                                  }
                                ],
                                'raceSession': {
                                  'laps': 3,
                                  'maxLapTimeMs': 30000,
                                  'quickRace': true
                                }
                              }
                            }}";

            var init = JsonConvert.DeserializeObject<GameInitRoot>(sample);

            Assert.AreEqual("Indianapolis", init.Data.Race.Track.Name);
            Assert.AreEqual(20, init.Data.Race.Track.Lanes[2].DistanceFromCenter);
        }

        [TestMethod]
        public void CarPositions_CorrectData()
        {
            var data = @"{'msgType': 'carPositions', 'data': [
                              {
                                'id': {
                                  'name': 'Schumacher',
                                  'color': 'red'
                                },
                                'angle': 0.0,
                                'piecePosition': {
                                  'pieceIndex': 0,
                                  'inPieceDistance': 0.0,
                                  'lane': {
                                    'startLaneIndex': 0,
                                    'endLaneIndex': 0
                                  },
                                  'lap': 0
                                }
                              },
                              {
                                'id': {
                                  'name': 'Rosberg',
                                  'color': 'blue'
                                },
                                'angle': 45.0,
                                'piecePosition': {
                                  'pieceIndex': 0,
                                  'inPieceDistance': 20.0,
                                  'lane': {
                                    'startLaneIndex': 1,
                                    'endLaneIndex': 1
                                  },
                                  'lap': 0
                                }
                              }
                            ], 'gameId': 'OIUHGERJWEOI', 'gameTick': 65}";

            var positions = JsonConvert.DeserializeObject<CarPositionsRoot>(data);

            Assert.AreEqual("Schumacher", positions.Data[0].Id.Name);
            Assert.AreEqual(65, positions.GameTick);
        }

        [TestMethod]
        public void GameEnd_CorrectData()
        {
            var data = @"{'msgType': 'gameEnd', 'data': {
                          'results': [
                            {
                              'car': {
                                'name': 'Schumacher',
                                'color': 'red'
                              },
                              'result': {
                                'laps': 3,
                                'ticks': 9999,
                                'millis': 45245
                              }
                            },
                            {
                              'car': {
                                'name': 'Rosberg',
                                'color': 'blue'
                              },
                              'result': {}
                            }
                          ],
                          'bestLaps': [
                            {
                              'car': {
                                'name': 'Schumacher',
                                'color': 'red'
                              },
                              'result': {
                                'lap': 2,
                                'ticks': 3333,
                                'millis': 20000
                              }
                            },
                            {
                              'car': {
                                'name': 'Rosberg',
                                'color': 'blue'
                              },
                              'result': {}
                            }
                          ]
                        }}";

            var gameEnd = JsonConvert.DeserializeObject<GameEndRoot>(data);

            Assert.AreEqual(3, gameEnd.Data.Results[0].Result.Laps);
            Assert.AreEqual("Rosberg", gameEnd.Data.Results[1].Car.Name);
            Assert.AreEqual(2, gameEnd.Data.BestLaps[0].Result.Lap);
        }

        [TestMethod]
        public void LapFinished_CorrectData()
        {
            var data = @"{'msgType': 'lapFinished', 'data': {
                  'car': {
                    'name': 'Schumacher',
                    'color': 'red'
                  },
                  'lapTime': {
                    'lap': 1,
                    'ticks': 666,
                    'millis': 6660
                  },
                  'raceTime': {
                    'laps': 1,
                    'ticks': 666,
                    'millis': 6660
                  },
                  'ranking': {
                    'overall': 1,
                    'fastestLap': 1
                  }
                }, 'gameId': 'OIUHGERJWEOI', 'gameTick': 300}";

            var lapFinished = JsonConvert.DeserializeObject<LapFinishedRoot>(data);

            Assert.AreEqual("Schumacher", lapFinished.Data.Car.Name);
            Assert.AreEqual(1, lapFinished.Data.LapTime.Lap);
        }

        [TestMethod]
        public void Dnf_CorrectData()
        {
            var data = @"{'msgType': 'dnf', 'data': {
                          'car': {
                            'name': 'Rosberg',
                            'color': 'blue'
                          },
                          'reason': 'disconnected'
                        }, 'gameId': 'OIUHGERJWEOI', 'gameTick': 650}";

            var dnf = JsonConvert.DeserializeObject<DnfRoot>(data);

            Assert.AreEqual("Rosberg", dnf.Data.Car.Name);
            Assert.AreEqual("disconnected", dnf.Data.Reason);
            Assert.AreEqual(650, dnf.GameTick);
        }

        [TestMethod]
        public void TurboAvailable_CorrectData()
        {
            var data = @"{'msgType': 'turboAvailable', 'data': {
                        'turboDurationMilliseconds': 500.0,
                        'turboDurationTicks': 30,
                        'turboFactor': 3.0
                        }}";

            var turbo = JsonConvert.DeserializeObject<TurboAvailableRoot>(data);

            Assert.AreEqual(500, turbo.Data.TurboDurationMilliseconds);
            Assert.AreEqual(30, turbo.Data.TurboDurationTicks);
            Assert.AreEqual(3, turbo.Data.TurboFactor);
        }

        [TestMethod]
        public void Crash_CorrectData()
        {
            var data = @"{'msgType':'crash','data':{'name':'Bit-Bil','color':'red'},'gameId':'11bed212-4891-4ced-986e-9b972bac06dc','gameTick':264}";
            var crash = JsonConvert.DeserializeObject<CrashRoot>(data);

            Assert.AreEqual("red", crash.Data.Color);
            Assert.AreEqual("Bit-Bil", crash.Data.Name);
            Assert.AreEqual(264, crash.GameTick);
        }

        [TestMethod]
        public void Spawn_CorrectData()
        {
            var data = @"{'msgType':'spawn','data':{'name':'Bit-Bil','color':'red'},'gameId':'11bed212-4891-4ced-986e-9b972bac06dc','gameTick':666}";

            var spawn = JsonConvert.DeserializeObject<SpawnRoot>(data);

            Assert.AreEqual("red", spawn.Data.Color);
            Assert.AreEqual("Bit-Bil", spawn.Data.Name);
            Assert.AreEqual(666, spawn.GameTick);
        }
    }
}