using Godot;

public class Coin : Area2D
{

    private bool taken;

    public override void _Ready()
    {
        this.taken = false;
    }
    
    public void OnBodyEnter(RigidBody2D body)
    {
        if (!taken && body is Player)
        {
            this.taken = true;
            AnimationPlayer animationPlayer = GetNode("Anim") as AnimationPlayer;
            animationPlayer.Play("taken");
        }
    }

}
