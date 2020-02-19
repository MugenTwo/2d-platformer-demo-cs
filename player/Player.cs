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

    public override void _IntegrateForces(Physics2DDirectBodyState bodyState)
    {
        Vector2 linearVelocity = bodyState.LinearVelocity;
        float step = bodyState.Step;

        String newAnimation = this.animation;
        bool newSidingLeft = this.sidingLeft;

        PlayerInputInteraction playerInputInteraction = ListenToPlayerInput();

        if (playerInputInteraction.Spawn)
        {
            Spawn();
        }

        linearVelocity.x -= this.floorHVelocity;
        floorHVelocity = 0.0f;

        FloorContact floor = FindFloorContact(bodyState);

    }

    private PlayerInputInteraction ListenToPlayerInput()
    {
        bool moveLeft = Input.IsActionPressed("move_left");
        bool moveRight = Input.IsActionPressed("move_right");
        bool jump = Input.IsActionPressed("jump");
        bool shoot = Input.IsActionPressed("shoot");
        bool spawn = Input.IsActionPressed("spawn");

        return new PlayerInputInteraction(moveLeft, moveRight, jump, shoot, spawn);
    }

    private void Spawn()
    {
        RigidBody2D enemy = this.enemyScene.Instance() as RigidBody2D;
        Vector2 position = Position;

        position.y = position.y - 100;
        enemy.Position = position;

        GetParent().AddChild(enemy);
    }

    private FloorContact FindFloorContact(Physics2DDirectBodyState bodyState)
    {
        FloorContact floorContact = new FloorContact(false, -1);

        for (int i = 0; i < bodyState.GetContactCount(); i++)
        {
            Vector2 contactLocalNormal = bodyState.GetContactLocalNormal(i);

            if (contactLocalNormal.Dot(new Vector2(0, -1)) > 0.6f)
            {
                floorContact.FoundFloor = true;
                floorContact.FloorIndex = i;
            }
        }

        return floorContact;
    }

    private class FloorContact
    {

        public bool FoundFloor { get; set; }
        public int FloorIndex { get; set; }

        public FloorContact(bool foundFloor, int floorIndex)
        {
            this.FoundFloor = false;
            this.FloorIndex = floorIndex;
        }

    }

    private class PlayerInputInteraction
    {

        public bool MoveLeft { get; set; }
        public bool MoveRight { get; set; }
        public bool Jump { get; set; }
        public bool Shoot { get; set; }
        public bool Spawn { get; set; }

        public PlayerInputInteraction(bool moveLeft, bool moveRight, bool jump, bool shoot, bool spawn)
        {
            this.MoveLeft = moveLeft;
            this.MoveRight = moveRight;
            this.Jump = jump;
            this.Shoot = shoot;
            this.Spawn = spawn;
        }

    }

}
