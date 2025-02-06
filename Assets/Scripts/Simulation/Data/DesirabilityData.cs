
public class DesirabilityData : DataModule
{
	public int desirability = 1;
	public int lastTickTime;

	public virtual RealSpacePosition GetCellCenter()
	{
		return null;
	}

	public virtual RealSpacePosition GetActualPosition() 
	{
		return GetCellCenter();
	}

	public virtual void UpdateDesirability()
	{

	}
}