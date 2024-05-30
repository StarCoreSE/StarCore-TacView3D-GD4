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
        if (@event is InputEventMouseButton buttonEvent1 && buttonEvent1.ButtonIndex == MouseButton.Right && buttonEvent1.Pressed)
        {
            Input.MouseMode = Input.MouseModeEnum.Captured;
        }
        else if (@event is InputEventMouseButton buttonEvent2 && buttonEvent2.ButtonIndex == MouseButton.Right && !buttonEvent2.Pressed)
        {
            Input.MouseMode = Input.MouseModeEnum.Visible;
        }

        isDragging = Input.IsMouseButtonPressed(MouseButton.Right);

        if (@event is InputEventMouseButton buttonEvent)
        {
            float dynamicZoomSpeed = zoomSpeed * (distanceFromTarget / 10.0f);

            switch (buttonEvent.ButtonIndex)
            {
                case MouseButton.Right:
                    isDragging = true;
                    break;
                case MouseButton.WheelUp:
                    if (distanceFromTarget <= minDistanceFromTarget) break;
                    Translate(Vector3.Back * -dynamicZoomSpeed);
                    distanceFromTarget = this.GlobalTransform.Origin.DistanceTo(Pivot.GlobalTransform.Origin);
                    break;
                case MouseButton.WheelDown:
                    if (distanceFromTarget >= maxDistanceFromTarget) break;
                    Translate(Vector3.Back * dynamicZoomSpeed);
                    distanceFromTarget = this.GlobalTransform.Origin.DistanceTo(Pivot.GlobalTransform.Origin);
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
            Camera3D camera = GetViewport().GetCamera3D();
            if (camera == null)
            {
                GD.PrintErr("Error: Unable to get Camera3D from SubViewport.");
                return;
            }

            Vector3 rayOrigin = camera.ProjectRayOrigin(GetViewport().GetMousePosition());
            Vector3 rayNormal = camera.ProjectRayNormal(GetViewport().GetMousePosition());
            PhysicsDirectSpaceState3D spaceState = GetWorld3D().DirectSpaceState;

            var parameters = new PhysicsRayQueryParameters3D
            {
                From = rayOrigin,
                To = rayOrigin + rayNormal * 100000,
                CollisionMask = 1,
                CollideWithBodies = true,
                CollideWithAreas = true
            };

            var result = spaceState.IntersectRay(parameters);

            if (result != null && result.ContainsKey("collider"))
            {
                Node3D collider = (Node3D)result["collider"];
                if (collider == null) return;

                GD.Print("3D object under cursor:", collider);
                TrackedSpatial = collider;
            }
        }
    }

    public override void _Process(double delta)
    {
        RotateCamera();

        if (Input.IsActionJustPressed("ui_cancel"))
        {
            TrackedSpatial = null;
            Pivot.Position = Vector3.Zero;
        }

        if (TrackedSpatial != null)
        {
            Pivot.Position = TrackedSpatial.GlobalPosition;
        }
    }

    private void RotateCamera()
    {
        if (Input.MouseMode == Input.MouseModeEnum.Captured)
        {
            rotationOffset.X += -_mouseDelta.Y * sensitivity;
            rotationOffset.Y += -_mouseDelta.X * sensitivity;
            _mouseDelta = Vector2.Zero;
        }

        Pivot.RotationDegrees = Vector3.Zero;
        Pivot.RotateX(rotationOffset.X);
        Pivot.RotateY(rotationOffset.Y);
    }
}
