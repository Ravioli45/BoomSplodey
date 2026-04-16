using Godot;
using System;

[GlobalClass]
[Tool]
public partial class WeaponResource : Resource
{
    [ExportSubgroup("Sprite")]
    private SpriteFrames _spriteAnimations = null;
    [Export]
    public SpriteFrames SpriteAnimations
    {
        get => _spriteAnimations;
        set
        {
            //field = value;
            _spriteAnimations = value;
            //EmitSignalChanged();
            EmitChanged();
        }
    }

    private Vector2 _spriteOffset = Vector2.Zero;
    [Export]
    public Vector2 SpriteOffset
    {
        get => _spriteOffset;
        set
        {
            _spriteOffset = value;
            EmitChanged();
        }
    }

    [ExportSubgroup("Stats")]
    private float _fireRate = 0.0f;
    [Export]
    public float FireRate
    {
        get => _fireRate;
        set
        {
            _fireRate = value;
            EmitChanged();
        }
    }
    private float _recoilStrength = 0.0f;
    [Export]
    public float RecoilStrength
    {
        get => _recoilStrength;
        set
        {
            _recoilStrength = value;
            EmitChanged();
        }
    }
    private float _fireStrength = 0.0f;
    [Export]
    public float FireStrength
    {
        get => _fireStrength;
        set
        {
            _fireStrength = value;
            EmitChanged();
        }
    }

    [ExportSubgroup("Bullet")]
    private Vector2 _bulletSpawnOffset = Vector2.Zero;
    [Export]
    public Vector2 BulletSpawnOffset
    {
        get => _bulletSpawnOffset;
        set
        {
            //field = value;
            _bulletSpawnOffset = value;
            //EmitSignalChanged();
            EmitChanged();
        }
    }

    private PackedScene _bulletScene;
    [Export]
    public PackedScene BulletScene
    {
        get => _bulletScene;
        set
        {
            _bulletScene = value;
            EmitChanged();
        }
    }

    [ExportSubgroup("Sound")]
    private AudioStream _shootSound;
    [Export]
    public AudioStream ShootSound
    {
        get => _shootSound;
        set
        {
            _shootSound = value;
            EmitChanged();
        }
    }
    private float _volumeDb;
    [Export]
    public float VolumeDb
    {
        get => _volumeDb;
        set
        {
            _volumeDb = value;
            EmitChanged();
        }
    }

    public WeaponResource() : this(null, Vector2.Zero, 0.0f, 0.0f, 0.0f, Vector2.Zero, null, null, 0) { }

    public WeaponResource(SpriteFrames spriteAnimations, Vector2 spriteOffset, float fireRate, float recoilStrength, float fireStrength, Vector2 bulletSpawnOffset, PackedScene bulletScene, AudioStream shootSound, float volumeDb)
    {
        //SpriteAnimations = spriteAnimations;
        //BulletSpawnOffset = bulletSpawnOffset;
        _spriteAnimations = spriteAnimations;
        _spriteOffset = spriteOffset;

        _fireRate = fireRate;
        _recoilStrength = recoilStrength;
        _fireStrength = fireStrength;

        _bulletSpawnOffset = bulletSpawnOffset;
        _bulletScene = bulletScene;

        _shootSound = shootSound;
        _volumeDb = volumeDb;
    }
}
