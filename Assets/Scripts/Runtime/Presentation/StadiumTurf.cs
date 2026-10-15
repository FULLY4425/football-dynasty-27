using UnityEngine;
using UnityEngine.Rendering;

namespace FootballDynasty
{
    public sealed class StadiumTurf : MonoBehaviour
    {
        [SerializeField]
        private Material turfMaterial;
        private Mesh mesh;
        public void Create(Transform parent)
        {
            if (!turfMaterial)
                turfMaterial = Resources.Load<Material>("Graphics/StadiumTurf");
            if (!turfMaterial)
            {
                Debug.LogError("Falta StadiumTurf.mat. Ejecuta Preparar gráficos URP.");
                return;
            }

            var pitch = new GameObject("Pitch · one quad / normal-mapped grass", typeof(MeshFilter), typeof(MeshRenderer));
            pitch.transform.SetParent(parent, false);
            mesh = new Mesh
            {
                name = "105 × 68 m · four vertices"
            };
            mesh.vertices = new[]
            {
                new Vector3(-52.5f, .026f, -34),
                new Vector3(-52.5f, .026f, 34),
                new Vector3(52.5f, .026f, -34),
                new Vector3(52.5f, .026f, 34)
            };
            mesh.uv = new[]
            {
                new Vector2(0, 0),
                new Vector2(0, 1),
                new Vector2(1, 0),
                new Vector2(1, 1)
            };
            mesh.triangles = new[]
            {
                0,
                1,
                2,
                2,
                1,
                3
            };
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
            pitch.GetComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = pitch.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = turfMaterial;
            renderer.receiveShadows = true;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
        }

        void OnDestroy()
        {
            if (mesh)
                Destroy(mesh);
        }
    }
}
