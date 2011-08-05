using Omi.Xna.Collada.Model;
using Omi.Xna.Collada.Model.Materials;

namespace Omi.Xna.Collada.Importer.Processing.Effects
{
    /// <summary>
    /// Factory class for custom effects fitting certain materials.
    /// The generator must consider context parameters as defined in [X].
    /// </summary>
    public abstract class EffectGenerator
    {
        /// <summary>
        /// Create an effect that suits the given material and model.
        /// </summary>
        /// <param name="material"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public abstract EffectDescription CreateEffect(Material material, IntermediateModel model);
    }
}
