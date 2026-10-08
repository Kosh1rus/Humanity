using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;

namespace Content.Client.Humanity.Mortar;

public sealed class MortarDialSpinBox : FloatSpinBox
{
    public event Action<float>? Changed;

    public MortarDialSpinBox() : base(1, 0)
    {
        HorizontalExpand = true;
        IsValid = float.IsFinite;
        OnValueChanged += args => Changed?.Invoke(args.Value);
    }

    protected override void MouseWheel(GUIMouseWheelEventArgs args)
    {
        Value += args.Delta.Y > 0 ? 1 : -1;
        Changed?.Invoke(Value);
        args.Handle();
    }
}
