using Godot;

public class Bullet : RigidBody2D
{

    public bool Disabled{get; set;}
    private Timer timer;

    public override void _Ready()
    {
        this.Disabled = false;
        this.timer = GetNode("Timer") as Timer;
        this.timer.Start();
    }

    public void Disable()
    {
        if (!Disabled)
        {
            AnimationPlayer animationPlayer = GetNode("Anim") as AnimationPlayer;
            animationPlayer.Play("shutdown");
            this.Disabled = true;
        }
    }

}
