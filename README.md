# eKvarovi

eKvarovi je web aplikacija za prijavu, obradu i praćenje kvarova na lokacijama.

Projekt je izrađen u ASP.NET Core / Blazor okruženju i sastoji se od tri projekta:

- `eKvarovi.Api` – ASP.NET Core Web API
- `eKvarovi.App` – Blazor aplikacija
- `eKvarovi.Shared` – zajednički modeli, DTO-i i zajednički tipovi

## Tehnologije

- .NET 10
- ASP.NET Core Web API
- Blazor Interactive Server
- Entity Framework Core
- SQLite
- MudBlazor
- JWT Bearer autentikacija i autorizacija
- Swagger / OpenAPI

## Struktura solutiona

### eKvarovi.Api

API projekt sadrži:

- API controllere
- `EKvaroviDbContext`
- Entity Framework Core migracije
- seed podataka
- JWT autentikaciju i autorizaciju
- poslovna pravila aplikacije
- upload i brisanje privitaka
- dashboard agregate

### eKvarovi.App

Blazor aplikacija sadrži:

- MudBlazor korisničko sučelje
- prijavu i odjavu korisnika
- navigaciju ovisnu o ulozi
- forme za unos i uređivanje podataka
- tablične prikaze
- osobne prikaze za prijavitelja i izvršitelja
- dashboard
- komunikaciju s API projektom preko `HttpClient`

### eKvarovi.Shared

Shared projekt sadrži:

- modele
- DTO klase
- enum vrijednosti
- zajedničke tipove koje koriste API i Blazor aplikacija

## Baza podataka

Aplikacija koristi SQLite bazu podataka.

Connection string:

```text
Data Source=eKvarovi.db
```

Struktura baze definira se kroz Entity Framework Core migracije.

Za ručnu primjenu migracija:

```powershell
dotnet ef database update --project eKvarovi.Api --startup-project eKvarovi.Api
```

API pri pokretanju primjenjuje postojeće migracije i dodaje početne podatke ako oni još ne postoje.

## Početni šifrarnici

### Vrste lokacija

- Upravna zgrada
- Škola
- Zdravstvena ustanova
- Skladište

### Vrste kvarova

- Elektrika
- Voda
- Grijanje
- Mreža
- Građevinski radovi
- Ostalo

### Prioriteti

- Nizak
- Srednji
- Visok
- Kritičan

### Statusi prijave

- Zaprimljeno
- Pregledano
- Dodijeljeno
- U radu
- Riješeno
- Zatvoreno

### Statusi intervencije

- Planirana
- U tijeku
- Završena
- Neuspješna

### Jedinice materijala

- Komad
- Metar
- Litra
- Kilogram
- Paket

## Korisničke uloge

Aplikacija podržava više uloga na jednom korisničkom računu.

### Admin

Administrator ima puni administrativni pristup aplikaciji. Može upravljati lokacijama, djelatnicima, izvršiteljima, materijalima i korisničkim računima te pregledavati sve prijave, radne naloge, intervencije i dashboard.

### Manager

Upravitelj vodi proces obrade prijave. Može pregledavati sve prijave, odrediti vrstu kvara, prioritet i rok, dodijeliti ili ponovno dodijeliti izvršitelja uz očuvanje povijesti, pratiti intervencije i materijale te zatvoriti riješenu prijavu.

### Reporter

Prijavitelj je povezan s djelatnikom. Može prijaviti kvar za lokaciju kojoj pripada, dodati početnu fotografiju i pregledavati samo vlastite prijave te njihov status, prioritet, dodijeljenog izvršitelja i rezultat rješavanja.

Prijavitelj ne može sam dodjeljivati izvršitelja, mijenjati prioritet ili zatvarati prijavu.

### Technician

Izvršitelj radi na vlastitim dodijeljenim radnim nalozima. Može pokrenuti intervenciju, evidentirati bilješku rada i utrošeni materijal, dodati fotografiju nakon rada i PDF dokument te završiti intervenciju uspješno ili neuspješno.

## Demo korisnici

| Uloga | Email | Lozinka |
|---|---|---|
| Admin | `admin@ekvarovi.local` | `Admin123!` |
| Manager | `manager@ekvarovi.local` | `Manager123!` |
| Reporter | `reporter@ekvarovi.local` | `Reporter123!` |
| Technician | `technician@ekvarovi.local` | `Technician123!` |

Lozinke se u bazi spremaju kao hash vrijednosti.

## JWT autentikacija

API koristi JWT Bearer autentikaciju.

JWT sadrži podatke potrebne za identitet i autorizaciju korisnika, uključujući:

- ID korisnika
- email
- prikazno ime
- korisničke uloge
- `employee_id` ako je korisnik povezan s djelatnikom
- `technician_id` ako je korisnik povezan s izvršiteljem

Endpointi:

```text
GET /api/faultreports/mine
GET /api/workassignments/mine
```

identitet korisnika određuju iz JWT claima, a ne iz ID-a poslanog iz korisničkog sučelja.

## JWT tajni ključ

JWT `SigningKey` nije spremljen u `appsettings.json` niti u Git repozitoriju.

Za development se koristi .NET User Secrets.

Primjer postavljanja:

```powershell
dotnet user-secrets set "Jwt:SigningKey" "development-signing-key-with-at-least-32-characters" --project eKvarovi.Api
```

Secret mora biti postavljen prije pokretanja API projekta.

## Glavne funkcionalnosti

Aplikacija podržava:

- upravljanje lokacijama
- upravljanje djelatnicima
- upravljanje izvršiteljima
- kreiranje i uređivanje prijava kvarova
- pregled detalja prijave
- serversko filtriranje i sortiranje prijava
- dodjelu i ponovnu dodjelu izvršitelja uz očuvanje povijesti
- pregled radnih naloga
- pokretanje i završavanje intervencija
- serversko filtriranje i sortiranje intervencija
- evidentiranje utrošenog materijala
- upravljanje materijalima
- pregled vlastitih prijava prijavitelja
- pregled vlastitih radnih naloga izvršitelja
- upload početnih fotografija
- upload fotografija nakon rada
- upload PDF dokumenata
- administraciju korisnika
- više uloga na jednom korisničkom računu
- dashboard

## Poslovni tok prijave kvara

Osnovni životni ciklus prijave je:

```text
Zaprimljeno
    ↓
Pregledano
    ↓
Dodijeljeno
    ↓
U radu
    ↓
Riješeno
    ↓
Zatvoreno
```

Prijavitelj kreira prijavu.

Upravitelj pregledava prijavu, određuje vrstu, prioritet i rok te dodjeljuje izvršitelja.

Izvršitelj pokreće intervenciju i evidentira izvršene radove, privitke i utrošeni materijal.

Neuspješna intervencija ostaje sačuvana u povijesti.

Uspješno završena intervencija postavlja prijavu u status `Riješeno`.

Nakon uspješnog rješavanja prijavu zatvara upravitelj.

## Dodjele izvršitelja

Jedna prijava u jednom trenutku može imati najviše jednu aktivnu dodjelu.

Kod ponovne dodjele prethodna aktivna dodjela se završava, ali se ne briše. Nova dodjela dobiva novi zapis, čime se čuva povijest svih izvršitelja koji su radili na prijavi.

## Intervencije i materijali

Radni nalog može imati više intervencija.

Intervencija može koristiti više materijala, a isti materijal može se pojaviti u više intervencija. Veza se sprema kroz `InterventionMaterial`, zajedno s količinom.

Količina materijala mora biti veća od nule.

Materijal koji je već korišten u intervenciji ne smije se fizički izbrisati jer je dio poslovne povijesti. Takav materijal može se deaktivirati.

Materijal koji još nije korišten može se fizički izbrisati.

## Privici

Aplikacija podržava:

- početnu fotografiju kvara
- fotografiju nakon rada
- PDF dokument

Datoteke se fizički spremaju u:

```text
eKvarovi.Api/wwwroot/uploads/faultreports
```

Baza sprema metapodatke o datoteci, uključujući originalni naziv, spremljeni naziv, content type, veličinu, namjenu privitka, vrijeme uploada, pripadajuću prijavu i pripadajuću intervenciju kada postoji.

Za spremljeni naziv koristi se sigurno generirani GUID naziv.

## Dashboard

Dashboard agregate izračunava API.

Admin i Manager vide:

- broj otvorenih prijava
- broj kritičnih prijava
- broj zakašnjelih prijava
- broj prijava bez aktivnog izvršitelja
- broj aktivnih intervencija
- prosječno vrijeme rješavanja
- zadnjih pet prijava

Reporter vidi broj svojih prijava.

Technician vidi broj svojih radnih naloga.

Korisnik koji ima više uloga može vidjeti više odgovarajućih osobnih podataka.

## Pokretanje projekta

Prije prvog pokretanja potrebno je postaviti JWT secret:

```powershell
dotnet user-secrets set "Jwt:SigningKey" "development-signing-key-with-at-least-32-characters" --project eKvarovi.Api
```

Pokretanje API projekta:

```powershell
dotnet run --project eKvarovi.Api
```

Pokretanje Blazor aplikacije:

```powershell
dotnet run --project eKvarovi.App
```

Development adrese projekta:

```text
API: https://localhost:7294
App: https://localhost:7194
```

Swagger je dostupan u development okruženju preko API projekta.

## Migracije

Za primjenu migracija na praznu bazu:

```powershell
dotnet ef database update --project eKvarovi.Api --startup-project eKvarovi.Api
```

Lookup podaci definirani su kroz EF Core seed konfiguraciju.

Poslovni demo podaci i demo korisnici dodaju se kroz aplikacijski seed ako već ne postoje.

## Sigurnost

Autorizacija poslovnih akcija provodi se na API strani. Skrivanje gumba ili navigacijskog linka u Blazor sučelju ne predstavlja sigurnosnu zaštitu samo po sebi.

API koristi odgovarajuće HTTP statuse, uključujući `200`, `201`, `204`, `400`, `401`, `403` i `404`.

## Napomena

Projekt je izrađen kao edukacijska aplikacija za završni projekt.
