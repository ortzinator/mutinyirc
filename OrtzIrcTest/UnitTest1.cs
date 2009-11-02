using System.Drawing;
using System.Linq;
using System.Xml.Linq;
using FlamingIRC;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows.Forms;
using OrtzIRC;
using OrtzIRC.Common;
using System;
using System.Diagnostics;

namespace FlamingIrcTest
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class DccUtilTest
    {
        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        [TestMethod]
        public void IPAddressToLongTest1()
        {
            System.Net.IPAddress comp = System.Net.IPAddress.Parse("127.0.0.1");

            long result = DccUtil.IPAddressToLong(comp);
            Assert.AreEqual(DccUtil.NetworkUnsignedLong(comp.Address), result);
        }

        [TestMethod]
        public void IPAddressToLongTest2()
        {
            System.Net.IPAddress comp = new System.Net.IPAddress(2130706433);

            long result = DccUtil.IPAddressToLong(comp);
            Assert.AreEqual(DccUtil.NetworkUnsignedLong(comp.Address), result);
        }

        [TestMethod]
        public void LoopbackTest()
        {
            Assert.AreEqual(System.Net.IPAddress.Parse("127.0.0.1"), DccUtil.LocalHost());
        }

        [TestMethod]
        public void IRCParserTest()
        {
            var listener = new Listener();
            listener.OnNames += listener_OnNames;

            listener.Parse(":TAL.DE.EU.GameSurge.net 353 OrtzIRC = #sourcescrim :iheartyou conmc J-UNIT blade0047 aW|FLeX MCasdf nX-CSS|Midnight priss eMg-css|FINALEXOXO|Keys[jS] [5iM]ecco[22] zF`Pervsquirrel detox-css|requ1em[x] kOrRuPtIoN Acessssss Strife|rptoR chunk [187]Kurupt` BobDole69 wallstars|ck-___^ Dropkick blight|DPing SiR-DreW Jimjeff etre zackari maT_T zF`shawcs mnathan Hostage Quantum|Brad Fable player11 saboteur`streetlight Rookie jaronator cleana LIFTED`RZK blaze^^ reverse ppG`css|Hairs");
            listener.Parse(":TAL.DE.EU.GameSurge.net 353 OrtzIRC= #sourcescrim :fra[acem]med juked Derek tranceNH bLackout-Mono BRIDGEKIDS|zodiak ScrubCups bizzy sF`R-j4y ppppp|Trigger Nye|fRosteh xXxStalker BRIDGEKIDS|tswift btF Depredation ATL-P|MikeILL newERA`tommy eniGmatic|JuelzSantana^HeLL CrazycArl H2flow`Serial_Hunter TAANK TypeR Homer k|Chad jurP saturnine tnf|Glockpoppas WoRm ae|Capone[css] Pandora|l0st cTc-Bant liveRudy boxxy BURR|jMaiNNNEE WM-euro MyLittlePwnies|Fairbanks0_o Ephex` Creck^Shoutcasting");
            listener.Parse(":TAL.DE.EU.GameSurge.net 366 OrtzIRC #sourcescrim :End of /NAMES list.");
        }

        private void listener_OnNames(string channel, string[] nicks, bool last)
        {
            Debug.WriteLine("Added nicks: " + string.Join(", ", nicks));
        }

        [TestMethod]
        public void ColorParseTest()
        {
            var form = new Form();
            form.Size = new Size(600, 800);
            var tb = new IrcTextBox();
            form.Controls.Add(tb);
            tb.Dock = DockStyle.Fill;

            form.Show();

           // tb.AppendLine((char)017 + (char)003 + "1,15 www." +   + "4\002MAX\0031FRAG\002.net - \037Affordable\037 VENTRILO SERVERS \017");
        }


    }
}
