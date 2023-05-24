using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proiect_LFT
{
    internal class AtomLexical : NodSintactic
    {
        //tip, index, text, valoare

        public override TipAtomLexical Tip { get; }
        public int Index { get; }
        public string Text { get; }
        public object Valoare { get; }


        public AtomLexical(TipAtomLexical tip, int index, string text, object valoare)
        {
            this.Tip = tip;
            this.Index = index;
            this.Text = text;
            this.Valoare = valoare;
        }

        public override IEnumerable<NodSintactic> GetCopii()
        {
            return Enumerable.Empty<NodSintactic>();
        }
    }
}
