# Datamodel

Postgres, één schema, `tenant_id` op elke tabel, row-level security als
vangnet. De policies zelf staan in `db/001_tenancy_rls.sql`. Tabel- en
kolomnamen in de database zijn Engels (zie CLAUDE.md); dit document
beschrijft ze in het Nederlands.

## ERD

```mermaid
erDiagram
    TENANT ||--o{ PERSON : heeft
    TENANT ||--o{ TEAM_ROLE : definieert
    TENANT ||--o{ EVENT_TYPE : heeft
    TENANT ||--o{ QUALIFICATION_TYPE : heeft
    TENANT ||--o{ CALENDAR_SOURCE : heeft
    TENANT ||--o{ LOCATION : heeft
    TENANT ||--o{ GUIDELINE : heeft
    TENANT ||--o{ DOCUMENT : beheert
    TENANT ||--o{ ADVISORY : heeft
    TENANT ||--o{ CONTACT : heeft
    TENANT ||--o{ EVENT : bezit

    PERSON ||--o{ PERSON_TEAM_ROLE : vervult
    PERSON ||--o{ PERSON_APP_ROLE : heeft
    PERSON ||--o{ PERSON_EVENT_TYPE_EXCEPTION : kent
    PERSON ||--o{ QUALIFICATION : bezit
    PERSON ||--o{ AVAILABILITY : geeft_op
    PERSON ||--o{ ASSIGNMENT : krijgt
    PERSON ||--o{ NOTIFICATION : ontvangt
    PERSON ||--o{ ACTION_TOKEN : krijgt

    TEAM_ROLE ||--o{ PERSON_TEAM_ROLE : op
    TEAM_ROLE ||--o{ SHIFT_TEMPLATE : voor
    TEAM_ROLE ||--o{ SHIFT : voor
    TEAM_ROLE ||--o{ EVENT_TYPE : vereiste_bekwaamheid
    QUALIFICATION_TYPE ||--o{ QUALIFICATION : soort
    QUALIFICATION_TYPE }o--o| TEAM_ROLE : vereist_voor

    EVENT_TYPE ||--o{ SHIFT_TEMPLATE : bevat
    EVENT_TYPE ||--o{ PERSON_EVENT_TYPE_EXCEPTION : kent
    EVENT_TYPE ||--o{ EVENT : typeert

    CALENDAR_SOURCE ||--o{ CANDIDATE_EVENT : levert
    CANDIDATE_EVENT ||--o| EVENT : wordt

    EVENT ||--o{ CALENDAR_MISMATCH : signaleert
    EVENT ||--o{ SHIFT : bevat
    EVENT ||--o{ EVENT_GUEST_TENANT : nodigt_uit
    EVENT }o--|| LOCATION : op
    LOCATION ||--o{ CHECK_IN : via_qr

    SHIFT ||--o{ ASSIGNMENT : vult
    SHIFT ||--o{ SWAP_REQUEST : over
    ASSIGNMENT ||--o| CHECK_IN : bevestigd_door
```

## Tabellen

### Tenant en identiteit

**tenant** — `id`, `name`, `slug` (subdomein), `active`, `created_at`

**person** — `id`, `tenant_id` (thuis-tenant), `first_name`,
`last_name_prefix`, `last_name`, `date_of_birth`, `email`, `phone`,
`chat_id` (nullable), `status` (Active | Inactive), `stopped_on`,
`pseudonymized_at`

**person_app_role** — `person_id`, `app_role` (PlatformAdmin |
TenantAdmin | Planner | Participant)

**team_role** — `id`, `tenant_id`, `name`, `kind` (ShiftRole | Skill),
`vest_color` (nullable), `active`

> Eén tabel, twee soorten. **ShiftRole** = waarvoor je diensten inricht:
> hoofd-BHV, BHV, toezicht, EHBO. **Skill** = losse vaardigheden zonder
> eigen dienst: reanimatie, sleutelbeheer, techniek. Configureerbaar per
> tenant, geen enum — er zijn er in de praktijk minstens elf, waaronder
> zaalwacht, ontruimingscoördinator en parkeerwacht.

**person_team_role** — `person_id`, `team_role_id`, `confirmed_at`,
`self_confirmed`

> De bevestigingskolommen zijn essentieel. De werkwijze is dat de
> coördinator een indeling voorstelt en iedereen die controleert op zijn
> eigen beleving. De applicatie ondersteunt die ronde, hij vervangt hem niet.

**action_token** — `id`, `person_id`, `purpose` (Login | ShiftAction |
ChatLink), `token_hash`, `scope_id` (bv. `assignment_id`), `valid_until`,
`used_at`

> Tokens altijd gehasht opslaan. Eenmalig gebruik voor login, kort geldig
> voor dienstacties, en altijd beperkt tot één dienst.

### Kwalificaties en beschikbaarheid

**qualification_type** — `id`, `tenant_id`, `name`,
`required_for_team_role_id` (nullable), `default_validity_months`

**qualification** — `id`, `person_id`, `qualification_type_id`,
`obtained_on`, `valid_until`, `note`

**availability** — `id`, `person_id`, `from`, `until`, `kind`
(Unavailable | Preferred), `note`

> Zonder dit is de sortering fictie: je plant iemand in die op vakantie is
> en dat blijkt pas op de dag zelf.

### Evenementen

**event_type** — `id`, `tenant_id`, `name`, `target_audience_description`,
`target_age_from` (nullable), `target_age_to` (nullable),
`deployable_age_from` (nullable), `deployable_age_to` (nullable),
`required_skill_id` (nullable), `expected_visitor_count` (nullable),
`active`

> **Doelgroep** (target audience) = leeftijd van de bezoekers;
> beschrijvend, niet in de matching. **Inzetbaar** (deployable) = leeftijd
> van veiligheidsmensen die voor dit type ingezet kunnen worden; leidend
> maar niet blokkerend, en overschrijfbaar per dienstsjabloon. **Vereiste
> bekwaamheid** (required skill) is een optionele haak voor types waar een
> specifieke vaardigheid telt.
>
> **Types zijn fijnmazig: activiteit × leeftijdsgroep.** IJshockey voor
> kinderen is een ander type dan voor jeugd. Reken op flink meer types dan
> je vooraf denkt, en houd het aanmaken daarom licht — naam, twee bereiken,
> dienstsjabloon, klaar. Een "dupliceer dit type"-knop verdient zichzelf
> snel terug.

**person_event_type_exception** — `person_id`, `event_type_id`,
`verdict` (AlwaysDeploy | NeverDeploy), `reason` (verplicht),
`recorded_by_person_id`, `recorded_at`

> Overrulet het leeftijdsbereik in beide richtingen. Hier legt de
> coördinator vast dat iemand bewust wel of niet bij een type wordt
> ingezet, mét reden — zodat dat besluit zijn vertrek overleeft in plaats
> van te verdwijnen als toevallige uitkomst van een bereik.

**shift_template** — `id`, `event_type_id`, `team_role_id`, `count`,
`start_offset_minutes`, `duration_minutes`, `deployable_age_from`
(nullable), `deployable_age_to` (nullable)

> Het leeftijdsbereik mag per teamrol afwijken: bij een jeugdevenement wil
> je jonge toezichthouders, maar de leeftijd van de EHBO'er doet er niet
> toe. **Leeg betekent: neem het bereik van het evenementtype over.**

**calendar_source** — `id`, `tenant_id`, `ics_url`, `last_synced_at`,
`last_sync_status`, `active`

**candidate_event** — `id`, `calendar_source_id`, `ics_uid`,
`recurrence_id`, `title`, `start`, `end`, `location_text`, `content_hash`,
`status` (New | Linked | Ignored | Changed | RemovedFromSource)

> Sleutel is `(calendar_source_id, ics_uid, recurrence_id)`. De
> `content_hash` maakt sync idempotent en detecteert wijzigingen.

**location** — `id`, `tenant_id`, `name`, `address`, `qr_slug`

**event** — `id`, `tenant_id` (eigenaar), `event_type_id`,
`candidate_event_id` (**nullable — handmatig aanmaken is een
hoofdpad**), `location_id`, `title`, `start`, `end`, `status` (Draft |
Scheduled | Cancelled), `source` (Calendar | Manual)

> Bruiloften, sportwedstrijden en externe activiteiten staan wel in het
> rooster maar niet in de agendafeed. De locatie is dus ook niet altijd de
> eigen accommodatie.

**calendar_mismatch** — `id`, `event_id`, `kind` (SourceRemoved |
TimeChanged | NoSourceLeft), `detected_at`, `resolved_at`

> Mismatch-detectie tussen rooster en agenda. Zonder dit ontdekt iemand pas
> bij het ruilen dat het evenement waarvoor hij ingedeeld staat niet meer
> bestaat.

**event_guest_tenant** — `event_id`, `tenant_id`, `owner_tenant_id`,
`status` (Invited | Accepted | Declined)

> `owner_tenant_id` is een bewuste denormalisatie van `event.tenant_id`.
> Zonder die kolom moet de RLS-policy hier in `event` kijken, terwijl de
> leespolicy van `event` op zijn beurt deze tabel raadpleegt — Postgres
> weigert dat als cirkelverwijzing ("infinite recursion detected in
> policy"). Zie `db/001_tenancy_rls.sql`.

### Rooster

**shift** — `id`, `event_id`, `team_role_id`, `start`, `end`,
`required_count`, `note`

**assignment** — `id`, `shift_id`, `person_id`, `status` (Assigned |
Withdrawn | CheckedIn | NoShow), `assigned_by`, `assigned_at`,
`withdrawn_at`, `withdrawal_reason` (nullable), `warnings_at_assignment`
(jsonb)

> `warnings_at_assignment` legt vast dat de planner bewust tegen een
> zacht signaal in heeft gepland. Puur informatief; er wordt niets
> geblokkeerd, dus er valt niets te overrulen.
>
> Let op `withdrawal_reason`: "ziek" is een gezondheidsgegeven. Vrij
> invulbaar laten of weglaten, en kort bewaren.

**swap_request** — `id`, `shift_id`, `assignment_id` (nullable),
`requested_by_person_id`, `target_person_id` (nullable), `kind` (Swap |
OpenCall), `status` (Open | Accepted | Rejected | Expired), `expires_at`

> Twee patronen, één tabel. **Swap**: A draagt over aan B,
> `target_person_id` gevuld. **OpenCall** (open oproep): een vraag aan de
> pool, `target_person_id` leeg, wie het eerst claimt krijgt hem. De open
> oproep komt in de praktijk vaker voor dan de ruil en ontstaat ook
> automatisch bij een afmelding. Acceptatie werkt de toewijzing direct bij
> — anders liegt het dagoverzicht.

**check_in** — `id`, `assignment_id`, `method` (Qr | Self | Supervisor),
`by_person_id`, `timestamp`

### Informatielaag

**guideline** — `id`, `tenant_id`, `title`, `sanitized_html`,
`visibility` (General | Restricted), `kind` (Card | Document),
`sort_order`, `version`, `published_at`, `updated_by`

> HTML wordt server-side gesaniteerd met een allow-list en gesaniteerd
> opgeslagen. Nooit `bypassSecurityTrustHtml` gebruiken om van een
> waarschuwing af te komen; dat is opgeslagen XSS.
>
> `visibility = Restricted` betekent: alleen leidinggevenden en
> coördinatoren, verse login vereist, **niet** offline gecachet. Daar hoort
> alles in wat over de fysieke beveiliging van een locatie gaat.

**document** — `id`, `tenant_id`, `title`, `version_label`, `file_ref`,
`is_current`, `published_at`

> Eén canonieke versie van het handboek. Oudere versies blijven bestaan
> maar zijn niet `is_current`.

**advisory** — `id`, `tenant_id`, `title`, `text`, `valid_from`,
`valid_until` (**niet nullable**), `event_type_id` (nullable), `priority`

> De verplichte einddatum is het hele punt. Een aandachtspunt zonder
> vervaldatum wordt binnen twee maanden behang.

**contact** — `id`, `tenant_id`, `name`, `function`, `phone`,
`is_emergency_number`, `sort_order`

### Techniek

**notification** — `id`, `tenant_id`, `person_id`, `channel` (Email |
Chat | WebPush), `template`, `context_id`, `scheduled_at`, `sent_at`,
`status`, `idempotency_key` (uniek)

> De unieke `idempotency_key` voorkomt dubbele herinneringen na een
> herstart van de scheduler.

**audit_log** — `id`, `tenant_id`, `actor_person_id`, `entity`,
`entity_id`, `action`, `old_value` (jsonb), `new_value` (jsonb),
`timestamp`

## Row-level security

De volledige set policies staat in `db/001_tenancy_rls.sql`. In het kort:

- Elke tenant-tabel heeft `ENABLE` **én** `FORCE ROW LEVEL SECURITY`.
  Zonder `FORCE` omzeilt de tabeleigenaar zijn eigen policies.
- `app.current_tenant()` leest `current_setting('app.tenant_id', true)` en
  geeft NULL als de waarde ontbreekt. Elke policy vergelijkt daarmee, dus
  een defect levert een lege lijst op in plaats van gegevens van een andere
  tenant.
- Afgeleide tabellen (`shift`, `assignment`, `qualification`) erven hun
  zichtbaarheid via een `EXISTS` op de bovenliggende tabel, die zelf al
  gefilterd wordt. Zo staat de gastlogica op één plek.
- De applicatie verbindt als `bcc_app`: geen `BYPASSRLS`, geen
  eigenaarschap. Migraties draaien als `bcc_owner`.
- **Eén uitzondering**: `tenant` zelf is voor `SELECT` niet achter
  `current_tenant()` verstopt. De API moet een tenant kunnen opzoeken op
  basis van het subdomein vóórdat `app.tenant_id` gezet is, en welke
  tenants bestaan staat toch al publiek in de DNS en de Caddyfile. Alle
  tabellen mét deelnemersgegevens blijven strikt gefilterd.

### Valkuil: connection pooling

`app.tenant_id` mag nooit blijven hangen op een gerecyclede verbinding.
De interceptor in `src/Infrastructure/Tenancy/` zet hem bij openen en wist
hem bij sluiten. Zet `Multiplexing=false` in de connectiestring, want
sessievariabelen overleven multiplexing niet.

De integratietest in `tests/Tenancy/` is het enige echte bewijs dat dit
werkt. Laat hem in CI draaien.

## Geschiktheidsberekening

Bij het vullen van een dienst wordt de pool gesorteerd:

1. **Uitsluiten (hard).** Persoon heeft de gevraagde dienstrol niet, of geen
   geldige kwalificatie voor die rol op de datum van de dienst.
2. **Uitzondering (wint van al het overige).** Staat er een
   `person_event_type_exception`, dan geldt die. `AlwaysDeploy`
   onderdrukt elk leeftijdssignaal; `NeverDeploy` toont de reden.
3. **Leidend (zacht).** Leeftijd op de datum van het evenement binnen het
   inzetbaar-bereik — dat van het dienstsjabloon als het gevuld is, anders
   dat van het evenementtype. Binnen bereik bovenaan, daarbuiten eronder
   met de reden erbij. **Nooit blokkerend**: bij uitval kort voor een
   evenement moet de planner iedereen kunnen kiezen.
4. **Overige waarschuwingen.** Vereiste bekwaamheid ontbreekt;
   beschikbaarheid overlapt met `Unavailable`; al ingedeeld op een
   overlappende dienst; kwalificatie verloopt binnen 30 dagen.

Leeftijd wordt berekend op `event.start`, niet op vandaag — iemand kan
tussen inplannen en uitvoeren uit het bereik schuiven. Bereken de signalen
daarom bij het tonen van een rooster, niet alleen bij het aanmaken van een
toewijzing.

Bij gedeelde evenementen gelden de bereiken en uitzonderingen van de
**eigenaar-tenant**.
