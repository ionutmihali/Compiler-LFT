using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Proiect_LFT
{
    internal class Lexer
    {
        private readonly string text;
        private int index;
        private List<string> erori = new List<string>();
        private int count = 0;

        public List<string> Erori => erori;

        public Lexer(string text)
        {
            this.text = text;
        }

        private char SimbolCurent
        {
            get
            {
                if (index >= text.Length)
                    return '\0';

                return text[index];
            }
        }

        private void UrmatorulSimbol()
        {
            index++;
        }

        public AtomLexical Atom()
        {
            //terminator de sir
            if(SimbolCurent == '\0')
            {
                return new AtomLexical(TipAtomLexical.TerminatorSirAtomLexical, index, "\0", null);
            }

            //numere
            if (char.IsDigit(SimbolCurent) && count==0)
            {
                int start = index;
                UrmatorulSimbol();

                while (char.IsDigit(SimbolCurent))
                {
                    UrmatorulSimbol();
                }

                if (index < text.Length)
                {
                    if (text[index] == '.')
                    {
                        UrmatorulSimbol();
                        while (char.IsDigit(SimbolCurent))
                        {
                            UrmatorulSimbol();
                        }

                        string v = text.Substring(start, (index - start));
                        float f = float.Parse(v);
                        return new AtomLexical(TipAtomLexical.DoubleAtomLexical, start, v, f);

                    }
                }
                string val = text.Substring(start, (index - start));

                if (Int32.TryParse(val, out int valoare))
                {
                    return new AtomLexical(TipAtomLexical.NumarAtomLexical, start, val, valoare);
                }
                else
                {
                    erori.Add($"Nu s-a putut parsa valoarea {val} la Int32");

                }
            }

            //string-uri
            if (char.IsLetterOrDigit(SimbolCurent) && count%2==1)
            {

                int start = index;

                while (SimbolCurent != '\0' && SimbolCurent != '\"')
                {
                    UrmatorulSimbol();
                }

                if(SimbolCurent=='\0')
                {
                    erori.Add($"Lexer: Exceptie: Eroare ghilimele.");
                }

                string val = text.Substring(start, (index - start));
                return new AtomLexical(TipAtomLexical.StringAtomLexical, start, val, val);
            }

            //cuvant cheie sau nume de variabila
            if(char.IsLetter(SimbolCurent)||SimbolCurent=='_')
            {
                int start = index;
                while(char.IsLetterOrDigit(SimbolCurent)||SimbolCurent=='_')
                {
                    UrmatorulSimbol();
                }

                string val = text.Substring(start, (index - start));
                string path = @"(int|double|string)";
                Match m = Regex.Match(val, path);
                if(m.Success)
                {
                    return new AtomLexical(TipAtomLexical.CuvantCheieAtomlexical, start, val, val);
                }
                else
                {
                    path = @"^[a-zA-Z_$][a-zA-Z_$0-9]*$";
                    m = Regex.Match(val, path);
                    if(m.Success)
                    {
                        return new AtomLexical(TipAtomLexical.VariabilaAtomLexical, start, val, val);
                    }
                }
                erori.Add($"Lexer: Exceptie: Atom Lexical de tip string invalid '{val}'");
                throw new Exception("Lexer: Atom lexical de tip string nerecunoscut");
            }

            //spatii
            if (SimbolCurent == ' ')
            {
                int start = index;
                UrmatorulSimbol();

                while (SimbolCurent == ' ')
                {
                    UrmatorulSimbol();
                }

                string val = text.Substring(start, (index - start));

                return new AtomLexical(TipAtomLexical.SpatiuAtomLexical, start, val, null);

            }

            //operatori

            if (SimbolCurent == '+')
                return new AtomLexical(TipAtomLexical.PlusAtomLexical, index++, "+", null);

            if (SimbolCurent == '-')
                return new AtomLexical(TipAtomLexical.MinusAtomLexical, index++, "", null);

            if (SimbolCurent == '*')
                return new AtomLexical(TipAtomLexical.StarAtomLexical, index++, "*", null);

            if (SimbolCurent == '/')
                return new AtomLexical(TipAtomLexical.SlashAtomLexical, index++, "/", null);

            if (SimbolCurent == '(')
                return new AtomLexical(TipAtomLexical.ParantezaDeschisaAtomLexical, index++, "(", null);

            if (SimbolCurent == ')')
                return new AtomLexical(TipAtomLexical.ParantezaInchisaAtomLexical, index++, ")", null);

            if(SimbolCurent=='=')
                return new AtomLexical(TipAtomLexical.EgalAtomLexical, index++, "=", null);

            if(SimbolCurent==';')
                return new AtomLexical(TipAtomLexical.PunctVirgulaAtomLexical, index++, ";", null);

            if (SimbolCurent == ',')
                return new AtomLexical(TipAtomLexical.VirgulaAtomLexical, index++, ",", null);

            if (SimbolCurent == '"')
            {
                count++;
                return new AtomLexical(TipAtomLexical.GhilimeleAtomLexical, index++, "\"", null);
            }

            erori.Add($"Simbol invalid '{SimbolCurent}'");
            return new AtomLexical(TipAtomLexical.InvalidAtomLexical, index, null, null);
        }
    }
}
