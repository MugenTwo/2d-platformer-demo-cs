using Godot;
using System;

public class Player : RigidBody2D
{

    private static readonly float WALK_ACCELERATION = 800.0f;
    private static readonly float WALK_DEACCELERATION = 800.0f;
    private static readonly float WALK_MAX_VELOCITY = 200.0f;
    private static readonly float AIR_ACCELERATION = 200.0f;
    private static readonly float AIR_DEACCELERATION = 200.0f;
    private static readonly float JUMP_VELOCITY = 460.0f;
    private static readonly float STOP_JUMP_FORCE = 900.0f;
    private static readonly float MAX_SHOOT_POSE_TIME = 0.3f;
    private static readonly float MAX_FLOOR_AIRBORNE_TIME = 0.15f;

    private bool sidingLeft;
    private bool jumping;
    private bool stoppingJump;
    private bool shooting;
    private float floorHVelocity;
    private float airborneTime;
    private float shootTime;
    private String animation;
    private PackedScene bulletScene;
    private PackedScene enemyScene;

    public override void _Ready()
    {
        this.animation = "";
        this.sidingLeft = false;
        this.jumping = false;
        this.stoppingJump = false;
        this.shooting = false;
        this.floorHVelocity = 0.0f;
        this.airborneTime = 10 ^ 20;
        this.shootTime = 10 ^ 20;
        this.bulletScene = ResourceLoader.Load("res://player/Bullet.tscn") as PackedScene;
        this.enemyScene = ResourceLoader.Load("res://enemy/Enemy.tscn") as PackedScene;
    }

    public void ShotBullet()
    {
        this.shootTime = 0.0f;
        RigidBody2D bullet = bulletScene.Instance() as RigidBody2D;
        float side = sidingLeft ? -1.0f : 1.0f;
        Position2D bulletShoot = GetNode("BulletShoot") as Position2D;
        Vector2 bulletPosition = Position + bulletShoot.Position * (new Vector2(side, 1.0f));

        bullet.Position = bulletPosition;
        GetParent().AddChild(bullet);

        bullet.LinearVelocity = new Vector2(800.0f * side, -80.0f);

        Particles2D particles = GetNode("Sprite/Smoke") as Particles2D;
        particles.Restart();
        AudioStreamPlayer2D soundShoot = GetNode("SoundShoot") as AudioStreamPlayer2D;
        soundShoot.Play();

        AddCollisionExceptionWith(bullet);
    }

}
