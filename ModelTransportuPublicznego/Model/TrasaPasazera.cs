using System.Collections.Generic;

namespace ModelTransportuPublicznego.Model {
    public struct TrasaPasazera {
        public Przystanek przystanekPoczatkowy;
        public List<Przystanek> oczekiwanePrzystanki;
        public List<Linia> oczekiwaneLinie;

        public TrasaPasazera(Przystanek przystanekPoczatkowy, IEnumerable<Przystanek> oczekiwanePrzystanki, IEnumerable<Linia> oczekiwaneLinie) {
            this.przystanekPoczatkowy = przystanekPoczatkowy;
            this.oczekiwanePrzystanki = new List<Przystanek>();
            this.oczekiwaneLinie = new List<Linia>();

            foreach (var przystanek in oczekiwanePrzystanki) {
                this.oczekiwanePrzystanki.Add(przystanek);
            }

            foreach (var linia in oczekiwaneLinie) {
                this.oczekiwaneLinie.Add(linia);
            }
        }

        public Linia ZwrocOczekiwanaLinie() {
            return oczekiwaneLinie[0];
        }

        public Przystanek ZwrocOczekiwanyPrzystanek() {
            return oczekiwanePrzystanki[0];
        }

        public void UsunPierwszaOczekiwanaLinie() {
            oczekiwaneLinie.RemoveAt(0);
        }

        public void UsunPierwszyOczekiwanyPrzystanek() {
            oczekiwanePrzystanki.RemoveAt(0);
        }

        public Przystanek ZwrocPrzystanekKoncowy() {
            return oczekiwanePrzystanki[oczekiwanePrzystanki.Count - 1];
        }
    }
}