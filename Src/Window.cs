using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

public class Window : GameWindow
{
    private int _vertexBufferHandle;
    private int _shaderProgramHandle;
    private int _vertexArrayHandle;

    public Window() : base(GameWindowSettings.Default, NativeWindowSettings.Default)
    {
        CenterWindow();
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        GL.Viewport(0, 0, e.Width, e.Height);
        base.OnResize(e);
    }

    protected override void OnLoad()
    {
        GL.ClearColor(new Color4(.3f, .4f, .4f, 1f));

        var verticies = new float[]{
            0f, .5f, 0f,
            .5f, -.5f, 0f,
            -.5f, -.5f, 0f
        };

        _vertexBufferHandle = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferHandle);
        GL.BufferData(BufferTarget.ArrayBuffer, verticies.Length * sizeof(float), verticies, BufferUsageHint.StaticDraw);
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

        _vertexArrayHandle = GL.GenVertexArray();
        GL.BindVertexArray(_vertexArrayHandle);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferHandle);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        GL.BindVertexArray(0);

        string vertexShaderCode = 
        @"
        #version 330 core

        layout (location = 0) in vec3 aPosition;

        void main()
        {
            gl_Position = vec4(aPosition, 1f);
        }
        ";

        string pixelShaderCode =
        @"
        #version 330 core

        out vec4 pixelColor;

        void main()
        {
            pixelColor = vec4(.8f, .8f, .1f, 1f);
        }
        ";

        var vertexShaderHandle = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vertexShaderHandle, vertexShaderCode);
        GL.CompileShader(vertexShaderHandle);

        var pixelShaderHandle = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(pixelShaderHandle, pixelShaderCode);
        GL.CompileShader(pixelShaderHandle);

        _shaderProgramHandle = GL.CreateProgram();

        GL.AttachShader(_shaderProgramHandle, vertexShaderHandle);
        GL.AttachShader(_shaderProgramHandle, pixelShaderHandle);

        GL.LinkProgram(_shaderProgramHandle);

        GL.DetachShader(_shaderProgramHandle, vertexShaderHandle);
        GL.DetachShader(_shaderProgramHandle, pixelShaderHandle);

        GL.DeleteShader(vertexShaderHandle);
        GL.DeleteShader(pixelShaderHandle);

        base.OnLoad();
    }

    protected override void OnUnload()
    {
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        GL.DeleteBuffer(_vertexBufferHandle);

        GL.UseProgram(0);
        GL.DeleteProgram(_shaderProgramHandle);

        base.OnUnload();
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        GL.Clear(ClearBufferMask.ColorBufferBit);

        GL.UseProgram(_shaderProgramHandle);
        GL.BindVertexArray(_vertexArrayHandle);
        GL.DrawArrays(PrimitiveType.Triangles, 0, 3);

        Context.SwapBuffers();
        base.OnRenderFrame(args);
    }
}