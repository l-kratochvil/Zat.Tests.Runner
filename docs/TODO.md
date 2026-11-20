# TODO

## HiPrio

- Po dokončení hlavní produkční verze TuiApp: squash merge develop -> main
- Vyřešit veškeré warningy (celý solution): Stojí tokeny!
- Před testem kontrolovat spojení se stanicemi: Viz legacy runner RunTest (TestEnvironmentUtils.CheckTestStationsConnection)
- TuiApp:
  - Doladit barvu při multiselection: Na Test PC není čitelný text aktuálně zaměřené položky
  - Implementovat výpis při startu testu (viz [test-run-start_output](test-run-start_output.png))
  - Z2xxTests: Odstranit legacy runner (uchovat v historii ve vlastním commitu na main)
  - Sdílet logiku pro získávání verzí nainstalovaných runtime s WebApp TestConfigurationViewModel
- WebApp:
  - Rework BindingSelect
  - Zrušit fluxor a zaměnit za WeakReferenceMessanger??
  - Vylepšit pojmenování pro "fluxor stores" a "stores StoreBase": Obojí fungujou jinak, ale zároveň obojí je store
  - TestConfiguration: Musí umožnovat vybrat vícero (comobobox) HwAssemblyType (TestedHwAssemblyType umožňuje pouze jeden. Musí se hlídat navolené možnosti stejně jako u TuiApp (nesmí být navoleno zároven BB1M a BB37M))
  - Vyřešit Deployment
  - Refactor TestDiscovery feature
  - Kompletní code-review + refactor celé projektu WebApp (včetně testů) a docs
- Vyřešit code todos

## MidPrio

- Update nuget balíčků solutionu
- TuiApp:
  - Pro práci s filesystém používat službu IFileSystem (viz WebApp TestConfigurationViewModel)
  - Rozšířit možnosti nastavení Debug režimu: Při povolení debug režimu bude možné nastavit: a. telegram notifikace, b. attach debuggeru
- TestLink: Použít jiný balíček než CookComputing:
  - Použít Horizon.XmlRpc
  - Použít /code-review -> /implement
- Použít m pocock skill: improve-codebase-architecture
- Změna adresáře "Automized Tests":
  - Přesunout do "C" do "Program Data" / Roaming AppData
  - Pojmenovaat na Zat.Tests
  - Přejmenovat adresář "Test libs" (test-libs? runner-libs?)
- WebApp:
  - Vyřešit multi select HW tested assembly: Využít ItemList komponentu
  - Rozšíření testů pro komponenty DataContext, BinindgInput, BindingSelect, BindingCheckbox
  - Lokalizace textů
  - Vylepšení vzhledu
- Dokončit větev fix_concurrent-test-execution-defense: Je major change a je nutné poctivě otestovat!
- TuiApp:
  - Migrace do Terminal.Gui?
  - Převést na Features (ať je struktura projektu s WebApp konzistentní)
  - Installer: Ať je součástí instalace test-runner.bat + zápis cesty k nemu do env variable Paths

## LoPrio

- WebApp:
  - Změna "Main" v menu na ikonku "home"
  - Playwright/vitest E2E tests
- TuiApp:
  - Migrace do Terminal.Gui?
- Vypisování manuálních předpokladů (získá se z TL)?
- Zautomatizování kroků testera - pokročilé?

## HiPrio - Detaily

### Rework BindingSelect

- Viz docs/spec/bindingselect-rework.md
- Po dokončení spec smazat!

### Lokalizace textů

- Použít nějaký balíček, nebo vytvořit vlastní?
- V AppSettings skupina Uživatelská nastavení se nastaví jazyk a podle něho se aplikace lokalizuje
- Použít oficiální doporučené řešení IStringLocalizer

## MidPrio - Detaily

### Vylepšení vzhledu

- Nasylovat:
  - Logger toggle
  - Toggle uzlu + Checkbox
- Použít prototype (ať prototyp ukáže varianty redesignu) + ladění v integrated browser
- Po schválení prorotypu nastylovat komplet

### Migrace do Terminal.Gui

- Agent: Vytvořit nejprve prázdný prototyp UI (bez chování)
- UI inspirované WebApp
- Vytvoří wireframe/screenshot WebApp?
- Šel by použít viewmodel WebApp?

## LoPrio - Detaily

### Rozšíření testů pro komponenty DataContext, BinindgInput, BindingSelect, BindingCheckbox

- Rozšířit o testy:
  - Komponenta dostala DataContext (očekáváno)
  - Chování komponenty, když nebyl poskytnut DataContext
  - Binding z viewmodel funguje
  - Chyby z viewmodel jsou propagovány do komponenty

### Playwright/vitest E2E tests

- Aplikace by se před testem pustila s mock službami (např INunitRunnerProxy)
- Spousta dosavadních testů by se pak asi mohla vyhodit

### Změna "Main" v menu na ikonku "home"

- Použít toto řešení:
  - balíčky: a. Blazicons (některé sady nejsou free), b. MudBlazor
  - css: a. Bootstrap Icons; b. Font Awesome (některé sady nejsou free)

### Zautomatizování kroků testera - pokročilé

- Jedná se o tyto kroky:
  1. Instalace testovaných aplikací
  2. Nasazení VM skrze COM
  3. Stanice HW00_ST01 do vých. stavu?
- Instalace testovaných aplikací: Budou se procházet adresáře s instalacemi na GOGO a nabídne se výběr
