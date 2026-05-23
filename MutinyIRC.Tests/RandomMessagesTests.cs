using System;
using System.IO;
using MutinyIRC.Common;
using NUnit.Framework;
using Assert = NUnit.Framework.Legacy.ClassicAssert;

namespace MutinyIRC.Tests
{
    [TestFixture]
    public class RandomMessagesTests
    {
        private string _tempDir;
        private string _tempFile;

        [SetUp]
        public void Setup()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), "MutinyIRC.Tests-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempDir);
            _tempFile = Path.Combine(_tempDir, "random-messages.json");
        }

        [TearDown]
        public void Teardown()
        {
            try
            {
                if (Directory.Exists(_tempDir))
                    Directory.Delete(_tempDir, recursive: true);
            }
            catch
            {
                // Best-effort cleanup; ignore failures
            }
        }

        private RandomMessages NewSut(int? seed = null)
        {
            var rand = seed.HasValue ? new Random(seed.Value) : null;
            return new RandomMessages(_tempFile, rand);
        }

        [Test]
        public void GetMessage_UnregisteredType_ReturnsNull()
        {
            var sut = NewSut();
            Assert.IsNull(sut.GetMessage("quit"));
        }

        [Test]
        public void GetMessage_RegisteredTypeWithNoMessages_ReturnsNull()
        {
            var sut = NewSut();
            sut.RegisterMessageType("quit");
            Assert.IsNull(sut.GetMessage("quit"));
        }

        [Test]
        public void GetMessage_SingleMessage_ReturnsThatMessage()
        {
            var sut = NewSut();
            sut.RegisterMessageType("quit");
            sut.AddMessage("quit", "Bye");
            Assert.AreEqual("Bye", sut.GetMessage("quit"));
        }

        [Test]
        public void GetMessage_SeededRandom_PicksDeterministically()
        {
            // With seed 0, Random.Next(0, 3) returns the same sequence every run.
            var sut = NewSut(seed: 0);
            sut.RegisterMessageType("quit");
            sut.AddMessage("quit", "a");
            sut.AddMessage("quit", "b");
            sut.AddMessage("quit", "c");

            var pick1 = sut.GetMessage("quit");
            var pick2 = sut.GetMessage("quit");
            var pick3 = sut.GetMessage("quit");

            // Same seed should produce the same picks if the test is re-run.
            var sut2 = NewSut(seed: 0);
            sut2.RegisterMessageType("quit");
            sut2.AddMessage("quit", "a");
            sut2.AddMessage("quit", "b");
            sut2.AddMessage("quit", "c");

            Assert.AreEqual(pick1, sut2.GetMessage("quit"));
            Assert.AreEqual(pick2, sut2.GetMessage("quit"));
            Assert.AreEqual(pick3, sut2.GetMessage("quit"));
        }

        [Test]
        public void RegisterMessageType_NewType_ReturnsTrue()
        {
            var sut = NewSut();
            Assert.IsTrue(sut.RegisterMessageType("quit"));
        }

        [Test]
        public void RegisterMessageType_DuplicateType_ReturnsFalse()
        {
            var sut = NewSut();
            sut.RegisterMessageType("quit");
            Assert.IsFalse(sut.RegisterMessageType("quit"));
        }

        [Test]
        public void AddMessage_UnregisteredType_SilentlyDoesNothing()
        {
            // Documents current behavior: AddMessage on an unregistered type is a no-op.
            var sut = NewSut();
            sut.AddMessage("quit", "Bye");
            Assert.IsNull(sut.GetMessage("quit"));
        }

        [Test]
        public void AddMessage_DuplicateMessage_NotAddedTwice()
        {
            var sut = NewSut(seed: 0);
            sut.RegisterMessageType("quit");
            sut.AddMessage("quit", "Bye");
            sut.AddMessage("quit", "Bye");

            // Only one message in the pool, so any pick must be "Bye".
            Assert.AreEqual("Bye", sut.GetMessage("quit"));
        }

        [Test]
        public void RemoveMessage_ExistingMessage_RemovesIt()
        {
            var sut = NewSut();
            sut.RegisterMessageType("quit");
            sut.AddMessage("quit", "Bye");
            sut.RemoveMessage("quit", "Bye");
            Assert.IsNull(sut.GetMessage("quit"));
        }

        [Test]
        public void RemoveMessage_UnregisteredType_DoesNotThrow()
        {
            var sut = NewSut();
            Assert.DoesNotThrow(() => sut.RemoveMessage("quit", "Bye"));
        }

        [Test]
        public void UnregisterMessageType_RemovesTypeAndItsMessages()
        {
            var sut = NewSut();
            sut.RegisterMessageType("quit");
            sut.AddMessage("quit", "Bye");

            Assert.IsTrue(sut.UnregisterMessageType("quit"));
            Assert.IsNull(sut.GetMessage("quit"));
        }

        [Test]
        public void UnregisterMessageType_UnknownType_ReturnsFalse()
        {
            var sut = NewSut();
            Assert.IsFalse(sut.UnregisterMessageType("quit"));
        }

        [Test]
        public void Load_MissingFile_IsNoOp()
        {
            Assert.IsFalse(File.Exists(_tempFile));
            var sut = NewSut();

            Assert.DoesNotThrow(() => sut.Load());
            Assert.IsNull(sut.GetMessage("quit"));
        }

        [Test]
        public void Load_CorruptJson_DoesNotThrow()
        {
            File.WriteAllText(_tempFile, "{ this is not json");
            var sut = NewSut();

            Assert.DoesNotThrow(() => sut.Load());
            Assert.IsNull(sut.GetMessage("quit"));
        }

        [Test]
        public void SaveAndLoad_Roundtrip_PreservesMessages()
        {
            var writer = NewSut();
            writer.RegisterMessageType("quit");
            writer.AddMessage("quit", "Bye");
            writer.AddMessage("quit", "Later");
            writer.RegisterMessageType("part");
            writer.AddMessage("part", "brb");
            writer.Save();

            Assert.IsTrue(File.Exists(_tempFile));

            var reader = NewSut(seed: 0);
            reader.Load();

            // "part" only has one message, so pick is deterministic regardless of seed.
            Assert.AreEqual("brb", reader.GetMessage("part"));

            // "quit" should contain one of the two persisted messages.
            var quit = reader.GetMessage("quit");
            Assert.IsTrue(quit == "Bye" || quit == "Later", $"Unexpected quit message: {quit}");
        }

        [Test]
        public void Save_CreatesParentDirectory()
        {
            string nestedFile = Path.Combine(_tempDir, "sub", "nested", "messages.json");
            var sut = new RandomMessages(nestedFile);
            sut.RegisterMessageType("quit");
            sut.AddMessage("quit", "Bye");

            sut.Save();

            Assert.IsTrue(File.Exists(nestedFile));
        }
    }
}