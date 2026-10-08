namespace Content.Shared.Humanoid.Markings;

public partial record struct Marking
{
    public Marking(Marking marking) : this(marking.MarkingId, marking.MarkingColors) { Forced = marking.Forced; Visible = marking.Visible; }
    public void SetColor(int index, Color color) => _markingColors[index] = color;

    public void SetColor(Color color)
    {
        for (var i = 0; i < _markingColors.Count; i++)
            _markingColors[i] = color;
    }
}
