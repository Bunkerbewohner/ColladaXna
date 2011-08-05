using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Omi.Xna.Collada.Model;

namespace Omi.Xna.Collada.Model
{
    /// <summary>
    /// Common interface for COLLADA based models
    /// </summary>
    public interface IModel
    {
        /// <summary>
        /// Gets the data of the underlying model
        /// </summary>
        ModelData Data { get; }

        /// <summary>
        /// Gets or sets the EnableCulling option.
        /// If set to false no culling may be performed.
        /// </summary>
        bool EnableCulling { get; set; }

        /// <summary>
        /// Draws the model 
        /// </summary>
        /// <param name="world">Object to World matrix</param>
        /// <param name="view">World to View matrix</param>
        /// <param name="projection">View to Projection matrix</param>
        /// <param name="cameraPosition">Camera position</param>
        void Draw(Matrix world, Matrix view, Matrix projection, Vector3 cameraPosition);
    }

    /// <summary>
    /// Extensions methods for models
    /// </summary>
    public static class ModelExtensions
    {
        /// <summary>
        /// Draw the model and calculate the camera position from the inverted
        /// view matrix. If the camera position is known the Draw() method with
        /// explicit cameraPosition should be used for performance reasons.
        /// </summary>
        /// <param name="model">Model</param>
        /// <param name="world">Object to World matrix</param>
        /// <param name="view">World to View matrix</param>
        /// <param name="projection">View to Projection matrix</param>
        public static void Draw(this IModel model, Matrix world, Matrix view, Matrix projection)
        {
            // Calculate camera position from inverse view matrix
            Matrix viewIT = Matrix.Invert(Matrix.Transpose(view));
            Vector3 camPos = new Vector3(viewIT.M14, viewIT.M24, viewIT.M34);
            model.Draw(world, view, projection, camPos);
        }
    }
}
