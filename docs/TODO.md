# TODO

## HiPrio

- Vyrešit issues
- Dokončit TestLinkApi
- Zautomatizování kroků testera - základní
- WebApp:
  - Rework BindingSelect
  - TestDiscovery: Nahradit store za Fluxor
  - Vytvořit viewmodely pro features (viz TestConfigurationViewModel)
  - Vyřešit Deploy
  - Kompletní code-review + refactor celé projektu WebApp (včetně testů) a docs
  - Lokalizace textů
  - Vylepšení vzhledu

## MidPrio

- Update nuget balíčků solutionu
- Refactor TestDiscovery feature
- Rozšíření testů pro komponenty DataContext, BinindgInput, BindingSelect, BindingCheckbox
- Playwright/vitest E2E tests

## LoPrio

- Změna "Main" v menu na ikonku "home".
- Vypisování manuálních předpokladů (získá se z TL)?
- Email notifikace (po dokončení testu)
- Zautomatizování kroků testera - pokročilé?

## HiPrio - Detaily

### Issues

1. Testy se z assembly nenačítají:

- [viz link](https://claude.ai/share/2111234d-351e-4c9b-93c9-846f62da3015)
- Zkusit nastavit x86 u Zat.Z2xxTests?

### Dokončit TestLinkApi

- ITestLink.Config.Default: API key musí být secret!
- Použít jiný balíček než CookComputing?:
  - Použít Horizon.XmlRpc / TouchSocket.XmlRpc
  - Použít /code-review -> /implement
- Přejemnovat TestLink.API namespace
- Nutno refaktorovat a vyřešit warningy

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
- TestLink integrace:
  - Bude vyžádat specifikovat datum vydání instalací (IDE, RT)
  - Ověřit na začátku testu spojení s testlink (v api je něco jako Hello metoda): pokud se nepodaří oznámit tuto skutečnost uživateli a zakázat využívání testlink (vyřadit prompt, zda se má nahrát výsledek)
- Nahrávání přílohy k výsledku testu:
  - Zavolat metodu tl.uploadExecutionAttachment a předat získávané execution_id společně s parametry přílohy:
    - executionId: ID vykonaného testu
    - fileName: Název souboru
    - fileType / mimetype: Typ souboru (MIME type)
    - content: Obsah souboru zakódovaný do Base64
    - title / description (volitelně): Název a popis přílohy
  - Příloha se vytáhne z 'c:\Automized tests\Tests Output\Screenshots\Current\': Do tohoto adresáře test bude sypat veškeré screenshoty
  - V názvu souboru bude ID testcasu (executionId)

### Vylepšení vzhledu

- Nasylovat:
  - Logger toggle
  - Toggle uzlu + Checkbox
- Použít prototype (ať prototyp ukáže varianty redesignu) + ladění v integrated browser
- Po schválení prorotypu nastylovat komplet

## MidPrio - Detaily

### Rozšíření testů pro komponenty DataContext, BinindgInput, BindingSelect, BindingCheckbox

- Rozšířit o testy:
  - Komponenta dostala DataContext (očekáváno)
  - Chování komponenty, když nebyl poskytnut DataContext
  - Binding z viewmodel funguje
  - Chyby z viewmodel jsou propagovány do komponenty

### Playwright/vitest E2E tests

- Aplikace by se před testem pustila s mock službami (např INunitRunnerProxy)
- Spousta dosavadních testů by se pak asi mohla vyhodit

## LoPrio - Detaily

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
