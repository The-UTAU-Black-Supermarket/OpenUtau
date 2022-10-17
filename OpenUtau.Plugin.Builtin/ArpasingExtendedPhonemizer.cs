﻿using System.Linq;
using System.Collections.Generic;
using OpenUtau.Api;
using OpenUtau.Core.G2p;
using OpenUtau.Core.Ustx;

namespace OpenUtau.Plugin.Builtin {
    [Phonemizer("ARPAsing Extended Phonemizer", "EN ARPA-X", "TUBS", language: "EN")]
    public class ArpasingExtendedPhonemizer : SyllableBasedPhonemizer {
        protected IG2p g2p;
        private List<string> vowels;
        private List<string> consonants;

        public ArpasingExtendedPhonemizer() {
            g2p = new ArpabetG2p();
            vowels = new List<string>();
            vowels.AddRange("aa ae ah ao eh er ih iy uh uw ay ey oy ow aw ax".Split());
            consonants = new List<string>();
            consonants.AddRange("b ch d dh f g hh jh k l m n ng p q r s sh t th v w y z zh".Split());
        }

        public override void SetSinger(USinger singer) {
            this.singer = singer;
            // load singer g2p stuff here
        }

        protected override string[] GetVowels() => vowels.ToArray();
        protected override string[] GetConsonants() => consonants.ToArray();

        protected override string[] GetSymbols(Note note) {
            if (string.IsNullOrEmpty(note.phoneticHint)) {
                return g2p.Query(note.lyric.ToLowerInvariant());
            } else {
                return note.phoneticHint.Split(" ")
                    .Where(s => GetVowels().Contains(s) || GetConsonants().Contains(s))
                    .ToArray();
            }
        }

        protected override List<string> ProcessSyllable(Syllable syllable) {
            if (CanMakeAliasExtension(syllable)) {
                return new List<string> { "null" };
            }

            var phonemes = new List<string>();
            var symbols = new List<string>();
            symbols.Add(syllable.prevV == "" ? "-" : syllable.prevV);
            symbols.AddRange(syllable.cc);
            symbols.Add(syllable.v);

            for (int i = 0; i < symbols.Count-1; i++) {
                phonemes.Add($"{symbols[i]} {symbols[i+1]}");
            }
            return phonemes;
        }

        protected override List<string> ProcessEnding(Ending ending) {
            var phonemes = new List<string>();
            var symbols = new List<string>();
            symbols.Add(ending.prevV);
            symbols.AddRange(ending.cc);
            symbols.Add("-");

            for (int i = 0; i < symbols.Count - 1; i++) {
                phonemes.Add($"{symbols[i]} {symbols[i + 1]}");
            }
            return phonemes;
        }
    }
}
