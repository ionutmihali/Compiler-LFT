using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proiect_LFT
{
    internal class Evaluator
    {
        public List<string> erori = new List<string>();
        public Expresie Expresie { get; set; }
        public Evaluator(Expresie expr)
        {
            Expresie = expr;
        }

        public dynamic Evalueaza()
        {
            if(Expresie is ExpresieInvalida)
            {
                return null;
            }
            return EvalueazaExpresie(Expresie);
        }
        private dynamic EvalueazaExpresie(Expresie expresie)
        {
            if (expresie is ExpresieNumerica en)
                return en.Numar.Valoare;

            if (expresie is ExpresieString es)
                return es.String.Valoare;

            if (expresie is ExpresieDouble ed)
                return ed.Numar.Valoare;

            if(expresie is ExpresieGhilimele eg)
            {
                var e = eg.Expresie;
                return EvalueazaExpresie(e);
            }

            if (expresie is ExpresieBinara eb)
            {
                var stanga = eb.Stanga;
                var dreapta =eb.Dreapta;
                var op = eb.Operator;

                if(stanga is ExpresieGhilimele st && dreapta is ExpresieGhilimele)
                {
                    var s = st.Expresie;
                    var d = st.Expresie;

                    if (op.Tip == TipAtomLexical.PlusAtomLexical)
                        return EvalueazaExpresie(stanga) + EvalueazaExpresie(dreapta);
                    else
                    {
                        erori.Add($"Nu puteti efectua acest tip de operatie cu string-uri.");
                        return new ExpresieInvalida("Nu puteti efectua acest tip de operatie cu string-uri.");
                    }
                }

                if (!(stanga is ExpresieGhilimele) || !(dreapta is ExpresieGhilimele))
                {
                    if (op.Tip == TipAtomLexical.PlusAtomLexical)
                        return EvalueazaExpresie(stanga) + EvalueazaExpresie(dreapta);

                    if (op.Tip == TipAtomLexical.MinusAtomLexical)
                        return EvalueazaExpresie(stanga) - EvalueazaExpresie(dreapta);

                    if (op.Tip == TipAtomLexical.StarAtomLexical)
                        return EvalueazaExpresie(stanga) * EvalueazaExpresie(dreapta);

                    if (op.Tip == TipAtomLexical.SlashAtomLexical)
                    {
                        if (EvalueazaExpresie(dreapta) == 0)
                        {
                            erori.Add($"Impartire la 0");
                            return new ExpresieInvalida("Impartire la 0");
                        }
                        return EvalueazaExpresie(stanga) / EvalueazaExpresie(dreapta);
                    }
                }
                    
            }

            if(expresie is ExpresieParanteze ep)
            {
                return EvalueazaExpresie(ep.Expresie);
            }

            erori.Add($"Expresie necunoascuta");
            return new ExpresieInvalida("Expesie necunoscuta.");
        }
    }
}
