
public class RefineryData : DataModule
{
	public RealSpacePosition refineryPosition;
	public bool putInReserves = false;
	public bool productionActive = true;
	public bool autoFillFleets = false;

	public float refineryCollectionStorageCapacity = 10;
	public float productionSpeed = 1;
	public bool productionAffectedBySimSpeed = true;
}