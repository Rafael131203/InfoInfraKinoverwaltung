-- Fügt Preise ein, falls sie noch nicht existieren.
-- 0 = Parkett, 1 = Loge, 2 = LogePlus (entsprechend deinem Enum)

INSERT INTO "PreisZuKategorie" ("Id", "Kategorie", "Preis")
VALUES 
(1, 0, 9.00),
(2, 1, 12.00),
(3, 2, 15.00)
ON CONFLICT ("Id") DO NOTHING;


DO $$
DECLARE
    -- one record per Saal: name, number of rows, seats per row
    k RECORD;
    v_kinosaal_id   INTEGER;
    v_reihe_id      INTEGER;
    v_row           INTEGER;
    v_seat          INTEGER;
    v_kategorie     INTEGER;
    v_bezeichnung   TEXT;
    v_preis         NUMERIC(10,2);
BEGIN
    -- Define the halls we want to create
    FOR k IN
        SELECT *
        FROM (VALUES
            ('Saal 1', 20, 20),
            ('Saal 2', 15, 15),
            ('Saal 3', 15, 23)
        ) AS t("Name", "Rows", "SeatsPerRow")
    LOOP
        -- Skip if this Kinosaal already exists
        IF EXISTS (SELECT 1 FROM "Kinosaal" WHERE "Name" = k."Name") THEN
            CONTINUE;
        END IF;

        -- Create Kinosaal
        INSERT INTO "Kinosaal" ("Name")
        VALUES (k."Name")
        RETURNING "Id" INTO v_kinosaal_id;

        -- Create Sitzreihen for this hall
        FOR v_row IN 1..k."Rows" LOOP

            -- Map row index -> Kategorie
            -- 1-6  : cheap (Parkett = 0)
            -- 7-8  : medium (Loge = 1)
            -- 9-10 : luxury (LogePlus = 2)
            -- 11-12: medium (Loge = 1)
            -- 13+  : cheap (Parkett = 0)
            IF v_row <= 6 THEN
                v_kategorie := 0;  -- cheap
            ELSIF v_row <= 8 THEN
                v_kategorie := 1;  -- medium
            ELSIF v_row <= 10 THEN
                v_kategorie := 2;  -- luxury
            ELSIF v_row <= 12 THEN
                v_kategorie := 1;  -- medium
            ELSE
                v_kategorie := 0;  -- cheap
            END IF;

            -- Row label: A, B, C, ...
            v_bezeichnung := chr(64 + v_row); -- 1->A, 2->B, ...

            INSERT INTO "Sitzreihe" ("Kategorie", "Bezeichnung", "KinosaalId")
            VALUES (v_kategorie, v_bezeichnung, v_kinosaal_id)
            RETURNING "Id" INTO v_reihe_id;

            -- Lookup price for this Kategorie
            SELECT "Preis"
            INTO v_preis
            FROM "PreisZuKategorie"
            WHERE "Kategorie" = v_kategorie
            LIMIT 1;

            -- Create seats in this row
            FOR v_seat IN 1..k."SeatsPerRow" LOOP
                INSERT INTO "Sitzplatz" ("Nummer", "Preis", "SitzreiheId")
                VALUES (v_seat, v_preis, v_reihe_id);
            END LOOP;

        END LOOP; -- rows
    END LOOP; -- halls
END
$$;
