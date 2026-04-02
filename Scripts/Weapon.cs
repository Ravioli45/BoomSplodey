using Godot;
using System;

[Tool]
public partial class Weapon : AnimatedSprite2D
{
    private static readonly StringName DefaultAnimation = new("default");
    private static readonly StringName ShootAnimation = new("shoot");

    //private Callable ResourceChangedCallable => Callable.From(OnResourceChanged);
    //private WeaponResource _resource;
    [Export]
    public WeaponResource Resource
    {
        get => field;
        set
        {
            field = value;
            OnResourceChanged();
        }
    }

    [Export]
    public Node2D BulletSpawnpoint;

    public override void _Ready()
    {
        base._Ready();
        //GD.Print($"ready: {GetInstanceId()}");

        if (!IsInstanceValid(BulletSpawnpoint))
        //if(BulletSpawnpoint == null)
        {
            BulletSpawnpoint = GetNode<Node2D>("BulletSpawnpoint");
        }
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        // yes I tried other ways, no I couldn't find anything better
        if (Engine.IsEditorHint())
        {
            OnResourceChanged();
            return;
        }
    }

    public override bool _Set(StringName property, Variant value)
    {
        //GD.Print(property);
        if (AnimatedSprite2D.PropertyName.Offset == property && Resource != null)
            Resource.SpriteOffset = value.As<Vector2>();

        return base._Set(property, value);
    }

    public void PlayShootAnimation()
    {
        if (SpriteFrames != null)
        {
            Play(ShootAnimation);
        }
    }

    private void OnAnimationFinished()
    {
        Animation = DefaultAnimation;
    }

    private void IndicatorOnEditorMoved(Vector2 newPosition)
    {
        //GD.Print(newPosition);
        Resource?.BulletSpawnOffset = newPosition;
    }

    private void OnResourceChanged()
    {
        //GD.Print($"resource changed to {Resource}: {GetInstanceId()}");
        if (Resource != null)
        {
            SpriteFrames = Resource.SpriteAnimations;
            Offset = Resource.SpriteOffset;

            if (IsInstanceValid(BulletSpawnpoint))
                BulletSpawnpoint.Position = Resource.BulletSpawnOffset;
        }
        else
        {
            SpriteFrames = null;
            //Animation = new StringName("default");

            Offset = Vector2.Zero;

            if (IsInstanceValid(BulletSpawnpoint))
                BulletSpawnpoint.Position = Vector2.Zero;
        }
        
        //GD.Print($"Weapon resource changed: {GetInstanceId()}, {Resource}");
    }
}
