﻿using System;
using System.Runtime.InteropServices;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Template_P3 {

    // mesh and loader based on work by JTalton; http://www.opentk.com/node/642

    public class Mesh
    {
	    // data members
	    public ObjVertex[] vertices;			// vertex positions, model space
	    public ObjTriangle[] triangles;			// triangles (3 vertex indices)
	    public ObjQuad[] quads;					// quads (4 vertex indices)
	    int vertexBufferId;						// vertex buffer
	    int triangleBufferId;					// triangle buffer
	    int quadBufferId;						// quad buffer

        public Matrix4 meshTransform;
        public Matrix4 meshScale;

        public float brightness = 1;

	    // constructor
	    public Mesh( string fileName, float scale )
	    {
		    MeshLoader loader = new MeshLoader();
		    loader.Load( this, fileName );
            meshTransform = Matrix4.Identity;
            meshScale = Matrix4.CreateScale(scale);
        }

	    // initialization; called during first render
	    public void Prepare( Shader shader )
	    {
            if (vertexBufferId != 0) return; // already taken care of

            // generate interleaved vertex data (uv/normal/position (total 8 floats) per vertex)
		    GL.GenBuffers( 1, out vertexBufferId );
		    GL.BindBuffer( BufferTarget.ArrayBuffer, vertexBufferId );
		    GL.BufferData( BufferTarget.ArrayBuffer, (IntPtr)(vertices.Length * Marshal.SizeOf( typeof( ObjVertex ) )), vertices, BufferUsageHint.StaticDraw );

		    // generate triangle index array
		    GL.GenBuffers( 1, out triangleBufferId );
		    GL.BindBuffer( BufferTarget.ElementArrayBuffer, triangleBufferId );
		    GL.BufferData( BufferTarget.ElementArrayBuffer, (IntPtr)(triangles.Length * Marshal.SizeOf( typeof( ObjTriangle ) )), triangles, BufferUsageHint.StaticDraw );

		    // generate quad index array
		    GL.GenBuffers( 1, out quadBufferId );
		    GL.BindBuffer( BufferTarget.ElementArrayBuffer, quadBufferId );
		    GL.BufferData( BufferTarget.ElementArrayBuffer, (IntPtr)(quads.Length * Marshal.SizeOf( typeof( ObjQuad ) )), quads, BufferUsageHint.StaticDraw );
	    }


	    // render the mesh using the supplied shader and matrix
	    public void Render( Shader shader, Matrix4 transform, Matrix4 worldTransform, Texture texture , Vector4[] lightData, Vector4 camDir)
	    {
		    // on first run, prepare buffers
		    Prepare( shader );

            // enable texture
            int texLoc = GL.GetUniformLocation(shader.programID, "pixels");
            GL.Uniform1(texLoc, 0);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, texture.id);
                        
            // enable shader
            GL.UseProgram( shader.programID );

            Vector4 ambient_Color_1 = lightData[00];

            Vector4 lightPosition_1 = Vector4.Transform(lightData[01], meshScale  * worldTransform);
            Vector4 diffuse_Color_1 = lightData[02];
            Vector4 speculr_Color_1 = lightData[03];

            Vector4 lightPosition_2 = Vector4.Transform(lightData[04], meshScale * worldTransform);
            Vector4 diffuse_Color_2 = lightData[05];
            Vector4 speculr_Color_2 = lightData[06];

            Vector4 lightPosition_3 = Vector4.Transform(lightData[07], meshScale * worldTransform);
            Vector4 diffuse_Color_3 = lightData[08];
            Vector4 speculr_Color_3 = lightData[09];

            Vector4 lightPosition_4 = Vector4.Transform(lightData[10], meshScale * worldTransform);
            Vector4 diffuse_Color_4 = lightData[11];
            Vector4 speculr_Color_4 = lightData[12];
            Vector4 spotlightDir_4 = lightData[13];

            // pass lightPos + spotlight direction
            int light = GL.GetUniformLocation(shader.programID, "lightPos1");
            GL.Uniform4(light, lightPosition_1);
            light = GL.GetUniformLocation(shader.programID, "lightPos2");
            GL.Uniform4(light, lightPosition_2);
            light = GL.GetUniformLocation(shader.programID, "lightPos3");
            GL.Uniform4(light, lightPosition_3);
            light = GL.GetUniformLocation(shader.programID, "lightPos4");
            GL.Uniform4(light, lightPosition_4);
            light = GL.GetUniformLocation(shader.programID, "spotLightDir_4");
            GL.Uniform4(light, spotlightDir_4);

            // pass light colors
            int ambient = GL.GetUniformLocation(shader.programID, "ambient_Color");
            GL.Uniform4(ambient, ambient_Color_1);

            int diffuse = GL.GetUniformLocation(shader.programID, "diffuse_Color_L1");
            GL.Uniform4(diffuse, diffuse_Color_1);
            diffuse = GL.GetUniformLocation(shader.programID, "diffuse_Color_L2");
            GL.Uniform4(diffuse, diffuse_Color_2);
            diffuse = GL.GetUniformLocation(shader.programID, "diffuse_Color_L3");
            GL.Uniform4(diffuse, diffuse_Color_3);
            diffuse = GL.GetUniformLocation(shader.programID, "diffuse_Color_L4");
            GL.Uniform4(diffuse, diffuse_Color_4);

            int specular = GL.GetUniformLocation(shader.programID, "speculr_Color_L1");
            GL.Uniform4(specular, speculr_Color_1);
            specular = GL.GetUniformLocation(shader.programID, "speculr_Color_L2");
            GL.Uniform4(specular, speculr_Color_2);
            specular = GL.GetUniformLocation(shader.programID, "speculr_Color_L3");
            GL.Uniform4(specular, speculr_Color_3);
            specular = GL.GetUniformLocation(shader.programID, "speculr_Color_L4");
            GL.Uniform4(specular, speculr_Color_4);

            int bright = GL.GetUniformLocation(shader.programID, "brightness");
            GL.Uniform1(bright, brightness);

            // pass view transform to vertex shader
            Matrix4 m = meshScale * meshTransform * transform;
            GL.UniformMatrix4(shader.uniform_mview, false, ref m);

            // pass world transform to vertex shader
            m = meshScale * meshTransform * worldTransform;
            GL.UniformMatrix4(shader.uniform_2wrld, false, ref m);

            int camD = GL.GetUniformLocation(shader.programID, "camDir");
            //((Vector3 camDir = transform.ExtractRotation().Xyz;
            GL.Uniform4(camD, camDir);

            // bind interleaved vertex data
            GL.EnableClientState( ArrayCap.VertexArray );
		    GL.BindBuffer( BufferTarget.ArrayBuffer, vertexBufferId );
		    GL.InterleavedArrays( InterleavedArrayFormat.T2fN3fV3f, Marshal.SizeOf( typeof( ObjVertex ) ), IntPtr.Zero );

		    // link vertex attributes to shader parameters 
		    GL.VertexAttribPointer( shader.attribute_vuvs, 2, VertexAttribPointerType.Float, false, 32, 0 );
		    GL.VertexAttribPointer( shader.attribute_vnrm, 3, VertexAttribPointerType.Float, true, 32, 2 * 4 );
		    GL.VertexAttribPointer( shader.attribute_vpos, 3, VertexAttribPointerType.Float, false, 32, 5 * 4 );

            // enable position, normal and uv attributes
            GL.EnableVertexAttribArray( shader.attribute_vpos );
            GL.EnableVertexAttribArray( shader.attribute_vnrm );
            GL.EnableVertexAttribArray( shader.attribute_vuvs );

		    // bind triangle index data and render
		    GL.BindBuffer( BufferTarget.ElementArrayBuffer, triangleBufferId );
		    GL.DrawArrays( PrimitiveType.Triangles, 0, triangles.Length * 3 );

		    // bind quad index data and render
		    if (quads.Length > 0)
		    {
			    GL.BindBuffer( BufferTarget.ElementArrayBuffer, quadBufferId );
			    GL.DrawArrays( PrimitiveType.Quads, 0, quads.Length * 4 );
		    }

		    // restore previous OpenGL state
		    GL.UseProgram( 0 );
	    }

	    // layout of a single vertex
	    [StructLayout(LayoutKind.Sequential)] public struct ObjVertex
	    {
		    public Vector2 TexCoord;
		    public Vector3 Normal;
		    public Vector3 Vertex;
	    }

	    // layout of a single triangle
	    [StructLayout(LayoutKind.Sequential)] public struct ObjTriangle
	    {
		    public int Index0, Index1, Index2;
	    }

	    // layout of a single quad
	    [StructLayout(LayoutKind.Sequential)] public struct ObjQuad
	    {
		    public int Index0, Index1, Index2, Index3;
	    }
    }

} // namespace Template_P3