using Godot;
using System;

public partial class Player : CharacterBody2D
{
    private static readonly StringName DefaultAnimation = new("default");
    private static readonly StringName WalkAnimation = new("walk");
    private static readonly StringName JumpAnimation = new("jump");

    [ExportSubgroup("Multiplayer")]
    [Export]
    public long OwnerId
    {
        get;
        set
        {
            field = value;
            InputSync.SetMultiplayerAuthority((int)field);
        }
    } = 1;

    [Export]
    private InputSynchronizer InputSync;

    [ExportSubgroup("Movement")]
    [Export]
    private float MoveSpeed;
    [Export]
    private float GroundMoveAcceleration;
    [Export]
    private float GroundIdleAcceleration;
    [Export]
    private float AirMoveAcceleration;
    [Export]
    private float AirIdleAcceleration;
    [Export]
    private float GravityScale = 1.0f;
    [Export]
    private float TerminalVelocity = 1.0f;
    [Export]
    private float JumpStrength;

    [ExportGroup("")]

    [Export(PropertyHint.Enum, "None:-1,Shotgun:0,RPG:1,Potato Launcher:2")]
    public int WeaponIndex
    {
        get => field;
        set
        {
            if (!IsInstanceValid(GlobalResources.Instance))
            {
                field = value;
                return;
            }

            if (value < 0 || value >= GlobalResources.Instance.Weapons.Count)
            {
                field = -1;
            }
            else
            {
                field = value;
            }

            if (field == -1)
            {
                PlayerWeapon.Resource = null;
            }
            else
            {
                PlayerWeapon.Resource = GlobalResources.Instance.Weapons[field];
            }
        }
    } = -1;
    [Export]
    private Weapon PlayerWeapon;
    [Export]
    private AnimatedSprite2D PlayerSprite;

    private Vector2 nextRecoil = new();

    public override void _Ready()
    {
        base._Ready();

        WeaponIndex = WeaponIndex;
        
        // temporary
        //SetPhysicsProcess(Multiplayer.IsServer());
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        Vector2 currentVelocity = Velocity;

        PlayerWeapon.Rotation = Mathf.Atan2(InputSync.MousePosition.Y - PlayerWeapon.GlobalPosition.Y, InputSync.MousePosition.X - PlayerWeapon.GlobalPosition.X);
        //PlayerWeapon.FlipV = (-Mathf.Pi / 2) <= PlayerWeapon.Rotation && PlayerWeapon.Rotation < (Mathf.Pi/2);
        PlayerWeapon.Scale = new Vector2(1, (-Mathf.Pi / 2) <= PlayerWeapon.Rotation && PlayerWeapon.Rotation < (Mathf.Pi / 2) ? 1 : -1);

        if (InputSync.Shooting && PlayerWeapon.Resource != null)
        {
            Vector2 recoil = new Vector2(Mathf.Cos(PlayerWeapon.Rotation), Mathf.Sin(PlayerWeapon.Rotation)) * PlayerWeapon.Resource.RecoilStrength * -1;
            TakeKnockback(recoil);

            // TODO: replace with actually shooting something
            PlayerWeapon.PlayShootAnimation();

            ShootProjectile();
                

        }
        InputSync.Shooting = false;

        if (nextRecoil != Vector2.Zero)
        {
            currentVelocity.X += nextRecoil.X;
            currentVelocity.Y = nextRecoil.Y;
            nextRecoil = Vector2.Zero;
        }

        if (!IsOnFloor())
            {
                currentVelocity += GetGravity() * GravityScale * (float)delta;

                if (currentVelocity.Y > TerminalVelocity)
                {
                    currentVelocity.Y = TerminalVelocity;
                }
            }
            else if (InputSync.Jumping)
            {
                currentVelocity.Y = -JumpStrength;
            }
        InputSync.Jumping = false;

        float targetXVelocity = InputSync.InputMove * MoveSpeed;
        float targetVelDiff = targetXVelocity - currentVelocity.X;

        // Math.f.IsZeroApprox has too small of a tolerance, so I'm using a custom threshold instead
        if (Mathf.Abs(targetVelDiff) <= 0.0001) targetVelDiff = 0;

        // targetVelDiff and targetAcceleration direction will have the same sign...
        float targetAccelerationDirection = Mathf.Sign(targetVelDiff);

        float acceleration = (IsOnFloor(), InputSync.InputMove == 0) switch
        {
            (true, false) => GroundMoveAcceleration,
            (true, true) => GroundIdleAcceleration,
            (false, false) => AirMoveAcceleration,
            (false, true) => AirIdleAcceleration,
        };

        // ...and desiredVelChange gets its sign from targetAcceleration direction
        float desiredVelChange = targetAccelerationDirection * acceleration * (float)delta;

        currentVelocity.X += Mathf.Abs(desiredVelChange) < Mathf.Abs(targetVelDiff) ? desiredVelChange : targetVelDiff;
        if (Mathf.Abs(currentVelocity.X) <= 0.0001) currentVelocity.X = 0;

        if (InputSync.InputMove > 0)
        {
            PlayerSprite.FlipH = false;
        }
        else if (InputSync.InputMove < 0)
        {
            PlayerSprite.FlipH = true;
        }

        if (!IsOnFloor())
        {
            PlayerSprite.Play(JumpAnimation);
        }
        else if (InputSync.InputMove == 0)
        {
            PlayerSprite.Play(DefaultAnimation);
        }
        else
        {
            PlayerSprite.Play(WalkAnimation);
        }

        Velocity = currentVelocity;

        MoveAndSlide();
    }

    public void TakeKnockback(Vector2 recoil)
    {
        nextRecoil.X += recoil.X;
        nextRecoil.Y = recoil.Y;
    }

    public void ShootProjectile()
    {
        
        if(!Multiplayer.IsServer())return;
                if(WeaponIndex == 0 || WeaponIndex == 1)
                {
                    var projectile =  PlayerWeapon.Resource.BulletScene.Instantiate() as Projectile;
                    projectile.GetNode<LinearMovement>("LinearMovement").Direction = PlayerWeapon.BulletSpawnpoint.GlobalTransform.X;

                    if(WeaponIndex == 0 ){
                            GD.Print("SHOTGUN SPREAADS");
                            //We need to insert Shotgun spread logic here.
                    }

                        //Spawn and Set
                        GetParent().AddChild(projectile, true);
                        projectile.Rotation = PlayerWeapon.Rotation;
                        projectile.GlobalPosition  =  PlayerWeapon.BulletSpawnpoint.GlobalPosition;
                }
                else if(WeaponIndex == 2)
                {
                    var projectile =  PlayerWeapon.Resource.BulletScene.Instantiate<AbstractPotato>();
                    projectile.Direction = PlayerWeapon.BulletSpawnpoint.GlobalTransform.X;
                    projectile.GlobalPosition  =  PlayerWeapon.BulletSpawnpoint.GlobalPosition;
                    GetParent().AddChild(projectile, true);
                
                }                
    }
}
