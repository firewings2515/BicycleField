using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace QuadTerrain
{
    [BurstCompile]
    public struct BuildTerrainJob : IJob
    {
        public NativeArray<float2> CameraPositions;
        public NativeList<RenderNode> RenderNodes;
        public NativeArray<float4> Planess;
        public NodeBounds Bounds;

        public static BuildTerrainJob Create(float extents, float3 cameraPosition, NativeArray<float4> planes, NativeList<RenderNode> renderNodes)
        {
            NativeArray<float3> cameraPositions = new NativeArray<float3>(1, Allocator.Persistent);
            cameraPositions[0] = cameraPosition;
            return Create((int)math.round(extents / 64), (int)math.round(extents / 64), cameraPositions, planes, renderNodes, 1);            
        }

        public static BuildTerrainJob Create(int x_extents, int z_extents, NativeArray<float3> cameraPositions, NativeArray<float4> planess, NativeList<RenderNode> renderNodes, int observation_num)
        {
            NativeArray<float2> camPositions = new NativeArray<float2>(1024, Allocator.Persistent);
            int2 bounds_center = default;
            float2 average_center = default;
            int final_extents = x_extents;

            for (int observation_index = 0; observation_index < observation_num; observation_index++)
            {
                camPositions[observation_index] = new float2(cameraPositions[observation_index].x, cameraPositions[observation_index].z);
                //center.x = FloorToMultiple(camPosition.x, (float)x_extents); // 32f
                //center.y = FloorToMultiple(camPosition.y, (float)z_extents); // 32f
                average_center.x += camPositions[observation_index].x;
                average_center.y += camPositions[observation_index].y;
            }
            average_center /= observation_num;

            bounds_center.x = ((int)average_center.x / 16) * 16;
            bounds_center.y = ((int)average_center.y / 16) * 16;
            NodeBounds bounds = new NodeBounds
            {
                min = new int2(bounds_center.x - x_extents, bounds_center.y - z_extents),
                max = new int2(bounds_center.x + x_extents, bounds_center.y + z_extents)
            };

            BuildTerrainJob job = new BuildTerrainJob
            {
                CameraPositions = camPositions,
                Bounds = bounds,
                Planess = planess,
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
            TerrainTreeBuilder builder = new TerrainTreeBuilder { CameraPositions = CameraPositions };
            QuadTree tree = QuadTree.Create(32768, QuadTreeType.Terrain, Allocator.Temp);
            tree.TerrainTreeBuilder = builder;
            tree.Construct(Bounds);
            builder.GetRenderNodes(tree, RenderNodes, Planess);

            for (int i = 0; i < RenderNodes.Length; i++)
            {
                var node = RenderNodes[i].Node;

                if (!node.IsLeaf)
                {
                    continue;
                }

                DEMCoord coord = new DEMCoord(node.Bounds.min.x, node.Bounds.min.y);
                int node_level = node.Bounds.Size.x; //  QuadTreePatch.calcLevel(node.Bounds.Size.x)
                int lasted_level = QuadTreePatch.fetchNodeLevel(coord);
                if (lasted_level == 0) // not contain
                    QuadTreePatch.addNodeLevel(coord, node_level);
                else
                    TerrainGenerator.meow($"find {coord.x} {coord.z} same value {node_level}={lasted_level}");
                //else if (lasted_level > node.Bounds.Size.x) // update min value
                //QuadTreePatch.updateNodeLevel(coord, node_level);
            }
        }
    }
}