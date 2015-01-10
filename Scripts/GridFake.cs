using UnityEngine;
using System.Collections;

public class GridFake<T> {



	private T[,,] grid;


	//変更
	private const int minX = 0;
	private const int minY = 0;
	private const int minZ = 0;
	private const int maxX = 1;
	private const int maxY = 1;
	private const int maxZ = 1;
	//private int minX, minY, minZ;
	//private int maxX, maxY, maxZ;

	public GridFake() {
		//修正
		//grid = new T[0, 0, 0];
		Vector3i size = GetSize();
		grid = new T[size.z, size.y, size.x];
	}

	/* 消去
	public GridFake(Vector3i min, Vector3i max) {
		this.minX = min.x;
		this.minY = min.y;
		this.minZ = min.z;

		this.maxX = max.x;
		this.maxY = max.y;
		this.maxZ = max.z;

		Vector3i size = GetSize();
		grid = new T[size.z, size.y, size.x];
	}
	*/


	public void Set(T obj, Vector3i pos) {
		Set(obj, pos.x, pos.y, pos.z);
	}
	public void Set(T obj, int x, int y, int z) {
		//追加
		int cx = x % maxX;
		int cy = y % maxY;
		int cz = z % maxZ;
	
		//修正
		//grid[z - minZ, y - minY, x - minX] = obj;
		grid[cz, cy, cx] = obj;
	}

	public T Get(Vector3i pos) {
		return Get(pos.x, pos.y, pos.z);
	}
	public T Get(int x, int y, int z) {
		//追加
		int cx = x % maxX;
		int cy = y % maxY;
		int cz = z % maxZ;

		//修正
		//Debug.Log("x:"+x + " y:"+y + " z:"+z);
		//return grid[z - minZ, y - minY, x - minX];
		return grid[cz, cy, cx];
	}

	public T SafeGet(Vector3i pos) {
		if (!IsCorrectIndex(pos.x, pos.y, pos.z)) return default(T);
		
		//修正
		//return grid[pos.z - minZ, pos.y - minY, pos.x - minX];
		return Get(pos);
	}

	public T SafeGet(int x, int y, int z) {
		if (!IsCorrectIndex(x, y, z)) return default(T);

		//修正
		//return grid[z - minZ, y - minY, x - minX];
		return Get(x, y, z);
	}

	
	
	
	
	public void AddOrReplace(T obj, Vector3i pos) {
		AddOrReplace(obj, pos.x, pos.y, pos.z);
	}

	public void AddOrReplace(T obj, int x, int y, int z) {
		/* 修正
		int dMinX = 0, dMinY = 0, dMinZ = 0;
		int dMaxX = 0, dMaxY = 0, dMaxZ = 0;

		if (x < minX) dMinX = x - minX;
		if (y < minY) dMinY = y - minY;
		if (z < minZ) dMinZ = z - minZ;

		if (x >= maxX) dMaxX = x - maxX + 1;
		if (y >= maxY) dMaxY = y - maxY + 1;
		if (z >= maxZ) dMaxZ = z - maxZ + 1;

		if (dMinX != 0 || dMinY != 0 || dMinZ != 0 ||
			 dMaxX != 0 || dMaxY != 0 || dMaxZ != 0) {
			Increase(dMinX, dMinY, dMinZ,
						 dMaxX, dMaxY, dMaxZ);
		}

		grid[z - minZ, y - minY, x - minX] = obj;
		 */
		Set(obj, x, y, z);
	}
	
	/*削除
	private void Increase(int dMinX, int dMinY, int dMinZ,
											int dMaxX, int dMaxY, int dMaxZ) {
		int oldMinX = minX;
		int oldMinY = minY;
		int oldMinZ = minZ;

		int oldMaxX = maxX;
		int oldMaxY = maxY;
		int oldMaxZ = maxZ;

		T[, ,] oldGrid = grid;

		minX += dMinX;
		minY += dMinY;
		minZ += dMinZ;

		maxX += dMaxX;
		maxY += dMaxY;
		maxZ += dMaxZ;

		int sizeX = maxX - minX;
		int sizeY = maxY - minY;
		int sizeZ = maxZ - minZ;
		grid = new T[sizeZ, sizeY, sizeX];

		for (int z = oldMinZ; z < oldMaxZ; z++) {
			for (int y = oldMinY; y < oldMaxY; y++) {
				for (int x = oldMinX; x < oldMaxX; x++) {
					grid[z - minZ, y - minY, x - minX] = oldGrid[z - oldMinZ, y - oldMinY, x - oldMinX];
				}
			}
		}
	}
	 */ 

	/* 削除
	public bool TestIndex(int x, int y, int z) {
		if (!(x >= 0 && x < grid.GetLength(2))) Debug.Log("Error X " + x);
		if (!(y >= 0 && y < grid.GetLength(1))) Debug.Log("Error Y " + y);
		if (!(z >= 0 && z < grid.GetLength(0))) Debug.Log("Error Z " + z);
		return x >= 0 && x < grid.GetLength(2) &&
				 y >= 0 && y < grid.GetLength(1) &&
				 z >= 0 && z < grid.GetLength(0);
	} */

	// 削除
	public bool IsCorrectIndex(Vector3i pos) {
		return IsCorrectIndex(pos.x, pos.y, pos.z);
	}
	public bool IsCorrectIndex(int x, int y, int z) {
		if (x < minX || y < minY || z < minZ) return false;
		if (x >= maxX || y >= maxY || z >= maxZ) return false;
		return true;
	}
	

	public Vector3i GetMin() {
		return new Vector3i(minX, minY, minZ);
	}

	public Vector3i GetMax() {
		return new Vector3i(maxX, maxY, maxZ);
	}

	public Vector3i GetSize() {
		return new Vector3i(maxX - minX, maxY - minY, maxZ - minZ);
	}

	public int GetMinX() {
		return minX;
	}
	public int GetMinY() {
		return minY;
	}
	public int GetMinZ() {
		return minZ;
	}

	public int GetMaxX() {
		return maxX;
	}
	public int GetMaxY() {
		return maxY;
	}
	public int GetMaxZ() {
		return maxZ;
	}
}