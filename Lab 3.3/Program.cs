using ImGuiNET;
using LabirintusProjekt;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;
using System.Numerics;
using System.Reflection;
namespace Labirintus_projekt
{
    internal class Program
    {
        private static IWindow graphicWindow;

        private static GL Gl;

        private static ImGuiController imGuiController;

        // labirint components
        private static LabirintMap labirint;
        private static LabirintComponents components;
        private static WallObject floor;
        private static SkyboxObject skybox;

        // camera  and his starting position
        private static int startX;
        private static int startY;
        private static CameraDescriptor camera = new CameraDescriptor();

        // character
        private static PacmanCharacter player;

        // collision message
        private static string collMessage;

        // game variables
        private static bool winner = false;
        private static bool gameRunning = false;
        private static double globalTimer = 0.0f;

        private static bool collisionMessage = false;
        private static double messageTimer = 0.0f;
        private static bool isGhostTimerActive = false;
        private static double ghostTimer = 0.0f;

        private const string ModelMatrixVariableName = "uModel";
        private const string NormalMatrixVariableName = "uNormal";
        private const string ViewMatrixVariableName = "uView";
        private const string ProjectionMatrixVariableName = "uProjection";

        private const string TextureVariableName = "uTexture";

        private const string LightColorVariableName = "uLightColor";
        private const string LightPositionVariableName = "uLightPos";
        private const string ViewPositionVariableName = "uViewPos";

        private const string ShinenessVariableName = "uShininess";
        private const string AmbientVariableName = "uAmbientStrength";
        private const string DiffuseVariableName = "uDiffuseStrength";
        private const string SpecularVariableName = "uSpecularStrength";

        private static float shininess = 50;
        private static Vector3 ambientStrength = new Vector3(0.5f, 0.5f, 0.5f);
        private static Vector3 diffuseStrength = new Vector3(0.5f, 0.5f, 0.5f);
        private static Vector3 specularStrength = new Vector3(0.6f, 0.6f, 0.6f);

        private static float lightPosX = 0f;
        private static float lightPosY = 1.5f;
        private static float lightPosZ = 0f;

        private static uint program;

        static void Main(string[] args)
        {
            WindowOptions windowOptions = WindowOptions.Default;
            windowOptions.Title = "Labirintus";
            windowOptions.Size = new Vector2D<int>(1200, 800);

            graphicWindow = Window.Create(windowOptions);

            graphicWindow.Load += GraphicWindow_Load;
            graphicWindow.Update += GraphicWindow_Update;
            graphicWindow.Render += GraphicWindow_Render;
            graphicWindow.Closing += GraphicWindow_Closing;

            graphicWindow.Run();
        }

        private static void GraphicWindow_Closing()
        {
            components.Dispose();
            Gl.DeleteProgram(program);
        }

        private static void GraphicWindow_Load()
        {
            Gl = graphicWindow.CreateOpenGL();

            // initialize player
            player = new PacmanCharacter(Gl);

            // initialize labirint
            labirint = new LabirintMap();
            components = new LabirintComponents(labirint, Gl);
            floor = WallObject.CreateFloor(Gl);
            skybox = SkyboxObject.CreateSkyBox(Gl); ;

            // initialize starting position
            var pos = labirint.GetStartingPosition();
            startX = pos[0];
            startY = pos[1];

            // set camera and player position
            // default: third person!
            player.Position = (new Vector3D<float>(startX, 0, startY));
            player.updatePosition();
            camera.UpdateCameraPositionFromPlayer(player.Position, player.GetPlayerForward(), player.GetPlayerRight());

            // set lightning
            int[] camStartPos = labirint.getLabirintCenter();
            lightPosX = camStartPos[0];
            lightPosY = 5.5f;
            lightPosZ = camStartPos[1];


            // directing player
            var inputContext = graphicWindow.CreateInput();
            foreach (var keyboard in inputContext.Keyboards)
            {
                keyboard.KeyDown += Keyboard_KeyDown;
                keyboard.KeyUp += Keyboard_KeyUp;
            }

            // Handle resizes
            graphicWindow.FramebufferResize += s =>
            {
                // Adjust the viewport to the new window size
                Gl.Viewport(s);
            };



            imGuiController = new ImGuiController(Gl, graphicWindow, inputContext);

            Gl.ClearColor(System.Drawing.Color.White);
            
            Gl.Enable(EnableCap.CullFace);
            Gl.CullFace(TriangleFace.Back);
            Gl.Disable(EnableCap.CullFace);

            Gl.Enable(EnableCap.DepthTest);
            Gl.DepthFunc(DepthFunction.Lequal);


            uint vshader = Gl.CreateShader(ShaderType.VertexShader);
            uint fshader = Gl.CreateShader(ShaderType.FragmentShader);

            Gl.ShaderSource(vshader, GetEmbeddedResourceAsString("Shaders.VertexShader.vert"));
            Gl.CompileShader(vshader);
            Gl.GetShader(vshader, ShaderParameterName.CompileStatus, out int vStatus);
            if (vStatus != (int)GLEnum.True)
                throw new Exception("Vertex shader failed to compile: " + Gl.GetShaderInfoLog(vshader));

            Gl.ShaderSource(fshader, GetEmbeddedResourceAsString("Shaders.FragmentShader.frag"));
            Gl.CompileShader(fshader);
            Gl.GetShader(fshader, ShaderParameterName.CompileStatus, out int fStatus);
            if (fStatus != (int)GLEnum.True)
                throw new Exception("Fragment shader failed to compile: " + Gl.GetShaderInfoLog(fshader));

            program = Gl.CreateProgram();
            Gl.AttachShader(program, vshader);
            Gl.AttachShader(program, fshader);
            Gl.LinkProgram(program);

            Gl.DetachShader(program, vshader);
            Gl.DetachShader(program, fshader);
            Gl.DeleteShader(vshader);
            Gl.DeleteShader(fshader);
            if ((ErrorCode)Gl.GetError() != ErrorCode.NoError)
            {

            }

            Gl.GetProgram(program, GLEnum.LinkStatus, out var status);
            if (status == 0)
            {
                Console.WriteLine($"Error linking shader {Gl.GetProgramInfoLog(program)}");
            }
        }

        private static string GetEmbeddedResourceAsString(string resourceRelativePath)
        {
            string resourceFullPath = Assembly.GetExecutingAssembly().GetName().Name + "." + resourceRelativePath;
            Console.WriteLine(resourceFullPath);

            using (var resStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceFullPath))
            using (var resStreamReader = new StreamReader(resStream))
            {
                var text = resStreamReader.ReadToEnd();
                return text;
            }
        }

        private static void GraphicWindow_Update(double deltaTime)
        {
            imGuiController.Update((float)deltaTime);

            if (gameRunning && !winner)
            {
                globalTimer += deltaTime;
                MovePlayer();
                MoveCamera();
                RotateCameraAndCharachter();
                foreach (var solider in components.soliders)
                {
                    solider.UpdatePosition(deltaTime, checkSoliderCollision);
                }

                // collision check (for now - just camera)
                if (camera.defaultMode)
                {
                    checkCollision();
                }
            }
        }

        private static unsafe void GraphicWindow_Render(double deltaTime)
        {
            Gl.Clear(ClearBufferMask.ColorBufferBit);
            Gl.Clear(ClearBufferMask.DepthBufferBit);

            Gl.UseProgram(program);

            if (gameRunning && !winner)
            {
                SetUniform3(LightColorVariableName, new Vector3(1f, 1f, 1f));
                SetUniform3(LightPositionVariableName, new Vector3(lightPosX, lightPosY, lightPosZ));
                SetUniform3(ViewPositionVariableName, new Vector3(camera.Position.X, camera.Position.Y, camera.Position.Z));
                SetUniform1(ShinenessVariableName, shininess);

                var viewMatrix = Matrix4X4.CreateLookAt(camera.Position, camera.Position + camera.ForwardVector, camera.UpVector);
                SetMatrix(viewMatrix, ViewMatrixVariableName);

                var projectionMatrix = Matrix4X4.CreatePerspectiveFieldOfView((float)(Math.PI / 2), 1024f / 768f, 0.1f, 100f);
                SetMatrix(projectionMatrix, ProjectionMatrixVariableName);

                // draw skybox
                DrawSkyBox();

                // render bars
                for (int i = 0; i < components.wallList.Count; i++)
                {
                    var wall = components.wallList[i];
                    var transform = components.wallTransformations[i];

                    SetModelMatrix(transform);
                    DrawModelObject(wall);
                }

                // render floor
                var t = Matrix4X4.CreateTranslation(0f, 0f, 0f);
                SetModelMatrix(t);
                DrawModelObject(floor);

                // render windows/bars
                for (int i = 0; i < components.windows.Count; i++)
                {
                    var window = components.windows[i];
                    var transform = components.windowTransformations[i];

                    SetModelMatrix(transform);
                    DrawGlObject(window);
                }

                // render player
                SetModelMatrix(player.transfHead);
                DrawGlObject(player.head);

                // render soliders
                for (int i = 0; i < components.soliders.Count; i++)
                {
                    var solider = components.soliders[i];
                    var transform = solider.transformation;

                    SetModelMatrix(transform);
                    DrawSolider(solider.body);
                }

                SetModelMatrix(player.transfLegs);
                DrawGlObject(player.legs);

                // collision message
                if (collisionMessage)
                {
                    ShowCollisionMessage(collMessage);
                    messageTimer -= deltaTime;
                    if (messageTimer <= 0.0f)
                    {
                        collisionMessage = false;
                    }
                }

                // ghost Mode button
                GhostModeButton();

                if (isGhostTimerActive)
                {
                    GhostModeTimer(deltaTime);
                }
                ImGui.End();

                ManageGlobalTimer();
            }
            else if (winner)
            {
                ImGui.Begin("Congratulation!", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.AlwaysAutoResize);
                ImGui.SetWindowPos(new System.Numerics.Vector2(graphicWindow.Size.X / 2 - 100, graphicWindow.Size.Y / 2 - 50), ImGuiCond.Always);

                int minutes = (int)(globalTimer / 60);
                double seconds = globalTimer % 60;
                string finalTimeText = $"Your Time: {minutes:D2}:{(int)seconds:D2}";

                ImGui.Text("Congratulations, you won!");
                ImGui.Text(finalTimeText); 

                if (ImGui.Button("Exit Game", new System.Numerics.Vector2(150, 30)))
                {
                    graphicWindow.Close();
                }

                ImGui.End();
            }
            else
            {
                ImGui.Begin("Start Menu", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.AlwaysAutoResize);
                ImGui.SetWindowPos(new System.Numerics.Vector2(graphicWindow.Size.X / 2 - 100, graphicWindow.Size.Y / 2 - 50), ImGuiCond.Always);

                if (ImGui.Button("Start Game", new System.Numerics.Vector2(200, 100)))
                {
                    Console.WriteLine("Gomb start kapcs");
                    if (!gameRunning)
                    {
                        gameRunning = true;
                        globalTimer = 0;
                        Console.WriteLine("Játék elindítva!");
                    }
                }
                ImGui.End();
            }
            

            imGuiController.Render();
        }

        private static unsafe void DrawSkyBox()
        {
            var modelMatrixSkyBox = Matrix4X4.CreateScale(100f);
            SetModelMatrix(modelMatrixSkyBox);

            // set the texture
            int textureLocation = Gl.GetUniformLocation(program, TextureVariableName);
            if (textureLocation == -1)
            {
                throw new Exception($"{TextureVariableName} uniform not found on shader.");
            }
            // set texture 0
            Gl.Uniform1(textureLocation, 0);
            Gl.ActiveTexture(TextureUnit.Texture0);
            Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)GLEnum.Linear);
            Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)GLEnum.Linear);
            Gl.BindTexture(TextureTarget.Texture2D, skybox.Texture.Value);

            DrawSkyObject(skybox);

            CheckError();
            Gl.BindTexture(TextureTarget.Texture2D, 0);
            CheckError();
        }

        private static unsafe void DrawSolider(TexturedObject obj)
        {;
            // set the texture
            int textureLocation = Gl.GetUniformLocation(program, TextureVariableName);
            if (textureLocation == -1)
            {
                throw new Exception($"{TextureVariableName} uniform not found on shader.");
            }
            // set texture 0
            Gl.Uniform1(textureLocation, 0);
            Gl.ActiveTexture(TextureUnit.Texture0);
            Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)GLEnum.Linear);
            Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)GLEnum.Linear);
            Gl.BindTexture(TextureTarget.Texture2D, obj.Texture.Value);

            DrawTexturedObject(obj);

            CheckError();
            Gl.BindTexture(TextureTarget.Texture2D, 0);
            CheckError();
        }

        private static unsafe void GhostModeButton()
        {
            ImGui.SetNextWindowPos(new System.Numerics.Vector2(10, 10), ImGuiCond.Always);
            ImGui.Begin("Camera Mode", ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoMove);


            if (ImGui.Button("GHOST MODE"))
            {
                camera.changeCameraModeToGhost();
                isGhostTimerActive = true;
                ghostTimer = 15;
                globalTimer += 30;
            }

            if (camera.defaultMode)
            {
                if (ImGui.Button("FIRST PERSON VIEW"))
                {
                    Console.WriteLine("FIRST PERSON!");
                    camera.changeViewToFirst();
                }

                if (ImGui.Button("THIRD PERSON VIEW"))
                {
                    Console.WriteLine("THIRD");
                    camera.changeViewToThird();
                }
            }
        }

        private static unsafe void ManageGlobalTimer()
        {
            int minutes = (int)(globalTimer / 60);
            double seconds = globalTimer % 60;
            string timerText = $"ELAPSED TIME: {minutes:D2}:{(int)seconds:D2}";

            System.Numerics.Vector2 textSize = ImGui.CalcTextSize(timerText);
            System.Numerics.Vector2 textPosition = new System.Numerics.Vector2(
                graphicWindow.Size.X / 2 - textSize.X / 2,
                10
            );
            System.Numerics.Vector2 labelSize = textSize + new System.Numerics.Vector2(10, 5);
            System.Numerics.Vector2 labelPosition = textPosition - new System.Numerics.Vector2(5, 2);

            ImGui.GetForegroundDrawList().AddRectFilled(labelPosition, labelPosition + labelSize, 0xFF000000);
            ImGui.GetForegroundDrawList().AddText(textPosition, 0xFFFFFFFF, timerText);
        }

        private static unsafe void SetModelMatrix(Matrix4X4<float> modelMatrix)
        {
            SetMatrix(modelMatrix, ModelMatrixVariableName);

            // set also the normal matrix
            int location = Gl.GetUniformLocation(program, NormalMatrixVariableName);
            if (location == -1)
            {
                throw new Exception($"{NormalMatrixVariableName} uniform not found on shader.");
            }

            // G = (M^-1)^T
            var modelMatrixWithoutTranslation = new Matrix4X4<float>(modelMatrix.Row1, modelMatrix.Row2, modelMatrix.Row3, modelMatrix.Row4);
            modelMatrixWithoutTranslation.M41 = 0;
            modelMatrixWithoutTranslation.M42 = 0;
            modelMatrixWithoutTranslation.M43 = 0;
            modelMatrixWithoutTranslation.M44 = 1;

            Matrix4X4<float> modelInvers;
            Matrix4X4.Invert(modelMatrixWithoutTranslation, out modelInvers);
            Matrix3X3<float> normalMatrix = new Matrix3X3<float>(Matrix4X4.Transpose(modelInvers));

            Gl.UniformMatrix3(location, 1, false, (float*)&normalMatrix);
            CheckError();
        }

        private static unsafe void SetUniform1(string uniformName, float uniformValue)
        {
            int location = Gl.GetUniformLocation(program, uniformName);
            if (location == -1)
            {
                throw new Exception($"{uniformName} uniform not found on shader.");
            }

            Gl.Uniform1(location, uniformValue);
            CheckError();
        }

        private static unsafe void SetUniform3(string uniformName, Vector3 uniformValue)
        {
            int location = Gl.GetUniformLocation(program, uniformName);
            if (location == -1)
            {
                throw new Exception($"{uniformName} uniform not found on shader.");
            }

            Gl.Uniform3(location, uniformValue);
            CheckError();
        }

        private static unsafe void DrawModelObject(WallObject modelObject)
        {
            Gl.BindVertexArray(modelObject.Vao);
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, modelObject.Indices);
            Gl.DrawElements(PrimitiveType.Triangles, modelObject.IndexArrayLength, DrawElementsType.UnsignedInt, null);
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, 0);
            Gl.BindVertexArray(0);
        }

        private static unsafe void DrawSkyObject(SkyboxObject modelObject)
        {
            Gl.BindVertexArray(modelObject.Vao);
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, modelObject.Indices);
            Gl.DrawElements(PrimitiveType.Triangles, modelObject.IndexArrayLength, DrawElementsType.UnsignedInt, null);
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, 0);
            Gl.BindVertexArray(0);
        }
        private static unsafe void DrawTexturedObject(TexturedObject modelObject)
        {
            Gl.BindVertexArray(modelObject.Vao);
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, modelObject.Indices);
            Gl.DrawElements(PrimitiveType.Triangles, modelObject.IndexArrayLength, DrawElementsType.UnsignedInt, null);
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, 0);
            Gl.BindVertexArray(0);
        }

        private static unsafe void DrawGlObject(GlObject obj)
        {
            Gl.BindVertexArray(obj.Vao);
            Gl.DrawElements(GLEnum.Triangles, obj.IndexArrayLength, GLEnum.UnsignedInt, null);
            Gl.BindVertexArray(0);
        }

        private static unsafe void SetMatrix(Matrix4X4<float> mx, string uniformName)
        {
            int location = Gl.GetUniformLocation(program, uniformName);
            if (location == -1)
            {
                throw new Exception($"{uniformName} uniform not found on shader.");
            }

            Gl.UniformMatrix4(location, 1, false, (float*)&mx);
            CheckError();
        }

        public static void CheckError()
        {
            var error = (ErrorCode)Gl.GetError();
            if (error != ErrorCode.NoError)
                throw new Exception("GL.GetError() returned " + error.ToString());
        }


        // CAMERA controll
        private static void Keyboard_KeyDown(IKeyboard keyboard, Key key, int arg3)
        {
            switch (key)
            {
                case Key.W:
                    camera.pressedForward = true;
                    break;
                case Key.S:
                    camera.pressedBackward = true;
                    break;
                case Key.A:
                    camera.pressedLeft = true;
                    break;
                case Key.D:
                    camera.pressedRight = true;
                    break;

                case Key.Space:
                    camera.MoveUp();
                    break;
                case Key.ShiftLeft:
                    camera.MoveDown();
                    break;

                case Key.Left:
                    camera.pressedRLeft = true;
                    break;
                case Key.Right:
                    camera.pressedRRight = true;
                    break;
                case Key.Up:
                    camera.pressedRUp = true;
                    break;
                case Key.Down:
                    camera.pressedRDown = true;
                    break;
            }
                 
        }

        private static void Keyboard_KeyUp(IKeyboard keyboard, Key key, int arg3)
        {
            switch (key)
            {
                case Key.W:
                    camera.pressedForward = false;
                    break;
                case Key.S:
                    camera.pressedBackward = false;
                    break;
                case Key.A:
                    camera.pressedLeft = false;
                    break;
                case Key.D:
                    camera.pressedRight = false;
                    break;
                case Key.Left:
                    camera.pressedRLeft = false;
                    break;
                case Key.Right:
                    camera.pressedRRight = false;
                    break;
                case Key.Up:
                    camera.pressedRUp = false;
                    break;
                case Key.Down:
                    camera.pressedRDown = false;
                    break;
            }
        }

        private static void MoveCamera()
        {
            if (camera.ghostMode)
            {
                if (camera.pressedForward)
                {
                    camera.MoveForward();
                }
                else if (camera.pressedBackward)
                {
                    camera.MoveBackward();
                }
                else if (camera.pressedLeft)
                {
                    camera.MoveLeft();
                }
                else if (camera.pressedRight)
                {
                    camera.MoveRight();
                }
            }
            
        }

        private static void RotateCameraAndCharachter()
        {
            // in ghost mode, camera can rotate separatelly
            if (camera.ghostMode)
            {
                if (camera.pressedRLeft)
                {
                    camera.Rotate(-1, 0);
                }
                else if (camera.pressedRRight)
                {
                    camera.Rotate(1, 0);
                }
                else if (camera.pressedRUp)
                {
                    camera.Rotate(0, 1);
                }
                else if (camera.pressedRDown)
                {
                    camera.Rotate(0, -1);
                }
            }
            
            // in default mode, camera follows the player
            if (camera.defaultMode)
            {
                if (camera.pressedRLeft)
                {
                    player.Rotate(-1);
                }
                else if (camera.pressedRRight)
                {
                    player.Rotate(1);
                }

                camera.Yaw = player.yaw;

            }
        }

        // update player`s position
        private static void MovePlayer()
        {
            if (!camera.defaultMode) return;

            Vector3D<float> direction = new Vector3D<float>(0, 0, 0);

            if (camera.pressedForward)
                direction += player.GetPlayerForward();
            if (camera.pressedBackward)
                direction -= player.GetPlayerForward();
            if (camera.pressedRight)
                direction += player.GetPlayerRight();
            if (camera.pressedLeft)
                direction -= player.GetPlayerRight();

            if (direction.Length > 0)
            {
                direction = Vector3D.Normalize(direction);
                player.Position += direction * 0.05f;
            }

            player.updatePosition();
            camera.UpdateCameraPositionFromPlayer(player.Position, player.GetPlayerForward(), player.GetPlayerRight());
        }

        // GAME LOGICS
        private static void checkCollision()
        {
            // wall
            int mapX = (int)Math.Floor(player.Position.X);
            int mapY = (int)Math.Floor(player.Position.Z);

            int tile = labirint.Get(mapX, mapY);
            bool isObstacle = tile == 1 || tile == 9;

            // check if the player wins
            if (tile == 3) winner = true;

            if (isObstacle)
            {
                collisionMessage = true;
                collMessage = "YOU COLLIDED WITH WALL! TRY AGAIN!";

                // reset player
                player.Position = (new Vector3D<float>(startX, 0, startY));

                // reset camera
                camera.changeViewToFirst();
                camera.UpdateCameraPositionFromPlayer(player.Position, player.GetPlayerForward(), player.GetPlayerRight());
                messageTimer = 2.0;

            }

            // solider
            foreach(var solider in components.soliders)
            {
                if (checkSoliderCollisionwithPlayer(solider.Position))
                {
                    collisionMessage = true;
                    collMessage = "YOU COLLIDED WITH A SOLIDER! TRY AGAIN!";

                    // reset player
                    player.Position = (new Vector3D<float>(startX, 0, startY));

                    // reset camera
                    camera.changeViewToFirst();
                    camera.UpdateCameraPositionFromPlayer(player.Position, player.GetPlayerForward(), player.GetPlayerRight());
                    messageTimer = 2.0;
                }
            }

        }


        // GRAPHICS
        private static unsafe void GhostModeTimer(double deltaTime)
        {
            ImGui.Begin("Ghost Mode Timer", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.AlwaysAutoResize);
            ImGui.TextColored(new System.Numerics.Vector4(1, 0, 0, 1),
                $"Ghost Mode: {ghostTimer:F1} sec");
            ImGui.End();

            ghostTimer -= deltaTime;
            if (ghostTimer <= 0f)
            {
                isGhostTimerActive = false;
                ghostTimer = 0f;
                camera.changeCameraModeToDefault();
            }
        }

        private static void ShowCollisionMessage(String message)
        {
            var io = ImGui.GetIO();
            var windowWidth = io.DisplaySize.X;
            var windowHeight = io.DisplaySize.Y;

            var messageWidth = 400f;
            var messageHeight = 20f;

            // Sáv pozíció a képernyő közepén (felülről kicsit lejjebb)
            var posX = (windowWidth - messageWidth) / 2f;
            var posY = windowHeight * 0.25f;

            ImGui.SetNextWindowPos(new Vector2(posX, posY), ImGuiCond.Always);
            ImGui.SetNextWindowSize(new Vector2(messageWidth, messageHeight));
            ImGui.SetNextWindowBgAlpha(1.0f); 

            ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(0f, 0f, 0f, 0.85f)); // fekete háttér
            ImGui.Begin("CollisionMessage", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize |
                                            ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse |
                                            ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoSavedSettings);

            ImGui.SetCursorPosX((messageWidth - ImGui.CalcTextSize(message).X) / 2f);
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1f, 0.3f, 0.3f, 1f)); // piros szöveg
            ImGui.Text(message);
            ImGui.PopStyleColor();

            ImGui.End();
            ImGui.PopStyleColor();
        }

        private static bool checkSoliderCollision(Vector3D<float> nextPosition)
        {
            float halfSize = 0.30f;

            Vector3D<float>[] checkPoints = new Vector3D<float>[]
            {
                nextPosition,
                nextPosition + new Vector3D<float>(halfSize, 0, 0),
                nextPosition - new Vector3D<float>(halfSize, 0, 0),
                nextPosition + new Vector3D<float>(0, 0, halfSize),
                nextPosition - new Vector3D<float>(0, 0, halfSize)
            };

            foreach (var point in checkPoints)
            {
                int mapX = (int)Math.Floor(point.X);
                int mapY = (int)Math.Floor(point.Z);

                if (mapX >= 0 && mapX < labirint.Width && mapY >= 0 && mapY < labirint.Height)
                {
                    int tile = labirint.Get(mapX, mapY);
                    if (tile == 1 || tile == 9)
                    {
                        return true;
                    }
                }
                else
                {
                    return true;
                }
            }

            return false;
        }

        private static bool checkSoliderCollisionwithPlayer(Vector3D<float> soliderPosition)
        {
            float deltaX = player.Position.X - soliderPosition.X;
            float deltaY = player.Position.Y - soliderPosition.Y;
            float deltaZ = player.Position.Z - soliderPosition.Z;

            float distanceSquared = (deltaX * deltaX) + (deltaY * deltaY) + (deltaZ * deltaZ);

            return distanceSquared < (0.30f * 0.30f);
        }


    }
}