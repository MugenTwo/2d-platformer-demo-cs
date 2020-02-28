public class FloorContact
{

    public bool FoundFloor { get; set; }
    public int FloorIndex { get; set; }

    public FloorContact(bool foundFloor, int floorIndex)
    {
        this.FoundFloor = false;
        this.FloorIndex = floorIndex;
    }

}
