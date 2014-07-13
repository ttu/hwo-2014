using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace HWO
{
    public class SimpleBot
    {
        private Game _game;

        private StreamReader _reader;
        private StreamWriter _writer;
        private string _name;
        private string _key;

        public SimpleBot(StreamReader reader, StreamWriter writer, string name, string key)
        {
            _reader = reader;
            _writer = writer;
            _name = name;
            _key = key;

            _game = new Game();
            _game.NewMessage += NewCommand;    
        }

        public void Start()
        {
            string line;

            //Send(new Join(_name, _key));
            Send(new CreateRace(_name, _key, "england"));

            while ((line = _reader.ReadLine()) != null)
            {
                var msg = JsonConvert.DeserializeObject<MsgWrapper>(line);

                switch (msg.msgType)
                {
                    case "carPositions":
                        var pos = JsonConvert.DeserializeObject<CarPositionsRoot>(line);
                        _game.AddUpdate(pos);
                        _game.ReceivedCarPositions();
                        break;

                    case "join":
                        Console.WriteLine("Joined");
                        break;

                    case "gameInit":
                        var init = JsonConvert.DeserializeObject<GameInitRoot>(line);
                        Console.WriteLine("Game init. GameId: {0}", init.GameId.ToString());
                        Console.WriteLine("Is Qualifying: {0}", init.Data.Race.RaceSession.IsQualifying());
                        _game.Initialize(init.GameId, init.Data.Race);
                        break;

                    case "gameEnd":
                        Console.WriteLine("Game ended");
                        _game.End();
                        break;

                    case "gameStart":
                        Console.WriteLine("Game starts");
                        _game.Start();
                        break;

                    case "yourCar":
                        Console.WriteLine("yourCar");
                        var car = JsonConvert.DeserializeObject<YourCarRoot>(line);
                        _game.SetCar(car.Data);
                        break;

                    case "lapFinished":
                        var lapFin = JsonConvert.DeserializeObject<LapFinishedRoot>(line);
                        if (lapFin.Data.Car.Color == _game.MyCar.Color)
                            Console.WriteLine("Lap finished. Time {0}ms", lapFin.Data.LapTime.Millis);
                        break;

                    case "dnf":
                        var dnf = JsonConvert.DeserializeObject<DnfRoot>(line);
                        if (dnf.Data.Car.Color == _game.MyCar.Color)
                            Console.WriteLine("Dnf. Reason {0} ", dnf.Data.Reason);
                        break;

                    case "crash":
                        var crash = JsonConvert.DeserializeObject<CrashRoot>(line);
                        _game.AddUpdate(crash);

                        break;

                    case "turboAvailable":
                        Console.WriteLine(msg.msgType);
                        var turbo = JsonConvert.DeserializeObject<TurboAvailableRoot>(line);
                        _game.AddUpdate(turbo);
                        break;

                    case "turboStart":
                    case "turboEnd":
                        // TODO: Who sent?
                        Console.WriteLine(msg.msgType);
                        break;

                    case "spawn":
                        var spawn = JsonConvert.DeserializeObject<SpawnRoot>(line);
                        _game.AddUpdate(spawn);

                        if (spawn.Data.Color == _game.MyCar.Color)
                            Console.WriteLine(msg.msgType);

                        break;

                    case "error":
                        Console.WriteLine(msg.msgType + ": " + msg.data.ToString());
                        break;

                    default:
                        Console.WriteLine(msg.msgType + " Not implemented!");
                        break;
                }
            }
        }

        private void NewCommand(object sender, BotMsg e)
        {
            Send(e);
        }

        private void Send(BotMsg msg)
        {
            _writer.WriteLine(msg.ToJson());
        }
    }
}