using System;
using System.Collections.Generic;
using System.Linq;
using ModelTransportuPublicznego.Implementacja.Graf;
using ModelTransportuPublicznego.Model;
using ModelTransportuPublicznego.Model.Przystanek;

namespace ModelTransportuPublicznego.Implementacja.Pasazerowie
{
    public class PrymitywnyPasazerDijkstry : Pasazer
    {

        private static List<List<Przystanek>> obliczoneTrasy = new List<List<Przystanek>>();
        private Graf<TimeSpan> graf;
        private List<Przystanek> trasaPasazera;

        private PrymitywnyPasazerDijkstry()
        {
            trasaPasazera = new List<Przystanek>();
        }

        public PrymitywnyPasazerDijkstry(IEnumerable<Przystanek> trasaPasazera) : this()
        {
            foreach (var p in trasaPasazera)
            {
                this.trasaPasazera.Add(p);
            }
        }

        public PrymitywnyPasazerDijkstry(IEnumerable<Przystanek> trasaPasazera, int czasWsiadania, int czasWysiadania) : this(
            trasaPasazera)
        {
            this.czasWsiadania = czasWsiadania;
            this.czasWysiadania = czasWysiadania;
        }

        public PrymitywnyPasazerDijkstry(int czasWsiadania, int czasWysiadania, Przystanek przystanekPoczatkowy, Przystanek przystanekKoncowy, Graf.Graf<TimeSpan> graf)
            : base(czasWsiadania, czasWysiadania, przystanekPoczatkowy, przystanekKoncowy)
        {
            this.graf = graf;
            trasaPasazera = ZnajdzTrase(graf);
            graf.ZresetujGraf(TimeSpan.MaxValue);

            if (trasaPasazera.Count > 1)
            {
                przystanekPoczatkowy = trasaPasazera[0];
                przystanekKoncowy = trasaPasazera[trasaPasazera.Count - 1];
                obecnyPrzystanek = przystanekPoczatkowy;
            }

            UsunPierwszyElementTrasy(); // Usuwanie pierwszego elementu dla ulatwienia implementacji logiki porozy.
        }

        public PrymitywnyPasazerDijkstry(int czasWsiadania, int czasWysiadania, Przystanek przystanekPoczatkowy,
            Przystanek przystanekKoncowy, Graf<TimeSpan> graf, TimeSpan czasStworzenia)
            : base(czasWsiadania, czasWysiadania, przystanekPoczatkowy, przystanekKoncowy)
        {
            this.graf = graf;
            trasaPasazera = ZnajdzTrase(graf);
        }

        private void UsunPierwszyElementTrasy()
        {
            if (trasaPasazera.Count == 0)
            {
                throw new ArgumentOutOfRangeException($"trasaPasazera", "Trasa PasazeraDijkstry nie posiada zadnych elementow!");
            }

            trasaPasazera.RemoveAt(0);
        }

        private bool CzyTrasaObliczona(Przystanek przystanekStartowy, Przystanek przystanekKoncowy)
        {
            foreach (var otrasa in obliczoneTrasy)
            {
                if (otrasa[0] == przystanekStartowy &&
                    otrasa[otrasa.Count - 1] == przystanekKoncowy)
                {
                    return true;
                }
            }

            return false;
        }

        public List<Przystanek> ZnajdzTrase(Graf.Graf<TimeSpan> graf)
        {
            foreach (var trasa in obliczoneTrasy)
            {
                if (trasa[0] == przystanekPoczatkowy &&
                    trasa[trasa.Count - 1] == przystanekKoncowy)
                {
                    return trasa;
                }
            }

            return ZnajdzNajkrotszaTrase(graf);
        }

        public static void DodajTrase(IEnumerable<Przystanek> trasaPasazera)
        {
            obliczoneTrasy.Add(trasaPasazera.ToList());
        }

        public List<Przystanek> ZnajdzNajkrotszaTrase(Graf.Graf<TimeSpan> graf)
        {
            var wStartowy = graf.ZnajdzWierzcholekZawierajacyPrzystanek(przystanekPoczatkowy);
            wStartowy.waga = TimeSpan.Zero;

            AlgorytmDijkstry(graf.OdwiedzNajmniejszy(), graf);
            var wierzcholekKoncowy = graf.WynikAlgorytmuDijkstry();

            var rezultat = KonwertujWynikAlgorytmuNaTrase(wierzcholekKoncowy);

            graf.ZresetujGraf(TimeSpan.MaxValue);

            return rezultat;
        }

        private static List<Przystanek> KonwertujWynikAlgorytmuNaTrase(Wierzcholek<TimeSpan> wierzcholekKoncowy)
        {
            var trasa = new List<Przystanek>();
            Wierzcholek<TimeSpan> w1;

            for (w1 = wierzcholekKoncowy; w1 != null; w1 = w1.poprzedniWierzcholek)
            {
                trasa.Insert(0, w1.przystanek);
            }

            return trasa;
        }

        private void AlgorytmDijkstry(Wierzcholek<TimeSpan> wierzcholek, Graf.Graf<TimeSpan> graf)
        {
            if (wierzcholek.przystanek != przystanekKoncowy)
            {

                foreach (var krawedz in wierzcholek.krawedzie)
                {
                    if (krawedz.wierzcholekKoncowy.czyOdwiedzony) continue;
                    if (krawedz.wierzcholekKoncowy.waga > wierzcholek.waga + krawedz.spodziewanyCzasPrzejazdu)
                    {
                        krawedz.wierzcholekKoncowy.waga = wierzcholek.waga + krawedz.spodziewanyCzasPrzejazdu;
                        krawedz.wierzcholekKoncowy.poprzedniWierzcholek = wierzcholek;
                    }
                }

                AlgorytmDijkstry(graf.OdwiedzNajmniejszy(), graf);
            }
        }

        private int OcenLinie(Linia linia)
        {
            var numerPrzystanku = linia.ZnajdzIndexPrzystanku(obecnyPrzystanek) + 1;
            var iterLimit = Math.Min(linia.Count - numerPrzystanku, trasaPasazera.Count);
            var rezultat = 0;

            if (numerPrzystanku == -1)
            {
                throw new InvalidOperationException($"Cos poszło nie tak... Autobus linii {linia} nie powinien być na przystanku {obecnyPrzystanek}!");
            }

            for (var i = 0; i < iterLimit; i++)
            {
                if (linia[numerPrzystanku + i].przystanek != trasaPasazera[i]) return rezultat;

                rezultat++;
            }

            return rezultat;
        }

        public override void Wysiadz(Przystanek przystanek)
        {
            obecnyPrzystanek = przystanek;

            UsunPrzebytePrzystanki();
        }

        public override void Wsiadz(Autobus autobus, TimeSpan czas)
        {
            var numerPrzystankuLinii = autobus.liniaAutobusu.ZnajdzIndexPrzystanku(obecnyPrzystanek);
            var iterLimit = Math.Min(autobus.liniaAutobusu.Count - numerPrzystankuLinii, trasaPasazera.Count);
            var i = 0;

            for (; i < iterLimit; i++)
            {
                if (trasaPasazera[i] != autobus.liniaAutobusu[numerPrzystankuLinii + i].przystanek)
                {
                    oczekiwanyPrzystanek = trasaPasazera[i];
                    break;
                }
            }

            //            for (; i > 0; i--) {
            //                trasaPasazera.RemoveAt(0);
            //            }

        }

        protected virtual void UsunPrzebytePrzystanki()
        {
            if (!trasaPasazera.Contains(obecnyPrzystanek)) return;

            foreach (var przystanek in trasaPasazera)
            {
                if (obecnyPrzystanek == przystanek)
                {
                    break;
                }

                trasaPasazera.Remove(przystanek);
            }

            trasaPasazera.RemoveAt(0);
        }

        public override Linia OczekiwanaLinia(TimeSpan obecnyCzas)
        {
            Linia linia = null;
            var maksymalnyWinik = 0;

            foreach (var autobus in obecnyPrzystanek.ObecneAutobusy)
            {
                var ocena = OcenLinie(autobus.liniaAutobusu);

                if (ocena > maksymalnyWinik)
                {
                    maksymalnyWinik = ocena;
                    linia = autobus.liniaAutobusu;
                }
            }

            return linia;
        }
    }
}