using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pamux.Lib.Procedural.Models;
using System.Threading.Tasks;

namespace Pamux.Lib.Procedural.Interfaces
{
    public interface ITerrainChunkProvider
    {
        Task<TerrainChunk> GetChunkAsync(Transform parent, int x, int y, int lodIndex);
    }
}