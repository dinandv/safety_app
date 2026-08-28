-- =====================================================================
--  001_tenancy_rls.sql
--  Multi-tenant foundation with Postgres row-level security.
-- =====================================================================
--  Starting point: RLS is the safety net, EF Core global query filters
--  are the ergonomics. The filters may duplicate this, never replace it.
--
--  Fails closed: app.current_tenant() returns NULL if the session
--  variable isn't set. Every comparison against NULL then yields no
--  rows. A broken interceptor gives zero results, not stray data.
-- =====================================================================

-- 1. Roles ---------------------------------------------------------------
-- bcc_owner  : owns the tables, runs migrations.
-- bcc_app    : what the application connects as. No BYPASSRLS, no
--              ownership. Without FORCE ROW LEVEL SECURITY an owner
--              would bypass its own policies.

DO $$
BEGIN
  IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'bcc_owner') THEN
    CREATE ROLE bcc_owner NOLOGIN;
  END IF;
  IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'bcc_app') THEN
    CREATE ROLE bcc_app LOGIN PASSWORD 'SET-THIS-FROM-THE-SECRET-STORE';
  END IF;
END$$;

ALTER ROLE bcc_app NOBYPASSRLS;
ALTER ROLE bcc_app NOSUPERUSER NOCREATEDB NOCREATEROLE;

-- 2. Tenant context --------------------------------------------------------

CREATE SCHEMA IF NOT EXISTS app;
GRANT USAGE ON SCHEMA app TO bcc_app;

CREATE OR REPLACE FUNCTION app.current_tenant() RETURNS uuid
LANGUAGE sql STABLE AS $$
  SELECT NULLIF(current_setting('app.tenant_id', true), '')::uuid
$$;
-- Note the second argument 'true' (missing_ok). Without that flag
-- current_setting throws when the variable was never set, and the
-- application would crash instead of getting nothing back.

GRANT EXECUTE ON FUNCTION app.current_tenant() TO bcc_app;

-- 3. Enable RLS on all tenant tables ------------------------------------

DO $$
DECLARE t text;
BEGIN
  FOREACH t IN ARRAY ARRAY[
    'person', 'person_app_role', 'person_team_role', 'team_role', 'action_token',
    'qualification_type', 'qualification', 'availability', 'event_type',
    'shift_template', 'person_event_type_exception', 'calendar_source',
    'candidate_event', 'location', 'event', 'calendar_mismatch',
    'event_guest_tenant', 'shift', 'assignment', 'swap_request', 'check_in',
    'guideline', 'document', 'advisory', 'contact', 'notification', 'audit_log'
  ]
  LOOP
    EXECUTE format('ALTER TABLE %I ENABLE ROW LEVEL SECURITY', t);
    EXECUTE format('ALTER TABLE %I FORCE ROW LEVEL SECURITY', t);
    EXECUTE format(
      'GRANT SELECT, INSERT, UPDATE, DELETE ON %I TO bcc_app', t);
  END LOOP;
END$$;

-- 4. Simple tenant tables ------------------------------------------------
-- Everything with a direct tenant_id that isn't shared.

DO $$
DECLARE t text;
BEGIN
  FOREACH t IN ARRAY ARRAY[
    'person', 'team_role', 'qualification_type', 'event_type',
    'calendar_source', 'location', 'guideline', 'document', 'advisory',
    'contact', 'notification', 'audit_log'
  ]
  LOOP
    EXECUTE format($f$
      CREATE POLICY %1$I_tenant ON %1$I
        USING (tenant_id = app.current_tenant())
        WITH CHECK (tenant_id = app.current_tenant())
    $f$, t);
  END LOOP;
END$$;

-- 5. Shared events ---------------------------------------------------
-- An event has one owner tenant and can invite other tenants. Reading
-- is allowed for the owner and every accepted guest. Writing is
-- restricted to the owner.
--
-- event_guest_tenant.owner_tenant_id is a deliberate denormalization of
-- event.tenant_id (set by the application when the invite is created,
-- never changed afterwards). Without that column the policy here would
-- have to look at event, and event's read policy in turn looks at
-- event_guest_tenant — Postgres reports that as "infinite recursion
-- detected in policy" because both policies need each other's outcome.
-- The denormalization breaks that cycle.

CREATE POLICY event_guest_tenant_tenant ON event_guest_tenant
  USING (
    tenant_id = app.current_tenant()
    OR owner_tenant_id = app.current_tenant()
  );

CREATE POLICY event_read ON event FOR SELECT
  USING (
    tenant_id = app.current_tenant()
    OR EXISTS (
      SELECT 1 FROM event_guest_tenant g
      WHERE g.event_id = event.id
        AND g.tenant_id = app.current_tenant()
        AND g.status = 'Accepted'
    )
  );

CREATE POLICY event_insert ON event FOR INSERT
  WITH CHECK (tenant_id = app.current_tenant());

CREATE POLICY event_update ON event FOR UPDATE
  USING (tenant_id = app.current_tenant())
  WITH CHECK (tenant_id = app.current_tenant());

CREATE POLICY event_delete ON event FOR DELETE
  USING (tenant_id = app.current_tenant());

-- 6. Derived tables: inherit the event's visibility --------------
-- The subquery on 'event' is itself already filtered by RLS. If you
-- can't see the event, the row here doesn't exist for you either. That
-- keeps the guest logic in one place instead of duplicated everywhere.

CREATE POLICY shift_read ON shift FOR SELECT
  USING (EXISTS (
    SELECT 1 FROM event e WHERE e.id = shift.event_id));

CREATE POLICY shift_write ON shift FOR ALL
  USING (EXISTS (
    SELECT 1 FROM event e
    WHERE e.id = shift.event_id
      AND e.tenant_id = app.current_tenant()))
  WITH CHECK (EXISTS (
    SELECT 1 FROM event e
    WHERE e.id = shift.event_id
      AND e.tenant_id = app.current_tenant()));

-- Calendar mismatches follow the same visibility as the event they
-- belong to: readable by owner and guest, resolving (writing) only by
-- the owner tenant.

CREATE POLICY calendar_mismatch_read ON calendar_mismatch FOR SELECT
  USING (EXISTS (
    SELECT 1 FROM event e WHERE e.id = calendar_mismatch.event_id));

CREATE POLICY calendar_mismatch_write ON calendar_mismatch FOR ALL
  USING (EXISTS (
    SELECT 1 FROM event e
    WHERE e.id = calendar_mismatch.event_id
      AND e.tenant_id = app.current_tenant()))
  WITH CHECK (EXISTS (
    SELECT 1 FROM event e
    WHERE e.id = calendar_mismatch.event_id
      AND e.tenant_id = app.current_tenant()));

-- Assignments may be created by the owner tenant, and by the home
-- tenant of the person themselves: a guest tenant fills in its own
-- people on a shared event.

CREATE POLICY assignment_read ON assignment FOR SELECT
  USING (EXISTS (
    SELECT 1 FROM shift s WHERE s.id = assignment.shift_id));

CREATE POLICY assignment_write ON assignment FOR ALL
  USING (
    EXISTS (SELECT 1 FROM shift s JOIN event e ON e.id = s.event_id
            WHERE s.id = assignment.shift_id
              AND e.tenant_id = app.current_tenant())
    OR EXISTS (SELECT 1 FROM person p
               WHERE p.id = assignment.person_id))
  WITH CHECK (
    EXISTS (SELECT 1 FROM shift s WHERE s.id = assignment.shift_id)
    AND EXISTS (SELECT 1 FROM person p WHERE p.id = assignment.person_id));

-- 7. Tables hanging off a person -------------------------------
-- person is already RLS-filtered, so an EXISTS is enough.

DO $$
DECLARE t text;
BEGIN
  FOREACH t IN ARRAY ARRAY[
    'person_app_role', 'person_team_role', 'qualification', 'availability',
    'person_event_type_exception', 'action_token'
  ]
  LOOP
    EXECUTE format($f$
      CREATE POLICY %1$I_via_person ON %1$I
        USING (EXISTS (SELECT 1 FROM person p WHERE p.id = %1$I.person_id))
        WITH CHECK (EXISTS (SELECT 1 FROM person p WHERE p.id = %1$I.person_id))
    $f$, t);
  END LOOP;
END$$;

-- 8. Tables hanging off an event type or calendar source ----------

CREATE POLICY shift_template_via_type ON shift_template
  USING (EXISTS (SELECT 1 FROM event_type et
                 WHERE et.id = shift_template.event_type_id))
  WITH CHECK (EXISTS (SELECT 1 FROM event_type et
                      WHERE et.id = shift_template.event_type_id));

CREATE POLICY candidate_event_via_source ON candidate_event
  USING (EXISTS (SELECT 1 FROM calendar_source s
                 WHERE s.id = candidate_event.calendar_source_id))
  WITH CHECK (EXISTS (SELECT 1 FROM calendar_source s
                      WHERE s.id = candidate_event.calendar_source_id));

-- 9. Tables hanging off an assignment or shift -------------------

CREATE POLICY swap_request_via_shift ON swap_request
  USING (EXISTS (SELECT 1 FROM shift s WHERE s.id = swap_request.shift_id))
  WITH CHECK (EXISTS (SELECT 1 FROM shift s WHERE s.id = swap_request.shift_id));

CREATE POLICY check_in_via_assignment ON check_in
  USING (EXISTS (SELECT 1 FROM assignment a WHERE a.id = check_in.assignment_id))
  WITH CHECK (EXISTS (SELECT 1 FROM assignment a WHERE a.id = check_in.assignment_id));

-- 10. The tenant table itself ---------------------------------------
-- Deliberate exception to "everything behind current_tenant()": the
-- application has to be able to look up a tenant by subdomain before
-- app.tenant_id is set (see the tenant resolution middleware in the
-- API), and which tenants exist is public via DNS and the Caddyfile
-- anyway. Every other table — including ones with participant data
-- like person — stays strictly behind current_tenant().
ALTER TABLE tenant ENABLE ROW LEVEL SECURITY;
ALTER TABLE tenant FORCE ROW LEVEL SECURITY;
GRANT SELECT ON tenant TO bcc_app;

CREATE POLICY tenant_read ON tenant FOR SELECT
  USING (true);
