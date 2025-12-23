CREATE TABLE IF NOT EXISTS "PreisZuKategorie" (
    "Id"    SERIAL PRIMARY KEY,
    "Kategorie"  INTEGER,
    "Preis"      DECIMAL(10,2)
);