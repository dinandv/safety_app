namespace BccSafety.Infrastructure.Entiteiten;

public enum PersoonStatus
{
    actief,
    inactief,
}

public enum Approl
{
    platformbeheerder,
    tenantbeheerder,
    planner,
    deelnemer,
}

public enum TeamrolSoort
{
    dienstrol,
    bekwaamheid,
}

public enum BeschikbaarheidSoort
{
    niet_beschikbaar,
    voorkeur,
}

public enum UitzonderingOordeel
{
    altijd_inzetten,
    niet_inzetten,
}

public enum KandidaatEvenementStatus
{
    nieuw,
    gekoppeld,
    genegeerd,
    gewijzigd,
    verwijderd_in_bron,
}

public enum EvenementStatus
{
    concept,
    gepland,
    geannuleerd,
}

public enum EvenementBron
{
    agenda,
    handmatig,
}

public enum AgendaAfwijkingSoort
{
    bron_verwijderd,
    tijd_gewijzigd,
    geen_bron_meer,
}

public enum GasttenantStatus
{
    uitgenodigd,
    geaccepteerd,
    geweigerd,
}

public enum ToewijzingStatus
{
    ingedeeld,
    afgemeld,
    ingecheckt,
    no_show,
}

public enum RuilverzoekSoort
{
    ruil,
    open_oproep,
}

public enum RuilverzoekStatus
{
    open,
    geaccepteerd,
    afgewezen,
    verlopen,
}

public enum CheckinMethode
{
    qr,
    zelf,
    leidinggevende,
}

public enum RichtlijnZichtbaarheid
{
    algemeen,
    beperkt,
}

public enum RichtlijnSoort
{
    kaart,
    document,
}

public enum NotificatieKanaal
{
    email,
    chat,
    webpush,
}
