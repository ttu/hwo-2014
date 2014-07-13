using Newtonsoft.Json;
using System;

namespace HWO
{
    // TODO: Refactor these so style follows rest of the application

    public class MsgWrapper
    {
        public string msgType;
        public Object data;

        public MsgWrapper(string msgType, Object data)
        {
            this.msgType = msgType;
            this.data = data;
        }
    }

    public class MsgWrapperWTick : MsgWrapper
    {
        public int gameTick;

        public MsgWrapperWTick(string msgType, Object data, int gameTick)
            : base(msgType, data)
        {
            this.gameTick = gameTick;
        }
    }

    public abstract class BotMsg
    {
        public virtual string ToJson()
        {
            return JsonConvert.SerializeObject(new MsgWrapper(this.MsgType(), this.MsgData()));
        }

        protected virtual Object MsgData()
        {
            return this;
        }

        protected abstract string MsgType();
    }

    public class Join : BotMsg
    {
        public string name;
        public string key;

        public Join(string name, string key)
        {
            this.name = name;
            this.key = key;
        }

        protected override string MsgType()
        {
            return "join";
        }
    }

    public class CreateRace : BotMsg
    {
        public BotId botId;
        public string trackName;
        public string password;
        public int carCount;

        public CreateRace(string name, string key, string track)
        {
            this.botId = new BotId { key = key, name = name };
            this.trackName = track;
            this.password = "sdjdsgnjksg";
            this.carCount = 1;
        }

        protected override string MsgType()
        {
            return "createRace";
        }
    }

    public class BotId
    {
        public string name;
        public string key;
    }

    public class Ping : BotMsg
    {
        protected override string MsgType()
        {
            return "ping";
        }
    }

    public class Throttle : BotMsg
    {
        public double value;
        public int gameTick;

        public Throttle(double value, int gameTick)
        {
            this.value = value;
            this.gameTick = gameTick;
        }

        public override string ToJson()
        {
            return JsonConvert.SerializeObject(new MsgWrapperWTick(this.MsgType(), this.MsgData(), this.gameTick));
        }

        protected override Object MsgData()
        {
            return this.value;
        }

        protected override string MsgType()
        {
            return "throttle";
        }
    }

    public class SwitchLane : BotMsg
    {
        public string direction;
        public int gameTick;

        public SwitchLane(int direction, int gameTick)
        {
            // Lets hope that no one sends this with 0 ;)
            this.direction = direction == -1 ? "Left" : "Right";
            this.gameTick = gameTick;
        }

        public override string ToJson()
        {
            return JsonConvert.SerializeObject(new MsgWrapperWTick(this.MsgType(), this.MsgData(), this.gameTick));
        }

        protected override Object MsgData()
        {
            return direction;
        }

        protected override string MsgType()
        {
            return "switchLane";
        }
    }

    public class Turbo : BotMsg
    {
        public string message;
        public int gameTick;

        public Turbo(string message, int gameTick)
        {
            this.gameTick = gameTick;
            this.message = message;
        }

        public override string ToJson()
        {
            return JsonConvert.SerializeObject(new MsgWrapperWTick(this.MsgType(), this.MsgData(), this.gameTick));
        }

        protected override Object MsgData()
        {
            return message;
        }

        protected override string MsgType()
        {
            return "turbo";
        }
    }
}