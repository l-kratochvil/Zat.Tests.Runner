namespace Zat.Tests.Runner.TuiApp.Stores;

using System;
using System.Text.Json;

internal class JsonPersistanceStore<TJsonModel>(
    Func<TJsonModel> defaultModelFactory,
    string jsonModelPath)
    : IJsonPersistanceStore<TJsonModel>
{
    /// <inheritdoc/>
    public TJsonModel Current { get; private set; } = defaultModelFactory();

    public static TStore InitStore<TStore>(
        string jsonPath,
        Func<TJsonModel> defaultModelFactory,
        Func<TJsonModel, TStore> storeFactory)
    {
        TJsonModel jsonModel;
        if (File.Exists(jsonPath))
        {
            jsonModel = File.ReadAllText(jsonPath)
                            .Pipe(json => JsonSerializer.Deserialize<TJsonModel>(
                                json, JsonPersistanceStore.JsonSerializerOptions))
                        ?? defaultModelFactory();
        }
        else
        {
            jsonModel = defaultModelFactory();
            Save(jsonModel, jsonPath);
        }

        return storeFactory(jsonModel);
    }

    /// <inheritdoc/>
    public void Update(Func<TJsonModel, TJsonModel> updator)
        => this.Update(updator(this.Current));

    /// <inheritdoc/>
    public void Update(TJsonModel currentSettings)
    {
        this.Current = currentSettings;
        Save(currentSettings, jsonModelPath);
    }

    private static void Save(TJsonModel jsonModel, string jsonPath)
        => JsonSerializer
            .Serialize(jsonModel, JsonPersistanceStore.JsonSerializerOptions)
            .Visit(serialized => File.WriteAllText(jsonPath, serialized));
}

internal static class JsonPersistanceStore
{
    public static readonly JsonSerializerOptions JsonSerializerOptions =
        new()
        {
            WriteIndented = true,
        };
}