using OpenUtau.Api;
using OpenUtau.Core.Ustx;

namespace OpenUtau.Plugin.Builtin {
    [Phonemizer("GraySlate X-SAMPA English", "EN Grayglish", "TUBS")]
    public class GrayglishPhonemizer : Phonemizer {
        private USinger singer;
        public override void SetSinger(USinger singer) => this.singer = singer;

        public override Result Process(Note[] notes, Note? prev, Note? next, Note? prevNeighbour, Note? nextNeighbour) {
            return new Result {
                phonemes = new Phoneme[] {
                    new Phoneme() {
                        phoneme = notes[0].lyric
                    }
                }
            };
        }
    }
}
