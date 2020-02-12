using Godot;
using System;

public class Enemy : RigidBody2D
{

    private static readonly int WALK_SPEED = 50;
    private static readonly int STATE_WALKING = 0;
    private static readonly int STATE_DYING = 1;

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
        state = STATE_DYING;

        AngularVelocity = Mathf.Sign(contactLocalNormal.x) * 33.0f;
        Friction = 1.0f;
        bullet.Disable();
        AudioStreamPlayer2D soundHit = GetNode("SoundHit") as AudioStreamPlayer2D;
        soundHit.Play();
    }

}
