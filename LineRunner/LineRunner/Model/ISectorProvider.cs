
namespace LineRunner.Model
{
    public interface ISectorProvider
    {
        int SectorCount { get; }
        Sector this[int index] { get; }
    }
}
