
using Omi.Xna.Collada.Model;

namespace Omi.Xna.Collada.Importer.Processing
{
    /// <summary>
    /// Common interface for COLLADA processors
    /// </summary>
    public interface IColladaProcessor
    {
        /// <summary>
        /// Process the given model and return the processed version.
        /// May return a different or the same instance.
        /// </summary>
        /// <param name="model">model instance</param>        
        /// <param name="options">options for processors</param>
        /// <returns>Processed model</returns>
        IntermediateModel Process(IntermediateModel model, ProcessingOptions options);
    }
}
