#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VillaDelChef.ScriptableObjects;

namespace VillaDelChef.Core.Editor
{
    public static class CharacterPipelineEditor
    {
        private const string FriendsRoot = "Assets/_Projet/Art/Characters/Friends";
        private const string ResourcesCharactersRoot = "Assets/_Projet/Resources/Characters";
        private const string AnimationsRoot = "Assets/_Projet/Art/Animations/Characters";

        [MenuItem("Tools/Villa del Chef/Characters/1. Process All Character Spritesheets & Previews", priority = 20)]
        public static void ProcessAllCharacterSprites()
        {
            if (!Directory.Exists(FriendsRoot))
            {
                Debug.LogError($"[CharacterPipeline] No se encontró la ruta: {FriendsRoot}");
                return;
            }

            string[] directories = Directory.GetDirectories(FriendsRoot);
            int processedPreviews = 0;
            int processedSheets = 0;

            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (string dir in directories)
                {
                    string folderName = Path.GetFileName(dir);
                    string[] pngFiles = Directory.GetFiles(dir, "*.png");

                    foreach (string pngPath in pngFiles)
                    {
                        string fileName = Path.GetFileName(pngPath).ToLower();
                        string unityPath = pngPath.Replace('\\', '/');

                        if (fileName.StartsWith("movimientos"))
                        {
                            // Spritesheet de movimiento (4 columnas x 16 filas)
                            if (ConfigureMovementSpritesheet(unityPath))
                            {
                                processedSheets++;
                            }
                        }
                        else
                        {
                            // Preview estático individual (Single Sprite)
                            if (ConfigureStaticPreview(unityPath))
                            {
                                processedPreviews++;
                            }
                        }
                    }
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            Debug.Log($"<color=green><b>[CharacterPipeline] Procesamiento completado: {processedPreviews} previews estáticos, {processedSheets} spritesheets de movimiento.</b></color>");
        }

        private static bool ConfigureStaticPreview(string assetPath)
        {
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null) return false;

            bool changed = false;
            if (importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                changed = true;
            }
            if (importer.spriteImportMode != SpriteImportMode.Single)
            {
                importer.spriteImportMode = SpriteImportMode.Single;
                changed = true;
            }
            if (importer.filterMode != FilterMode.Point)
            {
                importer.filterMode = FilterMode.Point;
                changed = true;
            }
            if (importer.textureCompression != TextureImporterCompression.Uncompressed)
            {
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                changed = true;
            }
            if (importer.mipmapEnabled)
            {
                importer.mipmapEnabled = false;
                changed = true;
            }
            if (importer.spritePixelsPerUnit != 16)
            {
                importer.spritePixelsPerUnit = 16;
                changed = true;
            }

            if (changed)
            {
                importer.SaveAndReimport();
            }
            return true;
        }

        private static bool ConfigureMovementSpritesheet(string assetPath)
        {
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null) return false;

            // Cargar textura para inspeccionar dimensiones reales
            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            int width = tex != null ? tex.width : 1024;
            int height = tex != null ? tex.height : 1536;

            bool div4 = (width % 4 == 0);
            bool div16 = (height % 16 == 0);

            if (!div4 || !div16)
            {
                Debug.LogWarning($"[CharacterPipeline] Spritesheet '{assetPath}' dimensiones no estándar ({width}x{height}). W%4={width % 4}, H%16={height % 16}. Se calculará corte proporcional.");
            }

            float cellWidth = (float)width / 4f;
            float cellHeight = (float)height / 16f;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.spritePixelsPerUnit = 16;

            string baseName = Path.GetFileNameWithoutExtension(assetPath);
            SpriteMetaData[] sheet = new SpriteMetaData[64];

            // Unity texture Y=0 está abajo. Fila 1 (Idle Down) está arriba: y = height - (row + 1) * cellHeight
            for (int row = 0; row < 16; row++)
            {
                float y = height - (row + 1) * cellHeight;
                for (int col = 0; col < 4; col++)
                {
                    float x = col * cellWidth;
                    int frameIndex = row * 4 + col;

                    sheet[frameIndex] = new SpriteMetaData
                    {
                        name = $"{baseName}_{frameIndex}",
                        rect = new Rect(x, y, cellWidth, cellHeight),
                        alignment = (int)SpriteAlignment.BottomCenter,
                        pivot = new Vector2(0.5f, 0f)
                    };
                }
            }

#pragma warning disable CS0618
            importer.spritesheet = sheet;
#pragma warning restore CS0618
            importer.SaveAndReimport();
            return true;
        }

        [MenuItem("Tools/Villa del Chef/Characters/2. Build Character Database & Animators", priority = 21)]
        public static void BuildCharacterDatabaseAndAnimators()
        {
            if (!Directory.Exists(ResourcesCharactersRoot))
            {
                Directory.CreateDirectory(ResourcesCharactersRoot);
            }
            if (!Directory.Exists(AnimationsRoot))
            {
                Directory.CreateDirectory(AnimationsRoot);
            }

            string[] directories = Directory.GetDirectories(FriendsRoot);
            int createdOrUpdatedCount = 0;

            Debug.Log("<color=cyan><b>========================================================================================</b></color>");
            Debug.Log("<color=cyan><b>[CharacterPipeline] INFORME DE AUDITORÍA Y SINCRONIZACIÓN DE PERSONAJES</b></color>");
            Debug.Log("<color=cyan><b>========================================================================================</b></color>");

            foreach (string dir in directories)
            {
                string folderName = Path.GetFileName(dir);
                string characterID = NormalizeCharacterID(folderName);
                string displayName = FormatDisplayName(folderName);

                string normalPreviewPath = null;
                string blackChefPreviewPath = null;
                string whiteChefPreviewPath = null;

                string normalMovPath = null;
                string blackChefMovPath = null;
                string whiteChefMovPath = null;

                List<string> warnings = new List<string>();

                string[] pngs = Directory.GetFiles(dir, "*.png");
                foreach (string png in pngs)
                {
                    string fName = Path.GetFileName(png).ToLower();
                    string uPath = png.Replace('\\', '/');

                    if (fName.StartsWith("movimientos"))
                    {
                        if (fName == "movimientos_rnormal.png") normalMovPath = uPath;
                        else if (fName == "movimientos_rnchef.png") blackChefMovPath = uPath;
                        else if (fName == "movimientos_rbchef.png") whiteChefMovPath = uPath;
                        else if (fName == "movimientos.png")
                        {
                            warnings.Add("movimientos.png sin sufijo de vestuario (ambiguo)");
                            // Si no hay normalMovPath, podemos sugerirlo como fallback sin renombrar el archivo físico
                            if (normalMovPath == null) normalMovPath = uPath;
                        }
                    }
                    else
                    {
                        if (fName.EndsWith("_rnormal.png")) normalPreviewPath = uPath;
                        else if (fName.EndsWith("_rnchef.png")) blackChefPreviewPath = uPath;
                        else if (fName.EndsWith("_rbchef.png")) whiteChefPreviewPath = uPath;
                        else if (fName.Contains("rbnormal"))
                        {
                            warnings.Add($"{fName}: Clave 'rbnormal' no oficial");
                        }
                    }
                }

                // Cargar Sprites
                Sprite normalPreview = !string.IsNullOrEmpty(normalPreviewPath) ? AssetDatabase.LoadAssetAtPath<Sprite>(normalPreviewPath) : null;
                Sprite blackChefPreview = !string.IsNullOrEmpty(blackChefPreviewPath) ? AssetDatabase.LoadAssetAtPath<Sprite>(blackChefPreviewPath) : null;
                Sprite whiteChefPreview = !string.IsNullOrEmpty(whiteChefPreviewPath) ? AssetDatabase.LoadAssetAtPath<Sprite>(whiteChefPreviewPath) : null;

                // Crear / Actualizar AnimationClips & Animators por Outfit
                string charAnimDir = $"{AnimationsRoot}/{characterID}";
                if (!Directory.Exists(charAnimDir)) Directory.CreateDirectory(charAnimDir);

                RuntimeAnimatorController normalAnimator = BuildOutfitAnimator(characterID, "Normal", normalMovPath, charAnimDir);
                RuntimeAnimatorController blackChefAnimator = BuildOutfitAnimator(characterID, "ChefBlack", blackChefMovPath, charAnimDir);
                RuntimeAnimatorController whiteChefAnimator = BuildOutfitAnimator(characterID, "ChefWhite", whiteChefMovPath, charAnimDir);

                // Crear o actualizar CharacterSO
                string soPath = $"{ResourcesCharactersRoot}/{characterID}.asset";
                CharacterSO charSO = AssetDatabase.LoadAssetAtPath<CharacterSO>(soPath);
                bool isNew = false;
                if (charSO == null)
                {
                    charSO = ScriptableObject.CreateInstance<CharacterSO>();
                    charSO.characterID = characterID;
                    isNew = true;
                }

                charSO.displayName = displayName;
                charSO.normalPreview = normalPreview;
                charSO.blackChefPreview = blackChefPreview;
                charSO.whiteChefPreview = whiteChefPreview;
                charSO.portrait = normalPreview;

                charSO.normalAnimator = normalAnimator;
                charSO.blackChefAnimator = blackChefAnimator;
                charSO.whiteChefAnimator = whiteChefAnimator;

                if (string.IsNullOrEmpty(charSO.description))
                {
                    charSO.description = $"Chef y amigo de la villa: {displayName}.";
                }

                if (isNew)
                {
                    AssetDatabase.CreateAsset(charSO, soPath);
                }
                else
                {
                    EditorUtility.SetDirty(charSO);
                }

                createdOrUpdatedCount++;

                // Log audit table row
                string warnStr = warnings.Count > 0 ? string.Join(", ", warnings) : "OK";
                string normPrevOk = normalPreview != null ? "OK" : "FALTA";
                string blackPrevOk = blackChefPreview != null ? "OK" : "FALTA";
                string whitePrevOk = whiteChefPreview != null ? "OK" : "FALTA";
                string normMovOk = normalAnimator != null ? "OK" : "FALTA";
                string blackMovOk = blackChefAnimator != null ? "OK" : "FALTA";
                string whiteMovOk = whiteChefAnimator != null ? "OK" : "FALTA";

                Debug.Log($"<b>[{characterID}]</b> {displayName} | Preview: Normal={normPrevOk}, Black={blackPrevOk}, White={whitePrevOk} | Mov: Normal={normMovOk}, Black={blackMovOk}, White={whiteMovOk} | Warn: {warnStr}");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"<color=green><b>[CharacterPipeline] Base de datos completada: {createdOrUpdatedCount} CharacterSO sincronizados con éxito.</b></color>");
        }

        private static RuntimeAnimatorController BuildOutfitAnimator(string characterID, string outfitName, string spritesheetPath, string animDir)
        {
            if (string.IsNullOrEmpty(spritesheetPath)) return null;

            // Cargar los 64 sprites rebanados
            Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath(spritesheetPath);
            List<Sprite> frames = new List<Sprite>();
            foreach (var a in allAssets)
            {
                if (a is Sprite s) frames.Add(s);
            }

            if (frames.Count == 0) return null;

            // Ordenar por índice de nombre frame
            frames.Sort((a, b) =>
            {
                int idxA = ExtractIndex(a.name);
                int idxB = ExtractIndex(b.name);
                return idxA.CompareTo(idxB);
            });

            string outfitFolder = $"{animDir}/{outfitName}";
            if (!Directory.Exists(outfitFolder)) Directory.CreateDirectory(outfitFolder);

            // Generar o actualizar AnimationClips
            // Fila 1: Idle Down (0..3)
            var clipIdleDown = GetOrCreateClip($"{outfitFolder}/Idle_Down.anim", GetFramesSlice(frames, 0, 4), 6f, true);
            // Fila 2: Walk Down (4..7)
            var clipWalkDown = GetOrCreateClip($"{outfitFolder}/Walk_Down.anim", GetFramesSlice(frames, 4, 4), 8f, true);
            // Fila 3: Idle Up (8..11)
            var clipIdleUp = GetOrCreateClip($"{outfitFolder}/Idle_Up.anim", GetFramesSlice(frames, 8, 4), 6f, true);
            // Fila 4: Walk Up (12..15)
            var clipWalkUp = GetOrCreateClip($"{outfitFolder}/Walk_Up.anim", GetFramesSlice(frames, 12, 4), 8f, true);
            // Fila 5: Idle Left (16..19)
            var clipIdleLeft = GetOrCreateClip($"{outfitFolder}/Idle_Left.anim", GetFramesSlice(frames, 16, 4), 6f, true);
            // Fila 6: Walk Left (20..23)
            var clipWalkLeft = GetOrCreateClip($"{outfitFolder}/Walk_Left.anim", GetFramesSlice(frames, 20, 4), 8f, true);
            // Fila 7: Idle Right (24..27)
            var clipIdleRight = GetOrCreateClip($"{outfitFolder}/Idle_Right.anim", GetFramesSlice(frames, 24, 4), 6f, true);
            // Fila 8: Walk Right (28..31)
            var clipWalkRight = GetOrCreateClip($"{outfitFolder}/Walk_Right.anim", GetFramesSlice(frames, 28, 4), 8f, true);
            // Fila 9: Cook Down (32..35)
            var clipCookDown = GetOrCreateClip($"{outfitFolder}/Cook_Down.anim", GetFramesSlice(frames, 32, 4), 8f, true);
            // Fila 13: Think (48..51)
            var clipThink = GetOrCreateClip($"{outfitFolder}/Think.anim", GetFramesSlice(frames, 48, 4), 6f, true);
            // Fila 16: Celebrate (60..63)
            var clipCelebrate = GetOrCreateClip($"{outfitFolder}/Celebrate.anim", GetFramesSlice(frames, 60, 4), 8f, false);

            // Generar o actualizar AnimatorController
            string controllerPath = $"{outfitFolder}/{characterID}_{outfitName}_Controller.controller";
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
            if (controller == null)
            {
                controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);

                // Agregar parámetros estándar (Regla 40)
                controller.AddParameter("MoveX", AnimatorControllerParameterType.Float);
                controller.AddParameter("MoveY", AnimatorControllerParameterType.Float);
                controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
                controller.AddParameter("IsCooking", AnimatorControllerParameterType.Bool);
                controller.AddParameter("IsThinking", AnimatorControllerParameterType.Bool);
                controller.AddParameter("IsCarrying", AnimatorControllerParameterType.Bool);
                controller.AddParameter("Pickup", AnimatorControllerParameterType.Trigger);
                controller.AddParameter("Serve", AnimatorControllerParameterType.Trigger);
                controller.AddParameter("Celebrate", AnimatorControllerParameterType.Trigger);

                var rootStateMachine = controller.layers[0].stateMachine;

                // Crear Estados
                var stateIdle = rootStateMachine.AddState("Idle_Down");
                stateIdle.motion = clipIdleDown;
                rootStateMachine.defaultState = stateIdle;

                var stateWalk = rootStateMachine.AddState("Walk_Down");
                stateWalk.motion = clipWalkDown;

                var stateCook = rootStateMachine.AddState("Cook");
                stateCook.motion = clipCookDown;

                var stateThink = rootStateMachine.AddState("Think");
                stateThink.motion = clipThink;

                var stateCelebrate = rootStateMachine.AddState("Celebrate");
                stateCelebrate.motion = clipCelebrate;

                // Transición Idle -> Walk
                var toWalk = stateIdle.AddTransition(stateWalk);
                toWalk.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
                toWalk.hasExitTime = false;
                toWalk.duration = 0.1f;

                // Transición Walk -> Idle
                var toIdle = stateWalk.AddTransition(stateIdle);
                toIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");
                toIdle.hasExitTime = false;
                toIdle.duration = 0.1f;

                // Transición AnyState -> Cook
                var toCook = rootStateMachine.AddAnyStateTransition(stateCook);
                toCook.AddCondition(AnimatorConditionMode.If, 0, "IsCooking");
                toCook.hasExitTime = false;
                toCook.duration = 0.1f;

                var fromCook = stateCook.AddTransition(stateIdle);
                fromCook.AddCondition(AnimatorConditionMode.IfNot, 0, "IsCooking");
                fromCook.hasExitTime = false;
                fromCook.duration = 0.1f;
            }

            return controller;
        }

        private static AnimationClip GetOrCreateClip(string clipPath, List<Sprite> spriteFrames, float sampleRate, bool loop)
        {
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
            bool isNew = false;
            if (clip == null)
            {
                clip = new AnimationClip();
                isNew = true;
            }

            clip.frameRate = sampleRate;
            if (loop)
            {
                var settings = AnimationUtility.GetAnimationClipSettings(clip);
                settings.loopTime = true;
                AnimationUtility.SetAnimationClipSettings(clip, settings);
            }

            EditorCurveBinding binding = new EditorCurveBinding
            {
                type = typeof(SpriteRenderer),
                path = "",
                propertyName = "m_Sprite"
            };

            ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[spriteFrames.Count];
            float frameDuration = 1f / sampleRate;
            for (int i = 0; i < spriteFrames.Count; i++)
            {
                keyframes[i] = new ObjectReferenceKeyframe
                {
                    time = i * frameDuration,
                    value = spriteFrames[i]
                };
            }

            AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

            if (isNew)
            {
                AssetDatabase.CreateAsset(clip, clipPath);
            }
            else
            {
                EditorUtility.SetDirty(clip);
            }

            return clip;
        }

        private static List<Sprite> GetFramesSlice(List<Sprite> allFrames, int start, int count)
        {
            List<Sprite> slice = new List<Sprite>();
            for (int i = start; i < start + count && i < allFrames.Count; i++)
            {
                slice.Add(allFrames[i]);
            }
            return slice;
        }

        private static int ExtractIndex(string name)
        {
            int lastUnderscore = name.LastIndexOf('_');
            if (lastUnderscore >= 0 && lastUnderscore < name.Length - 1)
            {
                if (int.TryParse(name.Substring(lastUnderscore + 1), out int val))
                {
                    return val;
                }
            }
            return 0;
        }

        public static string NormalizeCharacterID(string folderName)
        {
            // Reemplazar espacios y caracteres especiales por guiones bajos
            string normalized = folderName.Trim().ToLower().Replace(' ', '_').Replace('.', '_').Replace('-', '_');
            return normalized;
        }

        public static string FormatDisplayName(string folderName)
        {
            // Devolver nombre legible (ej: "diego serena" -> "Diego Serena", "andres_arica" -> "Andrés Arica")
            string clean = folderName.Replace('_', ' ').Replace('.', ' ').Trim();
            var words = clean.Split(' ');
            for (int i = 0; i < words.Length; i++)
            {
                if (words[i].Length > 0)
                {
                    words[i] = char.ToUpper(words[i][0]) + words[i].Substring(1).ToLower();
                }
            }
            return string.Join(" ", words);
        }
    }
}
#endif
