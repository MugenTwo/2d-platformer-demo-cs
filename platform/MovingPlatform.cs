using Godot;

public class MovingPlatform : Node2D
{

    [Export]
    private Vector2 motion = new Vector2();
    [Export]
    private float cycle;
    private float accum;

    public MovingPlatform()
    {
        this.cycle = 1.0f;
        this.accum = 0.0f;
    }

    public override void _PhysicsProcess(float delta)
    {
        this.accum += delta * (1.0f / cycle) * Mathf.Pi * 2.0f;
        this.accum = this.accum % (Mathf.Pi * 2.0f);

        float d = Mathf.Sin(this.accum);
        Transform2D xf = new Transform2D();

        xf[2] = motion * d;

        RigidBody2D platform = GetNode("Platform") as RigidBody2D;
        platform.Transform = xf;
    }

}
