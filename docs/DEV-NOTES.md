# DEV NOTES

## Obecné poznámky

- IsRuntime flag na testsuite atributu
  - NUnitTestRunnerProxy by mohl filtrovat runtime test. sady na základě atributu (namísto jména)
  - Atribut by musel být nugetu/projektu sdíleném s Z2xxTest

### Virtuální stroj (VM)

- Bude se muset nacházet v síti (kvůli email notifikacím a přístupu na GOGO)
- Tzn, že se budou stahovat aktualizace win
- Tester bude muset vždy zkontrolovat, zda nejsou aktualizace, provést je a pak mi dá vědět, abych upravil image VM
- Oracle VirtualBox má problémy s odpojováním USB (VMWare se zdál být OK)

## Zat.Tests.Runner.WebApp

- V první fázi bude možné k apliakaci přistupovat pouze přímo z test. PC
- Web app server se na test. PC bude spouštět jako služba (automaticky při startu)
- Nahradit bootstrap za tailwind: použít [tento balíček](https://github.com/Practical-ASP-NET/Tailwind.Extensions.AspNetCore)

### Features

- Zaznamenávání průměrného času provádění entity?:
  - Průměrovaly by se časy testcasů, pro test suite by se vypočítala suma
  - Bude vyžadovat uložiště (pro začátek pro jednoduchost postačí soubor, ale v budoucnu by se hodila DB)

#### TestConfiguration

- Runtime - verze: volitelně půjde zadat datum vydání
- IDE - verze: volitelně půjde zadat datum vydání

#### TestExecution

- Blokovat při:
  1. Chybě editoru
  2. Již spuštěném testu (pro případ, kdy by 2 uživatele pustili test v jeden moment)
- Dvě úrovně řešení spuštění testů:
  1. UI (button disabled)
  2. Před skutečným spuštěním

#### TestDiscovery

- Nahradit store za Fluxor

### Vzdálený přístup k aplikaci

- Test. PC bude muset mít statickou IP a vlastní doménu
- Bylo by pak možné, že by test mohl být spuštěn z prohlížeče odkudkoliv a kdykoliv by bylo možné zkontrolovat průběh testu
- Při dokončení by uživatel dostal notifikaci
- Bylo by pak třeba řídit přístup (autorizace)
- Pokud by se vyřešilo automatické provádění instalací test. SW, tak by se mohlo využívat maximálně vzdáleně
- Web app server by pak mohl běžet na virtuálu (vmware): Host by se mohl v klidu za zamknout

### Auth

- [1](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/introduction?view=aspnetcore-10.0)
- [2](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/simple?view=aspnetcore-10.0)

## Zat.Tests.Runner.TuiApp

### HomeScreen

- Možnosti:
  - Runtime - verze: volitelné půjde zadat datum
  - IDE - verze: volitelné bude zadat datum
  - Dotaz: Odeslat výsledek do TestLinku?
- Rozšířit choices:
  - Před možností spuštění testu se vypíše možnost pro zobrazení preconditions (z TL) navolenych sad:
  - Formátování textu: Od H2 a další nadpisy se bude další text tabbovat (odsazovat)

### Spectre.Console

#### Užitečné features

- Progress + Status:
  - Zobrazování stavu, že něco probíhá
  - Zobrazit například ve chvíli kdy test běží:
    - Nezobrazovat output nunit console
    - Po dokončení vypsat chyby po svém pomocí spectre
- Live

#### Issues

- git issue: musí být title jinak bug
- AddChoicesGroup: select nenavrátí root

## Issues legacy runneru (TestRunner1.0)

### Issue #0001: System.BadImageFormatException : Nelze načíst soubor nebo sestavení Microsoft.SqlServer.BatchParser

- nastane v případě, že knihovny testů nejsou přeloženy pro x86
- řešením je nastavení platform target na x86 u projektu testů (Z200Tests)

### Issue #0002: Nedostupnost okna Konfigurátoru při procesu spuštěném jako správce

- Konzolová verze spouštěče (aplikace TestRunner) přistupuje k NUnit Console Runner skrze proces, batch verze spouštěče skrze Prompt Line Interpreter
- Chyba musí nějak souviset s TS.W, jelikož s ostatními kontroly iteragovat lze
- Výsledek z běhu testu při selhání uložen v TL pod test. plánem " debug " a sestavení " … Configurator issue "
- více informací viz. dokument Issue-0002
