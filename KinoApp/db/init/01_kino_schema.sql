CREATE TABLE IF NOT EXISTS "Warenkorb" (
    "Id"              SERIAL PRIMARY KEY,
    "Gesamtpreis"     DECIMAL(10,2) NOT NULL DEFAULT 0,
    "Zahlungsmittel"  INTEGER
);

CREATE TABLE IF NOT EXISTS "Film" (
    "Id"             TEXT PRIMARY KEY,
    "Titel"          TEXT NOT NULL,
    "Beschreibung"   TEXT NOT NULL,
    "Dauer"          INTEGER,
    "FSK"            INTEGER,
    "Genre"          TEXT NOT NULL
);

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

CREATE TABLE IF NOT EXISTS "Vorstellung" (
    "Id"         SERIAL PRIMARY KEY,
    "Datum"      TIMESTAMP NOT NULL,           -- (use TIMESTAMPTZ if you want TZ-aware)
    "Status"     INTEGER,
    "FilmId"     INTEGER NOT NULL REFERENCES "Film"("Id") ON DELETE CASCADE,
    "KinosaalId" INTEGER NOT NULL REFERENCES "Kinosaal"("Id") ON DELETE CASCADE,
    CONSTRAINT "UQ_Vorstellung_Hall_Time" UNIQUE ("KinosaalId","Datum")
);

CREATE TABLE IF NOT EXISTS "Kunde" (
    "Id"          SERIAL PRIMARY KEY,
    "Email"       TEXT NOT NULL,
    "Nachname"    TEXT NOT NULL,
    "Passwort"    TEXT NOT NULL,
    "Vorname"     TEXT NOT NULL,
    "WarenkorbId" INTEGER REFERENCES "Warenkorb"("Id") ON DELETE CASCADE,
    CONSTRAINT "UQ_Kunde_Email" UNIQUE ("Email")
);

CREATE TABLE IF NOT EXISTS "Gast" (
    "Id"          SERIAL PRIMARY KEY,
    "WarenkorbId" INTEGER NOT NULL REFERENCES "Warenkorb"("Id") ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS "Ticket" (
    "Id"            SERIAL PRIMARY KEY,
    "Status"        INTEGER,
    "SitzplatzId"   INTEGER NOT NULL REFERENCES "Sitzplatz"("Id") ON DELETE CASCADE,
    "WarenkorbId"   INTEGER NOT NULL REFERENCES "Warenkorb"("Id") ON DELETE CASCADE,
    "VorstellungId" INTEGER NOT NULL REFERENCES "Vorstellung"("Id") ON DELETE CASCADE,
    "KundeId"       INTEGER NOT NULL REFERENCES "Kunde"("Id") ON DELETE CASCADE,
    CONSTRAINT "UQ_Ticket_Seat_Per_Show" UNIQUE ("VorstellungId","SitzplatzId")
);