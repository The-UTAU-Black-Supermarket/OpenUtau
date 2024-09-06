﻿using OpenUtau.Api;
using OpenUtau.Plugin.Builtin;
using OpenUtau.Plugins;
using Xunit;
using Xunit.Abstractions;

namespace OpenUtau.Test.Plugins {
    public class EnCustomTest : PhonemizerTestBase {
        public EnCustomTest(ITestOutputHelper output) : base(output) { }
        protected override Phonemizer CreatePhonemizer() {
            return new EnglishCustomizablePhonemizer();
        }

        [Theory]
        [InlineData("en_custom_a",
            new string[] { "- h", "hV", "V l", "l@U", "@U w", "w@r", "@r l", "l d", "d -" })]
        [InlineData("en_custom_b",
            new string[] { "- HH", "HH AH", "AH L", "L OW", "OW W", "W ER", "ER L", "L D", "D -" })]
        public void BasicPhonemizingTest(string singerName, string[] aliases) {
            SameAltsTonesColorsTest(singerName, new string[] {"hello", "world"}, aliases, "", "C4", "");
        }

        // validate in UI that extra vowels behave as vowels for phoneme timing
        [Fact]
        public void ExtraPhonemeTest() {
            RunPhonemizeTest("en_custom_c", 
                new NoteParams[] { 
                    new NoteParams { lyric = "twinkle", hint = "", tone = "C4", phonemes = SamePhonemeParams(9, 0, 0, "") },
                    new NoteParams { lyric = "twinkle", hint = "T W ing K ul", tone = "C4", phonemes = SamePhonemeParams(9, 0, 0, "") },
                    new NoteParams { lyric = "little", hint = "L ih DX ul", tone = "C4", phonemes = SamePhonemeParams(9, 0, 0, "") },
                    new NoteParams { lyric = "star", hint = "st ar", tone = "C4", phonemes = SamePhonemeParams(9, 0, 0, "") }
                }, 
                new string[] { "- T", "T W", "W ih","ih ng", "ng K", "K ah", "ah L", "L T",
                    "T W", "W ing", "ing K", "K ul", "ul L",
                    "L ih", "ih DX", "DX ul", "ul st", "st ar", "ar -"});
        }

        [Theory]
        [InlineData(
            new string[] { "hey", "-"},
            new string[] { "- hh", "hh ey", "ey -"})]
        [InlineData(
            new string[] { "hey", "R"},
            new string[] { "- hh", "hh ey", "ey R" })]
        [InlineData(
            new string[] { "hey", "br", "yeah"},
            new string[] { "- hh", "hh ey", "ey br", "- y", "y ae", "ae -" })]
        public void CustomTailTest(string[] lyrics, string[] aliases) {
            SameAltsTonesColorsTest("en_custom_c", lyrics, aliases, "", "C4", "");
        }
    }
}
