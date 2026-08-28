# Ontwerp — Safety App

Roosterapplicatie voor vrijwillige veiligheidsteams: hoofd-BHV, BHV,
toezicht en EHBO rond evenementen van een vereniging of geloofsgemeenschap.
Multi-tenant, zodat meerdere afdelingen op één installatie draaien.

> Dit document beschrijft het ontwerp. De analyse van de organisatie
> waarvoor de eerste versie gebouwd wordt, staat bewust niet in deze repo.

## Probleem

Kleine vrijwilligersteams coördineren hun inzet doorgaans via een chatgroep
en een spreadsheet. Dat levert vier terugkerende klachten op:

1. Niemand weet wie er vandaag dienst heeft.
2. Uitzoeken wie inzetbaar is kost de coördinator tijd.
3. Mensen komen niet opdagen.
4. Richtlijnen en afspraken zijn onvindbaar of verouderd.

En één die zelden genoemd wordt maar het meeste kost: **onderlinge
ruilingen lopen buiten het systeem om**, waardoor het rooster binnen
enkele weken niet meer klopt met de werkelijkheid.

## Uitgangspunten

**De dienst is het roosterbare object, niet het evenement.** Een evenement
heeft meerdere te vullen plekken met verschillende rollen, tijdvakken en
aantallen. Bezetting, gaten, ruilen, herinneren en inchecken hangen
allemaal aan de dienst.

**Ruilen hoort in de applicatie.** Gebeurt het ernaast, dan liegt het
dagoverzicht en is de applicatie erger dan geen applicatie. Naast de ruil
tussen twee personen bestaat de open oproep: een vraag aan de pool waar de
eerste die reageert de dienst claimt.

**Alleen kwalificatie blokkeert.** Geen geldig certificaat voor een rol
betekent niet inplanbaar in die rol. Al het andere — leeftijdsbereik,
beschikbaarheid, bekwaamheden — sorteert en waarschuwt, maar houdt niemand
tegen. Bij uitval kort voor een evenement moet de coördinator iedereen
kunnen kiezen.

**Geen regelmotor waar een oordeel hoort.** Wie waar past is deels
uitrekenbaar en deels een menselijk oordeel. Het uitrekenbare deel doet de
applicatie; het oordeel wordt vastgelegd als expliciete uitzondering
*met reden*, zodat het de coördinator die het vastlegde overleeft.

## Model in het kort

**Wat iemand kan.** Teamrollen (hoofd-BHV, BHV, toezicht, EHBO) en losse
bekwaamheden in één configureerbare tabel. Volgt uit opleiding en
certificaat, met vervaldatum en tijdige signalering.

**Waar iemand past.** Het evenementtype draagt twee optionele
leeftijdsbereiken — één voor de doelgroep (beschrijvend) en één voor de
inzetbare veiligheidsmensen (leidend, niet blokkerend) — plus optioneel een
vereiste bekwaamheid. Het inzetbare bereik mag per teamrol afwijken via het
dienstsjabloon.

Evenementtypes zijn fijnmazig: activiteit maal leeftijdsgroep. Het aanmaken
van een type moet daarom licht blijven.

**Uitzonderingen** overrulen het bereik in beide richtingen: altijd
inzetten of niet inzetten, met verplichte reden.

Leeftijd wordt berekend op de datum van het evenement, nooit op vandaag.

## Evenementen

Een publieke ICS-feed levert **kandidaat-evenementen** die een planner
koppelt aan een type; titelregels geven hooguit een suggestie. Handmatig
aanmaken is een hoofdpad, niet een randgeval — niet elk evenement staat in
een agenda.

Mismatch-detectie is een eigen functie: toewijzingen op een datum waarvoor
de agenda niets meer heeft, of een verzette afspraak met bestaande
toewijzingen. Dat wordt gemeld, nooit stil opgelost.

Technisch: `RRULE`, `EXDATE` en overrides expanderen met een bestaande
library; sleutel is `UID` plus `RECURRENCE-ID`; sync is idempotent via een
inhouds-hash.

## Multi-tenancy

Eén database, één schema, `tenant_id` op elke tabel, isolatie via Postgres
row-level security. EF Core global query filters mogen dat dupliceren maar
nooit vervangen.

Schema- of database-per-tenant valt af omdat evenementen door meerdere
afdelingen samen bemenst kunnen worden: één eigenaar-tenant nodigt andere
tenants uit, en gedeeld wordt alleen naam, teamrol, telefoon en
kwalificatiestatus.

Het faalgedrag is bewust gekozen: `app.current_tenant()` levert NULL als de
sessievariabele ontbreekt, en elke policy vergelijkt daarmee. Een defect in
de applicatielaag levert dus een lege lijst op, geen gegevens van een andere
tenant. Zie `db/001_tenancy_rls.sql` en `tests/Tenancy/`.

## Toegang

Geen wachtwoorden: magic link of zescijferige code per e-mail. Voor de
meeste handelingen is inloggen niet nodig — elke notificatie bevat een
ondertekende, kort geldige actielink die tot één dienst beperkt is. PWA,
geen app stores.

Deelnemers zien alleen naam, rol en telefoon van wie met hen dezelfde
dienst draait, plus een vaste contactkaart. Geen volledige deelnemerslijst.

De informatielaag kent twee zichtbaarheidsniveaus. Algemeen zichtbaar en
offline beschikbaar: contactkaart, korte gedragskaarten, en de huidige
versie van het handboek. Beperkt zichtbaar, verse login vereist en niet
offline gecachet: alles wat over fysieke beveiliging van een locatie gaat.

## Notificaties

E-mail is het fundament (authenticatie plus vangnet). Een chatbot kan als
primair kanaal dienen zodra een deelnemer gekoppeld is, maar verstuurt
uitsluitend inhoudsloze berichten met een link — de inhoud blijft in het
eigen systeem, binnen de EU. Webpush is een bonus voor wie de PWA
installeert.

Escalatie doet meer tegen no-shows dan herinneringen: een melding aan de
planner zodra een dienst 48 uur voor aanvang niet gevuld is, en bij elke
afmelding, die meteen een open oproep start.

## Stack

ASP.NET Core, EF Core, PostgreSQL, Angular als PWA. Docker Compose op één
server, reverse proxy ervoor, wildcard-DNS zodat een nieuwe tenant een
DNS-record kost en geen deploy.

## Licentie

Code onder Apache 2.0. Het logo is afgeleid van Twemoji en valt onder
CC-BY 4.0 — zie `NOTICE`.
