using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Proiect_LFT
{
    internal class Parser
    {
        private List<AtomLexical> atomiLexicali = new List<AtomLexical> ();
        private int index;
        public List<string> erori = new List<string>();
        static public List<Variabila> variabila= new List<Variabila> ();


        public Parser(string input)
        {
            Lexer l = new Lexer(input);
            AtomLexical at;
            do
            {
                at = l.Atom();
                if(at.Tip != TipAtomLexical.SpatiuAtomLexical && at.Tip != TipAtomLexical.InvalidAtomLexical)
                {
                    atomiLexicali.Add(at);
                }

            } while (at.Tip != TipAtomLexical.TerminatorSirAtomLexical);

            erori.AddRange(l.Erori);
        }

        private AtomLexical GetAtomAvans(int k)
        {
            if (index + k > atomiLexicali.Count)
                return atomiLexicali[atomiLexicali.Count - 1];
            else
                return atomiLexicali[index + k];
        }

        private AtomLexical AtomCurent => GetAtomAvans(0);

        private AtomLexical AtomCurentSiIncrementeaza()
        {
            AtomLexical curent = AtomCurent;
            index++;
            return curent;
        }

        private AtomLexical VerificaTip(TipAtomLexical tip)
        {
            if (tip != AtomCurent.Tip)
            {
                erori.Add($"Atom lexical invalid. Atomul curent <{tip}>. Se asteapta <{AtomCurent.Tip}>.");
                return new AtomLexical(TipAtomLexical.InvalidAtomLexical, index, null, null);
            }

            return AtomCurentSiIncrementeaza();
        }

        public int tipInt(AtomLexical l)
        {
            if (l.Text == "int")
            {
                return 1;

            }
            else if (l.Text == "double")
            {
                return 2;
            }
            else if (l.Text == "string")
            {
                return 3;
            }
            else
            {
                return 0;
            }
        }

        public Expresie Parseaza()
        {

            return ParseazaTermen();

           // return stanga;
        }

        public Expresie ParseazaTermen()
        {
            var stanga = ParseazaFactor();

            if (stanga is ExpresieGhilimele && AtomCurent.Tip == TipAtomLexical.MinusAtomLexical)
            {
                erori.Add($"Nu se poate executa operatia.");
                stanga = new ExpresieInvalida("Nu se poate efectua operatia. Tipuri de date diferite.");
                return stanga;
            }

            while (AtomCurent.Tip == TipAtomLexical.PlusAtomLexical)
            {
                var operatorExpr = AtomCurentSiIncrementeaza();
                var dreapta = ParseazaFactor();

                if (stanga is ExpresieInvalida || dreapta is ExpresieInvalida)
                {
                    stanga = new ExpresieInvalida("Nu se poate efectua operatia. Tipuri de date diferite.");
                    return stanga;
                }

                if ((stanga is ExpresieGhilimele && !(dreapta is ExpresieGhilimele)) || (dreapta is ExpresieGhilimele && !(stanga is ExpresieGhilimele)))
                {
                    erori.Add($"Nu se poate executa operatia.");
                    stanga = new ExpresieInvalida("Nu se poate efectua operatia. Tipuri de date diferite.");
                }
                else
                {
                    stanga = new ExpresieBinara(stanga, operatorExpr, dreapta);
                }
            }

                return stanga;
        }

        public Expresie ParseazaFactor()
        {
            var stanga = ParseazaExpresie();
            while (
                AtomCurent.Tip == TipAtomLexical.StarAtomLexical ||
                AtomCurent.Tip == TipAtomLexical.SlashAtomLexical)
            {
                var operatorExpr = AtomCurentSiIncrementeaza();
                var dreapta = ParseazaExpresie();

                if (stanga is ExpresieInvalida || dreapta is ExpresieInvalida)
                {
                    stanga = new ExpresieInvalida("Nu se poate efectua operatia. Tipuri de date diferite.");
                    return stanga;
                }

                if ((stanga is ExpresieGhilimele && !(dreapta is ExpresieGhilimele)) || (dreapta is ExpresieGhilimele && !(stanga is ExpresieGhilimele)) || (dreapta is ExpresieGhilimele && stanga is ExpresieGhilimele))
                {
                    erori.Add($"Nu se poate executa operatia.");
                    stanga = new ExpresieInvalida("Nu se poate efectua operatia. Tipuri de date diferite.");
                }
                else
                {
                    stanga = new ExpresieBinara(stanga, operatorExpr, dreapta);
                }
            }

            return stanga;
        }

        private Expresie ParseazaExpresie()
        {
            if(AtomCurent.Tip==TipAtomLexical.ParantezaDeschisaAtomLexical)
            {
                var parantezaDeschisa = AtomCurentSiIncrementeaza();
                var expresie = Parseaza();
                var parantezaInchisa = VerificaTip(TipAtomLexical.ParantezaInchisaAtomLexical);
                return new ExpresieParanteze(parantezaDeschisa, expresie, parantezaInchisa);
            }

           
            if (AtomCurent.Tip==TipAtomLexical.GhilimeleAtomLexical)
            {
                var ghilimeleDeschise = AtomCurentSiIncrementeaza();
                var var = Parseaza();
                var ghimileleInchise = VerificaTip(TipAtomLexical.GhilimeleAtomLexical);
                return new ExpresieGhilimele(ghilimeleDeschise, var, ghimileleInchise);
            }

            if(AtomCurent.Tip==TipAtomLexical.StringAtomLexical)
            {
                var s = VerificaTip(TipAtomLexical.StringAtomLexical);
                return new ExpresieString(s);
            }

            if(AtomCurent.Tip==TipAtomLexical.DoubleAtomLexical)
            {
                var s = VerificaTip(TipAtomLexical.DoubleAtomLexical);
                return new ExpresieDouble(s);
            }

            if(AtomCurent.Tip==TipAtomLexical.CuvantCheieAtomlexical)
            {
                Variabila v = new Variabila();
                int tip = 0;

                if(AtomCurent.Text=="int")
                {
                    tip = 1;
                    v.setType(1);   
                }
                else if(AtomCurent.Text=="double")
                {
                    tip = 2;
                    v.setType(2);
                }
                else if(AtomCurent.Text=="string")
                {
                    tip = 3;
                    v.setType(3);
                }
                else
                {
                    erori.Add($"Cuvant cheie invalid. Tip curent <{AtomCurent.Text}>. Se asteapta <int, double, string>.");
                    return new ExpresieInvalida("Cuvant cheie invalid.");
                }

                index++;
                if (AtomCurent.Tip == TipAtomLexical.VariabilaAtomLexical)
                {
                    int counter = 0;
                    foreach (var i in variabila)
                    {
                        if (i.getName() == AtomCurent.Text && i.getType()==tip)
                        {
                            erori.Add($"Variabila {i.getName()} este deja definita, nu poti efectua redefinire. ");
                            return new ExpresieInvalida("Variabila existenta.");
                        }
                        else if(i.getName() == AtomCurent.Text && i.getType() != tip)
                        {
                            i.setType(tip);
                            i.setVal(null);
                        }
                    }
                    v.setName(AtomCurent.Text);

                    index++;
                    if (AtomCurent.Tip == TipAtomLexical.EgalAtomLexical)
                    {
                        int counter2 = 0;
                        object value = 0;
                        index++;
                        int op = index;
                        int minus = 0;

                        if(AtomCurent.Tip==TipAtomLexical.MinusAtomLexical)
                        {
                            minus++;
                            index++;
                        }

                        if(AtomCurent.Tip==TipAtomLexical.GhilimeleAtomLexical)
                        {
                            index++;
                            counter2++;
                        }

                        if (AtomCurent.Tip == TipAtomLexical.NumarAtomLexical && tip !=3)
                        {
                            if (tip == 1)
                            {
                                if (minus > 0)
                                {
                                    value = -(int)AtomCurent.Valoare;
                                }
                                else
                                {
                                    value = (int)AtomCurent.Valoare;
                                }
                            }
                            else if (tip == 2)
                            {
                                if (minus > 0)
                                {
                                    value = -(float)AtomCurent.Valoare;
                                }
                                else
                                {
                                    value = (float)AtomCurent.Valoare;
                                }
                            }

                            v.setVal(value);
                            v.setType(tip);
                            variabila.Add(v);
                            v = new Variabila();
                            counter = 1;
                            index++;
                        }
                        else if (AtomCurent.Tip == TipAtomLexical.DoubleAtomLexical && tip !=3)
                        {
                            if (tip == 1)
                            {
                                if (minus > 0)
                                {
                                    value = -(int)AtomCurent.Valoare;
                                }
                                else
                                {
                                    value = (int)AtomCurent.Valoare;
                                }
                            }
                            else if (tip == 2)
                            {
                                if (minus > 0)
                                {
                                    value = -(float)AtomCurent.Valoare;
                                }
                                else
                                {
                                    value = (float)AtomCurent.Valoare;
                                }
                            }
                            
                            v.setVal(value);
                            v.setType(tip);
                            variabila.Add(v);
                            v = new Variabila();
                            counter = 1;
                            index++;
                        }
                        else if (AtomCurent.Tip == TipAtomLexical.StringAtomLexical && tip == 3)
                        {
                            if(minus>0)
                            {
                                erori.Add($"Nu poti initializa un string cu o valoar negativa.");
                                return new ExpresieInvalida("Minus erorr.");
                            }

                            if(counter2==0)
                            {
                                erori.Add($"Nu au fost deschise ghilimele.");
                                return new ExpresieInvalida("Ghilimele erorr.");
                            }

                            value = AtomCurent.Valoare;
                            v.setVal(value);
                            v.setType(tip);
                            variabila.Add(v);
                            v = new Variabila();
                            counter = 1;
                            index++;


                            if (AtomCurent.Tip == TipAtomLexical.GhilimeleAtomLexical)
                            {
                                index++;
                                counter2++;
                            }
                            else
                            {
                                erori.Add($"Nu au fost inchise ghilimelele.");
                                return new ExpresieInvalida("ghilimele erorr.");
                            }

                        }
                        else if (AtomCurent.Tip==TipAtomLexical.VariabilaAtomLexical)
                        {
                            index++;
                            if (AtomCurent.Tip == TipAtomLexical.MinusAtomLexical || AtomCurent.Tip == TipAtomLexical.PlusAtomLexical || AtomCurent.Tip == TipAtomLexical.SlashAtomLexical || AtomCurent.Tip == TipAtomLexical.StarAtomLexical)
                            {
                                index = op;
                                Expresie ex = Parseaza();
                                var e = new Evaluator(ex);
                                var rezultat = e.Evalueaza();
                                if(tip==1)
                                {
                                    v.setVal((int)rezultat);
                                }
                                else if(tip==2)
                                {
                                    v.setVal((float)rezultat);
                                }
                                else
                                {
                                    v.setVal(rezultat);
                                }

                                v.setType(tip);
                                variabila.Add(v);
                                v = new Variabila();
                                counter = 1;
                                //index++;
                                
                            }

                        }
                        else
                        {
                            erori.Add($"Valoare invalida dupa egal.");
                            return new ExpresieInvalida("Caracter invalid dupa egal.");
                        }
                    }

                    if(counter==0)
                    {
                        variabila.Add(v);
                        v = new Variabila();
                        counter = 0;
                    }

                    if (AtomCurent.Tip == TipAtomLexical.VirgulaAtomLexical)
                    {
                        
                        index++;
                        v.setType(tip);
                        if (AtomCurent.Tip == TipAtomLexical.VariabilaAtomLexical)
                        {
                            foreach (var i in variabila)
                            {
                                if (i.getName() == AtomCurent.Text && i.getType()==tip)
                                {
                                    erori.Add($"Variabila {i.getName()} este deja definita, nu poti efectua redefinire. ");
                                    return new ExpresieInvalida("Variabila existenta.");
                                }
                            }
                            v.setName(AtomCurent.Text);

                            index++;
                            if (AtomCurent.Tip == TipAtomLexical.EgalAtomLexical)
                            {
                                int counter2 = 0;
                                object value = 0;
                                index++;
                                int minus = 0;

                                if (AtomCurent.Tip == TipAtomLexical.MinusAtomLexical)
                                {
                                    minus++;
                                    index++;
                                }

                                if (AtomCurent.Tip == TipAtomLexical.GhilimeleAtomLexical)
                                {
                                    index++;
                                    counter2++;
                                }

                                if (AtomCurent.Tip == TipAtomLexical.NumarAtomLexical && tip!=3)
                                {

                                    if (tip == 1)
                                    {
                                        if (minus > 0)
                                        {
                                            value = -(int)AtomCurent.Valoare;
                                        }
                                        else
                                        {
                                            value = (int)AtomCurent.Valoare;
                                        }
                                    }
                                    else if (tip == 2)
                                    {
                                        if (minus > 0)
                                        {
                                            value = -(float)AtomCurent.Valoare;
                                        }
                                        else
                                        {
                                            value = (float)AtomCurent.Valoare;
                                        }
                                    }

                                    v.setVal(value);
                                    v.setType(tip);
                                    variabila.Add(v);
                                    v = new Variabila();
                                    counter = 1;
                                    index++;
                                }
                                else if (AtomCurent.Tip == TipAtomLexical.DoubleAtomLexical && tip != 3)
                                {
                                    if (tip == 1)
                                    {
                                        if (minus > 0)
                                        {
                                            value = -(int)AtomCurent.Valoare;
                                        }
                                        else
                                        {
                                            value = (int)AtomCurent.Valoare;
                                        }
                                    }
                                    else if (tip == 2)
                                    {
                                        if (minus > 0)
                                        {
                                            value = -(float)AtomCurent.Valoare;
                                        }
                                        else
                                        {
                                            value = (float)AtomCurent.Valoare;
                                        }
                                    }

                                    v.setVal(value);
                                    v.setType(tip);
                                    variabila.Add(v);
                                    v = new Variabila();
                                    counter = 1;
                                    index++;
                                }
                                else if (AtomCurent.Tip == TipAtomLexical.StringAtomLexical && tip == 3)
                                {
                                    if (minus > 0)
                                    {
                                        erori.Add($"Nu poti initializa un string cu o valoar negativa.");
                                        return new ExpresieInvalida("minus erorr.");
                                    }

                                    if (counter2 == 0)
                                    {
                                        erori.Add($"Nu au fost deschise ghilimele.");
                                        return new ExpresieInvalida("ghilimele erorr.");
                                    }

                                    value = AtomCurent.Valoare;
                                    v.setVal(value);
                                    v.setType(tip);
                                    variabila.Add(v);
                                    v = new Variabila();
                                    counter = 1;
                                    index++;

                                    if (AtomCurent.Tip == TipAtomLexical.GhilimeleAtomLexical)
                                    {
                                        index++;
                                        counter2++;
                                    }
                                    else
                                    {
                                        erori.Add($"Nu au fost inchise ghilimelele.");
                                        return new ExpresieInvalida("ghilimele erorr.");
                                    }
                                }
                                else
                                {
                                    erori.Add($"Valoare invalida dupa egal.");
                                    return new ExpresieInvalida("Caracter invalid dupa egal.");
                                }

                               //index++;
                            }

                            if (counter == 0)
                            {
                                variabila.Add(v);
                                v = new Variabila();
                                counter = 0;
                            }

                            while (AtomCurent.Tip == TipAtomLexical.VirgulaAtomLexical)
                            {

                                index++;
                                v.setType(tip);
                                if (AtomCurent.Tip == TipAtomLexical.VariabilaAtomLexical)
                                {
                                    foreach (var i in variabila)
                                    {
                                        if (i.getName() == AtomCurent.Text)
                                        {
                                            erori.Add($"Variabila {i.getName()} este deja definita, nu poti efectua redefinire. ");
                                            return new ExpresieInvalida("Variabila existenta.");
                                        }
                                    }
                                    v.setName(AtomCurent.Text);

                                    index++;
                                    if (AtomCurent.Tip == TipAtomLexical.EgalAtomLexical)
                                    {
                                        object value = 0;
                                        index++;
                                        if (AtomCurent.Tip == TipAtomLexical.NumarAtomLexical)
                                        {
                                            value = AtomCurent.Valoare;
                                            v.setVal(value);
                                            variabila.Add(v);
                                            counter = 1;
                                        }
                                        else
                                        {
                                            erori.Add($"Expresie invalida: Nicio valoare dupa <=>.");
                                            return new ExpresieInvalida("Caracter invalid dupa egal.");
                                        }
                                        index++;
                                    }
                                }
                                else
                                {
                                    erori.Add($"Expresie invalida: Nicio variabila dupa <,>.");
                                    return new ExpresieInvalida("Variabila lipsa.");
                                }

                                if (counter == 0)
                                {
                                    variabila.Add(v);
                                    v = new Variabila();
                                    counter = 0;
                                }
                            }

                        }
                        else
                        {
                            erori.Add($"Expresie invalida: Nicio variabila dupa <,>.");
                            return new ExpresieInvalida("Variabila lipsa.");
                        }
                    }
                    if (AtomCurent.Tip != TipAtomLexical.PunctVirgulaAtomLexical)
                    {
                        erori.Add($"Expresie invalida: Lipseste <;>.");
                        return new ExpresieInvalida("Caracter lipsa.");
                    }

                }
                else
                {
                    erori.Add($"Expresie invalida.");
                    return new ExpresieInvalida("Expresie invalida.");
                }
            }

            if(AtomCurent.Tip==TipAtomLexical.VariabilaAtomLexical)
            {
                int ok = 0;
                object value;
                foreach (var i in variabila)
                {
                    if(AtomCurent.Text==i.getName())
                    {
                        ok++;
                        index++;
                        if(AtomCurent.Tip==TipAtomLexical.PunctVirgulaAtomLexical)
                        {
                            if(i.getType()==1)
                            {
                                AtomLexical a = new AtomLexical(TipAtomLexical.NumarAtomLexical, index, i.getName(), i.getVal());
                                return new ExpresieNumerica(a); 
                            }
                            else if(i.getType()==2)
                            {
                                AtomLexical a = new AtomLexical(TipAtomLexical.DoubleAtomLexical, index, i.getName(), i.getVal());
                                return new ExpresieDouble(a);
                            }
                            else if(i.getType()==3)
                            {
                                AtomLexical a = new AtomLexical(TipAtomLexical.StringAtomLexical, index, i.getName(), i.getVal());
                                AtomLexical b = new AtomLexical(TipAtomLexical.GhilimeleAtomLexical, index, "", "\"");
                                AtomLexical c = new AtomLexical(TipAtomLexical.GhilimeleAtomLexical, index, "", "\"");

                                ExpresieString d = new ExpresieString(a);
                                return new ExpresieGhilimele(b,d,c);
                            }
                        }
                        else if(AtomCurent.Tip==TipAtomLexical.EgalAtomLexical)
                        {
                            index++;
                            int minus = 0;

                            if (AtomCurent.Tip == TipAtomLexical.MinusAtomLexical)
                            {
                                minus++;
                                index++;
                            }

                            if (AtomCurent.Tip==TipAtomLexical.VariabilaAtomLexical)
                            {
                                int oux = 0;
                                int op = index;
                                foreach (var j in variabila)
                                {
                                    if (AtomCurent.Text == j.getName())
                                    {
                                        oux++;
                                        index++;
                                        if (AtomCurent.Tip == TipAtomLexical.PunctVirgulaAtomLexical)
                                        {
                                            i.setVal(j.getVal());
                                        }
                                        else if (AtomCurent.Tip == TipAtomLexical.MinusAtomLexical || AtomCurent.Tip == TipAtomLexical.PlusAtomLexical || AtomCurent.Tip == TipAtomLexical.SlashAtomLexical || AtomCurent.Tip == TipAtomLexical.StarAtomLexical)
                                        {
                                                index = op;
                                                Expresie ex = Parseaza();
                                                var e = new Evaluator(ex);
                                                var rezultat = e.Evalueaza();
                                                if(i.getType()==1)
                                                {
                                                    i.setVal((int)rezultat);
                                                }
                                                else if(i.getType()==2)
                                                {
                                                    i.setVal((float)rezultat);
                                                }
                                                else
                                                {
                                                i.setVal(rezultat);
                                                }
                                                
                                        }
                                        else
                                        {
                                            erori.Add($"Expresie invalida: Nicio variabila dupa <,>.");
                                            return new ExpresieInvalida("Variabila lipsa.");
                                        }
                                    }

                                }
                                if (oux == 0)
                                {
                                    erori.Add($"Nu exista aceasta variabila definita.");
                                    return new ExpresieInvalida("Variabila nedefinita.");
                                }
                            }
                            else if(AtomCurent.Tip==TipAtomLexical.NumarAtomLexical && i.getType() !=3)
                            {

                                if (minus > 0)
                                {
                                    value = -(int)AtomCurent.Valoare;
                                }
                                else
                                {
                                    value = (int)AtomCurent.Valoare;
                                }

                                i.setVal(value);
                            }
                            else if(AtomCurent.Tip == TipAtomLexical.DoubleAtomLexical && i.getType() !=3)
                            {
                                if (minus > 0)
                                {
                                    value = -(float)AtomCurent.Valoare;
                                }
                                else
                                {
                                    value = (float)AtomCurent.Valoare;
                                }

                                i.setVal(value);
                            }
                            else if(AtomCurent.Tip == TipAtomLexical.GhilimeleAtomLexical && i.getType() == 3)
                            {
                                if (minus > 0)
                                {
                                    erori.Add($"Nu poti initializa un string cu o valoar negativa.");
                                    return new ExpresieInvalida("Minus erorr.");
                                }

                                index++;
                                i.setVal(AtomCurent.Valoare);
                                index++;
                            }
                        }

                        
                        if (i.getType() == 1)
                        {
                            AtomLexical a = new AtomLexical(TipAtomLexical.NumarAtomLexical, index, i.getName(), i.getVal());
                            return new ExpresieNumerica(a);
                        }
                        else if (i.getType() == 2)
                        {
                            AtomLexical a = new AtomLexical(TipAtomLexical.DoubleAtomLexical, index, i.getName(), i.getVal());
                            return new ExpresieDouble(a);
                        }
                        else if (i.getType() == 3)
                        {
                            AtomLexical a = new AtomLexical(TipAtomLexical.StringAtomLexical, index, i.getName(), i.getVal());
                            AtomLexical b = new AtomLexical(TipAtomLexical.GhilimeleAtomLexical, index, "", "\"");
                            AtomLexical c = new AtomLexical(TipAtomLexical.GhilimeleAtomLexical, index, "", "\"");

                            ExpresieString d = new ExpresieString(a);
                            return new ExpresieGhilimele(b, d, c);
                        }
                    }

                }
                if (ok == 0)
                {
                    erori.Add($"Variabila nu a fost definita.");
                    return new ExpresieInvalida("Variabila nedefinita.");
                }
            }

            if (AtomCurent.Tip == TipAtomLexical.NumarAtomLexical)
            {
                AtomLexical nr = VerificaTip(TipAtomLexical.NumarAtomLexical);
                return new ExpresieNumerica(nr);
            }

            return new ExpresieInvalida("Sfarsit.");

        }
    }
}
