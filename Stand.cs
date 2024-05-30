using Godot;
using System;

public partial class Stand : Node3D
{
    private Node3D _parent;
    private Sprite3D _line;
    private Sprite3D _baseMarker;
    private Camera3D _camera;

    [Export] public float LineThicknessScalar = 0.004f;
    [Export] public Color Modulate;

    public override void _Ready()
    {
        // Get the parent node, which should be the new root Spatial node
        _parent = GetParent() as Node3D;
        if (_parent == null)
        {
            GD.PrintErr("Stand must be a child of a Node3D node.");
            QueueFree();
            return;
        }

        // Get the Line and Base nodes
        _line = GetNode<Sprite3D>("Line");
        _baseMarker = GetNode<Sprite3D>("Base");

        if (_line == null || _baseMarker == null)
        {
            GD.PrintErr("Stand must have Line and Base nodes as children.");
            QueueFree();
        }

        _camera = GetViewport().GetCamera3D();
        if (_camera == null)
        {
            GD.PrintErr("Camera3D node not found");
        }
    }

    public override void _Process(double delta)
    {
        if (_parent != null && _line != null && _baseMarker != null)
        {
            // Get parent's global transform
            Transform3D parentTransform = _parent.GlobalTransform;
            Vector3 parentPos = parentTransform.Origin;

            // Calculate the height between the parent and Y=0
            float height = Math.Abs(parentPos.Y);
            if (height < 0.01f)
            {
                height = 0.01f; // Prevent zero height to avoid zero determinant
            }

            // Adjust the scale of the Line
            float distanceToCamera = 100.0f;
            if (_camera != null)
            {
                distanceToCamera = GlobalTransform.Origin.DistanceTo(_camera.GlobalTransform.Origin);
            }
            if (float.IsNaN(distanceToCamera) || float.IsInfinity(distanceToCamera))
            {
                GD.PrintErr("Distance to camera is invalid.");
                return;
            }
            float scaledFactor = distanceToCamera * LineThicknessScalar;
            Vector3 currentScale = _line.Scale;
            _line.Scale = new Vector3(scaledFactor, height, currentScale.Z);
            
            // Adjust the offset to ensure the Line is correctly positioned
            var textureHeight = _line.Texture.GetHeight();
            _line.PixelSize = 1.0f / textureHeight;
            _line.Offset = new Vector2(0, textureHeight / 2.0f * -(float)Math.Sign(parentPos.Y));
            _line.Modulate = Modulate;

            // Position the Base marker at the parent's X and Z, but at Y=0 without changing its rotation
            Vector3 baseMarkerPos = new Vector3(parentPos.X, 0, parentPos.Z);
            Transform3D baseMarkerTransform = new Transform3D(Basis.Identity, baseMarkerPos);
            scaledFactor = Math.Max(scaledFactor, 1.0f);
            _baseMarker.Scale = new Vector3(scaledFactor, 1.0f, scaledFactor);
            _baseMarker.GlobalTransform = baseMarkerTransform;
            _baseMarker.Modulate = Modulate;
        }
        else
        {
            GD.PrintErr("Error: _parent, _line, or _baseMarker is null.");
        }
    }
}
