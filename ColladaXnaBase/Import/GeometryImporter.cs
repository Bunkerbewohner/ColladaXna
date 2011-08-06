using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Microsoft.Xna.Framework;
using System.Diagnostics;
using ColladaXna.Base.Geometry;
using ColladaXna.Base.Util;
using Microsoft.Xna.Framework.Graphics;

namespace ColladaXna.Base.Import
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

        public void Import(XmlNode xmlRoot, ColladaModel model)
        {
            // Find geometry nodes
            XmlNodeList xmlGeometries = xmlRoot.SelectNodes("library_geometries/geometry");
            if (xmlGeometries == null || xmlGeometries.Count == 0)
            {
                throw new ApplicationException("No geometry found");
            }

            ColladaModel existingInstance = model;

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
        /// <exception cref="Exception">Only works with meshes</exception>        
        Mesh ImportGeometry(XmlNode xmlGeometryNode, ColladaModel model)
        {
            // Find the mesh node
            XmlNode xmlMeshNode = xmlGeometryNode.SelectSingleNode(".//mesh");
            if (xmlMeshNode == null)
            {
                throw new Exception("No supported geometry (Mesh) found");
            }

            // Determine number of mesh parts
            XmlNodeList xmlTriangles = xmlMeshNode.SelectNodes("triangles|polygons|polylist");

            if (xmlTriangles.Count == 0)
            {
                throw new Exception("No triangles found in mesh. Only triangles are supported");
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
            List<VertexSource> sources = ReadSources(xmlMeshNode);

            Vector4[] jointIndices;
            Vector3[] jointWeights;

            // Skinning Information, if available
            GetJointWeightsAndIndices(xmlMeshNode, model, out jointIndices, out jointWeights);            

            if (sources.Count == 0)
            {
                throw new Exception("No data found");
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

            string[] semantics = new string[] { "VERTEX", "COLOR", "NORMAL", "TEXCOORD", "TEXTANGENT", "TEXBINORMAL" };

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
                                
                int[] indices = indexStream.ToArray();
                MeshPart part = new MeshPart();                

                try
                {
                    if (xmlPart.Attributes["material"] == null) throw new Exception("no material attribute found");
                    part.MaterialName = FindMaterial(xmlGeometryNode, xmlPart.Attributes["material"].Value);
                }
                catch (Exception)
                {
                    // No Material found
                    part.MaterialName = null;
                }

                // Read Vertex Channels
                List<VertexChannel> vertexChannels = new List<VertexChannel>();

                foreach (String semantic in semantics)
                {
                    XmlNode input = xmlPart.SelectSingleNode(".//input[@semantic='" + semantic + "']");
                    int offset;
                    String sourceId;

                    if (!input.TryGetAttribute("source", out sourceId))
                        throw new Exception("Referenced source of input with '" + semantic + "' semantic not found");
                    if (!input.TryGetAttribute("offset", out offset))
                        throw new Exception("No offset attribute of input with '" + semantic + "' semantic found");

                    sourceId = sourceId.Replace("#", "");    
                    VertexSource source = sources.Where(s => s.GlobalID.Equals(sourceId)).FirstOrDefault();
                    if (source == null) throw new Exception("Source '" + sourceId + "' not found");

                    VertexElement desc = new VertexElement();
                    desc.Offset = offset;
                    desc.UsageIndex = 0;
                    desc.VertexElementFormat = GetVertexElementFormat(source, semantic);         
                    desc.VertexElementUsage = GetVertexElementUsage(semantic);

                    VertexChannel channel = new VertexChannel(source, desc);
                    channel.CopyIndices(indices, offset, numTriangles * 3);
                    vertexChannels.Add(channel);
                }       
                
                var jointChannels = GenerateJointChannels(jointIndices, jointWeights, 
                    vertexChannels.Where(c => c.Description.VertexElementUsage == 
                        VertexElementUsage.Position).First().Indices);

                if (jointChannels != null)
                {
                    vertexChannels.Add(jointChannels.Item1);
                    vertexChannels.Add(jointChannels.Item2);
                }

                part.Vertices = new VertexContainer(vertexChannels);
                part.Indices = part.Vertices.Indices;
                
                mesh.VertexContainers[i] = part.Vertices;
                mesh.MeshParts[i] = part;
            }

            return mesh;
        }

        Tuple<VertexChannel,VertexChannel> GenerateJointChannels(Vector4[] jointIndices, Vector3[] jointWeights, int[] positionIndices)
        {
            if (jointIndices.Length == 0)            
                return null;

            VertexSource indexSource = new VertexSource();
            VertexSource weightSource = new VertexSource();

            indexSource.Stride = 4;
            indexSource.Data = new float[jointIndices.Length * 4];

            weightSource.Stride = 3;
            weightSource.Data = new float[jointWeights.Length * 3];

            for (int i = 0; i < jointIndices.Length; i++)
            {
                indexSource.Data[i * 4 + 0] = jointIndices[i].X;
                indexSource.Data[i * 4 + 1] = jointIndices[i].Y;
                indexSource.Data[i * 4 + 2] = jointIndices[i].Z;
                indexSource.Data[i * 4 + 3] = jointIndices[i].W;
            }

            for (int i = 0; i < jointWeights.Length; i++)
            {
                weightSource.Data[i * 3 + 0] = jointWeights[i].X;
                weightSource.Data[i * 3 + 1] = jointWeights[i].Y;
                weightSource.Data[i * 3 + 2] = jointWeights[i].Z;                
            }

            VertexElement indexDesc = new VertexElement();
            indexDesc.Offset = 0;            
            indexDesc.UsageIndex = 0;
            indexDesc.VertexElementFormat = VertexElementFormat.Vector4;
            indexDesc.VertexElementUsage = VertexElementUsage.BlendIndices;

            VertexElement weightDesc = new VertexElement();
            weightDesc.Offset = 0;
            weightDesc.UsageIndex = 0;
            weightDesc.VertexElementFormat = VertexElementFormat.Vector3;
            weightDesc.VertexElementUsage = VertexElementUsage.BlendWeight;

            VertexChannel indexChannel = new VertexChannel(indexSource, indexDesc);                       
            VertexChannel weightChannel = new VertexChannel(weightSource, weightDesc);            

            indexChannel.Indices = new int[positionIndices.Length];
            weightChannel.Indices = new int[positionIndices.Length];

            Array.Copy(positionIndices, indexChannel.Indices, positionIndices.Length);
            Array.Copy(positionIndices, weightChannel.Indices, positionIndices.Length);

            return new Tuple<VertexChannel, VertexChannel>(indexChannel, weightChannel);
        }

        VertexElementUsage GetVertexElementUsage(String semantic)
        {
            switch (semantic)
            {
                case "VERTEX":
                    return VertexElementUsage.Position;
                case "COLOR":
                    return VertexElementUsage.Color;
                case "NORMAL":
                    return VertexElementUsage.Normal;
                case "TEXCOORD":
                    return VertexElementUsage.TextureCoordinate;
                case "TEXTANGENT":
                    return VertexElementUsage.Tangent;
                case "TEXBINORMAL":
                    return VertexElementUsage.Binormal;

                default:                    
                    throw new Exception("Unsupported vertex element usage");
            }
        }

        VertexElementFormat GetVertexElementFormat(VertexSource source, String semantic)
        {
            switch (semantic)
            {
                case "VERTEX":
                case "NORMAL":
                case "TEXCOORD":
                case "TEXTANGENT":
                case "TEXBINORMAL":
                    if (source.Stride == 2) return VertexElementFormat.Vector2;
                    else if (source.Stride == 3) return VertexElementFormat.Vector3;
                    else return VertexElementFormat.Vector4;
                case "COLOR":
                    return VertexElementFormat.Color;                

                default:
                    throw new Exception("Unknown vertex element format");
            }
        }

        #region XML Parsing

        /// <summary>
        /// Parses all the source elements of the current mesh and returns
        /// the list of all found vertex sources.
        /// </summary>
        /// <param name="xmlMeshNode">XML Mesh node</param>
        /// <returns>List of found vertex sources</returns>
        protected List<VertexSource> ReadSources(XmlNode xmlMeshNode)
        {
            List<VertexSource> vertexSources = new List<VertexSource>();

            XmlNodeList nodes = xmlMeshNode.SelectNodes("source");
            if (nodes.Count == 0) return vertexSources;

            foreach (XmlNode node in nodes)
            {
                Source source = new Source(node, true);
                VertexSource v = new VertexSource();
                v.Stride = source.Stride;
                v.Data = (float[])source.Data;

                node.TryGetAttribute("id", out v.GlobalID);
                vertexSources.Add(v);
            }

            return vertexSources;
        }

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
                throw new Exception("No Material Instance '" + materialSymbol + "' found");
            }

            string materialId = xmlInstanceMaterial.Attributes["target"].Value.Substring(1);

            XmlNode xmlMaterial = root.SelectSingleNode(".//material[@id='" + materialId + "']");

            if (xmlMaterial == null)
            {
                throw new Exception("No Material Definition with id '" + materialId + "' found");
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
                throw new Exception("No " + semantic + " Input found");

            string sourceId = input.Attributes["source"].Value.Substring(1);
            XmlNode source = xmlMeshNode.SelectSingleNode("source[@id='" + sourceId + "']");
            if (source == null)
                throw new Exception("No Source found for " + semantic + " Input");

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
                throw new Exception("No input accessor for Source '" +
                    xmlSource.Attributes["id"].Value + "' found");

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
        protected static void GetJointWeightsAndIndices(XmlNode xmlMeshNode, ColladaModel model,
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
