using System;
using System.Collections.Generic;
using ModelTransportuPublicznego.Implementacja.Graf;
using ModelTransportuPublicznego.Model;

namespace ModelTransportuPublicznego.Implementacja.Pasazerowie {
    public class PasazerDjikstry : Pasazer {

        private static List<TrasaPasazera> obliczoneTrasy;

        public PasazerDjikstry(TrasaPasazera trasaPasazera) : base(trasaPasazera) {
            obliczoneTrasy = new List<TrasaPasazera>();
        }

        public PasazerDjikstry(TrasaPasazera trasaPasazera, int czasWsiadania, int czasWysiadania) : base(trasaPasazera,
            czasWsiadania, czasWysiadania) {
            obliczoneTrasy = new List<TrasaPasazera>();
        }

        private PasazerDjikstry(TrasaPasazera trasaPasazera, int czasWsiadania, int czasWysiadania,
            Przystanek przystanekPoczatkowy, Przystanek przystanekKoncowy)
            : base(trasaPasazera, czasWsiadania, czasWysiadania, przystanekPoczatkowy, przystanekKoncowy) {
            obliczoneTrasy = new List<TrasaPasazera>();
        }

        private bool CzyTrasaObliczona(Przystanek przystanekStartowy, Przystanek przystanekKoncowy) {
            foreach (var otrasa in obliczoneTrasy) {
                if (otrasa.przystanekPoczatkowy == przystanekStartowy &&
                    otrasa.ZwrocPrzystanekKoncowy() == przystanekKoncowy) {
                    return true;
                }
            }

            return false;
        }

        public void ZnajdzTrase(Graf.Graf graf) {
            foreach (var trasa in obliczoneTrasy) {
                if (trasa.przystanekPoczatkowy == trasaPasazera.przystanekPoczatkowy &&
                    trasa.ZwrocPrzystanekKoncowy() == przystanekKoncowy) {
                    trasaPasazera = trasa;
                }

                // trasaPasazera = ZnajdzNajkrotszaTrase(graf);
            }
        }

        public static void DodajTrase(TrasaPasazera trasaPasazera) {
            obliczoneTrasy.Add(trasaPasazera);
        }

        public void ZnajdzNajkrotszaTrase(Graf.Graf graf) {
            var wStartowy = graf.ZnajdzWierzcholekZawierajacyPrzystanek(trasaPasazera.przystanekPoczatkowy);
            wStartowy.waga = TimeSpan.Zero;

            AlgorytmDijkstry(graf.OdwiedzNajmniejszy(), graf);
        }

        private void AlgorytmDijkstry(Wierzcholek wierzcholek, Graf.Graf graf) {
            if (wierzcholek.przystanek != trasaPasazera.ZwrocPrzystanekKoncowy()) {

                foreach (var krawedz in wierzcholek.krawedzie) {
                    if (krawedz.wierzcholekKoncowy.czyOdwiedzony) continue;
                    if (krawedz.wierzcholekKoncowy.waga > wierzcholek.waga + krawedz.spodziewanyCzasPrzejazdu) {
                        krawedz.wierzcholekKoncowy.waga = wierzcholek.waga + krawedz.spodziewanyCzasPrzejazdu;
                        krawedz.wierzcholekKoncowy.poprzedniWierzcholek = wierzcholek;
                    }
                }
                
                AlgorytmDijkstry(graf.OdwiedzNajmniejszy(), graf);
            }
        }
    }
}