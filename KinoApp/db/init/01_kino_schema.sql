CREATE TABLE IF NOT EXISTS "Kunden" (
    "Id"        SERIAL PRIMARY KEY,
    "Email"     TEXT NOT NULL,
    "Nachname"  TEXT NOT NULL,
    "Passwort"  TEXT NOT NULL,
    "Vorname"   TEXT NOT NULL
);