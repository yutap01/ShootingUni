//ブロックを生成できる高さの範囲を規定する構造体
public struct ValueRange {
	int min;
	public int Min {
		get {
			return this.min;
		}
	}
	int max;
	public int Max {
		get {
			return this.max;
		}
	}
	public ValueRange(int min, int max) {
		this.min = min;
		this.max = max;
	}
}