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
                        phoneme = string.Join(" ", symbols)
                    }
                }
            };
        }

        // Bool to signify vowels
        private Dictionary<string, bool> validSymbols = new Dictionary<string, bool> {
            {"a", true},
            {"i", true},
            {"u", true},
            {"O", true},
            {"I", true},
            {"U", true},
            {"E", true},
            {"V", true},
            {"aI", true},
            {"eI", true},
            {"OI", true},
            {"oU", true},
            {"aU", true},
            {"O@", true},
            {"@", true},
            {"3", true},
            {"{", true},
            {"e@", true},
            {"mm", true},
            {"b", false},
            {"d", false},
            {"f", false},
            {"g", false},
            {"h", false},
            {"j", false},
            {"k", false},
            {"l", false},
            {"m", false},
            {"n", false},
            {"p", false},
            {"r", false},
            {"s", false},
            {"t", false},
            {"v", false},
            {"w", false},
            {"z", false},
            {"4", false},
            {"tS", false},
            {"D", false},
            {"dZ", false},
            {"S", false},
            {"T", false},
            {"Z", false},
            {"R", false},
            {"N", false}
        };

        private Dictionary<string, string> symbolMap = new Dictionary<string, string> {
            {"aa", "a"},
            {"ae", "{"},
            {"ah", "V"},
            {"ao", "O"},
            {"ax", "@"},
            {"eh", "E"},
            {"er", "3"},
            {"ih", "I"},
            {"iy", "i"},
            {"uh", "U"},
            {"uw", "u"},
            {"ay", "aI"},
            {"ey", "eI"},
            {"oy", "OI"},
            {"ow", "oU"},
            {"aw", "aU"},
            {"b", "b"},
            {"ch", "tS"},
            {"d", "d"},
            {"dh", "D"},
            {"dx", "4"},
            {"f", "f"},
            {"g", "g"},
            {"hh", "h"},
            {"jh", "dZ"},
            {"k", "k"},
            {"l", "l"},
            {"m", "m"},
            {"n", "n"},
            {"ng", "N"},
            {"p", "p"},
            {"r", "r"},
            {"s", "s"},
            {"sh", "S"},
            {"t", "t"},
            {"th", "T"},
            {"v", "v"},
            {"w", "w"},
            {"y", "j"},
            {"z", "z"},
            {"zh", "Z"}
        };

        string[] GetSymbols(Note note) {
            if (string.IsNullOrEmpty(note.phoneticHint)) {
                var arpabet = cmudict.Query(note.lyric.ToLowerInvariant());
                var converted = new List<string>();
                for (var i = 0; i < arpabet.Length; i++) {
                    var symbol = arpabet[i];
                    if (symbol == "ae" && (arpabet[i+1] == "n" || arpabet[i+1] == "m")) {
                        converted.Add("e@");
                        //converted.Add(next);
                        //i++;
                    } else {
                        converted.Add(symbolMap[symbol]);
                    }
                }
                return converted.ToArray();
            }
            return note.phoneticHint.Split()
                .Where(s => validSymbols.ContainsKey(s))
                .ToArray();
        }
    }
}
