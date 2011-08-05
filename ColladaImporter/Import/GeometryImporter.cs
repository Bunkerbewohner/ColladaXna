using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Omi.Xna.Collada.Importer.Exceptions;
using Omi.Xna.Collada.Model;
using Omi.Xna.Collada.Model.Geometry;
using Omi.Xna.Collada.Importer.Util;
using Microsoft.Xna.Framework;
using System.Diagnostics;
using Omi.Xna.Collada.Model.Animation;
using Omi.Xna.Collada.Model.Misc;

namespace Omi.Xna.Collada.Importer.Import
{
    /// <summary>
    /// Importer for mesh geometry. Imports meshes from library_geometries.
    /// If a skin exists in the document that references an imported mesh
    /// the needed joint weights and indices are imported as well and saved
    /// into the vertex container of the respective mesh.
    /// </summary>
    public class GeometryImporter : IColladaImporter
    {
        #region IColladaImporter Member

        public void Import(XmlNode xmlRoot, ref IntermediateModel model)
        {
            // Find geometry nodes
            XmlNodeList xmlGeometries = xmlRoot.SelectNodes("library_geometries/geometry");
            if (xmlGeometries == null || xmlGeometries.Count == 0)
            {
                throw new ApplicationException("No geometry found");
            }

            IntermediateModel existingInstance = model;

            model.Meshes = (from XmlNode xmlGeom in xmlGeometries 
                            where xmlGeom.SelectNodes(".//triangles|.//polygons|.//polylist").Count > 0
                            select ImportGeometry(xmlGeom, existingInstance)).ToList();

            // Check for unsupported geometry, silently ignore lines
            if ((from XmlNode xmlGeom in xmlGeometries 
             where xmlGeom.SelectNodes(".//trifangs|.//tristrips").Count > 0
             select xmlGeom).Any())
            {
                throw new ApplicationException("Only triangles are supported");
            }
        }

        #endregion

        /// <summary>
        /// Imports a piece of geometry, but does only support meshes, no NURBS.
        /// </summary>
        /// <param name="xmlGeometryNode">XML node of the geometry</param>
        /// <exception cref="NotFoundException">Only works with meshes</exception>        
        Mesh ImportGeometry(XmlNode xmlGeometryNode, IntermediateModel model)
        {
            // Find the mesh node
            XmlNode xmlMeshNode = xmlGeometryNode.SelectSingleNode(".//mesh");
            if (xmlMeshNode == null)
            {
                throw new NotFoundException("No supported geometry (Mesh) found", xmlGeometryNode);
            }

            // Determine number of mesh parts
            XmlNodeList xmlTriangles = xmlMeshNode.SelectNodes("triangles|polygons|polylist");

            if (xmlTriangles.Count == 0)
            {                
                throw new NotFoundException("No triangles found in mesh. Only triangles are supported",
                    xmlMeshNode);
            }

            if (xmlTriangles[0].Name != "triangles")
            {
                // If there are polygons or a polylist, check that all of them are triangles
                if (xmlTriangles[0].Attributes["vcount"] != null)
                {
                    var vcounts = XmlUtil.ParseInts(xmlTriangles[0].Attributes["vcount"].Value);
                    var nonTriangles = vcounts.Where(count => count != 3);
                    if (nonTriangles.Any())
                    {
                        throw new Exception("Found polygon with " + nonTriangles.First() + 
                            " elements. Only triangles are supported");
                    }
                }
            }

            // Source data for this mesh used by all mesh parts.            
            VertexSources sources = new VertexSources();
            sources.Positions = GetPositions(xmlMeshNode);
            sources.Colors = GetColors(xmlMeshNode);
            sources.Normals = GetNormals(xmlMeshNode);
            sources.Tangents = GetTangents(xmlMeshNode);
            sources.Binormals = GetBinormals(xmlMeshNode);
            sources.TexCoords = GetTextureCoordinates(xmlMeshNode);

            // Skinning Information, if available
            GetJointWeightsAndIndices(xmlMeshNode, model, out sources.JointIndices, 
                out sources.JointWeights);            

            if (sources.Positions.Length == 0)
            {
                throw new Exception("No position data found");
            }

            //-------------------------------------------------------
            // Create Mesh
            //-------------------------------------------------------
            Mesh mesh = new Mesh();
            mesh.Name = XmlUtil.GetName(xmlGeometryNode);
            mesh.MeshParts = new MeshPart[xmlTriangles.Count];

            // A mesh container for every mesh part, since every mesh part may use different
            // vertex types. This can be optimized in the content processor, if needed
            mesh.VertexContainers = new VertexContainer[xmlTriangles.Count];

            //-------------------------------------------------------
            // Create Mesh Parts
            //-------------------------------------------------------
            for (int i = 0; i < xmlTriangles.Count; i++)
            {
                XmlNode xmlPart = xmlTriangles[i];
                int numTriangles = int.Parse(xmlPart.Attributes["count"].Value);
                List<int> indexStream = new List<int>(numTriangles * 3);

                var pNodes = xmlPart.SelectNodes("p");
                if (pNodes.Count > 1)
                {
                    // Indices are scattered among numTriangles <p> tags
                    foreach (XmlNode p in pNodes)
                    {
                        indexStream.AddRange(XmlUtil.ParseInts(p.InnerText));
                    }
                }
                else
                {
                    // Indices are contained in one <p> tag
                    indexStream.AddRange(XmlUtil.ParseInts(pNodes[0].InnerText));
                }                
                
                int positionOffset = GetInputOffset(xmlPart, "VERTEX");
                int colorOffset = GetInputOffset(xmlPart, "COLOR");
                int normalOffset = GetInputOffset(xmlPart, "NORMAL");
                int texCoordOffset = GetInputOffset(xmlPart, "TEXCOORD");
                int tangentOffset = GetInputOffset(xmlPart, "TEXTANGENT");
                int binormalOffset = GetInputOffset(xmlPart, "TEXBINORMAL");

                MeshPart part = new MeshPart();

                try
                {
                    if (xmlPart.Attributes["material"] == null) throw new NotFoundException("no material attribute found", null);
                    part.MaterialName = FindMaterial(xmlGeometryNode, xmlPart.Attributes["material"].Value);
                }
                catch (Exception)
                {
                    // No Material found
                    part.MaterialName = null;
                }

                part.Vertices = mesh.VertexContainers[0];

                VertexInstructions instructions = new VertexInstructions(sources);
                int[] indices = indexStream.ToArray();

                instructions.CopyPositions(indices, positionOffset, numTriangles * 3);
                instructions.CopyColors(indices, colorOffset, numTriangles * 3);
                instructions.CopyNormals(indices, normalOffset, numTriangles * 3);
                instructions.CopyTangents(indices, tangentOffset, numTriangles * 3);
                instructions.CopyBinormals(indices, binormalOffset, numTriangles * 3);
                instructions.CopyTexCoords(indices, texCoordOffset, numTriangles * 3);                
                
                instructions.GenerateJointIndicesAndWeights();

                part.Vertices = VertexContainer.CreateVertexContainer(instructions, out part.Indices, 
                    out mesh.Bounds);
                mesh.VertexContainers[i] = part.Vertices;
                mesh.MeshParts[i] = part;
            }

            return mesh;
        }                

        #region XML Parsing

        /// <summary>
        /// When passed an material symbol (as used by &lt;triangles&gt;'s "material" attribute)
        /// the method finds the referenced material by looking for &lt;bind_material&gt;
        /// and &lt;material_instance&gt;.
        /// </summary>
        /// <param name="materialSymbol"></param>
        /// <returns></returns>
        protected string FindMaterial(XmlNode xmlNode, string materialSymbol)
        {
            XmlNode root = xmlNode.OwnerDocument.DocumentElement;

            XmlNode xmlInstanceMaterial = root.SelectSingleNode(".//instance_material[@symbol='" +
                materialSymbol + "']");

            if (xmlInstanceMaterial == null)
            {
                throw new NotFoundException("No Material Instance '" + materialSymbol + "' found", xmlNode);
            }

            string materialId = xmlInstanceMaterial.Attributes["target"].Value.Substring(1);

            XmlNode xmlMaterial = root.SelectSingleNode(".//material[@id='" + materialId + "']");

            if (xmlMaterial == null)
            {
                throw new NotFoundException("No Material Definition with id '" + materialId + "' found", xmlNode);
            }

            if (xmlMaterial.Attributes["name"] != null)
                return xmlMaterial.Attributes["name"].Value;
            else
                return materialId;
        }

        /// <summary>
        /// Finds an input source for a given semantic within a mesh.
        /// </summary>
        /// <param name="xmlMeshNode">XML mesh node</param>
        /// <param name="semantic">semantic name (e.g. POSITION, NORMAL, TEXCOORD)</param>
        /// <returns>XmlNode of the source or null</returns>
        protected XmlNode FindInputSource(XmlNode xmlMeshNode, string semantic)
        {
            XmlNode input = xmlMeshNode.SelectSingleNode(".//input[@semantic='" + semantic + "']");
            if (input == null)
                throw new NotFoundException("No " + semantic + " Input found", xmlMeshNode);

            string sourceId = input.Attributes["source"].Value.Substring(1);
            XmlNode source = xmlMeshNode.SelectSingleNode("source[@id='" + sourceId + "']");
            if (source == null)
                throw new NotFoundException("No Source found for " + semantic + " Input", input);

            return source;
        }

        /// <summary>
        /// Returns the offset of the first input of the given semantic
        /// below the xml node.
        /// </summary>
        /// <param name="xmlChildNode">An XML node</param>
        /// <param name="semantic">semantic name</param>
        /// <returns>offset >= 0 or -1 if none is found</returns>
        protected int GetInputOffset(XmlNode xmlNode, string semantic)
        {
            XmlNode input = xmlNode.SelectSingleNode(".//input[@semantic='" + semantic + "']");
            if (input == null) return -1;

            return Int32.Parse(input.Attributes["offset"].Value);
        }

        /// <summary>
        /// Returns the stride of the source accessor
        /// </summary>
        /// <param name="xmlSource">XML source node</param>
        /// <returns>stride (usally 2 or 3)</returns>
        protected int GetSourceStride(XmlNode xmlSource)
        {
            XmlNode xmlAccessor = xmlSource.SelectSingleNode(".//accessor");
            if (xmlAccessor == null)
                throw new NotFoundException("No input accessor for Source '" +
                    xmlSource.Attributes["id"].Value + "' found", xmlSource);

            return Int32.Parse(xmlAccessor.Attributes["stride"].Value);
        }

        /// <summary>
        /// This method extacts skinning information from a skin definition that
        /// refers to the given mesh node. If no skin definition is found jointWeights
        /// and jointIndices are assigned empty arrays. If there is a valid skin
        /// definition this method will output four joint indices per base vertex (Vector4)
        /// and three weights (Vector3). If there are more than four joints assigned to a
        /// vertex only the four most influencial are selected. The indices in the resulting
        /// jointIndices array are referring to positions within the given model's joint 
        /// collection. 0 refers to the joint at model.Joints[0], 1 refers to the joint at
        /// model.Joints[1] and so on. If the model contains no joints this method outputs
        /// two empty arrays, just as if no skin defintion existed.       
        /// </summary>
        /// <remarks>The outputted Vector3s for jointWeights represent four normalized weights
        /// in three components. The sum of all four weights is 1. The fourth weight is implicitly
        /// defined as (1 - X - Y -Z).</remarks>
        /// <param name="xmlMeshNode">XML mesh node</param>
        /// <param name="model">Model instance with non-empty joint collection</param>
        /// <param name="jointIndices">Array of 4-d vectors representing joint indices</param>
        /// <param name="jointWeights">Array of 3-d vectors representing their respective weights</param>
        protected static void GetJointWeightsAndIndices(XmlNode xmlMeshNode, IntermediateModel model,
            out Vector4[] jointIndices, out Vector3[] jointWeights)
        {
            // Look for a skin definition that references this mesh
            XmlNode xmlSkin = xmlMeshNode.SelectSingleNode("/COLLADA/library_controllers/" + 
                "controller/skin[@source='#" + xmlMeshNode.ParentNode.Attributes["id"].Value + "']");

            if (xmlSkin == null || model.Joints == null || model.Joints.Count == 0)
            {
                // no skinning information found
                jointIndices = new Vector4[0];
                jointWeights = new Vector3[0];
                return;
            }

            // Read number of vertex weight assignments (this is equivalent to the number of base vertices)
            XmlNode xmlVertexWeights = xmlSkin.SelectSingleNode("vertex_weights");            
            int count = int.Parse(xmlVertexWeights.Attributes["count"].Value);

            // Read weight source
            XmlNode xmlWeightInput = xmlSkin.SelectSingleNode("vertex_weights/input[@semantic='WEIGHT']");
            Source weightSource = Source.FromInput(xmlWeightInput, xmlSkin);
            var weights = weightSource.GetData<float>();           

            // Read assignments
            XmlNode xmlVertexCount = xmlSkin.SelectSingleNode("vertex_weights/vcount");
            XmlNode xmlVertices = xmlSkin.SelectSingleNode("vertex_weights/v");
            int[] vcount = XmlUtil.ParseInts(xmlVertexCount.InnerText);
            ContentAssert.AreEqual(vcount.Length, count, "vcount.Length");

            int[] data = XmlUtil.ParseInts(xmlVertices.InnerText);

            // How many items per vertex (this corresponds to the maximum offset of
            // all inputs within vertex_weights)
            int stride = (from XmlNode node in xmlVertexWeights.SelectNodes("input/@offset")
                          select int.Parse(node.Value)).Max() + 1;
            ContentAssert.IsTrue(stride >= 2, "Invalid weight data");                 

            // It is assumed that joint indices are at offset 0 and their weights at offset 1
            // For each base vertex there is one block of joint-weight assigments
            jointIndices = new Vector4[count];
            jointWeights = new Vector3[count];
            bool reachedEnd = false;

            for (int i = 0, k = 0; i < count; i++)
            {
                // There may be more than 4 weights defined
                List<JointWeightPair> pairs = new List<JointWeightPair>();
                
                // Add all defined joint-weight pairs
                for (int j = 0; j < vcount[i]; j++)
                {
                    int jointIndex = data[k + 0];
                    int weightIndex = data[k + 1];

                    pairs.Add(new JointWeightPair(jointIndex, weights[weightIndex]));
                    k += stride;
                }                

                // Take the four vertices with greatest influence
                JointWeightPair[] best = (from pair in pairs 
                                         orderby pair.Weight descending 
                                         select pair).Take(4).ToArray();

                Vector4 curIndices = new Vector4();
                Vector4 curWeights = new Vector4();

                ContentAssert.IsTrue((vcount[i] <= 4 && best.Length == vcount[i]) ||
                    best.Length == 4, "Invalid weight data", true);

                if (best.Length >= 1)
                {
                    curIndices.X = best[0].JointIndex;
                    curWeights.X = best[0].Weight;
                }

                if (best.Length >= 2)
                {
                    curIndices.Y = best[1].JointIndex;
                    curWeights.Y = best[1].Weight;
                }

                if (best.Length >= 3)
                {
                    curIndices.Z = best[2].JointIndex;
                    curWeights.Z = best[2].Weight;
                }

                if (best.Length == 4)
                {
                    curIndices.W = best[3].JointIndex;
                    curWeights.W = best[3].Weight;
                }

                // Normalize weights (sum must be 1)                
                float sum = curWeights.X + curWeights.Y + curWeights.Z + curWeights.Z;
                if (sum > 0)
                {
                    curWeights.X = 1.0f / sum * curWeights.X;
                    curWeights.Y = 1.0f / sum * curWeights.Y;
                    curWeights.Z = 1.0f / sum * curWeights.Z;
                    curWeights.W = 1.0f / sum * curWeights.W;
                }

                jointIndices[i] = curIndices;
                jointWeights[i] = curWeights.XYZ();

                if (k == data.Length) reachedEnd = true;
            }

            ContentAssert.IsTrue(reachedEnd, "Not all weights were read", true);

            // JointIndices are referring to indices in the joint source
            // so every index refers to a name in the source which refers to the 
            // actual bone            
            XmlNode xmlInput = xmlSkin.SelectSingleNode("vertex_weights/input[@semantic='JOINT']");
            Debug.Assert(xmlInput != null, "No joint input in skin found");

            Source jointSource = Source.FromInput(xmlInput, xmlSkin);
            var names = jointSource.GetData<string>();            

            // Create dictionary of model bones with source reference type as key
            // (source refers to joints either by name, idref or sidref)
            Dictionary<string, Joint> modelJoints = model.Joints.ToDictionary(j =>
                j.GetAddressPart(jointSource.ColladaType));

            // Check if the names actually refer to the joints in the dictionary
            if (modelJoints.ContainsKey(names[0]) == false)
            {
                Debug.Assert(model.Joints.All(j => 
                    j.Name != null || j.GlobalID != null || j.ScopedID != null),
                    "Joints cannot be referenced");

                // As of COLLADA 1.4 "name" can refer to name OR sid attribute!
                // If the former didn't work try the latter);)
                if (jointSource.ColladaType == "name")
                {                   
                    // Only elements that actually have a sid can be part of the dictionary.
                    // Elements which don't have a SID usually aren't referenced, so they are 
                    // not needed in the dictionary anyway
                    modelJoints = model.Joints.Where(j => j.GetAddressPart("sid") != null).
                        ToDictionary(j => j.GetAddressPart("sid"));   
                }

                // If that still didn't help it's hopeless
                if (modelJoints.ContainsKey(names[0]) == false)
                {
                    throw new ApplicationException("Invalid joint references in skin definition");
                }
            }

            try
            {

                // replace index that points to source with index that points to model's joint
                for (int i = 0; i < jointIndices.Length; i++)
                {
                    Vector4 indices = jointIndices[i];

                    // Find indices in model's joint collection
                    indices.X = modelJoints[names[(int) indices.X]].Index;
                    indices.Y = modelJoints[names[(int) indices.Y]].Index;
                    indices.Z = modelJoints[names[(int) indices.Z]].Index;
                    indices.W = modelJoints[names[(int) indices.W]].Index;

                    jointIndices[i] = indices;
                }

            } catch (IndexOutOfRangeException e)
            {
                throw new ApplicationException("Invalid joint indices read");
            }

            // Check data
            bool valid = jointIndices.All(v => Math.Abs((v.X + v.Y + v.Z + (1 - v.X - v.Y - v.Z)) - 1) < 0.001f);
            ContentAssert.IsTrue(valid, "All joint weights must sum up to 1f");            
        }

        /// <summary>
        /// Extracts vertex positions from a COLLADA "mesh" XML node.
        /// 3-dimensional position vectors are assumed.
        /// </summary>
        /// <param name="xmlMeshNode">mesh node within a COLLADA file</param>
        /// <returns>Vertex Array with 3D xmlData</returns>
        protected Vector3[] GetPositions(XmlNode xmlMeshNode)
        {
            XmlNode xmlSource = FindInputSource(xmlMeshNode, "POSITION");
            if (xmlSource == null) throw new NotFoundException("No POSITION Source found",
                xmlMeshNode);

            XmlNode xmlPositions = xmlSource.SelectSingleNode("float_array");
            int numPositions = Int32.Parse(xmlPositions.Attributes["count"].Value);
            int stride = GetSourceStride(xmlSource);
            if (stride != 3)
            {
                throw new FormatNotSupportedException("Unsupported position format (must be 3d)",
                    xmlPositions);
            }

            float[] data = XmlUtil.ParseFloats(xmlPositions.InnerText);
            Vector3[] positions = new Vector3[numPositions / stride];

            for (int i = 0; i < positions.Length; ++i)
            {
                int j = stride * i;
                positions[i] = new Vector3(data[j], data[j + 1], data[j + 2]);
            }

            return positions;
        }

        protected Color[] GetColors(XmlNode xmlMeshNode)
        {
            try
            {
                XmlNode xmlSource = FindInputSource(xmlMeshNode, "COLOR");                
                XmlNode xmlColors = xmlSource.SelectSingleNode("float_array");
                int numColors = Int32.Parse(xmlColors.Attributes["count"].Value);
                int stride = GetSourceStride(xmlSource);
                if (stride != 3 && stride != 4)
                {
                    throw new FormatNotSupportedException("Unsupported color format (must be 3d or 4d)",
                        xmlColors);
                }

                float[] data = XmlUtil.ParseFloats(xmlColors.InnerText);
                Color[] colors = new Color[numColors / stride];

                for (int i = 0; i < colors.Length; ++i)
                {
                    int j = stride * i;
                    if (stride == 4) colors[i] = new Color(data[j], data[j + 1], data[j + 2], data[j + 3]);
                    else colors[i] = new Color(data[j], data[j + 1], data[j + 2]);
                }

                return colors;
            }
            catch (NotFoundException ex)
            {
                // No Color Input or Source found => no colors
                return new Color[0];
            }
        }

        /// <summary>
        /// Extracts vertex normals from a COLLADA "mesh" XML node.
        /// 3-dimensional normal vectors are assumed.
        /// </summary>
        /// <param name="xmlMeshNode">mesh node within a COLLADA file</param>
        /// <returns>Vertex Array with 3D normals. The array is empty if no normals were found</returns>
        protected Vector3[] GetNormals(XmlNode xmlMeshNode)
        {
            try
            {
                XmlNode xmlSource = FindInputSource(xmlMeshNode, "NORMAL");

                XmlNode xmlNormals = xmlSource.SelectSingleNode("float_array");
                int numNormals = Int32.Parse(xmlNormals.Attributes["count"].Value);
                int stride = GetSourceStride(xmlSource);
                if (stride != 3)
                {
                    throw new FormatNotSupportedException("Unsupported normal format (must be 3d)",
                        xmlNormals);
                }

                float[] data = XmlUtil.ParseFloats(xmlNormals.InnerText);
                Vector3[] positions = new Vector3[numNormals / stride];

                for (int i = 0; i < positions.Length; ++i)
                {
                    int j = stride * i;
                    positions[i] = new Vector3(data[j], data[j + 1], data[j + 2]);
                }

                return positions;
            }
            catch (NotFoundException ex)
            {
                // No Normal Input or Source found => no normals
                return new Vector3[0];
            }
        }

        /// <summary>
        /// Extracts texture coordinates from a COLLADA "mesh" XML node.        
        /// </summary>
        /// <remarks>This method considers only UV/XY/ST data and discards W/Z/P.</remarks>
        /// <param name="xmlMeshNode">mesh node within a collada file</param>
        /// <returns>Texture coordinate array. The array is empty if no texture coordinate were found</returns>
        protected Vector2[] GetTextureCoordinates(XmlNode xmlMeshNode)
        {
            try
            {
                XmlNode xmlSource = FindInputSource(xmlMeshNode, "TEXCOORD");
                if (xmlSource == null) return new Vector2[0];

                XmlNode xmlData = xmlSource.SelectSingleNode("float_array");
                int numData = Int32.Parse(xmlData.Attributes["count"].Value);
                int stride = GetSourceStride(xmlSource);
                if (stride < 2)
                {
                    throw new FormatNotSupportedException("Unsupported texture coordinate format. Must be at least 2d.",
                        xmlData);
                }

                float[] data = XmlUtil.ParseFloats(xmlData.InnerText);
                Vector2[] texCoords = new Vector2[numData / stride];

                for (int i = 0; i < texCoords.Length; ++i)
                {
                    int j = stride * i;
                    texCoords[i] = new Vector2(data[j], 1.0f - data[j + 1]);
                }

                return texCoords;
            }
            catch (NotFoundException ex)
            {
                // No Texture coordinates found
                return new Vector2[0];
            }
        }

        /// <summary>
        /// Extracts vertex tangents from a COLLADA "mesh" XML node.
        /// 3-dimensional tangent vectors are assumed.
        /// </summary>
        /// <param name="xmlMeshNode">mesh node within a COLLADA file</param>
        /// <returns>Tangent Array. Array is empty if no tangents were found</returns>
        protected Vector3[] GetTangents(XmlNode xmlMeshNode)
        {
            try
            {
                XmlNode xmlSource = FindInputSource(xmlMeshNode, "TEXTANGENT");
                XmlNode xmlData = xmlSource.SelectSingleNode("float_array");
                int numData = Int32.Parse(xmlData.Attributes["count"].Value);
                int stride = GetSourceStride(xmlSource);

                if (stride != 3)
                {
                    throw new FormatNotSupportedException("Unsupported tangent format (must be 3d)",
                        xmlData);
                }

                float[] data = XmlUtil.ParseFloats(xmlData.InnerText);
                Vector3[] tangents = new Vector3[numData / stride];

                for (int i = 0; i < tangents.Length; ++i)
                {
                    int j = stride * i;
                    tangents[i] = new Vector3(data[j], data[j + 1], data[j + 2]);
                }

                return tangents;
            }
            catch (NotFoundException ex)
            {
                // No Tangents found
                return new Vector3[0];
            }
        }

        /// <summary>
        /// Extracts vertex binormals from a COLLADA "mesh" XML node.
        /// 3-dimensional binormal vectors are assumed.
        /// </summary>
        /// <param name="xmlMeshNode">mesh node within a COLLADA file</param>
        /// <returns>Binormal Array. Array is empty if no binormals were found</returns>
        protected Vector3[] GetBinormals(XmlNode xmlMeshNode)
        {
            try
            {
                XmlNode xmlSource = FindInputSource(xmlMeshNode, "TEXBINORMAL");
                XmlNode xmlData = xmlSource.SelectSingleNode("float_array");
                int numData = Int32.Parse(xmlData.Attributes["count"].Value);
                int stride = GetSourceStride(xmlSource);

                if (stride != 3)
                {
                    throw new FormatNotSupportedException("Unsupported tangent format (must be 3d)",
                        xmlData);
                }

                float[] data = XmlUtil.ParseFloats(xmlData.InnerText);
                Vector3[] binormals = new Vector3[numData / stride];

                for (int i = 0; i < binormals.Length; ++i)
                {
                    int j = stride * i;
                    binormals[i] = new Vector3(data[j], data[j + 1], data[j + 2]);
                }

                return binormals;
            }
            catch (NotFoundException ex)
            {
                // No Binormals found
                return new Vector3[0];
            }
        }

        #endregion

        /// <summary>
        /// Tuple replacement for joint weight assignments
        /// Since there are no tuples yet in C# 3.0 
        /// </summary>
        private struct JointWeightPair
        {
            public int JointIndex;
            public float Weight;

            public JointWeightPair(int jointIndex, float jointWeight)
            {
                JointIndex = jointIndex;
                Weight = jointWeight;
            }      
        }        
    }

    internal static class GeometryExtensions
    {
        /// <summary>
        /// Creates a Vector3 from this Vector4 by dropping W.
        /// </summary>
        /// <param name="v">A four-dimensional vector</param>
        /// <returns>A three-dimensional vector</returns>
        public static Vector3 XYZ(this Vector4 v)
        {
            return new Vector3(v.X, v.Y, v.Z);
        }
    }
}
