using System;
using System.Collections.Generic;
using ModelTransportuPublicznego.Model;
using System.IO;
using System.Linq;

namespace ModelTransportuPublicznego.Implementacja.Autobusy
{
    public class AutobusLiniowy : Autobus {
        private readonly double przyspieszenie;
        private readonly double trasaHamowania100;
        private readonly double predkoscMaksymalna;
        private readonly SortedDictionary<int, int> spowolnieniaPrzyspieszenia;
        private readonly SortedDictionary<int, int> wydluzenieHamowania;

        public double Przyspieszenie => przyspieszenie;

        public AutobusLiniowy() : base()
        {
            spowolnieniaPrzyspieszenia = new SortedDictionary<int, int>();
            wydluzenieHamowania = new SortedDictionary<int, int>();
        }

        public AutobusLiniowy(string idAutobusu, int maksymalnaPojemnosc, int iloscDzwi, double przyspieszenie, 
            double trasaHamowania100, double predkoscMaksymalna, double dlugoscAutobusu, string sciezkaPlikuKonfiguracyjnego = null, IEnumerable<KeyValuePair<int, int>> spowolnieniaPrzyspieszenia = null, 
            IEnumerable<KeyValuePair<int, int>> wydluzenieHamowania = null) : base(idAutobusu, maksymalnaPojemnosc, iloscDzwi, dlugoscAutobusu, sciezkaPlikuKonfiguracyjnego) {

            this.przyspieszenie = przyspieszenie;
            this.trasaHamowania100 = trasaHamowania100;
            this.predkoscMaksymalna = predkoscMaksymalna;
            this.dlugoscAutobusu = dlugoscAutobusu;
            this.spowolnieniaPrzyspieszenia = new SortedDictionary<int, int>();
            this.wydluzenieHamowania = new SortedDictionary<int, int>();
            this.sciezkaPlikuKonfiguracyjnego = sciezkaPlikuKonfiguracyjnego;

            if (spowolnieniaPrzyspieszenia != null)
            {
                foreach (var kvp in spowolnieniaPrzyspieszenia)
                {
                    this.spowolnieniaPrzyspieszenia.Add(kvp.Key, kvp.Value);
                }
            }

            if (wydluzenieHamowania != null)
            {
                foreach (var kvp in wydluzenieHamowania)
                {
                    this.wydluzenieHamowania.Add(kvp.Key, kvp.Value);
                }
            }

            //aHamowania = (Math.Pow(100, 2) / 3.6) / (2 * trasaHamowania100);
        }

        public AutobusLiniowy(string idAutobusu, int maksymalnaPojemnosc, int iloscDzwi, double przyspieszenie, 
            double trasaHamowania100, double predkoscMaksymalna, double dlugoscAutobusu, IEnumerable<Pasazer> obecniPasazerowie) 
            : this (idAutobusu, maksymalnaPojemnosc, iloscDzwi, przyspieszenie, trasaHamowania100, predkoscMaksymalna, dlugoscAutobusu) {

            foreach (var pasazer in obecniPasazerowie) {
                this.obecniPasazerowie.Add(pasazer);
            }
        }

        public override int PrzejedzTrase(Trasa trasa) {
            var tPrzysp = CzasPrzyspieszania(trasa);
            var tHamowania = CzasHamowania(trasa);

            var sPrzysp = (AktualnePrzyspieszenie() * Math.Pow(tPrzysp, 2));
            var sHamowania = (AktualneHamowanie() * Math.Pow(tHamowania, 2)) / 2;
            var sPrzejazduVMax = trasa.DystansTrasy - (sPrzysp + sHamowania);

            if (sPrzejazduVMax > 0)
            {
                double tVMax = CzasPrzejazduZVMax(trasa);
                return (int)Math.Ceiling(tPrzysp + tVMax + tHamowania);
            }
            else
            {
                var aktualnePrzyspieszenie = AktualnePrzyspieszenie();
                tPrzysp = (AktualneHamowanie() * Math.Sqrt((2 * trasa.DystansTrasy) / aktualnePrzyspieszenie)) / (przyspieszenie + aktualnePrzyspieszenie);
                double vMax = aktualnePrzyspieszenie * tPrzysp;
                tHamowania = CzasHamowania(vMax);

                return (int)Math.Ceiling(tPrzysp + tHamowania);
            }
        }

        protected virtual double CzasPrzejazduZVMax(Trasa trasa)
        {
            return trasa.DystansTrasy / Math.Min(predkoscMaksymalna, trasa.VMaxTrasy);
        }

        protected virtual double CzasPrzyspieszania(Trasa trasa)
        {
            return Math.Min(predkoscMaksymalna, trasa.VMaxTrasy) / AktualnePrzyspieszenie();
        }

        protected virtual double CzasHamowania(Trasa trasa)
        {
            return Math.Min(predkoscMaksymalna, trasa.VMaxTrasy) / (AktualneHamowanie());
        }

        protected virtual double CzasHamowania(double predkosc)
        {
            return predkosc / AktualneHamowanie();
        }

        protected virtual double AktualnePrzyspieszenie()
        {
            return przyspieszenie * (1 + AktualneSpowolnieniePrzyspieszenia());
        }

        protected virtual double AktualneHamowanie()
        {
            return (Math.Pow(100, 2) / 3.6) / (2 * AktualnaTrasaHamowania());
        }

        protected virtual double AktualnaTrasaHamowania()
        {
            var procentZapelnienia = ProcentZapelnieniaAutobusu;
            double zwiekszenieTrasyHamowania = 0;
            foreach (var kvp in wydluzenieHamowania)
            {
                if (kvp.Key > procentZapelnienia)
                {
                    break;
                }

                zwiekszenieTrasyHamowania = kvp.Value;
            }

            return trasaHamowania100 * (1 + zwiekszenieTrasyHamowania);
        }

        protected virtual double AktualneSpowolnieniePrzyspieszenia()
        {
            var procentZapelnienia = ProcentZapelnieniaAutobusu;
            double spowolnienie = 0;
            foreach (var kvp in spowolnieniaPrzyspieszenia)
            {
                if (kvp.Key > procentZapelnienia)
                {
                    return spowolnienie;
                } else
                {
                    spowolnienie = kvp.Value;
                }
            }

            return spowolnienie;
        }

        public virtual bool Zapisz(StreamWriter sw)
        {
            try
            {
                sw.WriteLine(string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}", modelAutobusu, maksymalnaPojemnosc, iloscDzwi, przyspieszenie, trasaHamowania100, predkoscMaksymalna, dlugoscAutobusu));

                var last = spowolnieniaPrzyspieszenia.Last();
                foreach (var kvp in spowolnieniaPrzyspieszenia)
                {
                    if (kvp.Key == 0 && kvp.Value == 0) continue;
                    sw.Write("{0}-{1}", kvp.Key, kvp.Value);
                    if (kvp.Key != last.Key)
                    {
                        sw.Write("|");
                    }
                }

                sw.WriteLine();
                last = wydluzenieHamowania.Last();
                foreach (var kvp in wydluzenieHamowania)
                {
                    if (kvp.Key == 0 && kvp.Value == 0) continue;
                    sw.Write("{0}-{1}", kvp.Key, kvp.Value);
                    if (kvp.Key != last.Key)
                    {
                        sw.Write("|");
                    }
                }
            } catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }

            return true;
        }

        public static AutobusLiniowy OdczytajPlik(string nazwaPliku)
        {
            string[] stale, przyspieszenia, hamowania;

            using (var sr = File.OpenText(nazwaPliku))
            {
                stale = sr.ReadLine().Split('|');
                przyspieszenia = sr.ReadLine().Split('|');
                hamowania = sr.ReadLine().Split('|');
            }

            var rezultat = new AutobusLiniowy(stale[0], Convert.ToInt32(stale[1]), Convert.ToInt32(stale[2]), Convert.ToDouble(stale[3]), 
                Convert.ToDouble(stale[4]), Convert.ToDouble(stale[5]), Convert.ToDouble(stale[6]));

            foreach (var tekst in przyspieszenia)
            {
                var elems = tekst.Split('-');
                rezultat.DodajProgWartoscSpowolnieniaPrzyspieszania(Convert.ToInt32(elems[0]), Convert.ToInt32(elems[1]));
            }

            foreach (var tekst in hamowania)
            {
                var elems = tekst.Split('-');
                rezultat.DodajProgWartoscWydluzeniaHamowania(Convert.ToInt32(elems[0]), Convert.ToInt32(elems[1]));
            }

            return rezultat;
        }

        public virtual void DodajProgWartoscSpowolnieniaPrzyspieszania(int prog, int wartosc)
        {
            spowolnieniaPrzyspieszenia.Add(prog, wartosc);
        }

        public virtual void DodajProgWartoscSpowolnieniaPrzyspieszania(IEnumerable<KeyValuePair<int, int>> wartosci)
        {
            foreach (var kvp in wartosci)
            {
                DodajProgWartoscSpowolnieniaPrzyspieszania(kvp.Key, kvp.Value);
            }
        }

        public virtual void DodajProgWartoscWydluzeniaHamowania(int prog, int wartosc)
        {
            wydluzenieHamowania.Add(prog, wartosc);
        }

        public virtual void DodajProgWartoscWydluzeniaHamowania(IEnumerable<KeyValuePair<int, int>> wartosci)
        {
            foreach (var kvp in wartosci)
            {
                DodajProgWartoscWydluzeniaHamowania(kvp.Key, kvp.Value);
            }
        }

        public virtual void WyczyscProgiSpowolnienia()
        {
            spowolnieniaPrzyspieszenia.Clear();
            wydluzenieHamowania.Clear();
        }
    }
}