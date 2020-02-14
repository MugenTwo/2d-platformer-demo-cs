using Godot;
using System;

public class Enemy : RigidBody2D
{

    private static readonly int WALK_SPEED = 50;
    private static readonly int STATE_WALKING = 0;
    private static readonly int STATE_DYING = 1;
    private static readonly float ANGULAR_VELOCITY_CONSTANT = 33.0f;
    private static readonly float FRICTION_CONSTANT = 1.0f;

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

    public void BulletCollider(Bullet bullet, Physics2DDirectBodyState bodyState, Vector2 contactLocalNormal)
    {
        Mode = ModeEnum.Rigid;
        this.state = STATE_DYING;

        bodyState.AngularVelocity = Mathf.Sign(contactLocalNormal.x) * ANGULAR_VELOCITY_CONSTANT;
        Friction = FRICTION_CONSTANT;
        bullet.Disable();

        AudioStreamPlayer2D soundHit = GetNode("SoundHit") as AudioStreamPlayer2D;
        soundHit.Play();
    }

    public override void _IntegrateForces(Physics2DDirectBodyState bodyState)
    {
        Vector2 linearVelocity = LinearVelocity;
        String newAnimation = this.animation;

        if (state == STATE_DYING)
        {
            newAnimation = "explode";
        }
        else if (state == STATE_WALKING)
        {
            newAnimation = "walk";
            float wallSide = 0.0f;

            for (int i = 0; i < bodyState.GetContactCount(); i++)
            {
                Godot.Object contactColliderObject = bodyState.GetContactColliderObject(i);
                Vector2 contactLocalNormal = bodyState.GetContactLocalNormal(i);

                if (contactColliderObject != null)
                {
                    if (contactColliderObject is Bullet)
                    {
                        Bullet bullet = contactColliderObject as Bullet;
                        CallDeferred("BulletCollider", contactColliderObject, bodyState, contactLocalNormal);
                        break;
                    }
                }

                if (contactLocalNormal.x > 0.9f)
                {
                    wallSide = 1.0f;
                }
                else
                {
                    wallSide = -1.0f;
                }
            }


            int correctDirection = findCorrectDirection(wallSide);

            if (this.direction != correctDirection)
            {
                this.direction = correctDirection;

                Sprite sprite = GetNode("Sprite") as Sprite;
                Vector2 scale = sprite.Scale;
                scale.x = -this.direction;
                sprite.Scale = scale;
            }

            linearVelocity.x = this.direction * WALK_SPEED;
        }

        bodyState.LinearVelocity = linearVelocity;


        if (animation != newAnimation)
        {
            UpdateAnimation(newAnimation);
        }
    }

    private int findCorrectDirection(float wallSide)
    {
        if (wallSide != 0 && wallSide != direction)
        {
            return -this.direction;
        }

        if (direction < 0 && !rayCastLeft.IsColliding() && rayCastRight.IsColliding())
        {
            return -this.direction;
        }
        else if (direction > 0 && !rayCastRight.IsColliding() && rayCastLeft.IsColliding())
        {
            return -this.direction;
        }

        return this.direction;
    }
    
    private void UpdateAnimation(String newAnimation)
    {
        this.animation = newAnimation;
        AnimationPlayer animationPlayer = GetNode("Anim") as AnimationPlayer;
        animationPlayer.Play(animation);
    }

}
