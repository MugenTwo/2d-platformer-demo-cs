using Godot;

public class Bullet : RigidBody2D
{

    private bool disabled;
    private Timer timer;

    public override void _Ready()
    {
        this.disabled = false;
        this.timer = GetNode("Timer") as Timer;
        this.timer.Start();
    }

    public void Disable()
    {
        if (!disabled)
        {
            AnimationPlayer animationPlayer = GetNode("Anim") as AnimationPlayer;
            animationPlayer.Play("shutdown");
            this.disabled = true;
        }
    }

}
