using Godot;

public partial class CameraShakeSlider : HSlider
{
    public override void _Ready()
    {
        base._Ready();

        SyncValue();
    }

    public void SyncValue()
    {
        Value = GameSettings.Instance.GLOBAL_CAMERA_SHAKE_INTENSITY;
    }

    public override void _ValueChanged(double newValue)
    {
        base._ValueChanged(newValue);

        GameSettings.Instance.SaveNewCameraShakePreference(newValue);
    }
}
