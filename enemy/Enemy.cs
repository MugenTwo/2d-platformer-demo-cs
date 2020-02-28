using Godot;
using System;

public class Enemy : RigidBody2D
{

    private static readonly int WALK_SPEED = 50;
    private static readonly int STATE_WALKING = 0;
    private static readonly int STATE_DYING = 1;
    private static readonly float ANGULAR_VELOCITY_CONSTANT = 33.0f;
    private static readonly float FRICTION_CONSTANT = 1.0f;
    private static readonly float WALL_SIDE = 1.0f;
    private static readonly float COLLISION_NORMAL_X_COMPONENT = 1.0f;

    private int state;
    private int direction;
    private String animation;
    private RayCast2D rayCastLeft;
    private RayCast2D rayCastRight;
    private Bullet bullet;

    public override void _Ready()
    {
        this.state = STATE_WALKING;
        this.direction = -1;
        this.animation = "";
        this.rayCastLeft = GetNode("RaycastLeft") as RayCast2D;
        this.rayCastRight = GetNode("RaycastRight") as RayCast2D;
    }

    public void Die()
    {
        QueueFree();
    }

    public void PreExplode()
    {
        GetNode("Shape1").QueueFree();
        GetNode("Shape2").QueueFree();
        GetNode("Shape3").QueueFree();

        Mode = ModeEnum.Static;
        AudioStreamPlayer2D soundExplode = GetNode("SoundExplode") as AudioStreamPlayer2D;
        soundExplode.Play();
    }

    public override void _IntegrateForces(Physics2DDirectBodyState bodyState)
    {
        String correctAnimation = FindCorrectAnimation();

        if (correctAnimation.Equals("walk"))
        {
            Walk(bodyState);
        }

        if (this.animation != correctAnimation)
        {
            UpdateAnimation(correctAnimation);
        }

        UpdateLinearVelocity(bodyState);
    }

    private void Walk(Physics2DDirectBodyState bodyState)
    {

        float wallSide = 0.0f;
        for (int i = 0; i < bodyState.GetContactCount(); i++)
        {
            Godot.Object contactColliderObject = bodyState.GetContactColliderObject(i);
            Vector2 contactLocalNormal = bodyState.GetContactLocalNormal(i);

            if (contactColliderObject != null && contactColliderObject is Bullet)
            {
                Bullet contactCollidedBullet = contactColliderObject as Bullet;
                if (!contactCollidedBullet.Disabled)
                {
                    CallDeferred("BulletCollider", contactCollidedBullet, bodyState, contactLocalNormal);
                    break;
                }
            }

            wallSide = FindCorrectWallSide(contactLocalNormal, wallSide);
        }

        int correctDirection = FindCorrectDirection(wallSide);
        if (this.direction != correctDirection)
        {
            this.direction = correctDirection;

            Sprite sprite = GetNode("Sprite") as Sprite;
            Vector2 scale = sprite.Scale;
            scale.x = -this.direction;
            sprite.Scale = scale;
        }
    }

    private void UpdateAnimation(String newAnimation)
    {
        this.animation = newAnimation;
        AnimationPlayer animationPlayer = GetNode("Anim") as AnimationPlayer;
        animationPlayer.Play(animation);
    }

    private void UpdateLinearVelocity(Physics2DDirectBodyState bodyState)
    {
        Vector2 linearVelocity = LinearVelocity;
        linearVelocity.x = this.direction * WALK_SPEED;
        bodyState.LinearVelocity = linearVelocity;
    }

    private String FindCorrectAnimation()
    {
        if (this.state == STATE_DYING)
        {
            return "explode";
        }
        else if (this.state == STATE_WALKING)
        {
            return "walk";
        }

        return this.animation;
    }

    private float FindCorrectWallSide(Vector2 contactLocalNormal, float wallSide)
    {
        // Subtract 0.1f for correct float comparison
        if (contactLocalNormal.x > COLLISION_NORMAL_X_COMPONENT - 0.1f)
        {
            return WALL_SIDE;
        }
        else if (contactLocalNormal.x < -COLLISION_NORMAL_X_COMPONENT + 0.1f)
        {
            return -WALL_SIDE;
        }

        return wallSide;
    }

    private int FindCorrectDirection(float wallSide)
    {
        if (wallSide != 0 && wallSide != this.direction)
        {
            return -this.direction;
        }

        if (this.direction < 0 && !this.rayCastLeft.IsColliding() && this.rayCastRight.IsColliding())
        {
            return -this.direction;
        }
        else if (direction > 0 && !this.rayCastRight.IsColliding() && this.rayCastLeft.IsColliding())
        {
            return -this.direction;
        }

        return this.direction;
    }


    private void BulletCollider(Bullet contactCollidedBullet, Physics2DDirectBodyState bodyState, Vector2 contactLocalNormal)
    {
        Mode = ModeEnum.Rigid;
        this.state = STATE_DYING;

        bodyState.AngularVelocity = Mathf.Sign(contactLocalNormal.x) * ANGULAR_VELOCITY_CONSTANT;
        Friction = FRICTION_CONSTANT;
        contactCollidedBullet.Disable();

        AudioStreamPlayer2D soundHit = GetNode("SoundHit") as AudioStreamPlayer2D;
        soundHit.Play();
    }

}
