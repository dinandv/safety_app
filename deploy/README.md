# Runbook — hosting

Server: Hetzner CX33, eu-central (Helsinki). 4 vCPU, 8 GB, 80 GB.
Ruim bemeten voor Postgres, de API, de statische frontend en de proxy.

Het IP-adres, de hostnamen en alle wachtwoorden horen in de secret store,
niet in dit bestand en niet in deze repo.

## Eenmalig inrichten

1. **SSH dichtzetten.** Alleen sleutels, geen wachtwoordlogin, root login
   uit. Een aparte gebruiker met sudo.

2. **Cloud firewall van Hetzner**, niet alleen een firewall op de machine.
   Open: 22 (liefst beperkt tot je eigen IP), 80 en 443. Verder niets.

   Let op IPv6. De server heeft een `/64`, en regels die alleen op IPv4
   staan laten de hele v6-kant open. Dat is de meest gemaakte fout op
   deze provider.

3. **Backups aanzetten** in het Hetzner-paneel. Circa 20% van de
   serverprijs. Staat nu uit.

4. **Daarnaast een eigen `pg_dump`.** Een Hetzner-backup is een image van
   de hele machine: daarmee draai je alles terug of niets. Een per ongeluk
   verwijderde tenant herstel je alleen uit een dump. Dagelijks, versleuteld,
   naar een andere locatie dan deze server.

5. **Restore één keer echt oefenen**, met een stopwatch erbij. Een
   ongeteste backup is een gevoel, geen voorziening.

6. **Automatische securityupdates** (`unattended-upgrades`) en `fail2ban`.

## Database

Twee rollen, en dat onderscheid is de kern van de tenantscheiding:

- `bcc_owner` — eigenaar van de tabellen, draait migraties. Nooit de
  applicatierol.
- `bcc_app` — waar de API mee verbindt. Geen `BYPASSRLS`, geen
  eigenaarschap. Zonder dit onderscheid staat row-level security
  effectief uit.

Postgres luistert alleen binnen het Docker-netwerk. Er staat bewust geen
`ports:` op de db-service. Moet je erbij, dan via een SSH-tunnel.

## DNS

Wildcard `*.bcc-safety.getanapp.nl` met een A-record naar het IPv4-adres
en een AAAA-record naar het IPv6-adres. Een nieuwe tenant is daarna een
blok in de `Caddyfile` en een reload — geen deploy.

## Deploy

```
docker compose -f deploy/docker-compose.yml up -d --build
```

Migraties draaien als `bcc_owner`, apart van de applicatiestart. Laat de
API nooit zijn eigen schema migreren met de app-rol.

## Wat te doen als het misgaat

- **Lege lijsten in de applicatie.** Waarschijnlijk staat `app.tenant_id`
  niet gezet. Dat is het bedoelde faalgedrag: liever niets tonen dan
  gegevens van een andere tenant. Controleer de interceptor.
- **Certificaat verloopt.** Caddy vernieuwt zelf; controleer of poort 80
  open is, want daar loopt de validatie over.
- **Schijf vol.** 80 GB is ruim, maar Postgres-WAL en Docker-images lopen
  op. Zet een monitor op vrije ruimte voordat het je verrast.
