-- Fügt Preise ein, falls sie noch nicht existieren.
-- 0 = Parkett, 1 = Loge, 2 = LogePlus (entsprechend deinem Enum)

INSERT INTO "PreisZuKategorie" ("Id", "Kategorie", "Preis")
VALUES 
(1, 0, 9.00),
(2, 1, 12.00),
(3, 2, 15.00)
ON CONFLICT ("Id") DO NOTHING;