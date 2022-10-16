using OpenUtau.Api;
using OpenUtau.Plugin.Builtin;
using Xunit;
using Xunit.Abstractions;

namespace OpenUtau.Plugins {
    public class EnArpaXTest : PhonemizerTestBase {
        public EnArpaXTest(ITestOutputHelper output) : base(output) { }
        protected override Phonemizer CreatePhonemizer() {
            return new ArpasingExtendedPhonemizer();
        }

        [Theory]
        // Basic ARPAsing
        [InlineData("en_arpa_x",
            new string[] { "test", "words"},
            new string[] { "C3", "C3" },
            new string[] { "", "",},
            new string[] { "- t", "t eh", "eh s", "s t", "t w", "w er", "er d", "d z", "z -"})]
        // Basic ARPAsing multipitch
        [InlineData("en_arpa_x",
            new string[] { "test", "words" },
            new string[] { "C3", "C4" },
            new string[] { "", "", },
            new string[] { "- t", "t eh", "eh s", "s t", "t w", "w er_H", "er d_H", "d z_H", "z -_H" })]
        // Basic ARPAsing voice colors
        // (Colors are correctly applied to each phoneme in manual testing)
        [InlineData("en_arpa_x",
            new string[] { "test", "words" },
            new string[] { "C3", "C3" },
            new string[] { "", "Power", },
            new string[] { "- t", "t eh", "eh s_P", "s t", "t w", "w er", "er d", "d z", "z -" })]
        public void PhonemizeTest(string singerName, string[] lyrics, string[] tones, string[] colors, string[] aliases) {
            RunPhonemizeTest(singerName, lyrics, tones, colors, aliases);
        }
    }
}
