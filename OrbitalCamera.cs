using System.Linq;
using Godot;

public partial class OrbitalCamera : Camera3D
{
    [Export]
    public float sensitivity = 0.01f;
    [Export]
    public float distanceFromTarget = 5.0f;
    [Export] public float maxDistanceFromTarget = 10000.0f;
    [Export] public float minDistanceFromTarget = 5.0f;
    private Vector2 _mouseDelta;
    private Vector2 rotationOffset = Vector2.Zero;
    private bool isDragging;
    public Node3D Pivot;
    public Node3D TrackedSpatial;

    public float zoomSpeed => 2.0f;

    public override void _Ready()
    {
        Pivot = GetParent() as Node3D;
        if (Pivot == null)
        {
            GD.PrintErr("Error: Pivot is not a Node3D node.");
            return;
        }
        Translate(Vector3.Back * distanceFromTarget);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton buttonEvent1 && buttonEvent1.ButtonIndex == (int)ButtonList.Right && buttonEvent1.Pressed)
        {
            Input.MouseMode = Input.MouseModeEnum.Captured;
        }
        else if (@event is InputEventMouseButton buttonEvent2 && buttonEvent2.ButtonIndex == (int)ButtonList.Right && !buttonEvent2.Pressed)
        {
            Input.MouseMode = Input.MouseModeEnum.Visible;
        }

        isDragging = Input.IsMouseButtonPressed((int)ButtonList.Right);

        if (@event is InputEventMouseButton buttonEvent)
        {
            float dynamicZoomSpeed = zoomSpeed * (distanceFromTarget / 10.0f);

            switch ((ButtonList)buttonEvent.ButtonIndex)
            {
                case ButtonList.Right:
                    isDragging = true;
                    break;
                case ButtonList.WheelUp:
                    if (distanceFromTarget <= minDistanceFromTarget) break;
                    Translate(Vector3.Back * -dynamicZoomSpeed);
                    distanceFromTarget = this.GlobalTransform.origin.DistanceTo(Pivot.GlobalTransform.origin);
                    break;
                case ButtonList.WheelDown:
                    if (distanceFromTarget >= maxDistanceFromTarget) break;
                    Translate(Vector3.Back * dynamicZoomSpeed);
                    distanceFromTarget = this.GlobalTransform.origin.DistanceTo(Pivot.GlobalTransform.origin);
                    break;
                default:
                    break;
            }
        }
        else if (@event is InputEventMouseMotion motion)
        {
            if (isDragging)
            {
                _mouseDelta = motion.Relative;
            }
        }

        if (Input.IsActionJustPressed("ui_click_left"))
        {
            Camera3D camera = GetViewport().GetCamera3d() as Camera3D;
            if (camera == null)
            {
                GD.PrintErr("Error: Unable to get Camera3D from SubViewport.");
                return;
            }

            Vector3 rayOrigin = camera.ProjectRayOrigin(GetViewport().GetMousePosition());
            Vector3 rayNormal = camera.ProjectRayNormal(GetViewport().GetMousePosition());
            PhysicsDirectSpaceState3D spaceState = GetWorld3d().DirectSpaceState;
            var result = spaceState.IntersectRay(rayOrigin, rayOrigin + rayNormal * 100000, collideWithAreas: true);

            if (result != null)
            {
                if (!result.Contains<>("collider")) return;
                Node3D collider = (Node3D)result["collider"];
                if (collider == null) return;

                GD.Print("3D object under cursor:", collider);
                TrackedSpatial = collider;
            }
        }
    }

    public override void _Process(float delta)
    {
        RotateCamera();

        if (Input.IsActionJustPressed("ui_cancel"))
        {
            TrackedSpatial = null;
            Pivot.Position = Vector3.Zero;
        }

        if (TrackedSpatial != null)
        {
            Pivot.Position = TrackedSpatial.GlobalTranslation;
        }
    }

    private void RotateCamera()
    {
        if (Input.MouseMode == Input.MouseModeEnum.Captured)
        {
            rotationOffset.x += -_mouseDelta.y * sensitivity;
            rotationOffset.y += -_mouseDelta.x * sensitivity;
            _mouseDelta = Vector2.Zero;
        }

        Pivot.RotationDegrees = Vector3.Zero;
        Pivot.RotateX(rotationOffset.x);
        Pivot.RotateY(rotationOffset.y);
    }
}
