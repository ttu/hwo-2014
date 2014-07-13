using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace HWO.Test
{
    [TestClass]
    public class InitHandlerTests
    {
        [TestMethod]
        public void InitHandler_StageChange()
        {
            var handler = new InitHandler();

            var throttle = handler.GetThrottle();
            Assert.AreEqual(1, throttle);

            var upTarget = handler.GetNextUpTarget();
            var downTarget = handler.GetNextDownTarget();
            Assert.AreEqual(0.2, upTarget);
            Assert.AreEqual(0.1, downTarget);

            var change = handler.SetNextTargets(0.1);
            Assert.IsTrue(change);

            upTarget = handler.GetNextUpTarget();
            downTarget = handler.GetNextDownTarget();
            Assert.AreEqual(0.9, upTarget);
            Assert.AreEqual(0.5, downTarget, 0.01);

            change = handler.SetNextTargets(0.5);
            Assert.IsFalse(change);

            change = handler.SetNextStage(0.6);
            Assert.IsTrue(change);

            throttle = handler.GetThrottle();

            Assert.AreEqual(0.5, throttle);

            upTarget = handler.GetNextUpTarget();
            downTarget = handler.GetNextDownTarget();
            Assert.AreEqual(0.8, upTarget);
            Assert.AreEqual(0.7, downTarget, 0.01);

            change = handler.SetNextTargets(0.7);
            Assert.IsTrue(change);

            upTarget = handler.GetNextUpTarget();
            downTarget = handler.GetNextDownTarget();
            Assert.AreEqual(1.5, upTarget, 0.01);
            Assert.AreEqual(1.1, downTarget, 0.01);

            change = handler.SetNextStage(1.1);
            Assert.IsFalse(change);

            // Still one more stage
        }

        [TestMethod]
        public void InitHandler_GetMagicNumbers()
        {
            var handler = new InitHandler();

            handler.Item.StartTick = 0;
            handler.Item.StartSpeed = 0;
            handler.Item.AccelerationTick = 9;
            handler.Item.SlideTick = 2;
            handler.Item.SlideSpeed = 0.39056655253039985;
            handler.Item.DecelerationTick = 36;

            var change = handler.SetNextTargets(0.19650862726024343);

            handler.Item.AccelerationTick = 28;
            handler.Item.SlideTick = 2;
            handler.Item.SlideSpeed = 0.95395906993171309;
            handler.Item.DecelerationTick = 26;

            change = handler.SetNextStage(0.58742923709844064);

            handler.Item.AccelerationTick = 5;
            handler.Item.SlideTick = 2;
            handler.Item.SlideSpeed = 1.0749080345167954;
            handler.Item.DecelerationTick = 18;

            change = handler.SetNextTargets(0.77801598522981408);
            
            handler.Item.AccelerationTick = 10;
            handler.Item.SlideTick = 2;
            handler.Item.SlideSpeed = 1.6861340504440676;
            handler.Item.DecelerationTick = 20;

            change = handler.SetNextStage(1.172091351024747);

            var slow = handler.GetSlowDownNumber();

            Assert.AreEqual(22, Math.Round(slow, 0));
        }
    }
}