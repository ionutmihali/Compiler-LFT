using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using static System.Net.Mime.MediaTypeNames;

namespace Proiect_LFT
{
    internal class Program
    {
        static string textFile = @"C:\Users\Ionut\Documents\FACULTATE\ANUL III\SEMESTRUL I\Limbaje Formale si Translatoare\in.txt";
        static void Main(string[] args)
        {
            int check = 0;
            while (check == 0)
            {
                Console.WriteLine("Alege modalitatea de introducere date:");
                Console.WriteLine("1. Fisier de input");
                Console.WriteLine("2. Linie de comanda");
                Console.WriteLine("3. Exit");
                var aux = Console.ReadLine();
                if (Int32.Parse(aux) == 1)
                {
                    if (File.Exists(textFile))
                    {
                        // Read a text file line by line.  
                        string[] lines = File.ReadAllLines(textFile);
                        foreach (string text in lines)
                        {
                            if (string.IsNullOrWhiteSpace(text))
                                return;

                            Console.WriteLine($"> {text}");
                            var parser = new Parser(text);
                            var arboreSintactic = parser.Parseaza();
                            AfiseazaArbore(arboreSintactic);

                            var culoare = Console.ForegroundColor;
                            if (parser.erori.Any())
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                foreach (var eroare in parser.erori)
                                    Console.WriteLine(eroare);
                                Console.ForegroundColor = culoare;
                            }
                            else
                            {
                                var e = new Evaluator(arboreSintactic);
                                var rezultat = e.Evalueaza();
                                if (rezultat != null)
                                {
                                    Console.WriteLine(rezultat);
                                }

                            }
                        }
                    }
                    else 
                    {
                        Console.WriteLine("Eroare la deschiderea fisierului.\n");
                    }
                    
                }
                else if (Int32.Parse(aux) == 2)
                {
                    check = 1;
                    while (check==1)
                    {
                        Console.Write("> ");
                        var text = Console.ReadLine();
                        if (string.IsNullOrWhiteSpace(text))
                            return;

                        if(text=="exit")
                        {
                            check = 0;
                            break;
                        }

                        var parser = new Parser(text);
                        var arboreSintactic = parser.Parseaza();
                        AfiseazaArbore(arboreSintactic);

                        var culoare = Console.ForegroundColor;
                        if (parser.erori.Any())
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            foreach (var eroare in parser.erori)
                                Console.WriteLine(eroare);
                            Console.ForegroundColor = culoare;
                        }
                        else
                        {
                            var e = new Evaluator(arboreSintactic);
                            var rezultat = e.Evalueaza();
                            if (rezultat != null)
                            {
                                Console.WriteLine(rezultat);
                            }

                        }
                    }
                }
                else if(Int32.Parse(aux) == 3)
                {
                    check= 1;
                }
                else
                {
                    Console.WriteLine("Nu exista aceasta optiune.\n");
                    check = 0;
                }
            }
        }
        static void AfiseazaArbore(NodSintactic nod, string indentare = "", bool ultimulNod = true)
        {
            if(nod.Tip==TipAtomLexical.ExpresieInvalida)
            {
                return;
            }

            var prefix = ultimulNod ? "└──" : "├──";
            Console.Write(indentare);
            Console.Write(prefix);
            Console.Write(nod.Tip);

            if (nod is AtomLexical t && t.Valoare != null)
            {
                Console.Write(" ");
                Console.Write(t.Valoare);
            }

            Console.WriteLine();

            indentare += ultimulNod ? "    " : "|   ";

            var ultimulCopil = nod.GetCopii().LastOrDefault();

            foreach (var c in nod.GetCopii())
            {
                AfiseazaArbore(c, indentare, c == ultimulCopil);
            }
        }
    }


    enum TipAtomLexical
    {
        DoubleAtomLexical,
        StringAtomLexical,
        NumarAtomLexical,
        PlusAtomLexical,
        MinusAtomLexical,
        StarAtomLexical,
        SlashAtomLexical,
        EgalAtomLexical,
        PunctVirgulaAtomLexical,
        VirgulaAtomLexical,
        ParantezaDeschisaAtomLexical,
        ParantezaInchisaAtomLexical,
        SpatiuAtomLexical,
        TerminatorSirAtomLexical,
        InvalidAtomLexical,
        ExpresieNumericaAtomLexical,
        ExpresieDoubleAtomLexical,
        ExpresieBinaraAtomLexical,
        ExpresieStringAtomLexical,
        ExpresieGhilimele,
        ExpresieInvalida,
        ExpresieParanteze,
        GhilimeleAtomLexical,
        CuvantCheieAtomlexical,
        VariabilaAtomLexical
    }


    abstract class NodSintactic
    {
        public abstract TipAtomLexical Tip { get; }
        public abstract IEnumerable<NodSintactic> GetCopii();
    }

    abstract class Expresie : NodSintactic
    {

    }

    class ExpresieParanteze : Expresie
    {
        public AtomLexical ParantezaDeschisa { get; }
        public Expresie Expresie { get; }
        public AtomLexical ParantezaInchisa { get; }

        public ExpresieParanteze(AtomLexical parantezaD, Expresie expresie, AtomLexical parantezaI)
        {
            ParantezaDeschisa = parantezaD;
            Expresie = expresie;
            ParantezaInchisa = parantezaI;
        }

        public override TipAtomLexical Tip => TipAtomLexical.ExpresieParanteze;

        public override IEnumerable<NodSintactic> GetCopii()
        {
            yield return ParantezaDeschisa;
            yield return Expresie;
            yield return ParantezaInchisa;
        }
    }

    class ExpresieGhilimele : Expresie
    {
        public AtomLexical GhilimeleDeschise { get; }
        public Expresie Expresie { get; }
        public AtomLexical GhilimeleInchise { get; }

        public ExpresieGhilimele(AtomLexical parantezaD, Expresie expresie, AtomLexical parantezaI)
        {
            GhilimeleInchise = parantezaD;
            Expresie = expresie;
            GhilimeleDeschise = parantezaI;
        }

        public override TipAtomLexical Tip => TipAtomLexical.ExpresieGhilimele;

        public override IEnumerable<NodSintactic> GetCopii()
        {
            yield return GhilimeleDeschise;
            yield return Expresie;
            yield return GhilimeleInchise;
        }
    }

    class ExpresieNumerica : Expresie
    {
        public AtomLexical Numar;

        public ExpresieNumerica(AtomLexical numar)
        {
            Numar = numar;
        }

        public override TipAtomLexical Tip => TipAtomLexical.ExpresieNumericaAtomLexical;
        public override IEnumerable<NodSintactic> GetCopii()
        {
            yield return Numar;
        }
    }


    class ExpresieDouble : Expresie
    {
        public AtomLexical Numar;

        public ExpresieDouble(AtomLexical numar)
        {
            Numar = numar;
        }

        public override TipAtomLexical Tip => TipAtomLexical.ExpresieDoubleAtomLexical;
        public override IEnumerable<NodSintactic> GetCopii()
        {
            yield return Numar;
        }
    }
    class ExpresieBinara : Expresie
    {
        public Expresie Stanga { get; }
        public Expresie Dreapta { get; }

        public AtomLexical Operator { get; }

        public ExpresieBinara(Expresie stanga, AtomLexical operatorExpresie, Expresie dreapta)
        {
            Stanga = stanga;
            Dreapta = dreapta;
            Operator = operatorExpresie;
        }


        public override TipAtomLexical Tip => TipAtomLexical.ExpresieBinaraAtomLexical;

        public override IEnumerable<NodSintactic> GetCopii()
        {
            yield return Stanga;
            yield return Operator;
            yield return Dreapta;

        }
    }

    class ExpresieString : Expresie
    {
        public AtomLexical String;

        public ExpresieString(AtomLexical string1)
        {
            String = string1;
        }
        public override TipAtomLexical Tip => TipAtomLexical.ExpresieStringAtomLexical;

        public override IEnumerable<NodSintactic> GetCopii()
        {
            yield return String;
        }

    }

    class ExpresieInvalida : Expresie
    {
        public string String;

        public ExpresieInvalida(string string1)
        {
            String = string1;
        }
        public override TipAtomLexical Tip => TipAtomLexical.ExpresieInvalida;

        public override IEnumerable<NodSintactic> GetCopii()
        {
            return Enumerable.Empty<NodSintactic>();
        }

    }
}
