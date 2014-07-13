using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HWO.Test
{
    [TestClass]
    public class BotMsgTests
    {
        [TestMethod]
        public void Join_ToJson()
        {
            var join = new Join("test", "mykey");

            var json = join.ToJson();
        }

        [TestMethod]
        public void CreateRace_ToJson()
        {
            var join = new CreateRace("test", "mykey", "hockenheim");

            var json = join.ToJson();
        }

        [TestMethod]
        public void Throttle_ToJson()
        {
            var join = new Throttle(0.8, 12);

            var json = join.ToJson();
        }


        [TestMethod]
        public void SwitchLane_ToJson()
        {
            var sl = new SwitchLane(1, 456);

            var json = sl.ToJson();
        }
    }
}
