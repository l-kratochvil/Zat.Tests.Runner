# TODO

## HiPrio

- Testy Zat.Z2xx okamžitě končí chybou: Problém v Castle.Core; Skrze VS test runner chodí
- Dokončit vazbu na TestLink
- Zautomatizování kroků testera - základní
- Změna adresáře "Automized Tests":
  - Přesunout do Program Data / Roaming AppData
  - Přejmenovat na Zat.Tests
  - Přejmenovat adresář "Test libs" (test-libs? runner-libs?)
- TuiApp:
  - Implementovat výpis při startu testu (viz [test-run-start_output](test-run-start_output.png))
  - Odstranit legacy runner (ve vlastním commitu na main)
- WebApp:
  - Rework BindingSelect
  - Vylepšit pojmenování pro "fluxor stores" a "stores StoreBase": Obojí fungujou jinak, ale zároveň obojí je store
  - TestDiscovery: Nahradit store za Fluxor
  - Vytvořit viewmodely pro features (viz TestConfigurationViewModel)
  - Konfigurace testu stejná jako u TuiApp
  - Vyřešit Deploy
  - Kompletní code-review + refactor celé projektu WebApp (včetně testů) a docs
  - Lokalizace textů
  - Vylepšení vzhledu
- Vyřešit code todos

## MidPrio

- Update nuget balíčků solutionu
- WebApp:
  - Refactor TestDiscovery feature
  - Rozšíření testů pro komponenty DataContext, BinindgInput, BindingSelect, BindingCheckbox
- Email notifikace (po dokončení testu)
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

### Dokončit vazbu na TestLink

- Po dokončení Testlink vazby přepsat kapitoly, které byky automatizovany v dokument Automatizované testování
- Dále v dokumentu uvést "povinnosti vývojáře testů": Musí nahrát knihovny testu do adresáře, kde je runner vyžaduje (neuvádět konkrétní cestu, jelikož ta se bude časem měnit)

#### 1. Nahrávání přílohy k výsledku testu

- Zavolat metodu tl.uploadExecutionAttachment a předat získávané execution_id společně s parametry přílohy
  - executionId: ID vykonaného testu
  - fileName: Název souboru
  - fileType / mimetype: Typ souboru (MIME type)
  - content: Obsah souboru zakódovaný do Base64
  - title / description (volitelně): Název a popis přílohy
- Příloha se vytáhne z 'c:\Automized tests\runner-exchange\': Do tohoto adresáře test bude sypat veškeré screenshoty
- V názvu souboru bude ID testcasu (executionId)

#### 2. Pročistit ITestLink a Testlink

- Týká se i Model a poté XmlRpcStructConvertor

#### 3. Ověřit na začátku testu spojení s testlink (v api je něco jako Hello metoda)

- Použít SayHello
- pokud se nepodaří oznámit tuto skutečnost uživateli a zakázat využívání testlink (vyřadit prompt, zda se má nahrát výsledek)

#### 4. ITestLink.Config.Default: API key musí být secret

#### 5. Použít jiný balíček než CookComputing

- Použít Horizon.XmlRpc
- Použít /code-review -> /implement

#### 6. Nutno refaktorovat a vyřešit warningy

### Rework BindingSelect

- Z kolekce options (Offered) se vytvoří TextValueItem { string DisplayText, Value }
- Offered se prejmenuje na Options
- Bude mít volitelný param, pomocí kterého blokuje z dat získat display text pro option selectu: Pokud nebuď poskytnut, tak se display text získá Value.ToString()
- Zbavit se pak TestStationLabels: TestConfiguration.razor předá BindingSelect kolekci KeyValueItem (sestavenou na viewmodelu)
- Options budou IEnumerable:
  - Pokud budou dodané options typu INotifyCollectionChanged, tak budou rerenderovat uvnitř ObservableCollection komponenty

### Lokalizace textů

- Použít nějaký balíček, nebo vytvořit vlastní?
- V user settings se nastaví jazyk a podle něho se aplikace lokalizuje
- Použít oficiální doporučené řešení IStringLocalizer

```cs
  class Localization // DI služba
  {
    private Dictionary<EntryKey, string> texts;
    private Dictionary<EntryKey, string> formats;

    enum Language { En, Cs } // Nastaví se v settings (settings bude mít 2 kategorie: a. Global (sdílené pro všechny uživatele; např nastav je test prostředí); b. User

    enum Text { TextA, TextB }
    enum Format { FormatorA, FormatorB }

    string this[Text text]
      => texts[new EntryKey(Language, Text);

    string this[Format text]
      => formats[new EntryKey(Language, Text);

    record EntryKey(Language, Text);
}
```

### Zautomatizování kroků testera - základní

- Tzn. automatizovat kapitolu 2.9.3 z dokumentu Automatizované testování

### Vylepšení vzhledu

- Nasylovat:
  - Logger toggle
  - Toggle uzlu + Checkbox
- Použít prototype (ať prototyp ukáže varianty redesignu) + ladění v integrated browser
- Po schválení prorotypu nastylovat komplet

## MidPrio - Detaily

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
