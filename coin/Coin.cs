using Godot;
using System;

public class Coin : Area2D
{

    private Boolean taken;
    
    public Coin()
    {
        this.taken = false;
    }

    public void OnBodyEnter(Area2D area2D)
    {
        if (!taken && area2D is Player)
        {
            this.taken = true;
            AnimationPlayer animationPlayer = GetNode("Anim") as AnimationPlayer;
            animationPlayer.Play("taken");
        }
    }

}
