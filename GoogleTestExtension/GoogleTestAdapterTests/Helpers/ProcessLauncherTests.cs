﻿using System;
using System.Collections.Generic;
using GoogleTestAdapter.Framework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GoogleTestAdapter.Helpers
{
    [TestClass]
    public class ProcessLauncherTests : AbstractGoogleTestExtensionTests
    {

        [TestMethod]
        public void GetOutputOfCommand_WithSimpleCommand_ReturnsOutputOfCommand()
        {
            List<string> output = new ProcessLauncher(TestEnvironment, false)
                .GetOutputOfCommand(".", "cmd.exe", "/C \"echo 2\"", false, false, null);

            Assert.AreEqual(1, output.Count);
            Assert.AreEqual("2", output[0]);
        }

        [TestMethod]
        public void GetOutputOfCommand_WhenDebugging_InvokesDebuggedProcessLauncherCorrectly()
        {
            int processId = -4711;
            Mock<IDebuggedProcessLauncher> MockLauncher = new Mock<IDebuggedProcessLauncher>();
            MockLauncher.Setup(l => l.LaunchProcessWithDebuggerAttached(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(processId);

            try
            {
                new ProcessLauncher(TestEnvironment, true)
                    .GetOutputOfCommand("theDir", "theCommand", "theParams", false, false, MockLauncher.Object);
                Assert.Fail();
            }
            catch (ArgumentException e)
            {
                Assert.IsTrue(e.Message.Contains(processId.ToString()));
            }

            MockLauncher.Verify(l => l.LaunchProcessWithDebuggerAttached(
                It.Is<string>(s => s == "theCommand"),
                It.Is<string>(s => s == "theDir"),
                It.Is<string>(s => s == "theParams")
                ), Times.Exactly(1));
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void GetOutputOfCommand_ThrowsIfProcessReturnsErrorCode_Throws()
        {
            new ProcessLauncher(TestEnvironment, false)
                .GetOutputOfCommand(".", "cmd.exe", "/C \"exit 2\"", false, true, null);
        }

        [TestMethod]
        public void GetOutputOfCommand_IgnoresIfProcessReturnsErrorCode_DoesNotThrow()
        {
            new ProcessLauncher(TestEnvironment, false)
                .GetOutputOfCommand(".", "cmd.exe", "/C \"exit 2\"", false, false, null);
        }

    }

}