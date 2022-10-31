﻿using OpenUtau.Api;
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
        // Read legacy format arpasing.yaml from plugin folder
        [InlineData("en_arpa_x",
            new string[] { "openutau" },
            new string[] { "C3" },
            new string[] { "", },
            new string[] { "- ow", "ow p", "p eh", "eh n", "n w", "w uw", "uw t", "t ah", "ah w", "w uw", "uw -"})]
        // Read legacy format arpasing.yaml from singer folder
        [InlineData("en_arpa_x",
            new string[] { "legacy2" },
            new string[] { "C3" },
            new string[] { "", },
            new string[] { "- s", "s ih", "ih ng", "ng er", "er -" })]
        // Check for CV without space and fall back on CV with space
        [InlineData("en_arpa_x",
            new string[] { "bed" },
            new string[] { "C3" },
            new string[] { "", },
            new string[] { "- b", "beh", "eh d", "d -" })]
        [InlineData("en_arpa_x",
            new string[] { "the", "bed" },
            new string[] { "C3", "C3" },
            new string[] { "", "" },
            new string[] { "- dh", "dh ah", "ah b", "beh", "eh d", "d -"})]
        [InlineData("en_arpa_x",
            new string[] { "just", "bed" },
            new string[] { "C3", "C3" },
            new string[] { "", "" },
            new string[] { "- jh", "jh ah", "ah s", "s t", "t b", "beh", "eh d", "d -" })]
        // Read custom phonemes from legacy format arpasing.yaml
        [InlineData("en_arpa_x",
            new string[] { "legacy3" },
            new string[] { "C3" },
            new string[] { "", },
            new string[] { "- a", "a dx", "dx -" })]
        public void PhonemizeTest(string singerName, string[] lyrics, string[] tones, string[] colors, string[] aliases) {
            RunPhonemizeTest(singerName, lyrics, tones, colors, aliases);
        }
    }
}
