namespace NITHlibrary.Nith.Internals
{
    /// <summary>
    /// Defines how the NithModule should handle queue overflow when samples arrive faster than they can be processed.
    /// </summary>
    public enum QueueOverflowBehavior
    {
        /// <summary>
        /// Drop the oldest sample in the queue when full (default).
        /// Best for real-time applications where latest data is most important.
        /// </summary>
        DropOldest,

        /// <summary>
        /// Drop the newest (incoming) sample when queue is full.
        /// Preserves older samples that haven't been processed yet.
        /// </summary>
        DropNewest,

        /// <summary>
        /// Block the receiver thread until space is available in the queue.
        /// May cause lag but guarantees no sample loss.
        /// Use with caution as it can cause the entire pipeline to slow down.
        /// </summary>
        Block
    }
}
