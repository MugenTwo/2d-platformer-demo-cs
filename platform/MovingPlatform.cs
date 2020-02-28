using Godot;

public class MovingPlatform : Node2D
{


    private static readonly float MOMENTUM_CONSTANT_1 = 1.0f;
    private static readonly float MOMENTUM_CONSTANT_2 = 2.0f;
    private static readonly float INITIAL_ACCUMULATED_MOMENTUM = 0.0f;

    [Export]
    private Vector2 motion = new Vector2();
    [Export]
    private float cycle = 1.0f;
    private float accumulatedMomentum;

    public override void _Ready()
    {
        this.accumulatedMomentum = INITIAL_ACCUMULATED_MOMENTUM;
    }

    public override void _PhysicsProcess(float delta)
    {
        this.accumulatedMomentum += delta * (MOMENTUM_CONSTANT_1 / this.cycle) * Mathf.Pi * MOMENTUM_CONSTANT_2;
        this.accumulatedMomentum = this.accumulatedMomentum % (Mathf.Pi * MOMENTUM_CONSTANT_2);

        float distance = Mathf.Sin(this.accumulatedMomentum);
        Transform2D translation = new Transform2D();

        translation.y = motion * distance;

        RigidBody2D platform = GetNode("Platform") as RigidBody2D;
        platform.Transform = translation;
    }

}
