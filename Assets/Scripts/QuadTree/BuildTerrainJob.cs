using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace QuadTerrain
{
    [BurstCompile]
    public struct BuildTerrainJob : IJob
    {
        public float2 CameraPosition;
        public NativeList<RenderNode> RenderNodes;
        public NativeArray<float4> Planes;
        public NodeBounds Bounds;

        public static BuildTerrainJob Create(float extents, float3 cameraPosition, NativeArray<float4> planes, NativeList<RenderNode> renderNodes)
        {
            float2 camPosition = new float2(cameraPosition.x, cameraPosition.z);

            float2 center = default;
            center.x = FloorToMultiple(camPosition.x, extents); // 32f
            center.y = FloorToMultiple(camPosition.y, extents); // 32f

            NodeBounds bounds = new NodeBounds
            {
                min = new float2(center.x - extents, center.y - extents),
                max = new float2(center.x + extents, center.y + extents)
            };

            BuildTerrainJob job = new BuildTerrainJob
            {
                CameraPosition = camPosition,
                Bounds = bounds,
                Planes = planes,
                RenderNodes = renderNodes
            };
            return job;
        }

        private static float FloorToMultiple(float value, float factor)
        {
            return math.floor(value / factor) * factor;
        }

        public void Execute()
        {
            TerrainTreeBuilder builder = new TerrainTreeBuilder { CameraPosition = CameraPosition };
            QuadTree tree = QuadTree.Create(2048, QuadTreeType.Terrain, Allocator.Temp);
            tree.TerrainTreeBuilder = builder;
            tree.Construct(Bounds);
            builder.GetRenderNodes(tree, RenderNodes, Planes);

            for (int i = 0; i < RenderNodes.Length; i++)
            {
                var node = RenderNodes[i].Node;

                if (!node.IsLeaf)
                {
                    continue;
                }

                float lasted_size = QuadTreePatch.findNodeSize(node.Bounds.min.x, node.Bounds.min.y);
                if (lasted_size < 0.5f) // not contain
                    QuadTreePatch.addNodeSize(node.Bounds.min.x, node.Bounds.min.y, node.Bounds.Size.x);
                else if (lasted_size - 1 > node.Bounds.Size.x) // update min value
                    QuadTreePatch.updateNodeSize(node.Bounds.min.x, node.Bounds.min.y, node.Bounds.Size.x);
            }
        }
    }
}