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

}
