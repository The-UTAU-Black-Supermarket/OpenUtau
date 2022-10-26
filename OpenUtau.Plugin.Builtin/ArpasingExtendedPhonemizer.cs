using System.Linq;
using System.Collections.Generic;
using OpenUtau.Api;
using OpenUtau.Core.G2p;
using OpenUtau.Core.Ustx;
using System.IO;
using System;
using Serilog;

namespace OpenUtau.Plugin.Builtin {
    [Phonemizer("ARPAsing Extended Phonemizer", "EN ARPA-X", "TUBS", language: "EN")]
    public class ArpasingExtendedPhonemizer : SyllableBasedPhonemizer {
        protected IG2p? g2p;
        private List<string> vowels;
        private List<string> consonants;

        public ArpasingExtendedPhonemizer() {
            vowels = new List<string>();
            vowels.AddRange("aa ae ah ao eh er ih iy uh uw ay ey oy ow aw ax".Split());
            consonants = new List<string>();
            consonants.AddRange("b ch d dh f g hh jh k l m n ng p q r s sh t th v w y z zh".Split());
            LoadSettings();
        }

        public override void SetSinger(USinger singer) {
            this.singer = singer;
            LoadSettings();
        }

        private void LoadSettings() {
            var g2ps = new List<IG2p>();

            // Load legacy dict from plugin folder.
            // No need to create if not available
            string path = Path.Combine(PluginDir, "arpasing.yaml");
            if (File.Exists(path)) {
                g2ps.Add(G2pDictionary.NewBuilder().Load(File.ReadAllText(path)).Build());
            }

            // Load legacy dict from singer folder.
            if (singer != null && singer.Found && singer.Loaded) {
                string file = Path.Combine(singer.Location, "arpasing.yaml");
                if (File.Exists(file)) {
                    try {
                        g2ps.Add(G2pDictionary.NewBuilder().Load(File.ReadAllText(file)).Build());
                    } catch (Exception e) {
                        Log.Error(e, $"Failed to load {file}");
                    }
                }
            }

            // Load base g2p.
            g2ps.Add(new ArpabetG2p());

            g2p = new G2pFallbacks(g2ps.ToArray());
        }

        protected override string[] GetVowels() => vowels.ToArray();
        protected override string[] GetConsonants() => consonants.ToArray();

        protected override string[] GetSymbols(Note note) {
            if (string.IsNullOrEmpty(note.phoneticHint) && g2p != null) {
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
