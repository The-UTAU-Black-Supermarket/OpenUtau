using OpenUtau.Api;
using OpenUtau.Core.Ustx;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenUtau.Plugin.Builtin {
    [Phonemizer("GraySlate X-SAMPA English", "EN Grayglish", "TUBS")]
    public class GrayglishPhonemizer : Phonemizer {
        private IG2p cmudict;

        public GrayglishPhonemizer() {
            cmudict = G2pDictionary.GetShared("cmudict");
        }

        private USinger singer;
        public override void SetSinger(USinger singer) => this.singer = singer;

        public override Result Process(Note[] notes, Note? prev, Note? next, Note? prevNeighbour, Note? nextNeighbour) {
            var note = notes[0];
            var symbols = GetSymbols(note);

            return new Result {
                phonemes = new Phoneme[] {
                    new Phoneme() {
                        phoneme = string.Join("", symbols)
                    }
                }
            };
        }

        // Bool to signify vowels
        //private Dictionary<string, bool> validSymbols = new Dictionary<string, bool> {
        //    {"A", true},
        //    {"i", true},
        //    {"u", true},
        //    {"O", true},
        //    {"I", true},
        //    {"U", true},
        //    {"E", true},
        //    {"V", true},
        //    {"oU", true},
        //    {"aI", true},
        //    {"eI", true},
        //    {"OI", true},
        //    {"oU", true},
        //    {"O@", true},
        //    {"@", true},
        //    {"3", true},
        //    {"{", true},
        //    {"e@", true},
        //    {"mm", true},
        //    {"b", false},
        //    {"d", false},
        //    {"f", false},
        //    {"g", false},
        //    {"h", false},
        //    {"j", false},
        //    {"k", false},
        //    {"l", false},
        //    {"m", false},
        //    {"n", false},
        //    {"p", false},
        //    {"r", false},
        //    {"s", false},
        //    {"t", false},
        //    {"v", false},
        //    {"w", false},
        //    {"z", false},
        //    {"4", false},
        //    {"tS", false},
        //    {"D", false},
        //    {"dZ", false},
        //    {"S", false},
        //    {"T", false},
        //    {"Z", false},
        //    {"R", false},
        //    {"N", false}
        //};

        string[] GetSymbols(Note note) {
            if (string.IsNullOrEmpty(note.phoneticHint)) {
                return cmudict.Query(note.lyric.ToLowerInvariant());
            }
            return note.phoneticHint.Split()
                .Where(s => cmudict.IsValidSymbol(s))
                .ToArray();
        }
    }
}
