/*
 * Reactor 3D MIT License
 * 
 * Copyright (c) 2010 Reiser Games
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

#region Using
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
//using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Reactor;
using Reactor.Content;
using System.Collections;
#endregion
namespace Reactor
{

    #region Actor
    public class RActor : RSceneNode, IDisposable
    {

        internal Model _model;
        internal ContentManager _content;
        internal ResourceContentManager _resourcecontent;
        internal Matrix[] _transforms;
        internal Matrix _objectMatrix;
        internal Vector3 scaling;
        internal Vector3 _rotation;
        internal Vector3 _position;
        internal Effect _defaultEffect;
        internal BasicEffect _basicEffect;
        internal RMaterial _material;
        internal AnimationPlayer _player;
        internal SkinningData _skinningData;
        internal BoundingSphere _sphere;
        internal bool _playing;

        public RActor()
        {
            _rotation = Vector3.Zero;
            _position = Vector3.One;
            scaling = Vector3.One;
        }
        internal void CreateActor(string name)
        {
            _name = name;
            _model = new Model();
            _content = new ContentManager(REngine.Instance._game.Services);
            _content.RootDirectory = _content.RootDirectory + "\\Content";
            _resourcecontent = new ResourceContentManager(REngine.Instance._game.Services, Resource1.ResourceManager);
            
        }
        public void Load(string filename)
        {
            _model = _content.Load<Model>(filename);
            float scale = _model.Meshes[0].BoundingSphere.Radius * _model.Bones[0].Transform.Right.Length();
            if (scale == 0)
                scale = 0.0001f;
            _objectMatrix = Matrix.CreateScale(1.0f / scale);
            scaling = new Vector3(scale, scale, scale);
            _objectMatrix = BuildScalingMatrix(_objectMatrix);
#if !XBOX
            _defaultEffect = _resourcecontent.Load<Effect>("Actor");
#else
            _defaultEffect = _resourcecontent.Load<Effect>("Actor1");
#endif
            _defaultEffect.CurrentTechnique = _defaultEffect.Techniques[0];
            _sphere = _model.Meshes[0].BoundingSphere;
            foreach (ModelMesh mesh in _model.Meshes)
            {
                _sphere = BoundingSphere.CreateMerged(_sphere, mesh.BoundingSphere);
                for (int i = 0; i < mesh.MeshParts.Count; i++)
                {
                    BasicEffect effect = (BasicEffect)mesh.MeshParts[i].Effect;

                    Texture t = effect.Texture;
                    mesh.MeshParts[i].Effect = _defaultEffect.Clone(REngine.Instance._graphics.GraphicsDevice);
                    mesh.MeshParts[i].Effect.Parameters["Texture"].SetValue(t);
                    mesh.MeshParts[i].Effect.CommitChanges();
                    
                }
            }
            _skinningData = _model.Tag as SkinningData;
            _player = new AnimationPlayer(_skinningData);
            _player.StartClip(_skinningData.AnimationIndexes[0], 1.0f);
            _playing = true;
        }

        public override void Render()
        {
            Render(_defaultEffect.Techniques[0].Name);
        }
        public void Render(string techniqueName)
        {
            
            
                if (_material == null)
                {
                    GraphicsDevice graphicsDevice = REngine.Instance._graphics.GraphicsDevice;

                    REngine.Instance._graphics.GraphicsDevice.RenderState.CullMode = CullMode.CullCounterClockwiseFace;
                    REngine.Instance._graphics.GraphicsDevice.RenderState.DepthBufferEnable = true;
                    REngine.Instance._graphics.GraphicsDevice.RenderState.DepthBufferWriteEnable = true;
                    REngine.Instance._graphics.GraphicsDevice.RenderState.AlphaBlendEnable = false;

                    REngine.Instance._graphics.GraphicsDevice.RenderState.SeparateAlphaBlendEnabled = false;

                    REngine.Instance._graphics.GraphicsDevice.RenderState.AlphaTestEnable = false;

                    
                    RenderState state = graphicsDevice.RenderState;


                    Matrix[] bones = _player.GetSkinTransforms();
                    RCamera camera = REngine.Instance._camera;
                    foreach (ModelMesh mesh in _model.Meshes)
                    {
                        foreach (Effect effect in mesh.Effects)
                        {
                            //effect.Begin();
                            try
                            {
                                effect.Parameters["Bones"].SetValue(bones);
                                effect.Parameters["View"].SetValue(camera.viewMatrix);
                                effect.Parameters["Projection"].SetValue(camera.projMatrix);
                                effect.Parameters["Light1Direction"].SetValue(Vector3.Negate(RAtmosphere.Instance.sunDirection));
                                effect.Parameters["Light2Direction"].SetValue(Vector3.Negate(RAtmosphere.Instance.sunDirection));
                                effect.Parameters["Light1Color"].SetValue(RAtmosphere.Instance.sunColor);
                                effect.Parameters["Light2Color"].SetValue(RAtmosphere.Instance.sunColor);
                                effect.Parameters["AmbientColor"].SetValue(RAtmosphere.Instance.ambientColor);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.ToString());
                                REngine.Instance.AddToLog(e.ToString());
                            }
                            /*foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                            {
                                pass.Begin();
                                foreach (ModelMeshPart part in mesh.MeshParts)
                                {
                                    graphicsDevice.VertexDeclaration = part.VertexDeclaration;
                                    graphicsDevice.Vertices[0].SetSource(mesh.VertexBuffer, part.StreamOffset, part.VertexStride);
                                    graphicsDevice.Indices = mesh.IndexBuffer;
                                    graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, part.BaseVertex, 0, part.NumVertices, part.StartIndex, part.PrimitiveCount);
                                }
                                pass.End();
                            }
                            effect.End();
                             */
                            
                        }
                        try
                        {
                            mesh.Draw();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.ToString());
                            REngine.Instance.AddToLog(e.ToString());
                        }
                    }

                }
                else
                {
                    RShader_Render(techniqueName);
                }
            
        }
        internal void RShader_Render(string techniqueName)
        {
            Matrix[] bones = _player.GetSkinTransforms();
            GraphicsDevice graphicsDevice = REngine.Instance._graphics.GraphicsDevice;
            
            REngine.Instance._graphics.GraphicsDevice.RenderState.CullMode = CullMode.CullCounterClockwiseFace;
            REngine.Instance._graphics.GraphicsDevice.RenderState.DepthBufferEnable = true;

            REngine.Instance._graphics.GraphicsDevice.RenderState.AlphaBlendEnable = false;
            //REngine.Instance._graphics.GraphicsDevice.RenderState.AlphaBlendOperation = BlendFunction.Add;
            //REngine.Instance._graphics.GraphicsDevice.RenderState.SourceBlend = Blend.One;
            //REngine.Instance._graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.Zero;
            REngine.Instance._graphics.GraphicsDevice.RenderState.SeparateAlphaBlendEnabled = false;

            REngine.Instance._graphics.GraphicsDevice.RenderState.AlphaTestEnable = false;
            
            RenderState state = graphicsDevice.RenderState;
            _material.Prepare(this);
            //graphicsDevice.VertexDeclaration = RVERTEXFORMAT.VertexDeclaration;
            Effect e = _material.shader.effect;
            e.CurrentTechnique = e.Techniques[techniqueName];
            RCamera camera = REngine.Instance._camera;
            
            foreach (ModelMesh mesh in _model.Meshes)
            {
                foreach (Effect effect in mesh.Effects)
                {
                    //effect.Begin();

                    effect.Parameters["Bones"].SetValue(bones);
                    effect.Parameters["View"].SetValue(camera.viewMatrix);
                    effect.Parameters["Projection"].SetValue(camera.projMatrix);
                    /*foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                    {
                        pass.Begin();
                        foreach (ModelMeshPart part in mesh.MeshParts)
                        {
                            graphicsDevice.VertexDeclaration = part.VertexDeclaration;
                            graphicsDevice.Vertices[0].SetSource(mesh.VertexBuffer, part.StreamOffset, part.VertexStride);
                            graphicsDevice.Indices = mesh.IndexBuffer;
                            graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, part.BaseVertex, 0, part.NumVertices, part.StartIndex, part.PrimitiveCount);
                        }
                        pass.End();
                    }
                    effect.End();
                     */

                }
                mesh.Draw();
            }
        }
        public override void Update()
        {
            if (_playing)
            {
                _player.Update(REngine.Instance._gameTime.ElapsedGameTime, true, _objectMatrix * _model.Bones[0].Transform);
            }

            base.Position = _position;
            base.Rotation = _rotation;
            base.Matrix = _objectMatrix;
            base.Quaternion = Quaternion.CreateFromRotationMatrix(base.Matrix);
        }
        public void PlayAnimation(string Name)
        {
            PlayAnimation(Name, 0.5f);
        }
        public void PlayAnimation(string Name, float BlendFactor)
        {
            _playing = true;
            
            _player.StartClip(_skinningData.AnimationClips[Name], BlendFactor);
        }

        public void StopAnimation()
        {
            _playing = false;
            
        }
        Vector3 dir = new Vector3(0, 0, 1);
        public void SetLookAt(R3DVECTOR vector)
        {
            
            dir = Vector3.Normalize(vector.vector - _position);
            Vector3 up = Vector3.Up;
            
            _objectMatrix = Matrix.CreateLookAt(_position, vector.vector, up);
            _objectMatrix = BuildScalingMatrix(_objectMatrix);
            _objectMatrix.Translation = _position;
            Quaternion q = Quaternion.CreateFromRotationMatrix(_objectMatrix);
            
            _rotation.X = q.X;
            _rotation.Y = q.Y;
            _rotation.Z = q.Z;

            
            
            
        }
        internal Matrix BuildRotationMatrix(Matrix m)
        {   
            m *= Matrix.CreateRotationX(_rotation.X);
            m *= Matrix.CreateRotationY(_rotation.Y);
            m *= Matrix.CreateRotationZ(_rotation.Z);
            return m;
        }
        internal Matrix BuildScalingMatrix(Matrix m)
        {
            m *= Matrix.CreateScale(scaling);
            return m;
        }
        internal Matrix BuildPositionMatrix(Matrix m)
        {
            m *= Matrix.CreateTranslation(_position);
            //_transforms[_model.Meshes[0].ParentBone.Index] = m;
            return m;
        }
        public void RotateX(float value)
        {
            _rotation.X += MathHelper.ToRadians(value);
            _objectMatrix *= Matrix.CreateRotationX(_rotation.X);
        }
        public void RotateY(float value)
        {
            _rotation.Y += MathHelper.ToRadians(value);
            _objectMatrix *= Matrix.CreateRotationY(_rotation.Y);
        }
        public void RotateZ(float value)
        {
            _rotation.Z += MathHelper.ToRadians(value);
            _objectMatrix *= Matrix.CreateRotationZ(_rotation.Z);
        }
        public void Rotate(float X, float Y, float Z)
        {
            RotateX(X);
            RotateY(Y);
            RotateZ(Z);
        }
        public R3DVECTOR GetPosition()
        {
            return R3DVECTOR.FromVector3(_position);
        }
        public void SetPosition(R3DVECTOR vector)
        {
            _position = vector.vector;
            Position = _position;
            _objectMatrix = BuildPositionMatrix(_objectMatrix);
            return;
        }
        public void SetPosition(float x, float y, float z)
        {
            _position = new Vector3(x, y, z);
            Position = _position;
            _objectMatrix.Translation = _position;
        }
        public void Move(R3DVECTOR vector)
        {
            Move(vector.X, vector.Y, vector.Z);
        }
        public void Move(float x, float y, float z)
        {
            _position += _objectMatrix.Left * x;
            _position += _objectMatrix.Up * y;
            _position += _objectMatrix.Forward * z;
            
            _objectMatrix.Translation = _position;
        }
        public void SetScale(float ScaleX, float ScaleY, float ScaleZ)
        {
            scaling = new Vector3(ScaleX, ScaleY, ScaleZ);
            _objectMatrix = BuildScalingMatrix(_objectMatrix);

        }
        public void SetTexture(int TextureID, int TextureLayer)
        {
            EffectParameter etexture;
            if (_material != null)
            {
                Texture t = RTextureFactory.Instance._textureList[TextureID];
                etexture = _material.shader.effect.Parameters.GetParameterBySemantic("TEXTURE" + (int)TextureLayer);
                etexture.SetValue(t);


            }
            else
            {
                Texture tex = (Texture2D)RTextureFactory.Instance._textureList[TextureID];
                _defaultEffect.Parameters.GetParameterBySemantic("TEXTURE" + TextureLayer).SetValue(tex);
                


                foreach (ModelMesh mesh in _model.Meshes)
                {
                    foreach (Effect effect in mesh.Effects)
                    {
                        effect.Parameters.GetParameterBySemantic("TEXTURE" + TextureLayer).SetValue(tex);
                    }
                }
            }
        }
        public void SetTexture(int TextureID, int TextureLayer, int MeshID)
        {
            EffectParameter etexture;
            if (_materials[MeshID] != null)
            {
                Texture t = RTextureFactory.Instance._textureList[TextureID];
                etexture = _material.shader.effect.Parameters.GetParameterBySemantic("TEXTURE" + (int)TextureLayer);
                etexture.SetValue(t);


            }
            else
            {
                Texture tex = (Texture2D)RTextureFactory.Instance._textureList[TextureID];
                _defaultEffect.Parameters.GetParameterBySemantic("TEXTURE" + TextureLayer).SetValue(tex);



                
                    foreach (Effect effect in _model.Meshes[MeshID].Effects)
                    {
                        effect.Parameters.GetParameterBySemantic("TEXTURE" + TextureLayer).SetValue(tex);
                    }
                

            }
        }
        public void SetMaterial(RMaterial material)
        {
            int i = 0;
            _materials.Clear();
            _materials.Add(i, material);
            i++;
            foreach (ModelMesh mesh in _model.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    _materials.Add(i, material);
                    part.Effect = _material.shader.effect;
                    i++;
                }
            }
        }
        internal Hashtable _materials = new Hashtable();
        public RMaterial GetMaterial()
        {
            return (RMaterial)_materials[0];
        }
        public RMaterial GetMaterial(int MeshID)
        {
            return (RMaterial)_materials[MeshID];

        }
        public void SetMaterial(RMaterial material, int MeshID)
        {

            _materials[MeshID] = material;
            foreach (ModelMeshPart part in _model.Meshes[MeshID].MeshParts)
            {
                part.Effect = material.shader.effect;
            }
        }
        public void SetMaterial(RMaterial material, string MeshName)
        {
            int mid = GetMeshIDFromName(MeshName);
            SetMaterial(material, mid);
        }
        public void GetMeshNames(out string[] meshNames)
        {
            List<string> mnames = new List<string>();

            foreach (ModelMesh mesh in _model.Meshes)
            {
                mnames.Add(mesh.Name);
            }
            meshNames = mnames.ToArray();
            mnames = null;
        }
        public int GetMeshIDFromName(string Name)
        {
            int i = 0;
            foreach (ModelMesh mesh in _model.Meshes)
            {
                if (mesh.Name == Name)
                    return i;
                else
                    i++;
            }
            return i;
        }
        public string GetMeshNameFromID(int MeshID)
        {
            return _model.Meshes[MeshID].Name;
        }
        public void GetMeshIDs(out int[] meshIDs)
        {
            List<int> mindex = new List<int>();
            int counter = 0;
            foreach (ModelMesh mesh in _model.Meshes)
            {
                mindex.Add(counter);
                counter++;
            }
            meshIDs = mindex.ToArray();
            mindex = null;
        }
        public R3DVECTOR GetRotation()
        {
            return R3DVECTOR.FromVector3(_rotation);
        }
        
        public R3DVECTOR GetScale()
        {
            return R3DVECTOR.FromVector3(scaling);
        }
        public R3DMATRIX GetMatrix()
        {
            return R3DMATRIX.FromMatrix(this.Matrix);
        }
        public void SetMatrix(R3DMATRIX Matrix)
        {
            _objectMatrix = Matrix.matrix;
        }
        public void Dispose()
        {
            if (_model != null)
            {
                _defaultEffect.Dispose();
                _defaultEffect = null;
                _model = null;
                _content.Unload();
                _content = null;
            }
        }
    }
    #endregion
}