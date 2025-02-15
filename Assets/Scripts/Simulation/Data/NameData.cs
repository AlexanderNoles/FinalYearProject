
public class NameData : DataModule
{
	public string baseName;

	public virtual string GetName() 
	{ 
		return baseName;
	}

	public virtual void Generate()
	{
		baseName = "";
	}
}