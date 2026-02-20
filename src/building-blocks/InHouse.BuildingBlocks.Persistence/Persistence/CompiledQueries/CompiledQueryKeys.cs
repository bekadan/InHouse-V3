namespace InHouse.BuildingBlocks.Persistence.CompiledQueries;

public static class CompiledQueryKeys
{
    // Stabil key üretmek için yardımcı.
    // Örn: $"{typeof(TContext).FullName}:{name}"
    public static string For<TContext>(string name)
        => $"{typeof(TContext).FullName}:{name}";
}