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

        PlayerInputInteraction playerInputInteraction = ListenToPlayerInput();

        linearVelocity.x -= this.floorHVelocity;
        floorHVelocity = 0.0f;

        FloorContact floorContact = FindFloorContact(bodyState);

        // TODO: The ProcessSpawn() method was located right after getting player inputs in godot code
        // I think I can move it here without major consequences, but I need to test it
        ProcessSpawn(playerInputInteraction);
        ProcessShooting(playerInputInteraction, step);
        ProcessFloorContact(floorContact, step);
        linearVelocity = ProcessJump(playerInputInteraction, linearVelocity, step);
        linearVelocity = ProcessPlayerMovement(playerInputInteraction, linearVelocity, step);
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

    private void ProcessSpawn(PlayerInputInteraction playerInputInteraction)
    {
        if (playerInputInteraction.Spawn)
        {
            RigidBody2D enemy = this.enemyScene.Instance() as RigidBody2D;
            Vector2 position = Position;

            position.y = position.y - 100;
            enemy.Position = position;

            GetParent().AddChild(enemy);
        }
    }

    private void ProcessShooting(PlayerInputInteraction playerInputInteraction, float step)
    {
        if (playerInputInteraction.Shoot && !this.shooting)
        {
            CallDeferred("ShotBullet");
        }
        else
        {
            this.shootTime += step;
        }
    }

    private void ProcessFloorContact(FloorContact floorContact, float step)
    {
        if (floorContact.FoundFloor)
        {
            this.airborneTime = 0.0f;
        }
        else
        {
            this.airborneTime += step;
        }
    }

    private Vector2 ProcessJump(PlayerInputInteraction playerInputInteraction, Vector2 linearVelocity, float step)
    {
        if (!this.jumping)
        {
            return linearVelocity;
        }

        if (linearVelocity.y > 0)
        {
            this.jumping = false;
        }
        else if (!playerInputInteraction.Jump)
        {
            this.stoppingJump = true;
        }

        if (this.stoppingJump)
        {
            linearVelocity.y += STOP_JUMP_FORCE * step;
        }

        return linearVelocity;
    }

    private Vector2 ProcessPlayerMovement(PlayerInputInteraction playerInputInteraction, Vector2 linearVelocity, float step)
    {

        bool onFloor = airborneTime < MAX_FLOOR_AIRBORNE_TIME;

        if (onFloor)
        {
            linearVelocity = ProcessPlayerDirectionalMovement(playerInputInteraction, linearVelocity, step);
            linearVelocity = ProcessJumpMovement(playerInputInteraction, linearVelocity, step);
            ProcessPlayerSiding(playerInputInteraction, linearVelocity);
        }

        return linearVelocity;
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

    private Vector2 ProcessPlayerDirectionalMovement(PlayerInputInteraction playerInputInteraction, Vector2 linearVelocity, float step)
    {
        if (playerInputInteraction.MoveLeft && !playerInputInteraction.MoveRight)
        {
            if (linearVelocity.x > -WALK_MAX_VELOCITY)
            {
                linearVelocity.x -= WALK_ACCELERATION * step;
            }
        }
        else if (playerInputInteraction.MoveRight && !playerInputInteraction.MoveLeft)
        {
            if (linearVelocity.x < WALK_MAX_VELOCITY)
            {
                linearVelocity.x += WALK_ACCELERATION * step;
            }
        }
        else
        {
            float linearVelocityX = Mathf.Abs(linearVelocity.x);
            linearVelocityX -= WALK_DEACCELERATION * step;
            linearVelocityX = linearVelocityX < 0 ? 0 : linearVelocityX;
            linearVelocity.x = Mathf.Sign(linearVelocity.x) * linearVelocityX;
        }

        return linearVelocity;
    }

    private Vector2 ProcessJumpMovement(PlayerInputInteraction playerInputInteraction, Vector2 linearVelocity, float step)
    {
        if (!this.jumping && playerInputInteraction.Jump)
        {
            linearVelocity.y = -JUMP_VELOCITY;
            this.jumping = true;
            this.stoppingJump = false;
            AudioStreamPlayer2D soundJump = GetNode("SoundJump") as AudioStreamPlayer2D;
            soundJump.Play();
        }

        return linearVelocity;
    }

    private void ProcessPlayerSiding(PlayerInputInteraction playerInputInteraction, Vector2 linearVelocity)
    {

        bool newSidingLeft = this.sidingLeft;
        if (linearVelocity.x < 0 && playerInputInteraction.MoveLeft)
        {
            newSidingLeft = true;
        }
        else if (linearVelocity.x > 0 && playerInputInteraction.MoveRight{
            newSidingLeft = false;
        }


        String newAnimation = this.animation;
        if (this.jumping)
        {
            newAnimation = "jumping";
        }
        else if (Mathf.Abs(linearVelocity.x) < 0.1)
        {
            if (this.shootTime < MAX_SHOOT_POSE_TIME)
            {
                newAnimation = "idle_weapon";
            }
            else
            {
                newAnimation = "idle";
            }
        }
        else
        {
            if (this.shootTime < MAX_SHOOT_POSE_TIME)
            {
                newAnimation = "run_weapon";
            }
            else
            {
                newAnimation = "run";
            }
        }

        // TODO: Do something with newSidingLeft and newAnimation
    }

}
