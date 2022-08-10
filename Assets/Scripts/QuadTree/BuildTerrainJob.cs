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
            return Create((int)math.round(extents / 64), (int)math.round(extents / 64), cameraPosition, planes, renderNodes);
        }

        public static BuildTerrainJob Create(int x_extents, int z_extents, float3 cameraPosition, NativeArray<float4> planes, NativeList<RenderNode> renderNodes)
        {
            float2 camPosition = new float2(cameraPosition.x, cameraPosition.z);

            int2 center = default;
            //var index = TerrainGenerator.getIndex(camPosition.x, camPosition.y);
            //center.x = index.x;
            //center.y = index.z;
            center.x = ((int)camPosition.x / 16) * 16;
            center.y = ((int)camPosition.y / 16) * 16;
            //var unity_loc = TerrainGenerator.getN25E121Location();
            //center.x = (float)(unity_loc.x + index.x * QuadTreePatch.x_dem_interval);
            //center.y = (float)(unity_loc.z + index.z * QuadTreePatch.z_dem_interval);
            //TerrainGenerator.meow($"point {index.x} {index.z}");
            //TerrainGenerator.meow($"center {center.x} {center.y}");
            //TerrainGenerator.meow($"camera position {cameraPosition.x} {cameraPosition.z}");

            //center.x = FloorToMultiple(camPosition.x, (float)x_extents); // 32f
            //center.y = FloorToMultiple(camPosition.y, (float)z_extents); // 32f

            NodeBounds bounds = new NodeBounds
            {
                min = new int2(center.x - x_extents, center.y - z_extents),
                max = new int2(center.x + x_extents, center.y + z_extents)
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
            QuadTree tree = QuadTree.Create(32768, QuadTreeType.Terrain, Allocator.Temp);
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

                DEMCoord coord = new DEMCoord(node.Bounds.min.x, node.Bounds.min.y);
                int node_level = node.Bounds.Size.x; //  QuadTreePatch.calcLevel(node.Bounds.Size.x)
                //int lasted_level = QuadTreePatch.fetchNodeLevel(coord);
                //if (lasted_level == 0) // not contain
                    QuadTreePatch.addNodeLevel(coord, node_level);
                //else if (lasted_level > node.Bounds.Size.x) // update min value
                //QuadTreePatch.updateNodeLevel(coord, node_level);
            }
        }
    }
}