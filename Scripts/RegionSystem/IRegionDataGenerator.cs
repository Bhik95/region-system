namespace Bhik95.RegionSystem
{
    /// <summary>
    /// This interface defines the methods to generate the game-specific data associated with the region.
    /// </summary>
    /// <typeparam name="TData">The type of the data associated with the region</typeparam>
    public interface IRegionDataGenerator<TData>
    {
        /// <summary>
        /// This method defines how to calculate the data associated with a new region 
        /// </summary>
        /// <param name="region"></param>
        /// <returns></returns>
        TData GenerateNew(Region<TData> region);

        /// <summary>
        /// This method defines how to calculate the data associated with a region that is created by the merging of
        /// two other regions, regionA and regionB, starting from the data of the source regions.
        /// </summary>
        /// <param name="regionA">The first region</param>
        /// <param name="regionB">The second region</param>
        /// <returns></returns>
        TData GenerateOnMerge(Region<TData> regionA, Region<TData> regionB);
    }
}
