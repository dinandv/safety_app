-- =====================================================================
--  001_tenancy_rls.sql
--  Multi-tenant fundament met Postgres row-level security.
-- =====================================================================
--  Uitgangspunt: RLS is het vangnet, EF Core global query filters zijn
--  de ergonomie. De filters mogen dit dupliceren, nooit vervangen.
--
--  Faalt-dicht: app.current_tenant() geeft NULL als de sessievariabele
--  niet gezet is. Elke vergelijking met NULL levert dan geen rijen op.
--  Een kapotte interceptor geeft dus nul resultaten, geen vreemde data.
-- =====================================================================

-- 1. Rollen ------------------------------------------------------------
-- bcc_owner  : eigenaar van de tabellen, draait migraties.
-- bcc_app    : waar de applicatie mee verbindt. Geen BYPASSRLS, geen
--              eigenaarschap. Zonder FORCE ROW LEVEL SECURITY zou een
--              eigenaar zijn eigen policies namelijk omzeilen.

DO $$
BEGIN
  IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'bcc_owner') THEN
    CREATE ROLE bcc_owner NOLOGIN;
  END IF;
  IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'bcc_app') THEN
    CREATE ROLE bcc_app LOGIN PASSWORD 'ZET-DIT-UIT-DE-SECRET-STORE';
  END IF;
END$$;

ALTER ROLE bcc_app NOBYPASSRLS;
ALTER ROLE bcc_app NOSUPERUSER NOCREATEDB NOCREATEROLE;

-- 2. Tenant-context ----------------------------------------------------

CREATE SCHEMA IF NOT EXISTS app;
GRANT USAGE ON SCHEMA app TO bcc_app;

CREATE OR REPLACE FUNCTION app.current_tenant() RETURNS uuid
LANGUAGE sql STABLE AS $$
  SELECT NULLIF(current_setting('app.tenant_id', true), '')::uuid
$$;
-- Let op de tweede parameter 'true' (missing_ok). Zonder die vlag gooit
-- current_setting een fout als de variabele nooit gezet is, en dan valt
-- de applicatie om in plaats van niets terug te geven.

GRANT EXECUTE ON FUNCTION app.current_tenant() TO bcc_app;

-- 3. RLS aanzetten op alle tenant-tabellen ------------------------------

DO $$
DECLARE t text;
BEGIN
  FOREACH t IN ARRAY ARRAY[
    'persoon', 'persoon_teamrol', 'teamrol', 'kwalificatie_type',
    'kwalificatie', 'beschikbaarheid', 'evenementtype', 'dienstsjabloon',
    'persoon_evenementtype_uitzondering', 'agenda_bron',
    'kandidaat_evenement', 'locatie', 'evenement', 'evenement_gasttenant',
    'dienst', 'toewijzing', 'ruilverzoek', 'checkin', 'richtlijn',
    'document', 'aandachtspunt', 'contact', 'notificatie', 'auditlog'
  ]
  LOOP
    EXECUTE format('ALTER TABLE %I ENABLE ROW LEVEL SECURITY', t);
    EXECUTE format('ALTER TABLE %I FORCE ROW LEVEL SECURITY', t);
    EXECUTE format(
      'GRANT SELECT, INSERT, UPDATE, DELETE ON %I TO bcc_app', t);
  END LOOP;
END$$;

-- 4. Eenvoudige tenant-tabellen ----------------------------------------
-- Alles wat een directe tenant_id heeft en niet gedeeld wordt.

DO $$
DECLARE t text;
BEGIN
  FOREACH t IN ARRAY ARRAY[
    'persoon', 'teamrol', 'kwalificatie_type', 'evenementtype',
    'agenda_bron', 'locatie', 'richtlijn', 'document', 'aandachtspunt',
    'contact', 'notificatie', 'auditlog'
  ]
  LOOP
    EXECUTE format($f$
      CREATE POLICY %1$I_tenant ON %1$I
        USING (tenant_id = app.current_tenant())
        WITH CHECK (tenant_id = app.current_tenant())
    $f$, t);
  END LOOP;
END$$;

-- 5. Gedeelde evenementen ----------------------------------------------
-- Een evenement heeft één eigenaar-tenant en kan andere tenants
-- uitnodigen. Lezen mag de eigenaar én elke geaccepteerde gast.
-- Schrijven mag alleen de eigenaar.

CREATE POLICY evenement_gasttenant_tenant ON evenement_gasttenant
  USING (
    tenant_id = app.current_tenant()
    OR EXISTS (
      SELECT 1 FROM evenement e
      WHERE e.id = evenement_gasttenant.evenement_id
        AND e.tenant_id = app.current_tenant()
    )
  );

CREATE POLICY evenement_lezen ON evenement FOR SELECT
  USING (
    tenant_id = app.current_tenant()
    OR EXISTS (
      SELECT 1 FROM evenement_gasttenant g
      WHERE g.evenement_id = evenement.id
        AND g.tenant_id = app.current_tenant()
        AND g.status = 'geaccepteerd'
    )
  );

CREATE POLICY evenement_invoegen ON evenement FOR INSERT
  WITH CHECK (tenant_id = app.current_tenant());

CREATE POLICY evenement_wijzigen ON evenement FOR UPDATE
  USING (tenant_id = app.current_tenant())
  WITH CHECK (tenant_id = app.current_tenant());

CREATE POLICY evenement_verwijderen ON evenement FOR DELETE
  USING (tenant_id = app.current_tenant());

-- 6. Afgeleide tabellen: erf de zichtbaarheid van het evenement --------
-- De subquery op 'evenement' wordt zelf al door RLS gefilterd. Zie je
-- het evenement niet, dan bestaat de rij hier ook niet voor jou. Dat
-- scheelt het dupliceren van de gastlogica op elke tabel.

CREATE POLICY dienst_lezen ON dienst FOR SELECT
  USING (EXISTS (
    SELECT 1 FROM evenement e WHERE e.id = dienst.evenement_id));

CREATE POLICY dienst_schrijven ON dienst FOR ALL
  USING (EXISTS (
    SELECT 1 FROM evenement e
    WHERE e.id = dienst.evenement_id
      AND e.tenant_id = app.current_tenant()))
  WITH CHECK (EXISTS (
    SELECT 1 FROM evenement e
    WHERE e.id = dienst.evenement_id
      AND e.tenant_id = app.current_tenant()));

-- Toewijzingen mogen door de eigenaar-tenant worden gemaakt, én door de
-- thuis-tenant van de persoon zelf: een gastgemeente vult haar eigen
-- mensen in op een gedeeld evenement.

CREATE POLICY toewijzing_lezen ON toewijzing FOR SELECT
  USING (EXISTS (
    SELECT 1 FROM dienst d WHERE d.id = toewijzing.dienst_id));

CREATE POLICY toewijzing_schrijven ON toewijzing FOR ALL
  USING (
    EXISTS (SELECT 1 FROM dienst d JOIN evenement e ON e.id = d.evenement_id
            WHERE d.id = toewijzing.dienst_id
              AND e.tenant_id = app.current_tenant())
    OR EXISTS (SELECT 1 FROM persoon p
               WHERE p.id = toewijzing.persoon_id))
  WITH CHECK (
    EXISTS (SELECT 1 FROM dienst d WHERE d.id = toewijzing.dienst_id)
    AND EXISTS (SELECT 1 FROM persoon p WHERE p.id = toewijzing.persoon_id));

-- 7. Tabellen die aan een persoon hangen -------------------------------
-- persoon is al RLS-gefilterd, dus een EXISTS volstaat.

DO $$
DECLARE t text;
BEGIN
  FOREACH t IN ARRAY ARRAY[
    'persoon_teamrol', 'kwalificatie', 'beschikbaarheid',
    'persoon_evenementtype_uitzondering'
  ]
  LOOP
    EXECUTE format($f$
      CREATE POLICY %1$I_via_persoon ON %1$I
        USING (EXISTS (SELECT 1 FROM persoon p WHERE p.id = %1$I.persoon_id))
        WITH CHECK (EXISTS (SELECT 1 FROM persoon p WHERE p.id = %1$I.persoon_id))
    $f$, t);
  END LOOP;
END$$;

-- 8. Tabellen die aan een evenementtype of agenda_bron hangen ----------

CREATE POLICY dienstsjabloon_via_type ON dienstsjabloon
  USING (EXISTS (SELECT 1 FROM evenementtype et
                 WHERE et.id = dienstsjabloon.evenementtype_id))
  WITH CHECK (EXISTS (SELECT 1 FROM evenementtype et
                      WHERE et.id = dienstsjabloon.evenementtype_id));

CREATE POLICY kandidaat_via_bron ON kandidaat_evenement
  USING (EXISTS (SELECT 1 FROM agenda_bron b
                 WHERE b.id = kandidaat_evenement.agenda_bron_id))
  WITH CHECK (EXISTS (SELECT 1 FROM agenda_bron b
                      WHERE b.id = kandidaat_evenement.agenda_bron_id));

-- 9. Tabellen die aan een toewijzing of dienst hangen -------------------

CREATE POLICY ruilverzoek_via_dienst ON ruilverzoek
  USING (EXISTS (SELECT 1 FROM dienst d WHERE d.id = ruilverzoek.dienst_id))
  WITH CHECK (EXISTS (SELECT 1 FROM dienst d WHERE d.id = ruilverzoek.dienst_id));

CREATE POLICY checkin_via_toewijzing ON checkin
  USING (EXISTS (SELECT 1 FROM toewijzing t WHERE t.id = checkin.toewijzing_id))
  WITH CHECK (EXISTS (SELECT 1 FROM toewijzing t WHERE t.id = checkin.toewijzing_id));

-- 10. De tenant-tabel zelf ---------------------------------------------
ALTER TABLE tenant ENABLE ROW LEVEL SECURITY;
ALTER TABLE tenant FORCE ROW LEVEL SECURITY;
GRANT SELECT ON tenant TO bcc_app;

CREATE POLICY tenant_eigen ON tenant FOR SELECT
  USING (id = app.current_tenant());
