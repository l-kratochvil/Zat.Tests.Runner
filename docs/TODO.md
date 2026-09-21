# TODO

## HiPrio

- Vyrešit issues
- Dokončit TestLinkApi
- Zautomatizování kroků testera - základní
- Vyřešit Deploy
- TestDiscovery: Nahradit store za Fluxor
- Kompletní code-review + refactor celé projektu WebApp (včetně testů) a docs
- Lokalizace textů napříč aplikacemi
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

### Dokončit TestLinkApi

- ITestLinkApiClient.Config.Default: API key musí být secret!
- Nešlo by použít klasické NET RPC namísto CookComputing.XmlRpc?
- Přejmenovat ITestLinkApiClient: ITestLinkApi? ITestLink?
- Nutno refaktorovat a vyřešit warningy

### Issues

1. Testy se z assembly nenačítají - [viz link](https://claude.ai/share/2111234d-351e-4c9b-93c9-846f62da3015)

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
