
using Microsoft.Xna.Framework;
using Omi.Xna.Collada.Model;

namespace Omi.Xna.Collada.Importer.Processing
{
    public class MeshProcessor : IColladaProcessor
    {
        #region IColladaProcessor Member

        public IntermediateModel Process(IntermediateModel model, ProcessingOptions options)
        {
            model.RootJoint.Transform = Matrix.Identity;

            if (options.ModelScale != 1)
            {
                model.RootJoint.Transform *= Matrix.CreateScale(options.ModelScale);
            }

            if (options.RootJoinRotation.X != 0 || options.RootJoinRotation.Y != 0 || options.RootJoinRotation.Z != 0)
            {
                model.RootJoint.Transform *= Matrix.CreateFromYawPitchRoll(options.RootJoinRotation.Y,
                    options.RootJoinRotation.X, options.RootJoinRotation.Z);
            }

            return model;
        }

        #endregion
    }
}
