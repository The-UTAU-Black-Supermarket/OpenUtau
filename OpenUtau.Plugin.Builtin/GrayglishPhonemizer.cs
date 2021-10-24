using System;
using System.Collections.Generic;
using System.Linq;
using OpenUtau.Api;
using OpenUtau.Core.Ustx;

namespace OpenUtau.Plugin.Builtin {
    [Phonemizer("GraySlate X-SAMPA English", "EN Grayglish", "TUBS")]
    public class GrayglishPhonemizer : Phonemizer {
        private IG2p cmudict;
        

        public GrayglishPhonemizer() {
            // Initialize phonemizer with cmudict
            cmudict = G2pDictionary.GetShared("cmudict");
        }

        // Store singer in field for OTO access
        private USinger singer;
        public override void SetSinger(USinger singer) => this.singer = singer;

        public override Result Process(Note[] notes, Note? prev, Note? next, Note? prevNeighbour, Note? nextNeighbour) {
            // Total length of current note
            int noteLength = 0;
            Array.ForEach(notes, i => noteLength += i.duration);

            var prevSymbols = prevNeighbour == null ? null : GetSymbols(prevNeighbour.Value);
            var prevSymbol = prevSymbols == null ? "-" : prevSymbols.Last();
            var note = notes[0];
            var symbols = GetSymbols(note);
            //var nextSymbols = nextNeighbour == null ? null : GetSymbols(nextNeighbour.Value);
            //var nextSymbol = nextSymbols == null ? "-" : nextSymbols.First();

            if (symbols == null || symbols.Length == 0) {
                // Can't find word in dictionary. Assume user is manually entering aliases.
                return new Result {
                    phonemes = new Phoneme[] {
                        new Phoneme() {
                            phoneme = note.lyric
                        }
                    }
                };
            }

            var phonemes = new List<Phoneme>();

            var isVowelArray = symbols.Select(s => isVowel[s]).ToArray();
            var vowelPositions = new List<int>();
            for (var i = 0; i < isVowelArray.Length; i++) {
                if (isVowelArray[i]) {
                    vowelPositions.Add(i);
                }
            }
            int firstVowelPos = vowelPositions[0];
            var firstVowel = symbols[firstVowelPos];

            if (firstVowelPos == 0) {
                // vowel onset
                var text = "";
                if (prevSymbol == "-") {
                    text = $"-{firstVowel}";
                } else {
                    if (isVowel.ContainsKey(prevSymbol) && isVowel[prevSymbol]) {
                        // VV transition
                        if (prevSymbol == firstVowel) {
                            text = firstVowel;
                        } else {
                            text = $"{prevSymbol} {firstVowel}";
                        }
                    } else {
                        text = $"{prevSymbol}{firstVowel}";
                        text = NSub(text, note.tone);
                    }
                }

                phonemes.Add(new Phoneme() {
                    phoneme = text
                });
            } else if (firstVowelPos == 1) {
                // 1 consonant onset
                var cv = $"{symbols[0]}{firstVowel}";
                cv = NSub(cv, note.tone);

                //if (isVowel.ContainsKey(prevSymbol) && isVowel[prevSymbol]) {
                //    var vc = $"{prevSymbol} {symbols[0]}";
                //    vc = NSub(vc, prevNeighbour.Value.tone);
                //    var length = 80;
                //    if (singer.TryGetMappedOto(cv, note.tone, out var oto)) {
                //        phonemes.Add(new Phoneme() {
                //            phoneme = vc,
                //            position = 0 - MsToTick(oto.Preutter)
                //        });
                //    }
                //}
                
                phonemes.Add(new Phoneme() {
                    phoneme = cv
                });
            } else {
                // cluster onset
                //var prefix = prevSymbol == "-" ? "-" : "";
                var cluster = "";
                for (var i = 0; i < firstVowelPos; i++) {
                    cluster += symbols[i];
                }
                cluster = NSub(cluster, note.tone);

                var cv = $"{symbols[firstVowelPos - 1]}{symbols[firstVowelPos]}";
                cv = NSub(cv, note.tone);
                var clusterLength = 80;
                if (singer.TryGetMappedOto(cv, note.tone, out var oto)) {
                    clusterLength = MsToTick(oto.Preutter);
                }

                //if (isVowel.ContainsKey(prevSymbol) && isVowel[prevSymbol]) {
                //    var vc = $"{prevSymbol} {symbols[0]}";
                //    vc = NSub(vc, prevNeighbour.Value.tone);
                //    var vcLength = 80;
                //    if (singer.TryGetMappedOto(cluster, note.tone, out oto)) {
                //        vcLength = MsToTick(oto.Preutter);
                //        phonemes.Add(new Phoneme() {
                //            phoneme = vc,
                //            position = 0 - vcLength - clusterLength
                //        });
                //    }
                //}

                phonemes.Add(new Phoneme() {
                    phoneme = cluster,
                    position = 0 - clusterLength

                });
                phonemes.Add(new Phoneme() {
                    phoneme = cv
                });
            }

            var lastVowelPos = firstVowelPos;
            if (vowelPositions.Count() > 1) {
                // more syllables
                // each syllable centers around a vowel, go through list of vowel positions
                for (var i = 1; i < vowelPositions.Count; i++) {
                    var interval = vowelPositions[i] - lastVowelPos;
                    var lastVow = symbols[lastVowelPos];
                    var thisVow = symbols[vowelPositions[i]];
                    var syllableStart = i * (noteLength / vowelPositions.Count);
                    if (interval == 1) {
                        // vowel onset
                        phonemes.Add(new Phoneme() {
                            phoneme = lastVow == thisVow ? thisVow : $"{lastVow} {thisVow}",
                            position = syllableStart
                        });
                    } else if (interval == 2) {
                        // 1 consonant onset
                        var cons = symbols[vowelPositions[i] - 1];
                        var vc = $"{lastVow} {cons}";
                        vc = NSub(vc, note.tone);
                        var cv = $"{cons}{thisVow}";
                        cv = NSub(cv, note.tone);
                        if(singer.TryGetMappedOto(cv, note.tone, out var oto)) {
                            phonemes.Add(new Phoneme() {
                                phoneme = vc,
                                position = syllableStart - Math.Min(MsToTick(oto.Preutter),
                                    ((noteLength / vowelPositions.Count) / 2))
                            });
                        }
                        phonemes.Add(new Phoneme() {
                            phoneme = cv,
                            position = syllableStart
                        });
                    } else {
                        // cluster onset
                        
                        var firstConPos = lastVowelPos + 1; // after the last vowel
                        var firstCon = symbols[firstConPos];
                        var lastConPos = vowelPositions[i] - 1; // before the current vowel
                        var lastCon = symbols[lastConPos];

                        var cluster = new List<string>();
                        for (var j = firstConPos; j <= lastConPos; j++) {
                            if (symbols[j] != symbols[j - 1]) {
                                cluster.Add(NSub(symbols[j],note.tone));
                            }
                        }
                        var clusterText = string.Join("", cluster);

                        if (singer.TryGetMappedOto(clusterText, note.tone, out var clusterOto)) {
                            // dedicated cluster phoneme found
                            var vc = $"{lastVow} {firstCon}";
                            vc = NSub(vc, note.tone);
                            var vcPos = 0;

                            var clusterPos = 0;
                            
                            var cv = $"{lastCon}{thisVow}";
                            cv = NSub(cv, note.tone);

                            if (singer.TryGetMappedOto(cv, note.tone, out var cvOto)) {
                                clusterPos = syllableStart - Math.Min(MsToTick(cvOto.Preutter),
                                    ((noteLength / vowelPositions.Count) / 3));
                                vcPos = clusterPos - Math.Min(MsToTick(clusterOto.Preutter),
                                    ((noteLength / vowelPositions.Count) / 3));
                            }

                            phonemes.Add(new Phoneme() {
                                phoneme = vc,
                                position = vcPos
                            });
                            phonemes.Add(new Phoneme() {
                                phoneme = clusterText,
                                position = clusterPos
                            });
                            phonemes.Add(new Phoneme() {
                                phoneme = cv,
                                position = syllableStart
                            });
                        } else {
                            // dedicated cluster phoneme not found, try to find 2 clusters
                            var found = false;
                            for (var j = 1; j < cluster.Count - 1; j++) {
                                var cluster1 = string.Join("", cluster.GetRange(0, j + 1)); 
                                var cluster2 = string.Join("", cluster.GetRange(j, cluster.Count-j));
                                if (singer.TryGetMappedOto(cluster1, note.tone, out var c1oto)
                                    && singer.TryGetMappedOto(cluster2, note.tone, out var c2oto)) {
                                    found = true;

                                    var vc = $"{lastVow} {firstCon}";
                                    vc = NSub(vc, note.tone);
                                    var vcPos = 0;

                                    var c1pos = 0;
                                    var c2pos = 0;

                                    var cv = $"{lastCon}{thisVow}";
                                    cv = NSub(cv, note.tone);

                                    if (singer.TryGetMappedOto(cv, note.tone, out var cvOto)) {
                                        c1pos = syllableStart - Math.Min(MsToTick(cvOto.Preutter),
                                            ((noteLength / vowelPositions.Count) / 4));
                                        c2pos = c1pos - Math.Min(MsToTick(c2oto.Preutter),
                                            ((noteLength / vowelPositions.Count) / 4));
                                        vcPos = c2pos - Math.Min(MsToTick(c1oto.Preutter),
                                            ((noteLength / vowelPositions.Count) / 4));
                                    }

                                    phonemes.Add(new Phoneme() {
                                        phoneme = vc,
                                        position = vcPos
                                    });
                                    phonemes.Add(new Phoneme() {
                                        phoneme = cluster1,
                                        position = c1pos
                                    });
                                    phonemes.Add(new Phoneme() {
                                        phoneme = cluster2,
                                        position = c2pos
                                    });
                                    phonemes.Add(new Phoneme() {
                                        phoneme = cv,
                                        position = syllableStart
                                    });
                                }
                            }

                            if (!found) {
                                // give up, only use 2 consonants
                                var vc = $"{lastVow}{firstCon}";
                                vc = NSub(vc, note.tone);
                                phonemes.Add(new Phoneme() {
                                    phoneme = vc,
                                    position = syllableStart - Math.Min(120, 
                                        ((noteLength / vowelPositions.Count) / 2))
                                });

                                var cv = $"{lastCon}{thisVow}";
                                cv = NSub(cv, note.tone);
                                phonemes.Add(new Phoneme() {
                                    phoneme = cv,
                                    position = syllableStart
                                });
                            }
                        }
                    }
                    lastVowelPos = vowelPositions[i];
                }
            }

            var lastVowel = symbols[lastVowelPos];

            // Final coda
            var remainder = symbols.Length - 1 - lastVowelPos;
            if (remainder > 0) {
                if (remainder == 1) {
                    var vc = $"{lastVowel}{symbols[lastVowelPos + 1]}";
                    vc = NSub(vc, note.tone);
                    phonemes.Add(new Phoneme() {
                        phoneme = vc,
                        position = noteLength - Math.Min(120, noteLength / 2)
                    });
                } else {
                    var vc = $"{lastVowel} {symbols[lastVowelPos + 1]}";
                    vc = NSub(vc, note.tone);

                    var cluster = "";
                    for (var i = lastVowelPos + 1; i < symbols.Length; i++) {
                        cluster += symbols[i];
                    }
                    cluster = NSub(cluster, note.tone);

                    var vcLength = 120;
                    var clusterLength = Math.Min(noteLength / 3, 120);

                    if (singer.TryGetMappedOto(cluster, note.tone, out var oto)) {
                        vcLength = Math.Min(noteLength / 3, MsToTick(oto.Preutter));
                    }

                    phonemes.Add(new Phoneme() {
                        phoneme = vc,
                        position = noteLength - clusterLength - vcLength
                    });
                    phonemes.Add(new Phoneme() {
                        phoneme = cluster,
                        position = noteLength - clusterLength
                    });
                }
            }
            //else {
            //    // vowel coda
            //    if (nextSymbol == "-") {
            //        phonemes.Add(new Phoneme() {
            //            phoneme = $"{symbols[firstVowel]}-",
            //            position = noteLength - 120
            //        });
            //    } else if (!isVowel[nextSymbol]) {
            //        // insert vc??
            //    }
            //}

            return new Result {
                phonemes = phonemes.ToArray()
            };
        }

        // Valid phonemes and whether or not they're vowels
        private Dictionary<string, bool> isVowel = new Dictionary<string, bool> {
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

        // Mapping from ARPAABET to Grayglish subset of X-SAMPA
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

        private string[] GetSymbols(Note note) {
            if (string.IsNullOrEmpty(note.phoneticHint)) {
                // Hint not available, check cmudict
                var arpabet = cmudict.Query(note.lyric.ToLowerInvariant());
                if (arpabet == null) {
                    return null;
                }
                // Remapping from arpabet to reclist phonemes
                var converted = new List<string>();
                for (var i = 0; i < arpabet.Length; i++) {
                    var symbol = arpabet[i];
                    if (symbol == "ae" && (arpabet[i+1] == "n" || arpabet[i+1] == "m")) {
                        // Special case for nasal ae
                        converted.Add("e@");
                    } else {
                        converted.Add(symbolMap[symbol]);
                    }
                }
                return converted.ToArray();
            }
            // Hint available, just use the hint
            return note.phoneticHint.Split()
                .Where(s => isVowel.ContainsKey(s)) // Filter hint to only contain valid phonemes
                .ToArray();
        }
        
        // Grayglish doesn't have full CVVC for the N phoneme, suggested sub with n instead
        private string NSub(string phoneme, int tone) {
            if (phoneme.IndexOf("N") > -1) {
                if (!singer.TryGetMappedOto(phoneme, tone, out var _)) {
                    return phoneme.Replace("N", "n");
                }
            }
            return phoneme;
        }
    }
}
