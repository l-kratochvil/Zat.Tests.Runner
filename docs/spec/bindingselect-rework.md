# Rework BindingSelect

## Část 1

- Komponenta bude mít volitelný parameter (Func<TValue, string>), pomocí kterého bude možné z dat získat display text kterým bude položka zobrazena ve výběru: Pokud parametr nebude nastaven, tak se display text získá Value.ToString()
- Pro Options lze použít komponentu ItemList!
- Pokud bude Options INotifyCollectionChanged tak při Changed eventu provést rerender
- K čemu je potřeba současný "offered" field?
- Proč k nečemu, aby komponenta dědila od BindingComponentBase?:
  - Pokud je zbytečné, mělo by se odstranit a přejmenovat komponentu na Select
  - Zbytečné ale NENÍ - Binding se použije pro: a. Přiřazení vybrané hodnoty na viewmodel; b. Validace (resp. získání chyby)

## Část 2

- Zbavit se pak TestStationLabels: TestConfiguration.razor předá BindingSelect kolekci KeyValueItem (sestavenou na viewmodelu)
