public class PlayerInputInteraction
{

    public bool MoveLeft { get; set; }
    public bool MoveRight { get; set; }
    public bool Jump { get; set; }
    public bool Shoot { get; set; }
    public bool Spawn { get; set; }

    public PlayerInputInteraction(bool moveLeft, bool moveRight, bool jump, bool shoot, bool spawn)
    {
        this.MoveLeft = moveLeft;
        this.MoveRight = moveRight;
        this.Jump = jump;
        this.Shoot = shoot;
        this.Spawn = spawn;
    }

}