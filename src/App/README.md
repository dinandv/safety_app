# Deelnemer-PWA

Angular-frontend voor de deelnemers: het dagoverzicht, de eigen diensten,
open oproepen en de informatietab. Mobiel eerst — 390px is de basis,
breder is dezelfde weergave met meer lucht.

Alle **code, paden en routes zijn Engels**, alle **tekst die een
deelnemer leest is Nederlands**. Zie `CLAUDE.md` in de repo-root.

## Lokaal draaien

De API en de app draaien apart. De API eerst:

```bash
dotnet run --project src/Api
```

Dan de app, met een proxy naar die API (`proxy.conf.json`):

```bash
npm start --prefix src/App
```

De app staat op <http://localhost:4200>, de API op poort 5080. Omdat
alles via de proxy op dezelfde origin binnenkomt, werkt het
sessiecookie hetzelfde als in productie.

In productie is er geen aparte frontendcontainer: `deploy/Dockerfile.api`
bouwt deze app en zet hem in de `wwwroot` van de API. Eén origin, geen
CORS, één Caddy-blok per tenant.

## Indeling

```
src/styles/      tokens en fonts — de designtokens uit het design system
src/app/core/    API-client, sessie, offline cache, datumopmaak
src/app/ui/      de UI-kit: knop, badge, hesjechip, melding, kaart, toast
src/app/shell/   de tabbalk en de router-outlet eromheen
src/app/features/ één map per scherm
```

## Offline

Twee lagen, met verschillende taken:

- **Service worker** (`ngsw-config.json`) cachet de app zelf: HTML, JS,
  CSS, fonts en iconen. Geen API-antwoorden — die staan er bewust niet
  in, zodat er maar één plek is waar gecachete gegevens vandaan komen en
  maar één plek om leeg te maken bij uitloggen.
- **`CachedResource`** (`core/cached-resource.ts`) bewaart het laatste
  antwoord van een GET in `localStorage`. Lukt een verzoek niet omdat de
  server onbereikbaar is, dan komt die kopie terug mét het tijdstip
  erbij. Nooit stilzwijgend: een dagoverzicht van vanochtend mist precies
  de afmelding die je wilde weten.

Antwoorden van de server — ook een 500 — zijn een echte fout en worden
als fout getoond. Alleen een onbereikbare server valt terug.

Uitloggen wist de cache. Daar staan telefoonnummers van dienstgenoten in,
en die horen niet langer op een toestel te staan dan de sessie.

## Iconen

De PWA-iconen worden gegenereerd:

```bash
python src/App/tools/generate-icons.py
```

Dat script is de bron van de vorm; bewerk de PNG's niet met de hand.

## Fonts

Zelf gehost, alleen de latijnse subsets (`src/styles/fonts.css`). Geen
font-CDN: de contactkaart moet werken zonder bereik, en het IP-adres van
een vrijwilliger hoort niet naar een derde partij te gaan om een kop te
tekenen.
