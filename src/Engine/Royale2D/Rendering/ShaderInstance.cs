using SFML.Graphics;
using SFML.Graphics.Glsl;

namespace Royale2D
{
    public class ShaderInstance
    {
        private Shader shader;
        public Dictionary<string, Texture> textureUniforms = new Dictionary<string, Texture>();
        public Dictionary<string, float> floatUniforms = new Dictionary<string, float>();
        public Dictionary<string, Vec4> vec4Uniforms = new Dictionary<string, Vec4>();
        public Dictionary<string, int> intUniforms = new Dictionary<string, int>();

        public ShaderInstance(string shaderName)
        {
            shader = Assets.shaders[shaderName];
        }

        public void SetUniform(string key, Texture texture)
        {
            textureUniforms[key] = texture;
        }

        public void SetUniform(string key, float val)
        {
            floatUniforms[key] = val;
        }

        public void SetUniform(string key, Vec4 vec4)
        {
            vec4Uniforms[key] = vec4;
        }

        public void SetUniform(string key, int val)
        {
            intUniforms[key] = val;
        }

        public Shader GetShader()
        {
            foreach (var kvp in textureUniforms)
            {
                shader.SetUniform(kvp.Key, kvp.Value);
            }
            foreach (var kvp in floatUniforms)
            {
                shader.SetUniform(kvp.Key, kvp.Value);
            }
            foreach (var kvp in vec4Uniforms)
            {
                shader.SetUniform(kvp.Key, kvp.Value);
            }
            foreach (var kvp in intUniforms)
            {
                shader.SetUniform(kvp.Key, kvp.Value);
            }
            return shader;
        }
    }
}
