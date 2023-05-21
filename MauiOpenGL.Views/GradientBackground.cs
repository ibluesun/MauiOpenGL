
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using OpenTK.Graphics.ES20;


using OpenTK.Mathematics;

namespace UniversalGraphicsEngine
{
    public enum GradientDirection
    {
        /// <summary>
        /// Top to Bottom
        /// </summary>
        LinearHorizontal,

        /// <summary>
        /// Left to Right
        /// </summary>
        LinearVertical,


        /// <summary>
        /// Center to edges
        /// </summary>
        Radial
    }

    public class GradientBackground
    {

        int _VertexShaderId;
        int _FragmentShaderId;

        int _ProgramId;

        bool _IsLinked;


        int vPosition;
        int vColor;

        void PrepareGradientBackground()
        {
            string TextureVertexShader = @"

                attribute  vec2 vPosition;
                attribute  vec3 vColor;
                varying vec3 color;
                void main() {
                    color = vColor;
                    gl_Position = vec4(vPosition.xy,1,1);
                } 
            ";

            string TextureFragmentShader = @"

                precision mediump float;
                varying vec3 color;
                void main() {
                    gl_FragColor = vec4(color,1);
                }
             ";


            _VertexShaderId = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(_VertexShaderId, TextureVertexShader);
            GL.CompileShader(_VertexShaderId);

            _FragmentShaderId = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(_FragmentShaderId, TextureFragmentShader);
            GL.CompileShader(_FragmentShaderId);



            _ProgramId = GL.CreateProgram();

            GL.AttachShader(_ProgramId, _VertexShaderId);
            GL.AttachShader(_ProgramId, _FragmentShaderId);

            GL.LinkProgram(_ProgramId);

            var link_log = GL.GetProgramInfoLog(_ProgramId);

            
            // read the link status
            GL.GetProgram(_ProgramId, GetProgramParameterName.LinkStatus, out var linkstatus);




            _IsLinked = linkstatus == 1 ? true : false;

            if (!_IsLinked) throw new Exception(link_log);



            vPosition = GL.GetAttribLocation(_ProgramId, "vPosition");
            vColor = GL.GetAttribLocation(_ProgramId, "vColor");


        }

        void BeginUse()
        {
            GL.UseProgram(_ProgramId);

            GL.EnableVertexAttribArray(vPosition);
            GL.EnableVertexAttribArray(vColor);
        }

        void EndUse()
        {

            GL.DisableVertexAttribArray(vPosition);
            GL.DisableVertexAttribArray(vColor);

        }

        public Color4 StartColor { get; set; } = Color4.SkyBlue;
        public Color4 StopColor { get; set; } = Color4.White;


        /// <summary>
        /// Top to Bottom
        /// </summary>
        void RenderHorizontal()
        {

            var vp = new Vector2[] {
                new Vector2(-1, 1),
                new Vector2(1, 1),
                new Vector2(1, -1),

                new Vector2(-1, 1),
                new Vector2(1, -1),
                new Vector2(-1, -1)
            };
            


            var vc = new Vector3[] {
                ((Vector4)StartColor).Xyz,
                ((Vector4)StartColor).Xyz,
                ((Vector4)StopColor).Xyz,

                ((Vector4)StartColor).Xyz,
                ((Vector4)StopColor).Xyz,
                ((Vector4)StopColor).Xyz
            };


            BeginUse();

            GL.VertexAttribPointer(vPosition, 2, VertexAttribPointerType.Float, false, 0, vp);
            GL.VertexAttribPointer(vColor, 3, VertexAttribPointerType.Float, false, 0, vc);


            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

            
            
            EndUse();
        }

        /// <summary>
        /// Left to Right
        /// </summary>
        void RenderVertical()
        {

            var vp = new Vector2[] {
                new Vector2(-1, 1),
                new Vector2(1, 1),
                new Vector2(1, -1),

                new Vector2(-1, 1),
                new Vector2(1, -1),
                new Vector2(-1, -1)
            };

            var vc = new Vector3[] {
                ((Vector4)StartColor).Xyz,
                ((Vector4)StopColor).Xyz,
                ((Vector4)StopColor).Xyz,

                ((Vector4)StartColor).Xyz,
                ((Vector4)StopColor).Xyz,
                ((Vector4)StartColor).Xyz
            };

            BeginUse();
            GL.VertexAttribPointer(vPosition, 2, VertexAttribPointerType.Float, false, 0, vp);
            GL.VertexAttribPointer(vColor, 3, VertexAttribPointerType.Float, false, 0, vc);


            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
            EndUse();
        }

        

        /// <summary>
        /// From Center to Edges
        /// </summary>
        void RenderRadial()
        {

            var vp = new Vector2[] {

                // Center
                new Vector2(0,0),

                // Top Triangle
                new Vector2(-1, 1),
                new Vector2(1, 1),
                

                // Right Triangle
                new Vector2(1, 1),
                new Vector2(1, -1),
                

                // Bottom Triangle
                new Vector2(1, -1),
                new Vector2(-1, -1),
                

                // Left Triangle
                new Vector2(-1, -1),
                new Vector2(-1, 1)

            };



            var vc = new Vector3[] {
                ((Vector4)StartColor).Xyz,

                ((Vector4)StopColor).Xyz,
                ((Vector4)StopColor).Xyz,

                ((Vector4)StopColor).Xyz,
                ((Vector4)StopColor).Xyz,


                ((Vector4)StopColor).Xyz,
                ((Vector4)StopColor).Xyz,


                ((Vector4)StopColor).Xyz,
                ((Vector4)StopColor).Xyz



                };

            BeginUse();
            GL.VertexAttribPointer(vPosition, 2, VertexAttribPointerType.Float, false, 0, vp);
            GL.VertexAttribPointer(vColor, 3, VertexAttribPointerType.Float, false, 0, vc);

            GL.DrawArrays(PrimitiveType.TriangleFan, 0, 9);
            EndUse();
        }

        
        public GradientDirection GradientDirection { get; set; } = GradientDirection.Radial;

        public void Render()
        {
            if (_IsLinked == false )
            {
                PrepareGradientBackground();
            }

            switch (GradientDirection)
            {
                case GradientDirection.LinearHorizontal:
                    this.RenderHorizontal();
                    break;
                    
                case GradientDirection.LinearVertical:
                    RenderVertical();
                    break;
                case GradientDirection.Radial:
                    RenderRadial();
                    break;
                default:
                    break;
            }
        }
    }
}
