# Geometriai modellezés Msc. beadandó
# Bézier-görbék: de Casteljau algoritmus és Bernstein polinom összehasonlítása

A projekt a Bézier-görbék két népszerű számítási módszerét valósítja meg: **de Casteljau algoritmust** és **Bernstein polinomot**. A program lehetőséget biztosít arra, hogy a felhasználó interaktívan dolgozzon Bézier-görbékkel, módosítva a kontrollpontokat, és megfigyelheti, hogyan változik a görbe a különböző algoritmusok szerint.

## Funkciók

- **Kontrollpontok hozzáadása és eltávolítása**: A felhasználó kattinthat a vászonra, hogy új kontrollpontokat adjon hozzá, vagy eltávolítsa a már létező pontokat jobb kattintással.
- **De Casteljau algoritmus**: A görbe kiszámítása és kirajzolása a de Casteljau algoritmus segítségével, amely rekurzív interpolációval közelíti meg a Bézier-görbét.
- **Bernstein polinom**: A görbe kiszámítása és kirajzolása a Bernstein polinom képlete alapján, amely a kontrollpontok súlyozott összege.
- **Interaktív vezérlés**: A felhasználó szabályozhatja a t paraméter értékét egy csúszkával, amely a görbén való helyet határozza meg.
- **Különböző nézetek**: Lehetőség van a de Casteljau és Bernstein görbéket egyaránt megjeleníteni, vagy csak az egyiket.

## Használat

1. **Kontrollpontok hozzáadása**: Kattints bal gombbal a vászonra, hogy új kontrollpontokat adj hozzá.
2. **Kontrollpontok eltávolítása**: Kattints jobb gombbal egy meglévő kontrollpontra, hogy eltávolítsd.
3. **Görbék váltása**: A három gomb segítségével választhatsz, hogy:
   - **de Casteljau** algoritmus alapján számított görbét látsz.
   - **Bernstein polinom** alapján számított görbét látsz.
   - **Mindkét görbét** egyszerre.
4. **T paraméter szabályozása**: A csúszkával módosíthatod a `t` paraméter értékét, amely meghatározza a görbe pontját.
5. **Számítások megjelenítése**: A számítási lépések részletesen láthatóak a képernyőn, bemutatva a kontrollpontok közötti interpolációkat és az egyes algoritmusok lépéseit.

Készítette: Nyeste Réka (GKE37T), mérnökinformatikus MSC
