namespace ENode.Snapshoting
{
    /// <summary>An interface represents a class support snapshot.
    /// </summary>
    public interface ISnapshotable<TSnapshot>
    {
        /// <summary>Create a snapshot for the current object.
        /// </summary>
        /// <returns></returns>
        TSnapshot CreateSnapshot();
        /// <summary>Restore the status of the current object from the given snapshot.
        /// </summary>
        /// <param name="snapshot"></param>
        void RestoreFromSnapshot(TSnapshot snapshot);
    }
}
