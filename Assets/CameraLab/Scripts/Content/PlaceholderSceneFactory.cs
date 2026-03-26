using CameraLab.Runtime;
using CameraLab.ScriptableObjects;
using CameraLab.Utilities;
using UnityEngine;

namespace CameraLab.Content
{
    public static class PlaceholderSceneFactory
    {
        public sealed class RuntimeSceneBuildResult
        {
            public GameObject root;
            public Transform subjectTransform;
        }

        public static RuntimeSceneBuildResult Build(SceneConfig config, Transform parent)
        {
            switch (config.sceneId)
            {
                case "indoor_tungsten":
                    return BuildIndoorTungsten(config, parent);
                case "sports_runner":
                    return BuildSportsScene(config, parent);
                default:
                    return BuildOutdoorPortrait(config, parent);
            }
        }

        private static RuntimeSceneBuildResult BuildOutdoorPortrait(SceneConfig config, Transform parent)
        {
            var root = CreateRoot(config, parent);
            CreateGround(root.transform, new Vector3(0f, 0f, 8f), new Vector3(18f, 1f, 18f), new Color(0.41f, 0.62f, 0.31f));
            CreateBackdrop(root.transform, config.backgroundDistanceMeters, new Color(0.7f, 0.78f, 0.64f), "Hedge");
            CreateTree(root.transform, new Vector3(-4f, 2.5f, 10f), new Color(0.25f, 0.38f, 0.18f));
            CreateTree(root.transform, new Vector3(5f, 2.8f, 12f), new Color(0.23f, 0.35f, 0.16f));
            var subject = CreateSubject(root.transform, new Vector3(0f, 0.95f, config.subjectDistanceMeters), new Color(0.88f, 0.74f, 0.62f), new Color(0.15f, 0.19f, 0.32f));
            CreateReflector(root.transform, new Vector3(2.2f, 1.2f, config.subjectDistanceMeters + 0.6f), new Color(0.95f, 0.93f, 0.85f));
            return new RuntimeSceneBuildResult { root = root, subjectTransform = subject };
        }

        private static RuntimeSceneBuildResult BuildIndoorTungsten(SceneConfig config, Transform parent)
        {
            var root = CreateRoot(config, parent);
            CreateGround(root.transform, new Vector3(0f, 0f, 5f), new Vector3(10f, 1f, 10f), new Color(0.33f, 0.23f, 0.17f));
            CreateWall(root.transform, new Vector3(0f, 2.5f, 10f), new Vector3(10f, 5f, 0.3f), new Color(0.58f, 0.47f, 0.39f));
            CreateWall(root.transform, new Vector3(-5f, 2.5f, 5f), new Vector3(0.3f, 5f, 10f), new Color(0.45f, 0.34f, 0.28f));
            CreateWall(root.transform, new Vector3(5f, 2.5f, 5f), new Vector3(0.3f, 5f, 10f), new Color(0.45f, 0.34f, 0.28f));
            CreateLamp(root.transform, new Vector3(-2.5f, 2.3f, 3.2f), new Color(1f, 0.74f, 0.42f));
            CreateTable(root.transform, new Vector3(0f, 0.75f, config.subjectDistanceMeters + 0.4f), new Color(0.39f, 0.28f, 0.18f));
            var subject = CreateSubject(root.transform, new Vector3(0f, 0.95f, config.subjectDistanceMeters), new Color(0.82f, 0.7f, 0.58f), new Color(0.35f, 0.24f, 0.18f));
            return new RuntimeSceneBuildResult { root = root, subjectTransform = subject };
        }

        private static RuntimeSceneBuildResult BuildSportsScene(SceneConfig config, Transform parent)
        {
            var root = CreateRoot(config, parent);
            CreateGround(root.transform, new Vector3(0f, 0f, 15f), new Vector3(24f, 1f, 36f), new Color(0.26f, 0.52f, 0.29f));
            CreateTrackLane(root.transform, new Vector3(0f, 0.02f, 10f), new Vector3(5f, 0.02f, 24f), new Color(0.68f, 0.24f, 0.19f));
            CreateBackdrop(root.transform, config.backgroundDistanceMeters, new Color(0.46f, 0.58f, 0.68f), "Stands");
            var subject = CreateSubject(root.transform, new Vector3(0f, 0.95f, config.subjectDistanceMeters), new Color(0.85f, 0.69f, 0.54f), new Color(0.79f, 0.11f, 0.18f));
            var mover = subject.gameObject.AddComponent<MovingSubjectController>();
            mover.Configure(config.subjectSpeedMetersPerSecond, 4f, Vector3.right);
            return new RuntimeSceneBuildResult { root = root, subjectTransform = subject };
        }

        private static GameObject CreateRoot(SceneConfig config, Transform parent)
        {
            var root = new GameObject($"{config.displayName}_Runtime");
            root.transform.SetParent(parent, false);
            return root;
        }

        private static void CreateGround(Transform parent, Vector3 position, Vector3 scale, Color color)
        {
            var ground = CreatePrimitive(parent, PrimitiveType.Cube, "Ground", position, scale, color);
            ground.transform.position = position - new Vector3(0f, scale.y * 0.5f, 0f);
        }

        private static void CreateBackdrop(Transform parent, float distance, Color color, string name)
        {
            CreatePrimitive(parent, PrimitiveType.Cube, name, new Vector3(0f, 2.8f, distance), new Vector3(14f, 5.5f, 0.4f), color);
        }

        private static void CreateTree(Transform parent, Vector3 position, Color color)
        {
            var trunk = CreatePrimitive(parent, PrimitiveType.Cylinder, "TreeTrunk", position - new Vector3(0f, 1.2f, 0f), new Vector3(0.35f, 1.2f, 0.35f), new Color(0.32f, 0.21f, 0.13f));
            trunk.transform.position += Vector3.up * 1.2f;
            var foliage = CreatePrimitive(parent, PrimitiveType.Sphere, "TreeFoliage", position + new Vector3(0f, 1.2f, 0f), new Vector3(2.2f, 2f, 2.2f), color);
            foliage.transform.position += Vector3.up * 1f;
        }

        private static void CreateWall(Transform parent, Vector3 position, Vector3 scale, Color color)
        {
            CreatePrimitive(parent, PrimitiveType.Cube, "Wall", position, scale, color);
        }

        private static void CreateLamp(Transform parent, Vector3 position, Color color)
        {
            var baseObject = CreatePrimitive(parent, PrimitiveType.Cylinder, "LampBase", position - new Vector3(0f, 1.2f, 0f), new Vector3(0.08f, 1.2f, 0.08f), new Color(0.16f, 0.14f, 0.12f));
            baseObject.transform.position += Vector3.up * 1.2f;
            var shade = CreatePrimitive(parent, PrimitiveType.Sphere, "LampShade", position, new Vector3(0.55f, 0.4f, 0.55f), color);
            var light = new GameObject("LampLight").AddComponent<Light>();
            light.transform.SetParent(parent, false);
            light.transform.position = position;
            light.type = LightType.Point;
            light.color = color;
            light.intensity = 6f;
            light.range = 10f;
            light.shadows = LightShadows.None;
            shade.transform.localScale = new Vector3(0.75f, 0.55f, 0.75f);
        }

        private static void CreateTable(Transform parent, Vector3 position, Color color)
        {
            var top = CreatePrimitive(parent, PrimitiveType.Cube, "TableTop", position, new Vector3(2.8f, 0.12f, 1.6f), color);
            top.transform.position = position;
            CreatePrimitive(parent, PrimitiveType.Cylinder, "TableLegA", position + new Vector3(-1f, -0.6f, -0.55f), new Vector3(0.07f, 0.6f, 0.07f), color);
            CreatePrimitive(parent, PrimitiveType.Cylinder, "TableLegB", position + new Vector3(1f, -0.6f, -0.55f), new Vector3(0.07f, 0.6f, 0.07f), color);
            CreatePrimitive(parent, PrimitiveType.Cylinder, "TableLegC", position + new Vector3(-1f, -0.6f, 0.55f), new Vector3(0.07f, 0.6f, 0.07f), color);
            CreatePrimitive(parent, PrimitiveType.Cylinder, "TableLegD", position + new Vector3(1f, -0.6f, 0.55f), new Vector3(0.07f, 0.6f, 0.07f), color);
        }

        private static void CreateReflector(Transform parent, Vector3 position, Color color)
        {
            var reflector = CreatePrimitive(parent, PrimitiveType.Cube, "Reflector", position, new Vector3(0.1f, 1.3f, 1.1f), color);
            reflector.transform.rotation = Quaternion.Euler(0f, -24f, 0f);
        }

        private static void CreateTrackLane(Transform parent, Vector3 position, Vector3 scale, Color color)
        {
            var lane = CreatePrimitive(parent, PrimitiveType.Cube, "TrackLane", position, scale, color);
            lane.transform.position = position - new Vector3(0f, scale.y * 0.5f, 0f);
        }

        private static Transform CreateSubject(Transform parent, Vector3 position, Color skinColor, Color shirtColor)
        {
            var root = new GameObject("Subject");
            root.transform.SetParent(parent, false);
            root.transform.localPosition = position;

            var body = CreatePrimitive(root.transform, PrimitiveType.Capsule, "Body", Vector3.zero, new Vector3(0.8f, 1.8f, 0.8f), shirtColor);
            body.transform.localPosition = new Vector3(0f, 0.9f, 0f);

            var head = CreatePrimitive(root.transform, PrimitiveType.Sphere, "Head", new Vector3(0f, 2.15f, 0f), new Vector3(0.55f, 0.55f, 0.55f), skinColor);
            head.transform.localPosition = new Vector3(0f, 2.05f, 0f);
            return root.transform;
        }

        private static GameObject CreatePrimitive(Transform parent, PrimitiveType type, string name, Vector3 position, Vector3 scale, Color color)
        {
            var primitive = GameObject.CreatePrimitive(type);
            primitive.name = name;
            primitive.transform.SetParent(parent, false);
            primitive.transform.localPosition = position;
            primitive.transform.localScale = scale;
            ApplyMaterial(primitive, color);
            return primitive;
        }

        private static void ApplyMaterial(GameObject target, Color color)
        {
            var renderer = target.GetComponent<Renderer>();
            if (renderer == null)
            {
                return;
            }

            // TODO: Swap these runtime materials for authored URP materials once art content exists.
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var material = new Material(shader);
            material.color = color;
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            if (material.HasProperty("_Smoothness"))
            {
                material.SetFloat("_Smoothness", 0.2f);
            }

            renderer.sharedMaterial = material;
        }
    }
}
