# CLAUDE.md

Context voor Claude Code. Lees dit voordat je iets wijzigt.

## Wat dit is

Roosterapplicatie voor vrijwillige veiligheidsteams (hoofd-BHV, BHV,
toezicht, EHBO) rond evenementen. Multi-tenant: meerdere afdelingen op één
installatie. Eerste tenant is een pilot, daarna volgen er drie.

Achtergrond en volledige afwegingen: `docs/ontwerp.md` en
`docs/datamodel.md`. Werk in issues, gelabeld `demo` (nodig voor de eerste
demo), `fundament`, `beveiliging` en `na-demo`.

## Stack

ASP.NET Core, EF Core, PostgreSQL 16, Angular als PWA. Docker Compose,
Caddy als reverse proxy. Deploy-configuratie in `deploy/`.

## Harde regels

Deze mogen niet worden aangepast zonder overleg met de eigenaar.

1. **Nooit echte persoonsgegevens in de database.** Tot mandaat,
   verwerkersovereenkomst en retentietermijn schriftelijk rond zijn, draait
   alles op verzonnen deelnemers. Een echte agendafeed mag wel.

2. **Nooit iets uit `.local/` committen.** Die map staat in `.gitignore` en
   bevat vertrouwelijke analyse over de klantorganisatie. De repo is
   publiek. Geen namen, locaties, sleutelposities of installatiecodes in
   code, commits, issues of documentatie.

3. **Row-level security is de beveiliging, EF Core global query filters
   zijn de ergonomie.** Verwijder of omzeil de policies niet, ook niet
   tijdelijk om een test te laten slagen.

4. **De applicatie verbindt als `bcc_app`.** Geen `BYPASSRLS`, geen
   tabeleigenaarschap. Migraties draaien als `bcc_owner`. Zonder dat
   onderscheid staat RLS effectief uit.

5. **Geen wachtwoorden of tokens in de repo.** Ook geen voorbeeldwaarden
   die op een echte lijken. Secrets komen uit de secret store.

6. **Geen geheimen aanmaken of invoeren.** Vraag de eigenaar om
   inloggegevens te zetten; doe dat niet zelf.

## Ontwerpbeslissingen die vaak verkeerd worden geraden

**De dienst is het roosterbare object, niet het evenement.** Een evenement
heeft meerdere diensten: teamrol, tijdvak, benodigd aantal. Alles hangt aan
de dienst.

**Alleen kwalificatie blokkeert.** Geen geldig certificaat voor een rol
betekent niet inplanbaar in die rol. Leeftijdsbereik, beschikbaarheid en
bekwaamheden sorteren en waarschuwen, maar houden niemand tegen. Er is geen
override-mechanisme omdat er niets geblokkeerd wordt. Bouw er ook geen.

**Twee leeftijdsbereiken per evenementtype.** Doelgroep is de leeftijd van
de bezoekers en wordt niet in de matching gebruikt. Inzetbaar is de
leeftijd van de veiligheidsmensen, leidend maar zacht, en mag per teamrol
afwijken via het dienstsjabloon. Leeg erft van het type.

**Een vastgelegde uitzondering wint van het leeftijdsbereik**, in beide
richtingen, met verplichte reden.

**Leeftijd wordt berekend op de datum van het evenement**, nooit op
vandaag, en bij het tonen van een rooster — niet alleen bij het aanmaken
van een toewijzing.

**Ruilen en de open oproep horen in de applicatie.** Gebeuren ze ernaast,
dan klopt het dagoverzicht binnen enkele weken niet meer en is de
applicatie erger dan geen applicatie.

**Handmatig een evenement aanmaken is een hoofdpad.** Niet elk evenement
staat in een agendafeed.

**Deelnemers zien alleen hun dienstgenoten**, plus de contactkaart. Geen
volledige deelnemerslijst, geen geboortedata.

## Valkuilen

- `app.tenant_id` mag niet blijven hangen op een gerecyclede verbinding.
  De interceptor in `src/Infrastructure/Tenancy/` zet en wist hem.
  `Multiplexing=false` in de connectiestring: sessievariabelen overleven
  multiplexing niet.
- `ENABLE ROW LEVEL SECURITY` is niet genoeg, `FORCE` is nodig. Een
  eigenaar omzeilt anders zijn eigen policies.
- Rich-text invoer server-side saniteren met een allow-list en gesaniteerd
  opslaan. Nooit `bypassSecurityTrustHtml`.
- ICS: gebruik een bestaande library voor `RRULE`, `EXDATE` en overrides.
  Niet zelf schrijven.
- Notificaties hebben een unieke idempotency-key nodig, anders komen ze
  dubbel na een herstart van de scheduler.

## Stand van zaken

Issues 1 t/m 3 zijn gedaan. Wat er ligt:

- `db/001_tenancy_rls.sql` — rollen, tenant-context en alle policies,
  inclusief de `__EFMigrationsHistory`-uitzondering (issue 2) en de
  gedenormaliseerde `event_guest_tenant.owner_tenant_id` die een
  cirkelverwijzing tussen `event` en `event_guest_tenant` voorkomt
- `src/Infrastructure/Entities/` — alle EF Core-entiteiten uit dit
  document, plus de migratie in `src/Infrastructure/Migrations/`
- `src/Infrastructure/Tenancy/` — de connectie-interceptor
- `src/Api/` — magic-link login (code + link, 90 dagen sessie),
  dienstacties zonder inloggen (bevestigen/afmelden — ruilen volgt in een
  latere issue), tenant-resolutie uit het subdomein
- `tests/Tenancy/` — de isolatietest
- `deploy/` — Compose, Caddy, `Dockerfile.api` en het runbook

Verder bouwen op een al compleet schema: volgende issue is 4 (ICS-import).

## Werkwijze

- Lokaal ontwikkelen. De server komt pas bij deploy in beeld.
- **Taal: alle software in het Engels, alle documentatie in het
  Nederlands.** Code, paden, klassen, methoden, tabel- en kolomnamen,
  commitberichten — Engels. `docs/*.md`, dit bestand, en de Nederlandse
  vaktermen die de opdrachtgever gebruikt (issues, gesprekken) — Nederlands.
  Geen mengvormen: geen Nederlandse mapnamen in `src/`, geen Engelse
  kolomnamen die niet overeenkomen met wat hier gedocumenteerd staat.
- Kleine commits per issue, met het issuenummer erin.
- Draai de isolatietest voordat je iets aan tenancy of policies raakt.
- Twijfel je of iets in de publieke repo mag: dan mag het niet. Vraag het.
