using Content.Shared.Civ14.CivResearch;

namespace Content.Shared.Humanity.Research;

public static class NomadResearch
{
    public static string AgeName(int age) => age switch
    {
        0 => "Каменный век",
        1 => "Бронзовый век",
        2 => "Железный век",
        3 => "Средневековье",
        4 => "Возрождение",
        5 => "Индустриальная эпоха",
        6 => "Эпоха мировых войн",
        7 => "Современность",
        _ => "Новейшая эпоха",
    };

    public static (string Stack, int Count, string Name) Experiment(int age) => age switch
    {
        0 => ("Stone", 5, "камень"),
        1 => ("Bronze", 3, "бронзовые слитки"),
        2 or 3 => ("Iron", 3, "железные слитки"),
        4 => ("Glass", 3, "стекло"),
        _ => ("Steel", 3, "стальные листы"),
    };

    public static string Status(CivResearchComponent research)
    {
        var age = research.GetCurrentAge();
        if (research.ResearchLevel >= research.MaxResearch)
            return $"{AgeName(age)} — развитие завершено.";
        if (!research.ResearchEnabled || research.IsTDM)
            return $"{AgeName(age)}. Исследования отключены.";

        var remaining = MathF.Min((age + 1) * 100f, research.MaxResearch) - research.ResearchLevel;
        var eta = research.ResearchSpeed > 0
            ? $"~{Math.Ceiling(remaining / (research.ResearchSpeed * 20f * 60f))} мин."
            : "Авторазвитие приостановлено.";
        return $"{AgeName(age)}: {research.ResearchLevel % 100f:F0}/100 · {eta} · Опыты: {research.ManualResearch:F0}/{research.ManualResearchLimit:F0}";
    }
}
