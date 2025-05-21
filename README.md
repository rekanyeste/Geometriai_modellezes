# Geometriai modellezés Msc. beadandó
# Bézier-görbék összehasonlítása: de Casteljau és Bernstein módszerek

Bézier-görbék két klasszikus számítási eljárását implementálja: a **de Casteljau algoritmust** és a **Bernstein-polinomos** megközelítést. A program képes ezek pontosságát és teljesítményét összehasonlítani különböző számítási lépések (t értékek) mentén.

## Fájlok áttekintése:

✅ BezierCalculator.cs
### Tartalmazza a Bézier-görbe pontjainak kiszámításához szükséges matematikai logikát:
- *CalculatePointCasteljau(List<PointF>, double t)*
   - Végrehajtja a de Casteljau algoritmust. Lépésenként lineárisan interpolál a kontrollpontok között, amíg egyetlen pontot nem kap.
- *CalculatePointBernstein(List<PointF>, double t)*
   - A Bézier-görbe pontját számítja ki Bernstein-polinomok segítségével. A pont a kontrollpontok és a megfelelő Bernstein-bázisfüggvények súlyozott összegeként adódik.
- *BinomialCoefficient(int n, int k)*
   - Binomiális együttható kiszámítása rekurzió nélkül, az optimális teljesítmény érdekében.
- *Bernstein(int n, int i, double t)*
   - Bernstein-bázisfüggvény számítása adott fokszám (n), index (i) és t paraméter esetén.

______

✅ BezierComparison.cs
### Lehetővé teszi a két algoritmus összehasonlítását pontosság és futási idő alapján:
- *Compare(List<PointF> controlPoints, float step = 0.01f)*
  - Összehasonlítja a Bernstein és de Casteljau algoritmus eredményeit:
     - Méri a két algoritmus által kiszámolt pontok közötti eltérést (átlagos és maximális hiba).
     - Időzítést végez, hogy mennyi idő alatt számítják ki a teljes görbét adott step felbontással.

- Az eredményeket egy *BezierComparisonResult* objektumban adja vissza, amely tartalmazza:
   - *MaxError*: a maximális eltérés a két módszer közt,
   - *AverageError*: az átlagos eltérés,
   - *CasteljauTimeMs* és *BernsteinTimeMs*: a számítási idő ezredmásodpercben.

______

✅ Bezier.cs
### Bézier-görbe adatmodell
- *ControlPoints*:
   - A Bézier-görbe kontrollpontjainak listája.
- *Step*:
   - A görbe pontjainak számításához használt felbontás (t lépésköz).
- *CalculateCurvePoints()*:
   - Meghívja a *BezierCalculator.CalculatePointCasteljau* metódust minden t értékre 0-tól 1-ig a megadott lépések szerint, és eltárolja az így kapott pontokat.
- *CurvePoints*:
   - A görbe kirajzolásához szükséges kiszámolt pontok listája.

______

⚙️ Működés
1. A felhasználó definiál egy sor kontrollpontot.
   - Ezt megteheti a bal egérgomb megnyomásával,
   - vagy a Fokszám 3, 20 és 50 gombok segítségével.
   - Törölni a jobb egérgombbal a pontra kattintva lehetséges.
2. A rendszer kiszámítja a Bézier-görbe pontjait mindkét algoritmus segítségével:
   - Bernstein polinom: piros színű görbe
   - de Casteljau algoritmus: kék színű görbe
3. Az BezierComparison osztály elemzi:
   - a két eredmény közti eltérést,
   - a teljesítményt (futási idő),
   - átlagos és maximális hibát.
4. Output:
   - de Casteljau és Bernstein polinom esetén:
        - t aktuális értéke,
        - eredmény,
        - számítási idő (másodperc),
        - eltérés a másik algoritmussal kiszámított ponttól.
   - Összehasonlítás:
        - átlagos és maximális hiba kiszámítása,
        - a két algoritmus számítási ideje,
        - gyorsabb algoritmus.
   - Fokszám gombok:
        - 3, 20 vagy 50 fokszámú görbét generál,
        - összehasonlítás gombot megnyomva kiírja ezek eredményét.

## Készítette: Nyeste Réka (GKE37T), mérnökinformatikus MSC
