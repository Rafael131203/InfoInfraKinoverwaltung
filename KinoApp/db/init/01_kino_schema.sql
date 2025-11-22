-- 1. Hilfstabellen (Preise & Filme)
CREATE TABLE IF NOT EXISTS "PreisZuKategorie" (
    "Id"          SERIAL PRIMARY KEY,
    "Kategorie"   INTEGER NOT NULL,
    "Preis"       DECIMAL(10,2) NOT NULL,
    "Beschreibung" TEXT
);

CREATE TABLE IF NOT EXISTS "Film" (
    "Id"          TEXT PRIMARY KEY,
    "Titel"       TEXT NOT NULL,
    "Beschreibung" TEXT NOT NULL,
    "Dauer"       INTEGER,
    "FSK"         INTEGER,
    "Genre"       TEXT NOT NULL,
    "ImageURL"    TEXT 
);

-- 2. Kino Struktur (Saal -> Reihe -> Platz)
CREATE TABLE IF NOT EXISTS "Kinosaal" (
    "Id"    SERIAL PRIMARY KEY,
    "Name"  TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS "Sitzreihe" (
    "Id"          SERIAL PRIMARY KEY,
    "Kategorie"   INTEGER,
    "Bezeichnung" TEXT NOT NULL,
    "KinosaalId"  INTEGER NOT NULL REFERENCES "Kinosaal"("Id") ON DELETE CASCADE,
    CONSTRAINT "UQ_Sitzreihe_Kinosaal_Bezeichnung" UNIQUE ("KinosaalId","Bezeichnung")
);

CREATE TABLE IF NOT EXISTS "Sitzplatz" (
    "Id"          SERIAL PRIMARY KEY,
    "Gebucht"     BOOLEAN NOT NULL DEFAULT FALSE,
    "Nummer"      INTEGER NOT NULL,
    "Preis"       DECIMAL(10,2),
    "SitzreiheId" INTEGER NOT NULL REFERENCES "Sitzreihe"("Id") ON DELETE CASCADE,
    CONSTRAINT "UQ_Sitzplatz_Row_Num" UNIQUE ("SitzreiheId","Nummer")
);

-- 3. Programm & Kunden
CREATE TABLE IF NOT EXISTS "Vorstellung" (
    "Id"         SERIAL PRIMARY KEY,
    "Datum"      TIMESTAMPTZ NOT NULL, -- Besser TZ für Zeitzonen
    "Status"     INTEGER,
    "FilmId"     TEXT NOT NULL REFERENCES "Film"("Id") ON DELETE CASCADE,
    "KinosaalId" INTEGER NOT NULL REFERENCES "Kinosaal"("Id") ON DELETE CASCADE,
    CONSTRAINT "UQ_Vorstellung_Hall_Time" UNIQUE ("KinosaalId","Datum")
);

CREATE TABLE IF NOT EXISTS "User" (
    "Id"       SERIAL PRIMARY KEY,
    "Email"    TEXT NOT NULL,
    "Nachname" TEXT NOT NULL,
    "Passwort" TEXT NOT NULL,
    "Vorname"  TEXT NOT NULL,
    "Role"     TEXT NOT NULL,
    CONSTRAINT "UQ_User_Email" UNIQUE ("Email")
);

-- 4. Das Ticket 
CREATE TABLE IF NOT EXISTS "Ticket" (
    "Id"            SERIAL PRIMARY KEY,
    "Status"        INTEGER,
    "SitzplatzId"   INTEGER NOT NULL REFERENCES "Sitzplatz"("Id") ON DELETE CASCADE,
    "VorstellungId" INTEGER NOT NULL REFERENCES "Vorstellung"("Id") ON DELETE CASCADE,
    "KundeId"       INTEGER REFERENCES "Kunde"("Id") ON DELETE SET NULL,
    CONSTRAINT "UQ_Ticket_Seat_Per_Show" UNIQUE ("VorstellungId","SitzplatzId")
);