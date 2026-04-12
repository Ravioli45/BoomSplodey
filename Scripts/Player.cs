using Godot;
using System;

public partial class Player : CharacterBody2D
{
    [Signal]
    public delegate void DamagedByEventHandler(long playerId, int damage);
    [Signal]
    public delegate void KilledByEventHandler(long playerId);

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
    public int HatIndex
    {
        get => field;
        set
        {
            field = value;

            if (IsNodeReady())
            {
                HatSprite.Frame = field;
            }
        }
    } = 0;
    [Export]
    private Sprite2D HatSprite;
    [Export]
    private AnimatedSprite2D PlayerSprite;
    [Export]
    private int maxHP = 6;
    [Export]
    private int currentHP
    {
        get => field;
        set
        {
            field = Mathf.Clamp(value, 0, 6);

            if (IsNodeReady())
            {
                HeartDisplay.UpdateHearts(field);
            }
        }
    } = 6;
    [Export]
    private bool CanShoot = true;
    [Export]
    private Timer FireRateTimer;
    [Export]
    private HealthBar HeartDisplay;
    [Export]
    public string PlayerName
    {
        get => field;
        set
        {
            field = value;

            if (IsNodeReady())
            {
                PlayerNameLabel.Text = field;
            }
        }
    } = "";
    [Export]
    private Label PlayerNameLabel;

    private Vector2 nextRecoil = new();

    public override void _Ready()
    {
        base._Ready();

        WeaponIndex = WeaponIndex;
        currentHP = currentHP;
        HatIndex = HatIndex;
        PlayerName = PlayerName;
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

        if (InputSync.Shooting && CanShoot && PlayerWeapon.Resource != null)
        {
            Vector2 recoil = new Vector2(Mathf.Cos(PlayerWeapon.Rotation), Mathf.Sin(PlayerWeapon.Rotation)) * PlayerWeapon.Resource.RecoilStrength * -1;
            TakeKnockback(recoil);

            // TODO: replace with actually shooting something
            PlayerWeapon.PlayShootAnimation();

            Shoot();

            CanShoot = false;
            FireRateTimer.Start(1/PlayerWeapon.Resource.FireRate);
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
            HatSprite.FlipH = false;
        }
        else if (InputSync.InputMove < 0)
        {
            PlayerSprite.FlipH = true;
            HatSprite.FlipH = true;
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

    public void TakeDamage(long damagedBy, int damage)
    {
        if (!Multiplayer.IsServer())
            return;

        EmitSignalDamagedBy(damagedBy, damage);
        currentHP = Mathf.Max(currentHP - damage, 0);
        GD.Print($"HP: {currentHP}/{maxHP}");
        if (currentHP == 0)
        {
            // Call the death/respawn method here
            EmitSignalKilledBy(damagedBy);
        }
    }

    public void Shoot()
    {

        if (!Multiplayer.IsServer()) return;


        Bullet newBullet = PlayerWeapon.Resource.BulletScene.Instantiate<Bullet>();
        newBullet.OwnerId = OwnerId;
        newBullet.Rotation = PlayerWeapon.Rotation;
        newBullet.GlobalPosition = PlayerWeapon.BulletSpawnpoint.GlobalPosition;
        newBullet.Velocity = PlayerWeapon.Resource.FireStrength * newBullet.GlobalTransform.X;

        // only the server has this
        // but bullet collision is disabled for clients
        newBullet.AddCollisionExceptionWith(this);

        GetParent().AddChild(newBullet, true);

        // for now I will leave the shotgun like this
        if (WeaponIndex == 0)
        {
            for (int i = -1; i <= 1; i += 2)
            {
                Bullet moreBullet = PlayerWeapon.Resource.BulletScene.Instantiate<Bullet>();
                moreBullet.OwnerId = OwnerId;
                moreBullet.Rotation = PlayerWeapon.Rotation + 0.1f * i;
                moreBullet.GlobalPosition = PlayerWeapon.BulletSpawnpoint.GlobalPosition;
                moreBullet.Velocity = PlayerWeapon.Resource.FireStrength * moreBullet.GlobalTransform.X;

                moreBullet.AddCollisionExceptionWith(this);
                GetParent().AddChild(moreBullet, true);
            }
        }
    }

    private void OnFireRateTimeout()
    {
        if (Multiplayer.IsServer())
        {
            CanShoot = true;
        }
    }
}
