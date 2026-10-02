namespace Content.Shared.Humanity.Combat;

public static class BattleFactionNames
{
    public static string Get(string faction) => faction switch
    {
        "Germany" => "Германия",
        "Soviet" or "SovietCW" => "СССР",
        "England" => "Англия",
        "France" => "Франция",
        "US" => "США",
        "Blugoslavia" => "Блугославия",
        "Insurgents" => "Повстанцы",
        "UnitedNations" => "ООН",
        "" => "Не определена",
        _ => faction,
    };
}
